namespace RedArrow.Jsorm.Core.Map.HasOne
{
    public interface IHasOneMap<TModel, TProp>
    {
        IHasOneMap<TModel, TProp> Lazy();

        IHasOneMap<TModel, TProp> Eager();

        IHasOneMap<TModel, TProp> CascadeDelete();
    }
}