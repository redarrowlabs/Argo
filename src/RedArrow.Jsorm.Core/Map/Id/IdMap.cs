using RedArrow.Jsorm.Core.Map.Id.Generator;
using RedArrow.Jsorm.Core.Session;
using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map.Id
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

        public Guid GetId(TModel model)
        {
            return _getId(model);
        }
    }
}