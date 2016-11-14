using System;
using System.Linq.Expressions;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Map.Attribute
{
    public class AttributeMap<TModel, TProp> : PropertyMap<TModel, TProp>
    {
        public AttributeMap(Expression<Func<TModel, TProp>> property, string attrName = null) : base(property, attrName)
        {
        }

        public override void Configure(SessionFactory factory)
        {
            throw new NotImplementedException();
        }
    }
}