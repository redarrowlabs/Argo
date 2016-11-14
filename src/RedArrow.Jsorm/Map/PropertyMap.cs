using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Jsorm.Extensions;
using RedArrow.Jsorm.Map.MapAttributes;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Map
{
    public abstract class PropertyMap<TModel, TProp> : IPropertyMap
    {
        protected Type PropertyType { get; }
        protected string PropertyName { get; }

        protected string AttributeName { get; }

        protected IDictionary<string, IMapAttribute> MapAttributes { get; }

        protected PropertyMap(Expression<Func<TModel, TProp>> expression, string attrName = null)
        {
            PropertyType = expression.ReturnType;
            PropertyName = expression.PropertyName();

            AttributeName = attrName ?? PropertyName;

            MapAttributes = new Dictionary<string, IMapAttribute>();
        }

        public abstract void Configure(SessionFactory factory);
    }
}