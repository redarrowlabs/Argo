using System;

namespace RedArrow.Jsorm.Client.Config
{
    public interface IModelConfigurator : IFluentConfigurator
    {
        IModelConfigurator Configure(Action<ModelScanner> scan);
    }
}