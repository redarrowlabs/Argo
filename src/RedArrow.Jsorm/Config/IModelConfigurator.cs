using System;

namespace RedArrow.Jsorm.Config
{
    public interface IModelConfigurator : IFluentConfigurator
    {
        IModelConfigurator Configure(Action<ModelLocator> configureModel);
    }
}