using System.Collections.Generic;

namespace RedArrow.Jsorm.Core.Map
{
	public interface IResourceMap
	{
		IIdMap IdMap { get; }

		IDictionary<string, IAttributeMap> AttributeMaps { get; }
		
		IDictionary<string, IHasOneMap> HasOneMaps { get; }

		IDictionary<string, IHasManyMap> HasManyMaps { get; }
	}
}
