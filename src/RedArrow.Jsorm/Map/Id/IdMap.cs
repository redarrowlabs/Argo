using RedArrow.Jsorm.Map.Id.Generator;
using RedArrow.Jsorm.Session;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RedArrow.Jsorm.Map.Id
{
    public class IdMap<TModel> : PropertyMap<TModel, Guid>, IIdMap<TModel>
        where TModel : class
    {
	    private readonly MethodInfo _setId;
	    private readonly MethodInfo _getId;

        private Func<Guid> _generator;

        public IdMap(Expression<Func<TModel, Guid>> property) : base(property, null)
        {
	        var propInfo = ((property.Body as MemberExpression)?.Member as PropertyInfo);

	        _setId = propInfo?.SetMethod;
	        _getId = propInfo?.GetMethod;
        }

        public override void Configure(ISessionFactory factory)
        {
            factory.RegisterIdAccessor<TModel>(_getId);
			factory.RegisterIdMutator<TModel>(_setId);
            factory.RegisterIdGenerator<TModel>(_generator ?? Guid.NewGuid);
        }

        public IIdMap<TModel> GeneratedBy(Func<Guid> generator)
        {
            _generator = generator;
            return this;
        }
    }
}