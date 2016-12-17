using System;

namespace RedArrow.Argo.Client.Config
{
    public interface IModelConfigurator : IFluentConfigurator
    {
        IModelConfigurator Configure(Action<ModelScanner> scan);
    }
}