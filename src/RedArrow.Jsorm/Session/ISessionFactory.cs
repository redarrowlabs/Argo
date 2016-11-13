using System;
using System.Reflection;

namespace RedArrow.Jsorm.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();

        void Register(Type modelType);

        void RegisterIdAccessor<TModel>(MethodInfo getId);

		void RegisterIdMutator<TModel>(MethodInfo setId);

		void RegisterIdGenerator<TModel>(Func<Guid> generator);
    }
}