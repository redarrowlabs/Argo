using System;
using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static string GetJsonName(this MemberInfo memberInfo, Type attrType)
        {
            var attr = memberInfo
                .CustomAttributes
                .Single(x => x.AttributeType == attrType);

            return attr.GetJsonName() ?? memberInfo.Name.Camelize();
        }

        public static string GetJsonName(this CustomAttributeData attr)
        {
            if (attr.AttributeType == typeof(IdAttribute)) return "id";

            return attr.ConstructorArguments
                .Where(x => x.ArgumentType == typeof(string))
                .Select(x => (string) x.Value)
                .FirstOrDefault();
        }
    }
}