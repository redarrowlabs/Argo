using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Config
{
	public interface IModelConfigurator : IFluentConfigurator
	{
		IModelConfigurator Configure(Action<ModelConfiguration> configureModel);
	}
}
