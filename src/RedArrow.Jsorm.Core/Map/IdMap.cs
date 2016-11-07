using System;
using System.Linq.Expressions;
using RedArrow.Jsorm.Core.Map.Id;

namespace RedArrow.Jsorm.Core.Map
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
