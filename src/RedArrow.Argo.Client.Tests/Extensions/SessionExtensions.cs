namespace RedArrow.Argo.Client.Tests.Extensions
{
    public static class SessionExtensions
    {
        public static TModel ManageModel<TModel>(this Client.Session.Session session, TModel model)
            where TModel : class
        {
            var resource = session.BuildModelResource(model);
            return session.CreateResourceModel<TModel>(resource);
        }
    }
}