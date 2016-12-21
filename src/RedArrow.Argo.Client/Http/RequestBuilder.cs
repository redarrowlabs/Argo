using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private const string JsonApiHeader = "application/vnd.api+json";

        private IModelRegistry ModelRegistry { get; }

        internal HttpRequestBuilder(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
        }

        public RequestContext GetResource(Guid id, Type modelType)
        {
            var resourceType = ModelRegistry.GetResourceType(modelType);

            return new RequestContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, $"{resourceType}/{id}"),

                ResourceId = id,
                ResourceType = resourceType
            };
        }

        public RequestContext GetRelated(object owner, string rltnName)
        {
            var id = ModelRegistry.GetModelId(owner);
            var resourceType = ModelRegistry.GetResourceType(owner.GetType());

            return new RequestContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, $"{resourceType}/{id}/{rltnName}"),

                ResourceId = id,
                ResourceType = resourceType,
            };
        }

        public RequestContext AddRelated(object owner, string rltnName, object related)
        {
            var ownerId = ModelRegistry.GetModelId(owner);
            var ownerResourceType = ModelRegistry.GetResourceType(owner.GetType());

            var relatedId = ModelRegistry.GetModelId(related);
            var relatedType = ModelRegistry.GetResourceType(related.GetType());

            return new RequestContext
            {
                Request = new HttpRequestMessage(HttpMethod.Post, $"{ownerResourceType}/{ownerId}/relationships/{rltnName}")
            }
        }

        public RequestContext CreateResource(Type modelType, object model)
        {
            var resourceType = ModelRegistry.GetResourceType(modelType);
            var attributes = model != null
                ? JObject.FromObject(ModelRegistry
                    .GetModelAttributes(modelType)
                    .ToDictionary(
                        x => x.AttributeName,
                        x => x.Property.GetValue(model)))
                : null;

            var root = ResourceRootCreate.FromAttributes(resourceType, attributes);

            return new RequestContext
            {
                Request = new HttpRequestMessage(HttpMethod.Post, resourceType)
                {
                    Content = BuildHttpContent(root.ToJson())
                },

                ResourceType = resourceType,
                Attributes = attributes
            };
        }

        public RequestContext UpdateResource(Guid id, object model, PatchContext patchContext)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var resourceType = ModelRegistry.GetResourceType(model.GetType());
            var root = ResourceRootSingle.FromResource(patchContext.Resource);
            
            return new RequestContext
            {
                Request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{resourceType}/{id}")
                {
                  Content  = BuildHttpContent(root.ToJson())
                },

                ResourceId = id,
                ResourceType = resourceType,
                Attributes = patchContext.Resource?.Attributes,
                Relationships = patchContext.Resource?.Relationships
            };
        }

        private static HttpContent BuildHttpContent(string content)
        {
            return new StringContent(content, Encoding.UTF8, JsonApiHeader);
        }
    }
}
