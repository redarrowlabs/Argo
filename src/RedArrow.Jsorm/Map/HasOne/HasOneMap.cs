using System;
using System.Linq.Expressions;
using RedArrow.Jsorm.Map.MapAttributes;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Map.HasOne
{
    public class HasOneMap<TModel, TProp> :
        PropertyMap<TModel, TProp>,
        IHasOneMap<TModel, TProp>
    {
        public HasOneMap(Expression<Func<TModel, TProp>> property, string attrName = null) :
            base(property, attrName)
        {
        }

        public override void Configure(SessionFactory factory)
        {
            throw new NotImplementedException();
        }

        public IHasOneMap<TModel, TProp> Lazy()
        {
            MapAttributes[nameof(FetchStrategyAttribute)] = FetchStrategyAttribute.Lazy;
            return this;
        }

        public IHasOneMap<TModel, TProp> Eager()
        {
            MapAttributes[nameof(FetchStrategyAttribute)] = FetchStrategyAttribute.Eager;
            return this;
        }

        public IHasOneMap<TModel, TProp> CascadeDelete()
        {
            throw new NotImplementedException();
        }
    }
}