using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Collections;
using RedArrow.Argo.Client.Collections.Generic;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Linq;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Logging;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Model;
using RedArrow.Argo.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Session
{
    public class Session : IModelSession, ISession, ICollectionSession
    {
        private static readonly ILog Log = LogProvider.For<Session>();

        private HttpClient HttpClient { get; }

        private IHttpRequestBuilder HttpRequestBuilder { get; }

        internal ICacheProvider Cache { get; }

        internal IModelRegistry ModelRegistry { get; }

        internal JsonSerializerSettings JsonSettings { get; }

        public bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
            IHttpRequestBuilder httpRequestBuilder,
            ICacheProvider cache,
            IModelRegistry modelRegistry,
            JsonSerializerSettings jsonSettings)
        {
            HttpClient = httpClientFactory();
            HttpRequestBuilder = httpRequestBuilder;
            Cache = cache;

            ModelRegistry = modelRegistry;
            JsonSettings = jsonSettings;
        }

        #region ISession

        public async Task<TModel> Create<TModel>()
        {
            return await Create<TModel>(typeof(TModel), null);
        }

        public async Task<TModel> Create<TModel>(TModel model)
        {
            return await Create<TModel>(typeof(TModel), model);
        }

        private async Task<TModel> Create<TModel>(Type rootModelType, object model)
        {
            ThrowIfDisposed();

            // set Id if unset
            var modelId = model == null
                ? Guid.NewGuid()
                : ModelRegistry.GetOrCreateId(model);
            if (modelId == Guid.Empty)
            {
                modelId = Guid.NewGuid();
                ModelRegistry.SetId(model, modelId);
            }

            // Create a new model instance if not already existing
            if (model == null)
            {
                model = Activator.CreateInstance<TModel>();
                ModelRegistry.SetId(model, modelId);
            }

            if (ModelRegistry.IsManagedModel(model))
            {
                throw new ManagedModelCreationException(model.GetType(), modelId);
            }

            // all unmanaged models in the object graph, including root
            var allModels = ModelRegistry.IncludedModelsCreate(model);
            foreach (var newModel in allModels)
            {
                var newResource = BuildModelResource(newModel);
                // Update the model instance in the argument
                var initialize = newModel.GetType().GetInitializeMethod();
                initialize.Invoke(newModel, new object[] { newResource, this });
            }

            var rootResource = ModelRegistry.GetResource(model);
            var includes = allModels.Where(x => x != model).Select(ModelRegistry.GetResource).ToArray();
            if (Log.IsDebugEnabled())
            {
                Log.Debug(() => $"preparing to POST {rootResource.Type}:{{{rootResource.Id}}}");
                foreach (var include in includes)
                {
                    Log.Debug(() => $"preparing to POST included {include.Type}:{{{include.Id}}}");
                }
            }

            var request = await HttpRequestBuilder.CreateResource(rootResource, includes);
            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            response.CheckStatusCode();
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var root = await response.GetContentModel<ResourceRootSingle>(JsonSettings);
                // Update the model instance in the argument
                var initialize = rootModelType.GetInitializeMethod();
                initialize.Invoke(model, new object[] { root.Data, this });
            }

            // create and cache includes
            await Task.WhenAll(allModels.Select(x => Task.Run(() =>
            {
                var resource = ModelRegistry.GetResource(x);
                Cache.Update(resource.Id, x);
            })));

            return (TModel)model;
        }

        public async Task Update<TModel>(TModel model)
        {
            // guard from kunckleheads
            if (model == null) throw new ArgumentNullException(nameof(model));
            ThrowIfUnmanaged(model);

            var patch = BuildPatch(model);
            // if nothing was updated, no need to continue
            if (patch == null) return;

            var allModels = ModelRegistry.IncludedModelsCreate(model);
            foreach (var newModel in allModels)
            {
                var newResource = BuildModelResource(newModel);
                // Update the model instance in the argument
                var initialize = newModel.GetType().GetInitializeMethod();
                initialize.Invoke(newModel, new object[] { newResource, this });
            }

            var includes = allModels.Select(ModelRegistry.GetResource).ToArray();

            var originalResource = ModelRegistry.GetResource(model);
            var request = await HttpRequestBuilder.UpdateResource(originalResource, patch, includes);
            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            response.CheckStatusCode();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var root = await response.GetContentModel<ResourceRootSingle>(JsonSettings);
                // Update the model instance in the argument
                var initialize = typeof(TModel).GetInitializeMethod();
                initialize.Invoke(model, new object[] { root.Data, this });
                Cache.Update(root.Data.Id, model);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                ModelRegistry.ApplyPatch(model, patch);
            }

            // create and cache includes
            await Task.WhenAll(allModels.Select(x => Task.Run(() =>
            {
                var resource = ModelRegistry.GetResource(x);
                Cache.Update(resource.Id, x);
            })));
        }

        public async Task<TModel> Get<TModel>(Guid id)
        {
            ThrowIfDisposed();

            var model = Cache.Retrieve<TModel>(id);
            if (model != null)
            {
                return model;
            }

            var resourceType = ModelRegistry.GetResourceType<TModel>();
            var include = ModelRegistry.GetInclude<TModel>();
            var request = HttpRequestBuilder.GetResource(id, resourceType, include);
            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(TModel); // null
            }
            response.CheckStatusCode();

            var root = await response.GetContentModel<ResourceRootSingle>(JsonSettings);
            model = CreateResourceModel<TModel>(root.Data);
            Cache.Update(id, model);
            if (root.Included != null)
            {
                await Task.WhenAll(root.Included.Select(x => Task.Run(() =>
                {
                    var includedModel = CreateResourceModel(x);
                    Cache.Update(x.Id, includedModel);
                })));
            }
            return model;
        }

        private object GetSingleRelated(object model, string rltnName)
        {
            var resource = ModelRegistry.GetResource(model);
            var request = HttpRequestBuilder.GetRelated(resource.Id, resource.Type, rltnName);
            var response = HttpClient.SendAsync(request).GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.CheckStatusCode();

            // TODO: perhaps use a 3rd ResourceRoot with JToken Data to determine if array was erroneously returned
            var root = response.GetContentModel<ResourceRootSingle>(JsonSettings).GetAwaiter().GetResult();

            var rltn = root.Data;
            if (rltn == null) return null;

            var rltnModel = CreateResourceModel(rltn);
            Cache.Update(rltn.Id, rltnModel);
            return rltnModel;
        }

        public Task Delete<TModel>(TModel model)
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetId(model);

            return Delete<TModel>(id);
        }

        public async Task Delete<TModel>(Guid id)
        {
            var resourceType = ModelRegistry.GetResourceType<TModel>();
            var request = HttpRequestBuilder.DeleteResource(resourceType, id);
            var response = await HttpClient.SendAsync(request);
            response.CheckStatusCode();
            var model = Cache.Retrieve<TModel>(id);
            if (model != null)
            {
                ModelRegistry.DetachModel(model);
            }
            Cache.Remove(id);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            Disposed = true;
        }

        #endregion ISession

        #region IQuerySession

        public IQueryable<TModel> CreateQuery<TModel>()
        {
            return new TypeQueryable<TModel>(
                this,
                new RemoteQueryProvider(this, JsonSettings));
        }

        public IEnumerable<TRltn> GetRelated<TParent, TRltn>(
            Guid id,
            Expression<Func<TParent, IEnumerable<TRltn>>> relationship)
        {
            var modelType = typeof(TParent);
            var mExpression = relationship.Body as MemberExpression;
            if (mExpression == null) throw new NotSupportedException();
            var attr = mExpression.Member.CustomAttributes
                .SingleOrDefault(a => a.AttributeType == typeof(HasManyAttribute));
            if (attr == null) throw new RelationshipNotRegisteredExecption(mExpression.Member.Name, modelType);

            var resourceType = ModelRegistry.GetResourceType(typeof(TParent));
            var rltnName = mExpression.Member.GetJsonName(typeof(HasManyAttribute));

            var request = HttpRequestBuilder.GetRelated(id, resourceType, rltnName);
            var response = HttpClient.SendAsync(request).GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<TRltn>();
            }

            response.CheckStatusCode();

            var root = response.GetContentModel<ResourceRootCollection>(JsonSettings).GetAwaiter().GetResult();
            var related = root.Data?
                .Select(CreateResourceModel)
                .Cast<TRltn>()
                .ToArray();

            return related;
        }

        public async Task<IEnumerable<TModel>> Query<TModel>(IQueryContext query)
        {
            query = query ?? new QueryContext<TModel>();

            if (query.PageLimit != null && query.PageLimit <= 0) return Enumerable.Empty<TModel>();
            if (query.PageSize != null && query.PageSize <= 0) return Enumerable.Empty<TModel>();

            var include = ModelRegistry.GetInclude<TModel>();

            var request = HttpRequestBuilder.QueryResources(query, include);

            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<TModel>();
            }
            response.CheckStatusCode();

            var root = await response.GetContentModel<ResourceRootCollection>(JsonSettings);

            var createData = Task.WhenAll(root.Data.Select(x => Task.Run(() =>
            {
                var dataModel = CreateResourceModel<TModel>(x);
                Cache.Update(x.Id, dataModel);
                return dataModel;
            })));
            var createIncludes = root.Included != null
                ? Task.WhenAll(root.Included.Select(x => Task.Run(() =>
                {
                    var includedModel = CreateResourceModel(x);
                    Cache.Update(x.Id, includedModel);
                })))
                : Task.CompletedTask;

            await Task.WhenAll(createIncludes, createData);

            return createData.Result;
        }

        #endregion IQuerySession

        #region IModelSession

        public Guid GetId<TModel>(TModel model)
        {
            return ModelRegistry.GetResource(model).Id;
        }

        public TAttr GetAttribute<TModel, TAttr>(TModel model, string attrName)
        {
            ThrowIfDisposed();

            // NOTE: GetAttribute is only used by model ctor. we can safely check the resource and ignore the patch
            return ModelRegistry.GetAttributeValue<TModel, TAttr>(model, attrName);
        }

        public TMeta GetMeta<TModel, TMeta>(TModel model, string metaName)
        {
            ThrowIfDisposed();

            // NOTE: GetMeta is only used by model ctor. we can safely check the resource and ignore the patch
            return ModelRegistry.GetMetaValue<TModel, TMeta>(model, metaName);
        }

        public Guid GetReferenceId<TModel>(TModel model, string attrName)
        {
            var relationships = ModelRegistry.GetResource(model)?.Relationships;

            if (relationships == null) return Guid.Empty;

            return relationships.TryGetValue(attrName, out var rltn)
                ? rltn.Data?.SelectToken("id").ToObject<Guid>() ?? Guid.Empty
                : Guid.Empty;
        }

        public TRltn GetReference<TModel, TRltn>(TModel model, string rltnName)
        {
            ThrowIfDisposed();

            // Assumption here is that GetReference is only used by HasOne to initialize itself
            var resource = ModelRegistry.GetResource(model);
            if (resource == null)
            {
                throw new UnmanagedModelException(model.GetType(), ModelRegistry.GetId(model));
            }
            var relationships = resource.Relationships;
            if (relationships == null || !relationships.TryGetValue(rltnName, out var rltn))
            {
                var related = GetSingleRelated(model, rltnName);
                rltn = new Relationship
                {
                    Data = related != null
                        ? JToken.FromObject(ModelRegistry.GetResource(related).ToResourceIdentifier())
                        : JValue.CreateNull()
                };
            }

            // if we make it to here, 'rltn' has been set
            if (rltn == null)
            {
                throw new ModelMapException("Cannot find HasOne relationship", model.GetType(), ModelRegistry.GetId(model));
            }
            var rltnData = rltn.Data;
            ResourceIdentifier rltnIdentifier;
            if (rltnData == null || rltnData.Type == JTokenType.Null) return default(TRltn);
            if (rltnData.Type == JTokenType.Object)
            {
                // TODO: I don't like that we're performing this conversion for every get
                rltnIdentifier = rltnData.ToObject<ResourceIdentifier>();
            }
            else
            {
                throw new ModelMapException(
                    $"Relationship {rltnName} mapped as [HasOne] but json relationship data was not an object",
                    typeof(TModel),
                    ModelRegistry.GetId(model));
            }
            // calling Get<TRltn>(...) here will check the cache first, then go remote if necessary
            return Get<TRltn>(rltnIdentifier.Id).GetAwaiter().GetResult();
        }

        public IEnumerable<Guid> GetRelationshipIds<TModel>(TModel model, string rltnName)
        {
            var relationships = ModelRegistry.GetResource(model)?.Relationships;

            if (relationships == null) return Array.Empty<Guid>();

            return relationships.TryGetValue(rltnName, out var rltn)
                ? rltn.Data?.SelectTokens("[*].id")
                    .Select(id => id.ToObject<Guid>())
                    .ToArray() ?? Array.Empty<Guid>()
                : Array.Empty<Guid>();
        }

        public IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(TModel model, string rltnName)
        {
            return GetRemoteCollection<TModel, TElmnt>(model, rltnName);
        }

        public ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(TModel model, string rltnName)
        {
            return GetRemoteCollection<TModel, TElmnt>(model, rltnName);
        }

        private RemoteGenericBag<TElmnt> GetRemoteCollection<TModel, TElmnt>(TModel model, string rltnName)
        {
            return new RemoteGenericBag<TElmnt>(this, model, rltnName);
        }

        #endregion IModelSession

        #region ICollectionSession

        public void InitializeCollection(IRemoteCollection collection)
        {
            // TODO: abstract this into a collection loader/initializer
            // TODO: brute force this for now
            // TODO: don't run this task if the resource collection is empty/null!

            var resource = ModelRegistry.GetResource(collection.Owner);

            // determine if the cache is missing any models for this relationship
            Relationship rltn;
            var relationships = ModelRegistry.GetResource(collection.Owner).Relationships;
            if (relationships != null && relationships.TryGetValue(collection.Name, out rltn))
            {
                if (rltn?.Data.Type == JTokenType.Array
                    && rltn.Data.ToObject<IEnumerable<ResourceIdentifier>>()
                        .All(x => Cache.Retrieve<object>(x.Id) != null))
                {
                    return;
                }
            }

            var request = HttpRequestBuilder.GetRelated(resource.Id, resource.Type, collection.Name);
            var response = HttpClient.SendAsync(request).GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound) return;

            response.CheckStatusCode();

            // TODO: perhaps use a 3rd ResourceRoot with JToken Data to determine if object was erroneously returned
            var root = response.GetContentModel<ResourceRootCollection>(JsonSettings).GetAwaiter().GetResult();
            var related = root.Data?.Select(x =>
                {
                    var rltnModel = CreateResourceModel(x);
                    Cache.Update(x.Id, rltnModel);
                    return rltnModel;
                })
                .ToArray();
            collection.SetItems(related);
        }

        #endregion ICollectionSession

        public TModel CreateResourceModel<TModel>(IResourceIdentifier resource)
        {
            return (TModel)CreateResourceModel(resource);
        }

        public object CreateResourceModel(IResourceIdentifier resource)
        {
            if (resource == null) return null;

            var type = ModelRegistry.GetModelType(resource.Type);
            Log.Debug(() => $"activating session-managed instance of {type.FullName}:{{{resource.Id}}}");
            var model = Activator.CreateInstance(type);
            var initialize = type.GetInitializeMethod();
            initialize.Invoke(model, new object[] { resource, this });
            return model;
        }

        public Resource BuildModelResource(object model)
        {
            var resourceType = ModelRegistry.GetResourceType(model.GetType());

            // attributes
            var modelAttributes = ModelRegistry.GetAttributeValues(model);
            foreach (var attr in modelAttributes.Properties().ToArray())
            {
                if (attr.Value.Type == JTokenType.Null)
                {
                    attr.Remove();
                }
            }

            // relationships
            // Note: this process sets unset model Ids in order to create relationships
            var relationships = ModelRegistry.GetRelationshipValues(model);
            foreach (var rtln in relationships.ToList())
            {
                if (rtln.Value.Data.Type == JTokenType.Null)
                {
                    relationships.Remove(rtln);
                }
            }

            var modelMeta = ModelRegistry.GetMetaValues(model);
            foreach (var meta in modelMeta.Properties().ToArray())
            {
                if (meta.Value.Type == JTokenType.Null)
                {
                    meta.Remove();
                }
            }

            return new Resource
            {
                Id = ModelRegistry.GetId(model),
                Type = resourceType,
                Attributes = modelAttributes.HasValues ? modelAttributes : null,
                Relationships = relationships.Any() ? relationships : null,
                Meta = modelMeta.HasValues ? modelMeta : null
            };
        }

        private Resource BuildPatch(object model)
        {
            var originalResource = ModelRegistry.GetResource(model);

            /* Trim any equivalent values from the patch.
             * We're NOT removing/nulling values that exist in the original resource,
             * but not in the new resource.  This trim is coarse-grained to avoid
             * recursion and provide the safest update (in regard to array updates). */

            // if not exists in original and null in model, then remove from patch
            // if original same as model including nulls, then remove from patch
            // if exists in original and null in model, then include in patch
            // if original is different than model, then include in patch
            var modelAttrs = ModelRegistry.GetAttributeValues(model) ?? new JObject();
            var originalAttrs = originalResource.Attributes ?? new JObject();
            foreach (var modelAttr in modelAttrs.Properties().ToArray())
            {
                if (!originalAttrs.TryGetValue(modelAttr.Name, out var ogValue))
                {
                    if (modelAttr.Value.Type == JTokenType.Null)
                    {
                        modelAttr.Remove();
                    }
                }
                else if (JToken.DeepEquals(ogValue, modelAttr.Name))
                {
                    modelAttr.Remove();
                }
            }

            var modelRtlns = ModelRegistry.GetRelationshipValues(model) ?? new Dictionary<string, Relationship>();
            var originalRtlns = originalResource.Relationships ?? new Dictionary<string, Relationship>();
            modelRtlns = modelRtlns.Where(modelRltn =>
            {
                if (!originalRtlns.TryGetValue(modelRltn.Key, out var ogRtln))
                {
                    if (modelRltn.Value == null || modelRltn.Value.Data.Type == JTokenType.Null)
                    {
                        return false;
                    }
                }
                // Using ToString comparison here because the ID is a Guid in the OG and String in New
                else if (JToken.DeepEquals(ogRtln.Data.ToString(), modelRltn.Value.Data.ToString()))
                {
                    return false;
                }
                return true;
            }).ToDictionary(x => x.Key, x => x.Value);

            var modelMetas = ModelRegistry.GetMetaValues(model) ?? new JObject();
            var originalMetas = originalResource.Meta ?? new JObject();
            foreach (var modelMeta in modelMetas.Properties().ToArray())
            {
                if (!originalMetas.TryGetValue(modelMeta.Name, out var ogValue))
                {
                    if (modelMeta.Value.Type == JTokenType.Null)
                    {
                        modelMeta.Remove();
                    }
                }
                else if (JToken.DeepEquals(ogValue, modelMeta.Name))
                {
                    modelMeta.Remove();
                }
            }
            // Links are not patched

            if (!modelAttrs.HasValues
                && !modelRtlns.Any()
                && !modelMetas.HasValues)
            {
                // Nothing to update
                return null;
            }
            return new Resource
            {
                Id = originalResource.Id,
                Type = originalResource.Type,
                Attributes = modelAttrs.HasValues ? modelAttrs : null,
                Relationships = modelRtlns.Any() ? modelRtlns : null,
                Meta = modelMetas.HasValues ? modelMetas : null
            };
        }

        private void ThrowIfDisposed()
        {
            if (Disposed)
            {
                throw new Exception("Session disposed");
            }
        }

        private void ThrowIfUnmanaged(object model)
        {
            if (ModelRegistry.IsUnmanagedModel(model) || !ModelRegistry.IsManagedBy(this, model))
            {
                throw new UnmanagedModelException(model.GetType(), ModelRegistry.GetId(model));
            }
        }
    }
}
