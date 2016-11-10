using RedArrow.Jsorm.Core.Extensions;
using RedArrow.Jsorm.Core.Infrastructure;
using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Core.Registry
{
    internal class DefaultModelRegistry : IModelRegistry
    {
        private ISet<Type> Container { get; }

        internal DefaultModelRegistry()
        {
            Container = new HashSet<Type>();
        }

        public void Register(Type type)
        {
            //TODO: verify that type has a resource mapping?
            Container.Add(type);
        }

        public T Resolve<T>()
        {
            var type = typeof(T);
            if (!Container.Contains(type))
            {
                throw new ModelNotRegisteredException(type);
            }

            var ctor = type.GetDefaultConstructor();
            var model = ctor.Invoke(null);
            return (T)model;
        }
    }
}