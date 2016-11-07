using System;
using RedArrow.Jsorm.Core.Map.Id;

namespace RedArrow.Jsorm.Core.Map
{
	public interface IIdMap : IPropertyMap
	{
		IIdMap GeneratedBy(IIdentifierGenerator generator);
	}
}
