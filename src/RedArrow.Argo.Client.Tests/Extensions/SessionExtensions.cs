using System.Linq;
using System.Reflection;

namespace RedArrow.Argo.Client.Tests.Extensions
{
    public static class SessionExtensions
    {
        private static MethodInfo CreateResourceModelMethodInfo { get; } = typeof(Client.Session.Session)
            .GetTypeInfo()
            .DeclaredMethods
            .Where(m => m.IsPrivate)
            .Where(m => m.IsGenericMethod)
            .Single(m => m.Name == "CreateResourceModel");

        private static MethodInfo CreateModelResourceMethodInfo { get; } = typeof(Client.Session.Session)
            .GetTypeInfo()
            .DeclaredMethods
            .Where(m => m.IsPrivate)
            .Single(m => m.Name == "CreateModelResource");

        public static TModel ManageModel<TModel>(this Client.Session.Session session, TModel model)
            where TModel : class
        {
            var resource = CreateModelResourceMethodInfo.Invoke(session, new object[] {model});

            var genericMethod = CreateResourceModelMethodInfo.MakeGenericMethod(typeof(TModel));
            return (TModel)genericMethod.Invoke(session, new [] {resource});
        }
    }
}
