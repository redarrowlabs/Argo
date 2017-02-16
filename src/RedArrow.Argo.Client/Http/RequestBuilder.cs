using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Services;
using RedArrow.Argo.Client.Services.Includes;
using RedArrow.Argo.Client.Services.Relationships;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private const string JsonApiHeader = "application/vnd.api+json";

        private IModelRegistry ModelRegistry { get; }

        private IIncludeResourcesService IncludeResources { get; }
        private IRelateResources RelateResources { get; }

        internal HttpRequestBuilder(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
            IncludeResources = new IncludeResourcesService(ModelRegistry);
            RelateResources = new RelateResources(ModelRegistry);
        }

        public RequestContext GetResource(Guid id, Type modelType)
        {
            var resourceType = ModelRegistry.GetResourceType(modelType);


            // TODO: easiest approach, replace when needed.
            var sparseFieldsets = new List<string>();
            sparseFieldsets.AddRange(ModelRegistry.GetModelAttributes(modelType).Select(x => x.AttributeName));
            sparseFieldsets.AddRange(ModelRegistry.GetCollectionConfigurations(modelType).Select(x => x.AttributeName));
            sparseFieldsets.AddRange(ModelRegistry.GetSingleConfigurations(modelType).Select(x => x.AttributeName));

            var url = $"{resourceType}/{id}"
                .SetQueryParam($"fields[{resourceType}]", string.Join(",", sparseFieldsets));

            return new RequestContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, url),
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
            return Task.Run(async () =>
            {
                var resourceType = ModelRegistry.GetResourceType(modelType);
                var attributes = model != null
                    ? JObject.FromObject(ModelRegistry
                        .GetModelAttributes(modelType)
                        .ToDictionary(
                            x => x.AttributeName,
                            x => x.Property.GetValue(model)))
                    : null;

                var included = await IncludeResources.Process(modelType, model);
                var relationships = RelateResources.Process(modelType, model);

                var root = ResourceRootCreate.FromObject(resourceType, attributes, included.SelectMany(x => x.Value).ToList(), relationships);

                return new RequestContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Post, resourceType)
                    {
                        Content = BuildHttpContent(root.ToJson()),
                    },
                    ResourceType = resourceType,
                    Attributes = attributes,
                    Included = included.SelectMany(x => x.Value).ToList(),
                    Relationships = relationships
                };
            }).Result;

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
                    Content = BuildHttpContent(root.ToJson())
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
