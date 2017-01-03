using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.JsonModels;

namespace RedArrow.Argo.Client.Session.Patch
{
    public class PatchContext
    {
        internal Resource Resource { get; }
        private IDictionary<string, Guid> TransientReferences { get; }

        internal PatchContext(Resource resource)
        {
            if(resource == null) throw new ArgumentNullException(nameof(resource));

            Resource = resource;
            TransientReferences = new Dictionary<string, Guid>();
        }

        public TAttr GetAttribute<TAttr>(string attrName)
        {
            JToken jValue;
            if (Resource.Attributes != null && Resource.Attributes.TryGetValue(attrName, out jValue))
            {
                return jValue.Value<TAttr>();
            }
            return default(TAttr);
        }

        public void SetAttriute(string attrName, object attrValue)
        {
            if (Resource.Attributes == null)
            {
                Resource.Attributes = new JObject();
            }
            Resource.Attributes[attrName] = JToken.FromObject(attrValue);
        }

        public bool ContainsAttribute(string attrName)
        {
            return Resource.Attributes[attrName] != null;
        }

        public void SetRelated(string attrName, ResourceIdentifier resourceIdentifier, bool transient)
        {
            if (Resource.Relationships == null)
            {
                Resource.Relationships = new Dictionary<string, Relationship>();
            }

            if ((!transient || resourceIdentifier == null) && TransientReferences.ContainsKey(attrName))
            {
                TransientReferences.Remove(attrName);
            }

            if (transient && resourceIdentifier != null)
            {
                TransientReferences[attrName] = resourceIdentifier.Id;
            }

            Resource.Relationships[attrName] = new Relationship
            {
                Data = resourceIdentifier.ToJToken()
            };
        }

        public Guid? GetRelated(string attrName)
        {
            return GetRelationship(attrName)
                ?.Data
                ?.SelectToken("id")
                ?.Value<Guid>();
        }

        public Relationship GetRelationship(string rltnName)
        {
            Relationship rltn;
            if (Resource.Relationships != null && Resource.Relationships.TryGetValue(rltnName, out rltn))
            {
                return rltn;
            }
            return null;
        }

        public void SetRelationship(string rltnName, Relationship rltn)
        {
            if (Resource.Relationships == null)
            {
                Resource.Relationships = new Dictionary<string, Relationship>();
            }

            Resource.Relationships[rltnName] = rltn;
        }

        public void AddRelated(string rltnName, Guid rltnId, string rltnType, bool persisted)
        {
            if (Resource.Relationships == null)
            {
                Resource.Relationships = new Dictionary<string, Relationship>();
            }

            if (persisted && TransientReferences.ContainsKey(rltnName))
            {
                TransientReferences.Remove(rltnName);
            }

            if (!persisted)
            {
                TransientReferences[rltnName] = rltnId;
            }

            Resource.Relationships[rltnName] = new Relationship
            {
                Data = JToken.FromObject(new ResourceIdentifier { Id = rltnId, Type = rltnType })
            };
        }

        public IDictionary<string, Guid> GetTransientReferences()
        {
            return new ReadOnlyDictionary<string, Guid>(TransientReferences);
        }
    }
}