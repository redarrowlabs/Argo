namespace RedArrow.Argo.Client.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();
    }
}