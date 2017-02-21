using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Services.Includes;
using RedArrow.Argo.Client.Services.Relationships;
using RedArrow.Argo.Client.Services.SparseFieldsets;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private const string JsonApiHeader = "application/vnd.api+json";

        private IModelRegistry ModelRegistry { get; }

        private ISparseFieldsetService SparseFieldsetService { get; }
        private IIncludeResourcesService IncludeResources { get; }
        private IRelateResources RelateResources { get; }

        internal HttpRequestBuilder(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
            SparseFieldsetService = new SparseFieldsetService(ModelRegistry);
            IncludeResources = new IncludeResourcesService(ModelRegistry);
            RelateResources = new RelateResources(ModelRegistry);
        }

        public RequestContext GetResource(Guid id, Type modelType)
        {
            var resourceType = ModelRegistry.GetResourceType(modelType);
            var url = $"{resourceType}/{id}";

            url = SparseFieldsetService.BuildSparseFieldsetUrl(modelType, url).Result;
            url = IncludeResources.BuildIncludesUrl(modelType, url).Result;

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

        public RequestContext CreateResource(Type modelType, object model, IDictionary<Guid, Resource> resourceState)
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

                var included = model != null ? await IncludeResources.Process(modelType, model, resourceState) : new List<Resource>();
                var relationships = model != null ? RelateResources.Process(modelType, model) : new Dictionary<string, Relationship>();

                var root = ResourceRootCreate.FromObject(resourceType, attributes, included, relationships);

                return new RequestContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Post, resourceType)
                    {
                        Content = BuildHttpContent(root.ToJson()),
                    },
                    ResourceType = resourceType,
                    Attributes = attributes,
                    Included = included,
                    Relationships = relationships
                };
            }).Result;

        }

        public RequestContext UpdateResource(Guid id, object model, PatchContext patchContext, IDictionary<Guid, Resource> resourceState)
        {
            return Task.Run(async () =>
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                var resourceType = ModelRegistry.GetResourceType(model.GetType());

                var included = model != null ? await IncludeResources.Process(model.GetType(), model, resourceState) : new List<Resource>();
                var relationships = model != null ? RelateResources.Process(model.GetType(), model) : new Dictionary<string, Relationship>();

                patchContext.Resource.Relationships = relationships;
                var root = ResourceRootSingle.FromResource(patchContext.Resource, included);

                return new RequestContext
                {
                    Request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{resourceType}/{id}")
                    {
                        Content = BuildHttpContent(root.ToJson())
                    },

                    ResourceId = id,
                    ResourceType = resourceType,
                    Attributes = patchContext.Resource?.Attributes,
                    Relationships = patchContext.Resource?.Relationships,
                    Included = included
                };
            }).Result;
        }

        private static HttpContent BuildHttpContent(string content)
        {
            return new StringContent(content, Encoding.UTF8, JsonApiHeader);
        }
    }
}
