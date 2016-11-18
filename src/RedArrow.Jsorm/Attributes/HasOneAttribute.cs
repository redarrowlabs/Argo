using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HasOneAttribute : Attribute
	{
		public HasOneAttribute()
		{
		}

		public HasOneAttribute(string type)
		{
		}
	}
}
