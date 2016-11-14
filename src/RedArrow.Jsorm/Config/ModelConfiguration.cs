using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Extensions;

namespace RedArrow.Jsorm.Config
{
    public class ModelConfiguration
    {
        private IList<Assembly> ScanAssemblies { get; }
        private IList<Type> ModelTypes { get; }

        internal ModelConfiguration()
        {
            ScanAssemblies = new List<Assembly>();
            ModelTypes = new List<Type>();
        }

        internal void Configure(SessionConfiguration config)
        {
	        var modelsToConfigure = ModelTypes.Concat(ScanAssemblies.SelectMany(x => x.ExportedTypes))
		        .Where(IsJsormModel)
		        .ToArray();

	        config.IdProperties = modelsToConfigure
		        .ToDictionary(
			        x => x,
			        x => x.GetTypeInfo()
				        .DeclaredProperties.Single(p => p.CustomAttributes
					        .Any(a => a.AttributeType == typeof (IdAttribute))));
        }

	    private static bool IsJsormModel(Type modelType)
	    {
		    var typeInfo = modelType.GetTypeInfo();
		    return typeInfo.IsClass
		           && typeInfo.IsPublic
		           && !typeInfo.IsAbstract
		           && typeInfo.DeclaredProperties
			           .Any(p => p.CustomAttributes.Any(a => a.AttributeType == typeof (IdAttribute)));
	    }

		public ModelConfiguration AddFromAssemblyOf<T>()
        {
            ScanAssemblies.Add(typeof(T).GetTypeInfo().Assembly);
            return this;
        }

        public ModelConfiguration AddFromAssembly(Assembly assembly)
        {
            ScanAssemblies.Add(assembly);
            return this;
        }

	    public ModelConfiguration Add<T>()
	    {
		    return Add(typeof (T));
	    }

	    public ModelConfiguration Add(Type type)
	    {
		    ModelTypes.Add(type);
			return this;
	    }
    }
}