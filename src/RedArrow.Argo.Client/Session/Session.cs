using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedArrow.Argo.Client.Collections;
using RedArrow.Argo.Client.Collections.Generic;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Logging;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Model;

namespace RedArrow.Argo.Client.Session
{
    public class Session : IModelSession, ISession, ICollectionSession
    {
        private static readonly ILog Log = LogProvider.For<Session>();

        private HttpClient HttpClient { get; }

        private IHttpRequestBuilder HttpRequestBuilder { get; }

        internal ICacheProvider Cache { get; }

        internal IModelRegistry ModelRegistry { get; }

        public bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
            IHttpRequestBuilder httpRequestBuilder,
            ICacheProvider cache,
            IModelRegistry modelRegistry)
        {
            HttpClient = httpClientFactory();
            HttpRequestBuilder = httpRequestBuilder;
            Cache = cache;

            ModelRegistry = modelRegistry;
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            Disposed = true;
        }

        public async Task<TModel> Create<TModel>()
            where TModel : class
        {
            return await Create<TModel>(typeof(TModel), null);
        }

        public async Task<TModel> Create<TModel>(TModel model)
            where TModel : class
        {
            return await Create<TModel>(typeof(TModel), model);
        }

        private async Task<TModel> Create<TModel>(Type rootModelType, object model)
			where TModel : class
        {
            ThrowIfDisposed();
			
			// set Id if unset
	        var modelId = model == null
				? Guid.NewGuid()
				: ModelRegistry.GetId(model);
	        if (modelId == Guid.Empty)
	        {
				modelId = Guid.NewGuid();
				ModelRegistry.SetId(model, modelId);
	        }

	        IDictionary<Guid, Resource> resourceIndex;

	        if (model != null) // map model to resource
	        {
		        if (ModelRegistry.IsManagedModel(model))
		        {
					throw new ManagedModelCreationException(ModelRegistry.GetId(model), model.GetType());
		        }
		     
				// all unmanaged models in the object graph, including root
		        resourceIndex = ModelRegistry.GetIncludedModels(model)
			        .Select(CreateModelResource)
			        .ToDictionary(x => x.Id);
	        }
	        else // model is null - all we know is resource type
	        {
		        resourceIndex = new Dictionary<Guid, Resource>
		        {
			        {
				        modelId, new Resource
				        {
					        Id = modelId,
					        Type = ModelRegistry.GetResourceType(rootModelType)
				        }
			        }
		        };
	        }

	        var rootResource = resourceIndex[modelId];
	        var includes = resourceIndex.Values.Where(x => x.Id != modelId).ToArray();

            var request = HttpRequestBuilder.CreateResource(rootResource, includes);

            if (Log.IsDebugEnabled())
            {
                Log.Debug(() => $"preparing to POST {rootResource.Type}:{{{rootResource.Id}}}");
                foreach (var include in includes)
                {
                    Log.Debug(() => $"preparing to POST included {include.Type}:{{{include.Id}}}");
                }
            }

            var response = await HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			// create and cache includes
            await Task.WhenAll(resourceIndex.Values.Select(x => Task.Run(() => 
			{
                var m = CreateResourceModel(x);
                Cache.Update(x.Id, m);
            })));
	        return Cache.Retrieve<TModel>(modelId);
		}

		public async Task Update<TModel>(TModel model)
			where TModel : class
		{
			// guard from kunckleheads
			if(model == null) throw new ArgumentNullException(nameof(model));
			ThrowIfUnmanaged(model);

			var patch = ModelRegistry.GetPatch(model);
			// if nothing was updated, no need to continue
			if (patch == null) return;

			var includes = ModelRegistry.GetIncludedModels(model)
				.Select(CreateModelResource)
				.ToArray();
			var request = HttpRequestBuilder.UpdateResource(patch, includes);
			var response = await HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			// create and cache includes
			await Task.WhenAll(includes.Select(x => Task.Run(() =>
			{
				var m = CreateResourceModel(x);
				Cache.Update(x.Id, m);
			})));

			ModelRegistry.ApplyPatch(model);
		}

