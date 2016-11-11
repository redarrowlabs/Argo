using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Moq;
using RedArrow.Jsorm.Core.Registry;
using RedArrow.Jsorm.Core.Session;
using System;
using System.Linq.Expressions;
using Xunit;

namespace RedArrow.Jsorm.Core.Tests.Session
{
    public class SessionFactoryTests
    {
        [Fact]
        public void Register__Given_IdAccessorFunc__Then_RegisterAccessor()
        {
            var model = new TestModel { Id = Guid.NewGuid() };

            var subject = CreateSubject();

            var idFunc = GetAccessor<TestModel, Guid>(x => x.Id);

            subject.Register<TestModel>(x => idFunc((TestModel)x));

            var id = subject.IdAccessors[model.GetType()](model);

            Assert.Equal(model.Id, id);
        }

        private Func<TModel, TId> GetAccessor<TModel, TId>(Expression<Func<TModel, TId>> expression)
        {
            return expression.Compile();
        }

        private SessionFactory CreateSubject(ICacheProvider cacheProvider = null)
        {
            return new SessionFactory(cacheProvider ?? Mock.Of<ICacheProvider>());
        }

        private class TestModel
        {
            public Guid Id { get; set; }
        }
    }
}