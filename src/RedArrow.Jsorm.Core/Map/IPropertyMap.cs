using System;

namespace RedArrow.Jsorm.Core.Map
{
	public interface IPropertyMap
	{
		Type PropertyType { get; }
		string PropertyName { get; }

		string AttributeName { get; }
	}
}
