using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Collections;
using RedArrow.Argo.Client.Collections.Generic;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Linq;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Logging;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
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

			IDictionary<Guid, Resource> resourceIndex;

			if (model != null) // map model to resource
			{
				if (ModelRegistry.IsManagedModel(model))
				{
					throw new ManagedModelCreationException(model.GetType(), modelId);
				}

				// all unmanaged models in the object graph, including root
				resourceIndex = ModelRegistry.IncludedModelsCreate(model)
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
			if (Log.IsDebugEnabled())
			{
				Log.Debug(() => $"preparing to POST {rootResource.Type}:{{{rootResource.Id}}}");
				foreach (var include in includes)
				{
					Log.Debug(() => $"preparing to POST included {include.Type}:{{{include.Id}}}");
				}
			}

            var request = HttpRequestBuilder.CreateResource(rootResource, includes);
            var response = await HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var root = await response.GetContentModel<ResourceRootSingle>();
                resourceIndex[modelId] = root.Data;
            }

			// create and cache includes
			await Task.WhenAll(resourceIndex.Values.Select(x => Task.Run(() =>
			{
				var m = CreateResourceModel(x);
				Cache.Update(x.Id, m);
			})));

			return Cache.Retrieve<TModel>(modelId);
		}

		public async Task Update<TModel>(TModel model)
		{
			// guard from kunckleheads
			if (model == null) throw new ArgumentNullException(nameof(model));
			ThrowIfUnmanaged(model);

			var patch = ModelRegistry.GetPatch(model);
			// if nothing was updated, no need to continue
			if (patch == null) return;

			var includes = Cache.Items
				.Where(ModelRegistry.IsUnmanagedModel)
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
			var response = await HttpClient.SendAsync(request);
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return default(TModel); // null
			}
			response.EnsureSuccessStatusCode();

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

			response.EnsureSuccessStatusCode();

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
			var response = await HttpClient.DeleteAsync($"{resourceType}/{id}");
			response.EnsureSuccessStatusCode();
			Detach<TModel>(id);
		}

		public void Detach<TModel>(Guid id)
		{
			var model = Cache.Retrieve<TModel>(id);
			if (model != null)
			{
				Detach(id, model);
			}
		}

		public void Detach<TModel>(TModel model)
		{
			var id = ModelRegistry.GetId(model);
			Detach(id, model);
		}

		private void Detach(Guid id, object model)
		{
			Cache.Remove(id);
			ModelRegistry.DetachModel(model);
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
		
		public IQueryable<TRltn> CreateQuery<TParent, TRltn>(TParent model, Expression<Func<TParent, IEnumerable<TRltn>>> relationship)
		{
			return CreateQuery(ModelRegistry.GetId(model), relationship);
		}
		
		public IQueryable<TRltn> CreateQuery<TParent, TRltn>(Guid id, Expression<Func<TParent, IEnumerable<TRltn>>> relationship)
		{
			var modelType = typeof(TParent);
			var mExpression = relationship.Body as MemberExpression;
			if (mExpression == null) throw new NotSupportedException();
			var attr = mExpression.Member.CustomAttributes
				.SingleOrDefault(a => a.AttributeType == typeof(HasManyAttribute));
			if (attr == null) throw new RelationshipNotRegisteredExecption(mExpression.Member.Name, modelType);
			
			var rltnName = mExpression.Member.GetJsonName(typeof(HasManyAttribute));
			return new RelationshipQueryable<TParent, TRltn>(
				id,
				rltnName,
				this,
				new RemoteQueryProvider(this, JsonSettings));
		}

		public async Task<IEnumerable<TModel>> Query<TModel>(IQueryContext query)
		{
			query = query ?? new QueryContext<TModel>();

			if (query.PageLimit != null && query.PageLimit <= 0) return Enumerable.Empty<TModel>();
			if (query.PageSize != null && query.PageSize <= 0) return Enumerable.Empty<TModel>();
			
			var include = ModelRegistry.GetInclude<TModel>();

			var request = HttpRequestBuilder.QueryResources(query, include);
			
			var response = await HttpClient.SendAsync(request);
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return Enumerable.Empty<TModel>();
			}
			response.EnsureSuccessStatusCode();

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

		#endregion

		#region IModelSession

		public Guid GetId<TModel>(TModel model)
		{
			return ModelRegistry.GetResource(model).Id;
		}

		public TAttr GetAttribute<TModel, TAttr>(TModel model, string attrName)
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
		{
			ThrowIfDisposed();

			ModelRegistry.GetOrCreatePatch(model).SetAttribute(attrName, value);
		}

        public TAttr GetMeta<TModel, TAttr>(TModel model, string metaName)
        {
            ThrowIfDisposed();

            // NOTE: GetMeta is only used by model ctor. we can safely check the resource and ignore the patch
            JToken attr;
            var attributes = ModelRegistry.GetResource(model).Meta;
            if (attributes == null || !attributes.TryGetValue(metaName, out attr))
            {
                // the attrName was not found in the resource
                return default(TAttr);
            }

            // if we make it here, 'attr' has been set
            return attr.ToObject<TAttr>();
        }

        public void SetMeta<TModel, TAttr>(TModel model, string metaName, TAttr value)
        {
            ThrowIfDisposed();

            ModelRegistry.GetOrCreatePatch(model).SetMeta(metaName, value);
        }

        public TRltn GetReference<TModel, TRltn>(TModel model, string rltnName)
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
					var related = GetSingleRelated(model, rltnName);
					rltn = new Relationship
					{
						Data = related != null
							? JToken.FromObject(ModelRegistry.GetResource(related).ToResourceIdentifier())
							: JValue.CreateNull()
					};
					// we're updating the resource, not the patch, since this is not a mutative action
					// updating the resource effectivly updates what the session knows about the data
					// the session won't try to hit the server again, and this change won't be persisted
					var resource = ModelRegistry.GetResource(model);
					resource.GetRelationships()[rltnName] = rltn;
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

		public void SetReference<TModel, TRltn>(TModel model, string rltnName, TRltn rltn)
		{
			ThrowIfDisposed();

			var relationship = new Relationship();
			if (rltn != null)
			{
				var rltnIdentifier = new ResourceIdentifier
				{
					Id = ModelRegistry.GetOrCreateId(rltn),
					Type = ModelRegistry.GetResourceType(rltn.GetType())
				};
				relationship.Data = JToken.FromObject(rltnIdentifier);
				Cache.Update(rltnIdentifier.Id, rltn);
			}
			else
			{
				relationship.Data = JValue.CreateNull();
			}
			ModelRegistry
				.GetOrCreatePatch(model)
				.GetRelationships()[rltnName] = relationship;
		}

		public IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(TModel model, string rltnName)
		{
			return GetRemoteCollection<TModel, TElmnt>(model, rltnName);
		}

		public IEnumerable<TElmnt> SetGenericEnumerable<TModel, TElmnt>(TModel model, string attrName, IEnumerable<TElmnt> value)
		{
			return SetRemoteCollection(model, attrName, value);
		}

		public ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(TModel model, string rltnName)
		{
			return GetRemoteCollection<TModel, TElmnt>(model, rltnName);
		}

		public ICollection<TElmnt> SetGenericCollection<TModel, TElmnt>(TModel model, string attrName, IEnumerable<TElmnt> value)
		{
			return SetRemoteCollection(model, attrName, value);
		}

		private RemoteGenericBag<TElmnt> GetRemoteCollection<TModel, TElmnt>(TModel model, string rltnName)
		{
			var rltnConfig = ModelRegistry.GetHasManyConfig<TModel>(rltnName);

			// TODO: configure collection based on rltnConfig

			return new RemoteGenericBag<TElmnt>(this, model, rltnName);
		}

		private RemoteGenericBag<TElmnt> SetRemoteCollection<TModel, TElmnt>(TModel model, string rltnName, IEnumerable<TElmnt> value)
		{
			var rltnConfig = ModelRegistry.GetHasManyConfig<TModel>(rltnName);

			// TODO: configure collection based on rltnConfig

			return new RemoteGenericBag<TElmnt>(this, model, rltnName, value);
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

			response.EnsureSuccessStatusCode();

			// TODO: perhaps use a 3rd ResourceRoot with JToken Data to determine if object was erroneously returned
			var root = response.GetContentModel<ResourceRootCollection>(JsonSettings).GetAwaiter().GetResult();
			var related = root.Data?.Select(x =>
			{
				var rltnModel = CreateResourceModel(x);
				Cache.Update(x.Id, rltnModel);
				return rltnModel;
			}).ToArray();
			collection.SetItems(related);
		}

		#endregion

		public TModel CreateResourceModel<TModel>(IResourceIdentifier resource)
		{
			return (TModel) CreateResourceModel(resource);
		}

		public object CreateResourceModel(IResourceIdentifier resource)
		{
			if (resource == null) return null;

			var type = ModelRegistry.GetModelType(resource.Type);
			Log.Debug(() => $"activating session-managed instance of {type.FullName}:{{{resource.Id}}}");
			return Activator.CreateInstance(type, resource, this);
		}

		public Resource CreateModelResource(object model)
		{
			var modelType = model.GetType();
			var resourceType = ModelRegistry.GetResourceType(modelType);

			JObject attrs = null;
			IDictionary<string, Relationship> rltns = null;
            IDictionary<string, JToken> meta = null;

            // attributes
            var modelAttributes = ModelRegistry.GetAttributeValues(model);
			if (modelAttributes != null)
			{
				attrs = modelAttributes;
			}

			// relationships
			// Note: this process sets unset model Ids in order to create relationships
			var relationships = ModelRegistry.GetRelationshipValues(model);
			if (!relationships.IsNullOrEmpty())
			{
				rltns = relationships;
			}

            var modelMeta = ModelRegistry.GetMetaValues(model);
            if (!modelMeta.IsNullOrEmpty())
            {
                meta = modelMeta;
            }

			return new Resource
			{
				Id = ModelRegistry.GetId(model),
				Type = resourceType,
				Attributes = attrs,
				Relationships = rltns,
                Meta = meta
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