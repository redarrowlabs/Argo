using RedArrow.Jsorm.Core.Extensions;
using RedArrow.Jsorm.Core.Map.MapAttributes;
using RedArrow.Jsorm.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map
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

        public abstract void Configure(ISessionFactory factory);
    }
}