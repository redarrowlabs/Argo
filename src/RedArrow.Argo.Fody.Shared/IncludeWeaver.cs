using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using RedArrow.Argo.Extensions;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private string GetIncludePath(ModelWeavingContext context)
        {
            var modelTypeDef = context.ModelTypeDef;
            var relationships = GetEagerRelationships(context, modelTypeDef, string.Empty, new[] {modelTypeDef});

            return string.Join(",", relationships);
        }

        //TODO: this might be the fugliest algo ever written.  maybe clean this up
        private IEnumerable<string> GetEagerRelationships(
            ModelWeavingContext context,
            TypeDefinition type,
            string path,
            TypeDefinition[] pathTypes)
        {
            var eagerRltns = type.Properties
                .Where(x => x.CustomAttributes
                    .Where(attr =>
                        attr.AttributeType.Resolve() == context.ImportReference(_hasOneAttributeTypeDef).Resolve()
                        || attr.AttributeType.Resolve() == context.ImportReference(_hasManyAttributeTypeDef).Resolve())
                    .Where(attr => attr.HasConstructorArguments)
                    .SelectMany(attr => attr.ConstructorArguments
                        .Where(arg => arg.Type.Resolve() == context.ImportReference(_loadStrategyTypeDef).Resolve()))
                    .Any(arg => (int) arg.Value == 1)) // eager
                .Where(eagerProp =>
                {
                    var eagerPropAttr = eagerProp.CustomAttributes.ContainsAttribute(Constants.Attributes.HasOne)
                        ? Constants.Attributes.HasOne
                        : Constants.Attributes.HasMany;
                    var eagerPropName = eagerProp.JsonApiName(TypeSystem, eagerPropAttr);
                    var eagerPropType = eagerProp.PropertyType.Resolve();
                    var nextPath = string.IsNullOrEmpty(path)
                        ? eagerPropName
                        : $"{path}.{eagerPropName}";
                    var typeVisited = pathTypes.Contains(eagerPropType);
                    if (typeVisited)
                    {
                        LogWarning($"Potential circular reference detected and omitted from eager load: {eagerProp.PropertyType.Resolve().FullName}::{nextPath}");
                    }
                    return !typeVisited;
                });

            if (eagerRltns.Any())
            {
                return eagerRltns.SelectMany(x =>
                    {
                        var eagerPropAttr = x.CustomAttributes.ContainsAttribute(Constants.Attributes.HasOne)
                            ? Constants.Attributes.HasOne
                            : Constants.Attributes.HasMany;
                        var eagerPropName = x.JsonApiName(TypeSystem, eagerPropAttr);
                        var eagerPropType = x.PropertyType.Resolve();
                        var nextPath = string.IsNullOrEmpty(path)
                            ? eagerPropName
                            : $"{path}.{eagerPropName}";
                        return GetEagerRelationships(
                            context,
                            x.PropertyType.Resolve(),
                            nextPath,
                            pathTypes.Concat(new[] {eagerPropType}).ToArray());
                    })
                    .ToArray();
            }

            return new[] {path};
        }
    }
}