using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Config
{
	public class SessionConfiguration
	{
		private IDictionary<Type, string> TypeLookup { get; }
		private IDictionary<Type, PropertyInfo> IdLookup { get; }
		private ILookup<Type, PropertyConfiguration> AttributeLookup { get; }
		private ILookup<Type, HasOneConfiguration> HasOneLookup { get; }
		private ILookup<Type, HasManyConfiguration> HasManyLookup { get; }

		private ILookup<Type, string> EagerHasOneLookup { get; } 
		private ILookup<Type, string> EagerHasManyLookup { get; } 

		internal SessionConfiguration(
			IDictionary<Type, string> typeLookup,
			IDictionary<Type, PropertyInfo> idLookup,
			ILookup<Type, PropertyConfiguration> attributeLookup,
			ILookup<Type, HasOneConfiguration> hasOneLookup,
			ILookup<Type, HasManyConfiguration> hasManyLookup)
		{
			TypeLookup = typeLookup;
			IdLookup = idLookup;
			AttributeLookup = attributeLookup;
			HasOneLookup = hasOneLookup;
			HasManyLookup = hasManyLookup;

			EagerHasOneLookup = hasOneLookup.SelectMany(group => group
				.Where(x => x.Eager)
				.Select(x => Tuple.Create(group.Key, x.AttributeName)))
				.ToLookup(
					x => x.Item1,
					x => x.Item2);

			EagerHasManyLookup = hasManyLookup.SelectMany(group => group
				.Where(x => x.Eager)
				.Select(x => Tuple.Create(group.Key, "TEST")))
				.ToLookup(
					x => x.Item1,
					x => x.Item2);
		}

		public string GetResourceType<TModel>()
		{
			return GetResourceType(typeof (TModel));
		}

		public string GetResourceType(Type modelType)
		{
			string ret;
			return TypeLookup.TryGetValue(modelType, out ret)
				? ret
				: null;
		}

		public Guid GetId<TModel>(TModel model)
		{
			PropertyInfo idPropInfo;
			if (IdLookup.TryGetValue(typeof(TModel), out idPropInfo))
			{
				return (Guid)idPropInfo.GetValue(model);
			}

			throw new Exception($"{typeof(TModel).FullName} is not configured as a manged model");
		}

		public IEnumerable<PropertyConfiguration> GetAttributes<TModel>()
		{
			return GetAttributes(typeof (TModel));
		} 

		public IEnumerable<PropertyConfiguration> GetAttributes(Type modelType)
		{
			return AttributeLookup.Contains(modelType)
				? AttributeLookup[modelType].ToArray()
				: Enumerable.Empty<PropertyConfiguration>();
		}

		public HttpRequestMessage CreateGetRequest<TModel>(Guid id)
		{
			var modelType = typeof (TModel);
			var resourceType = GetResourceType(modelType);
			var path = $"{resourceType}/{id}";

			var queryParams = new List<string>();

			// includes
			var eagerRltns = new List<string>();
			if (EagerHasOneLookup.Contains(modelType))
			{
				eagerRltns.AddRange(EagerHasOneLookup[modelType]);
			}
			if (EagerHasManyLookup.Contains(modelType))
			{
				eagerRltns.AddRange(EagerHasManyLookup[modelType]);
			}
			if (eagerRltns.Any())
			{
				var csv = string.Join(",", eagerRltns);
				queryParams.Add($"include={csv}");
			}

			if (queryParams.Any())
			{
				path += $"?{string.Join("&", queryParams)}";
			}

			return new HttpRequestMessage(HttpMethod.Get, path);
		}

		public void TypeCheck<T>()
		{
			var type = typeof(T);
            if (!TypeLookup.ContainsKey(type))
			{
				throw new Exception($"{type} is not manged by jsorm");
			}
		}
	}
}
