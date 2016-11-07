using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Core.Map.Attributes
{
	[Flags]
	public enum FilterOp
	{
		/// <summary>
		/// Equal
		/// </summary>
		Eq,
		/// <summary>
		/// Not Equal
		/// </summary>
		Ne,
		/// <summary>
		/// Greater Than
		/// </summary>
		Gt,
		/// <summary>
		/// Less Than
		/// </summary>
		Lt
	}

	public class FilterAttribute<TModel> : IMapAttribute
		where TModel : new()
	{
	}
}
