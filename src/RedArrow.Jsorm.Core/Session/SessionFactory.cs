using RedArrow.Jsorm.Core.Registry;
using System;

namespace RedArrow.Jsorm.Core.Session
{
    public class SessionFactory : ISessionFactory
    {
        private IModelRegistry ModelRegistry { get; }

        internal SessionFactory(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
        }

        public void Register(Type modelType)
        {
            ModelRegistry.Register(modelType);
        }
    }
}