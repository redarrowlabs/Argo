using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Argo.Extensions;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddCtor(ModelWeavingContext context)
        {
            // Ctor(Guid id, IResourceIdentifier resource, IModelSession session)
            // {
            //   Id = id;
			//   __argo__generated_Resource = resource;
            //   __argo__generated_session = session;
            //  }
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                TypeSystem.Void);

            ctor.Parameters.Add(
				new ParameterDefinition(
					"resource",
					ParameterAttributes.None,
					context.ImportReference(_resourceIdentifierTypeDef)));
            ctor.Parameters.Add(
                new ParameterDefinition(
                    "session",
                    ParameterAttributes.None,
                    context.ImportReference(_sessionTypeDef)));

            var objectCtor = context.ImportReference(TypeSystem.Object.Resolve().GetConstructors().First());

			var idBackingField = context
				.IdPropDef
				?.GetMethod
				?.Body
				?.Instructions
				?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
				?.Operand as FieldReference;
			// supply generic type arguments to template
			var sessionGetId = _session_GetId.MakeGenericMethod(context.ModelTypeRef);

            var proc = ctor.Body.GetILProcessor();

            // public Patient(IResourceIdentifier resource, IModelSession session)
            // {
            //   this.__argo__generated_Resource = resource;
            //   this.__argo__generated_session = session;
            //   this.__argo__generated_includePath = "model.include.path";
			//   this.Id = __argo__generated_session.GetId<TModel>();
            // }
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Call, objectCtor); // call base ctor on 'this'

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load arg 'resource' onto stack
            proc.Emit(OpCodes.Callvirt, context.ResourcePropDef.SetMethod);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_2); // load arg 'session' onto stack
            proc.Emit(OpCodes.Stfld, context.SessionField); // this.__argo__generated_session = session;
            
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldstr, GetIncludePath(context));
            proc.Emit(OpCodes.Stfld, context.IncludePathField); // this.__argo__generated_includePath = this.__argo__generated_session.GetIncludePath<TModel>()

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load this.__argo__generated_session
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionGetId));
            proc.Emit(OpCodes.Stfld, idBackingField); // this.<Id>K_backingField = this.__argo__generated_session.GetId<TModel>();

            // this._attrBackingField = this.__argo__generated_session.GetAttribute
            WeaveAttributeFieldInitializers(context, proc, context.MappedAttributes);

            proc.Emit(OpCodes.Ret); // return

            context.Methods.Add(ctor);
        }

        private void WeaveAttributeFieldInitializers(ModelWeavingContext context, ILProcessor proc, IEnumerable<PropertyDefinition> attrPropDefs)
        {
            foreach (var attrPropDef in attrPropDefs)
            {
                // supply generic type arguments to template
                var sessionGetAttr = _session_GetAttribute.MakeGenericMethod(context.ModelTypeRef, attrPropDef.PropertyType);

                var backingField = attrPropDef.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {attrPropDef?.FullName}");
                }

                var propAttr = attrPropDef.CustomAttributes.GetAttribute(Constants.Attributes.Property);
                var attrName = propAttr.ConstructorArguments
                    .Select(x => x.Value as string)
                    .SingleOrDefault() ?? attrPropDef.Name.Camelize();

                proc.Emit(OpCodes.Ldarg_0);

                proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
                proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
                proc.Emit(OpCodes.Ldarg_0); // load 'this'
                proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
                proc.Emit(OpCodes.Callvirt, context.ImportReference(
                    sessionGetAttr,
                    attrPropDef.PropertyType.IsGenericParameter
                        ? context.ModelTypeRef
                        : null)); // invoke session.GetAttribute(..)

                proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>
            }
        }

        private string GetIncludePath(ModelWeavingContext context)
        {
            var modelTypeDef = context.ModelTypeRef.Resolve();
            var relationships = GetEagerRelationships(context, modelTypeDef, string.Empty, new[] {modelTypeDef});

            return string.Join(",", relationships);
        }

        private IEnumerable<string> GetEagerRelationships(ModelWeavingContext context, TypeDefinition type, string path, TypeDefinition[] pathTypes)
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
                        pathTypes.Concat(new [] { eagerPropType}).ToArray());
                })
                .ToArray();
            }

            return new [] {path};
        }
    }
}