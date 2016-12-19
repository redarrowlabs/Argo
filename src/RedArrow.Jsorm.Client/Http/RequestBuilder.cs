using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using RedArrow.Jsorm.Client.JsonModels;
using RedArrow.Jsorm.Client.Session.Patch;
using RedArrow.Jsorm.Client.Session.Registry;

namespace RedArrow.Jsorm.Client.Http
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
