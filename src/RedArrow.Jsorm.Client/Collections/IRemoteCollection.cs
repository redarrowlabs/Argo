namespace RedArrow.Jsorm.Client.Collections
{
    public interface IRemoteCollection
    {
        bool Dirty { get; }

        object Owner { get; }
        string Name { get; }
    }
}
