namespace RedArrow.Jsorm.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();
    }
}