		public async Task<TModel> Get<TModel>(Guid id)
            where TModel : class
        {
            ThrowIfDisposed();

            var model = Cache.Retrieve<TModel>(id);
            if (model != null)
            {
                return model;
            }

            var resourceType = ModelRegistry.GetResourceType<TModel>();
            var request = HttpRequestBuilder.GetResource(id, resourceType, Enumerable.Empty<string>());
            var response = await HttpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(TModel); // null
            }
            response.EnsureSuccessStatusCode();

			// deserializing from stream is more performant than string
			// http://www.newtonsoft.com/json/help/html/Performance.htm
			ResourceRootSingle root;
			using(var stream = await response.Content.ReadAsStreamAsync())
			using (var sr = new StreamReader(stream))
			using(var reader = new JsonTextReader(sr))
			{
				root = new JsonSerializer().Deserialize<ResourceRootSingle>(reader);
			}
            model = CreateResourceModel<TModel>(root.Data);
            Cache.Update(id, model);

            return model;
        }
		
	    private async Task<IEnumerable<TRltn>> GetRelated<TModel, TRltn>(TModel model, string rltnName)
			where TRltn : class
	    {
		    ThrowIfDisposed();

			var resource = ModelRegistry.GetResource(model);
		    var request = HttpRequestBuilder.GetRelated(resource.Id, resource.Type, rltnName);
		    var response = await HttpClient.SendAsync(request);
		    if (response.StatusCode == HttpStatusCode.NotFound)
		    {
				return null; // null
			}
			response.EnsureSuccessStatusCode();
			
			// deserializing from stream is more performant than string
			// http://www.newtonsoft.com/json/help/html/Performance.htm
			ResourceRootCollection root;
			using (var stream = await response.Content.ReadAsStreamAsync())
			using (var sr = new StreamReader(stream))
			using (var reader = new JsonTextReader(sr))
			{
				root = new JsonSerializer().Deserialize<ResourceRootCollection>(reader);
			}

		    return root.Data?.Select(rltn =>
		    {
			    // use the actual resource type, not typeof(TRltn)
			    // TRltn may be a base class or interface!
			    var rltnModel = CreateResourceModel<TRltn>(rltn);
			    Cache.Update(rltn.Id, rltnModel);
			    return rltnModel;
		    }).ToArray();
		}

        public Task Delete<TModel>(TModel model)
            where TModel : class
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetId(model);
            return Delete<TModel>(id);
        }

        public async Task Delete<TModel>(Guid id)
            where TModel : class
        {
            var resourceType = ModelRegistry.GetResourceType<TModel>();
            var response = await HttpClient.DeleteAsync($"{resourceType}/{id}");
			response.EnsureSuccessStatusCode();

	        var model = Cache.Retrieve<TModel>(id);
	        if (model != null)
			{
				Cache.Remove(id);
				ModelRegistry.DetachModel(model);
			}
        }

        #region IModelSession

        public Guid GetId<TModel>(TModel model)
        {
            return ModelRegistry.GetResource(model).Id;
        }
        
        public TAttr GetAttribute<TModel, TAttr>(TModel model, string attrName)
            where TModel : class
        {
            ThrowIfDisposed();
			
			// NOTE: GetAttribute is only used by model ctor. we can safely check the resource and ignore the patch
			JToken attr;
	        var attributes = ModelRegistry.GetResource(model).Attributes;
			if (attributes == null || !attributes.TryGetValue(attrName, out attr))
			{
				// the attrName was not found in the resource
				return default(TAttr);
			}

			// if we make it here, 'attr' has been set
	        return attr.ToObject<TAttr>();
        }

		public void SetAttribute<TModel, TAttr>(TModel model, string attrName, TAttr value)
            where TModel : class
        {
			ThrowIfDisposed();

			ModelRegistry.GetOrCreatePatch(model).SetAttribute(attrName, value);
        }

        public TRltn GetReference<TModel, TRltn>(TModel model, string rltnName)
            where TModel : class
            where TRltn : class
        {
            ThrowIfDisposed();
			
			// check patch first, fall back on resource if rltnName not found
	        Relationship rltn;
			var relationships = ModelRegistry.GetPatch(model)?.Relationships;
	        if (relationships == null || !relationships.TryGetValue(rltnName, out rltn))
	        {
		        var modelResource = ModelRegistry.GetResource(model);
		        relationships = modelResource.Relationships;
		        if (relationships == null || !relationships.TryGetValue(rltnName, out rltn))
		        {
			        // the rltnName defined in model was not found in the patch or resource
			        // TODO? we're doing FirstOrDefault instead of SingleOrDefault in case of improperly structured data
					// TODO? i.e. relationship data containing a collection for this to-one relationship
			        // TODO? throw exception?  log warning? ¯\_(ツ)_/¯ 
			        var related = GetRelated<TModel, TRltn>(model, rltnName).Result?.FirstOrDefault();
			        var relatedResource = ModelRegistry.GetResource(related);
					var patch = ModelRegistry.GetOrCreatePatch(model);
					patch.GetRelationships()[rltnName] = new Relationship
			        {
				        Data = JObject.FromObject(relatedResource.ToResourceIdentifier())
			        };
			        return related;
		        }
	        }

	        // if we make it to here, 'rltn' has been set
	        var rltnData = rltn.Data;
	        ResourceIdentifier rltnIdentifier;
	        if (rltnData == null || rltnData.Type == JTokenType.Null) return default(TRltn);
	        if (rltnData.Type == JTokenType.Object)
			{
				// TODO: I don't like that we're performing this conversion for every get
				rltnIdentifier = rltnData.ToObject<ResourceIdentifier>();
			}
			else if (rltnData.Type == JTokenType.Array)
			{
				Log.Warn(() =>
				{
					var modelId = ModelRegistry.GetId(model);
					return $"{model.GetType()}:{{{modelId}}} {rltnName} is modeled as [HasOne], but json contains multiple items for this relationship.  The first item will arbitrarily be used.";
				});
				// TODO: I don't like that we're performing this conversion for every get
				// TODO: consider modeling RelationshipSingle and RelationshipCollection with relationship.IsSingular / relationship.IsCollection
				rltnIdentifier = rltnData.ToObject<IEnumerable<ResourceIdentifier>>().FirstOrDefault();
			}
	        else
			{
				Log.Error(() =>
				{
					var modelId = ModelRegistry.GetId(model);
					return $"{model.GetType()}:{{{modelId}}} {rltnName} data appears to be corrupt.  It is not an expected json token type: {rltnData.Type}";
				});
				return default(TRltn);
			}
			// calling Get<TRltn>(...) here will check the cache first, then go remote if necessary
	        return Get<TRltn>(rltnIdentifier.Id).Result;
        }

        public void SetReference<TModel, TRltn>(TModel model, string rltnName, TRltn rltn)
            where TModel : class
            where TRltn : class
        {
            ThrowIfUnmanaged(model);

	        var patch = ModelRegistry.GetOrCreatePatch(model);
	        var relationship = new Relationship();
	        if (rltn != null)
	        {
		        var rltnType = ModelRegistry.GetResourceType<TRltn>();
		        var rltnId = ModelRegistry.GetId(rltn);
		        if (rltnId == Guid.Empty)
		        {
			        rltnId = Guid.NewGuid();
					ModelRegistry.SetId(rltn, rltnId);
		        }
		        relationship.Data = JObject.FromObject(new ResourceIdentifier {Id = rltnId, Type = rltnType});
                Cache.Update(rltnId, rltn);
	        }
	        else
	        {
		        relationship.Data = JValue.CreateNull();
	        }
	        patch.GetRelationships()[rltnName] = relationship;
        }

        public void InitializeCollection(IRemoteCollection collection)
        {
            // TODO: determine if {id/type} from owner relationship are cached already
            // TODO: abstract this into a collection loader/initializer

            // TODO: brute force this for now

            // TODO: don't run this task if the resource collection is empty/null!

            //Task.Run(async () =>
            //{
            //    var requestContext = HttpRequestBuilder.GetRelated(collection.Owner, collection.Name);
            //    var response = await HttpClient.SendAsync(requestContext.Request);
            //    if (response.StatusCode == HttpStatusCode.NotFound)
            //    {
            //        return;
            //    }
            //    response.EnsureSuccessStatusCode();

            //    var contentJson = await response.Content.ReadAsStringAsync();
            //    var root = JsonConvert.DeserializeObject<ResourceRootCollection>(contentJson);

            //    if (root.Data == null)
            //    {
            //        return;
            //    }

            //    var items = root.Data.Select(x =>
            //    {
            //        ResourceState[x.Id] = x;
            //        var modelType = ModelRegistry.GetModelType(x.Type);
            //        if (modelType == null)
            //        {
            //            // TODO: ModelNotRegisteredException
            //            throw new Exception("TODO");
            //        }
            //        var model = CreateModel(modelType, x.Id);
            //        Cache.Update(x.Id, model);
            //        return model;
            //    });
            //    collection.SetItems(items);
            //}).Wait();
        }

        public IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(TModel model, string rltnName)
            where TModel : class
            where TElmnt : class
        {
            return GetRemoteCollection<TModel, TElmnt>(model, rltnName);
        }

        public IEnumerable<TElmnt> SetGenericEnumerable<TModel, TElmnt>(TModel model, string attrName,
            IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class
        {
            return SetRemoteCollection(model, attrName, value);
        }

        public ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(TModel model, string rltnName)
            where TModel : class
            where TElmnt : class
        {
            return GetRemoteCollection<TModel, TElmnt>(model, rltnName);
        }

        public ICollection<TElmnt> SetGenericCollection<TModel, TElmnt>(TModel model, string attrName,
            IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class
        {
            return SetRemoteCollection(model, attrName, value);
        }

        private RemoteGenericBag<TElmnt> GetRemoteCollection<TModel, TElmnt>(TModel model, string rltnName)
            where TModel : class
            where TElmnt : class
        {
            var rltnConfig = ModelRegistry.GetHasManyConfig<TModel>(rltnName);

            // TODO: configure collection based on rltnConfig

            return new RemoteGenericBag<TElmnt>(this)
            {
                Name = rltnName,
                Owner = model
            };
        }

        private RemoteGenericBag<TElmnt> SetRemoteCollection<TModel, TElmnt>(TModel model, string rltnName,
            IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class
        {
            var rltnConfig = ModelRegistry.GetHasManyConfig<TModel>(rltnName);

            // TODO: configure collection based on rltnConfig

            return new RemoteGenericBag<TElmnt>(this, value)
            {
                Name = rltnName,
                Owner = model
            };
        }

        #endregion IModelSession

	    private TModel CreateResourceModel<TModel>(IResourceIdentifier resource)
		    where TModel : class
	    {
			// TODO: check if model.GetType() is assignable to TModel, but then what? ¯\_(ツ)_/¯
			return (TModel)CreateResourceModel(resource);
	    }

        private object CreateResourceModel(IResourceIdentifier resource)
        {
	        var type = ModelRegistry.GetModelType(resource.Type);
            Log.Debug(() => $"activating new session-managed instance of {type}:{{{resource.Id}}}");
            return Activator.CreateInstance(type, resource, this);
        }

	    private Resource CreateModelResource(object model)
	    {
			var modelType = model.GetType();
			var resourceType = ModelRegistry.GetResourceType(modelType);

			JObject attrs = null;
			IDictionary<string, Relationship> rltns = null;

			// attribute bag
			var modelAttributeBag = ModelRegistry.GetAttributeBag(model);
			if (modelAttributeBag != null)
			{
				attrs = modelAttributeBag;
			}

			// attributes
			var modelAttributes = ModelRegistry.GetAttributeValues(model);
			if (modelAttributes != null)
			{
				if (attrs == null)
				{
					attrs = modelAttributes;
				}
				else // occurs when we already set from AttrBag
				{
					// mapped attrs override anything also in the AttrBag
					attrs.Merge(modelAttributes, new JsonMergeSettings
					{
						MergeNullValueHandling = MergeNullValueHandling.Ignore,
						MergeArrayHandling = MergeArrayHandling.Replace
					});
				}
			}

			// relationships
			// Note: this process sets unset model Ids in order to create relationships
			var relationships = ModelRegistry.GetRelationshipValues(model);
			if (!relationships.IsNullOrEmpty())
			{
				rltns = relationships;
			}

			return new Resource
			{
				Id = ModelRegistry.GetId(model),
				Type = resourceType,
				Attributes = attrs,
				Relationships = rltns
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
			    throw new UnmanagedModelException(ModelRegistry.GetId(model), model.GetType());
			}
		}
    }
}