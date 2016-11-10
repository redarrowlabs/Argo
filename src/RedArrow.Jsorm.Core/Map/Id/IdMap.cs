using RedArrow.Jsorm.Core.Session;
using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map.Id
{
    public class IdMap<TModel, TId> : PropertyMap<TModel, TId>, IIdMap
        where TModel : new()
    {
        private readonly Func<TModel, TId> _getId;

        private IIdentifierGenerator _generator;

        public IdMap(Expression<Func<TModel, TId>> property) : base(property, null)
        {
            _getId = property.Compile();
        }

        public override void Configure(ISessionFactory factory)
        {
            throw new NotImplementedException();
        }

        public TId GetId(TModel model)
        {
            return _getId(model);
        }

        public IIdMap GeneratedBy(IIdentifierGenerator generator)
        {
            _generator = generator;
            return this;
        }
    }
}