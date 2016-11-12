using RedArrow.Jsorm.Map.Id.Generator;
using RedArrow.Jsorm.Session;
using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Map.Id
{
    public class IdMap<TModel> : PropertyMap<TModel, Guid>, IIdMap<TModel>
        where TModel : class
    {
        private readonly Func<TModel, Guid> _getId;

        private IIdentifierGenerator _generator;

        public IdMap(Expression<Func<TModel, Guid>> property) : base(property, null)
        {
            _getId = property.Compile();
        }

        public override void Configure(ISessionFactory factory)
        {
            factory.Register<TModel>(x => _getId((TModel)x));
            factory.Register<TModel>(_generator ?? GuidCombGenerator.Instance);
        }

        public IIdMap<TModel> GeneratedBy(IIdentifierGenerator generator)
        {
            _generator = generator;
            return this;
        }
    }
}