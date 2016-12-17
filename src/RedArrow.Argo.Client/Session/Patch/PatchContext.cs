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
            return Resource.Attributes?[attrName] != null;
        }

        public void SetReference(string attrName, Guid rltnId, string rltnType, bool persisted)
        {
            if (Resource.Relationships == null)
            {
                Resource.Relationships = new Dictionary<string, Relationship>();
            }

            if (persisted && TransientReferences.ContainsKey(attrName))
            {
                TransientReferences.Remove(attrName);
            }

            if (!persisted)
            {
                TransientReferences[attrName] = rltnId;
            }

            Resource.Relationships[attrName] = new Relationship
            {
                Data = JToken.FromObject(new ResourceIdentifier { Id = rltnId, Type = rltnType })
            };
        }

        public Guid? GetReference(string attrName)
        {
            return Resource.Relationships?[attrName]?.Data?.SelectToken("Id")?.Value<Guid>();
        }

        public IDictionary<string, Guid> GetTransientReferences()
        {
            return new ReadOnlyDictionary<string, Guid>(TransientReferences);
        }
    }
}