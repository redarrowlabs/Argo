using Mono.Cecil;
using Mono.Cecil.Cil;
using RedArrow.Argo.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddInitialize(ModelWeavingContext context)
        {
            // public void __argo__generated_Initialize(IResourceIdentifier resource, IModelSession session)
            // {
            //   this.__argo__generated_Resource = resource;
            //   this.__argo__generated_session = session;
            //   this.__argo__generated_includePath = "model.include.path";
            //   this.Id = __argo__generated_session.GetId<TModel>();
            //  }
            var initialize = new MethodDefinition(
                "__argo__generated_Initialize",
                MethodAttributes.Public,
                TypeSystem.Void);

            initialize.Parameters.Add(
                new ParameterDefinition(
                    "resource",
                    ParameterAttributes.None,
                    context.ImportReference(_resourceIdentifierTypeDef)));
            initialize.Parameters.Add(
                new ParameterDefinition(
                    "session",
                    ParameterAttributes.None,
                    context.ImportReference(_sessionTypeDef)));

            var idBackingField = context
                .IdPropDef
                ?.GetMethod
                ?.Body
                ?.Instructions
                ?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
                ?.Operand as FieldReference;
            // supply generic type arguments to template
            var sessionGetId = _session_GetId.MakeGenericMethod(context.ModelTypeDef);

            var proc = initialize.Body.GetILProcessor();

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load arg 'resource' onto stack
            proc.Emit(OpCodes.Callvirt, context.ResourcePropDef.SetMethod);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_2); // load arg 'session' onto stack
            proc.Emit(OpCodes.Stfld, context.SessionField); // this.__argo__generated_session = session;

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load this.__argo__generated_session
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionGetId));
            proc.Emit(OpCodes.Stfld, idBackingField); // this.<Id>K_backingField = this.__argo__generated_session.GetId<TModel>();

            // this._attrBackingField = this.__argo__generated_session.GetAttribute
            WeaveAttributeFieldInitializers(context, proc, context.MappedAttributes);

            // this._attrBackingField = this.__argo__generated_session.GetMeta
            WeaveMetaFieldInitializers(context, proc, context.MappedMeta);

            proc.Emit(OpCodes.Ret); // return

            context.Methods.Add(initialize);
        }

        private void WeaveAttributeFieldInitializers(
            ModelWeavingContext context,
            ILProcessor proc,
            IEnumerable<PropertyDefinition> attrPropDefs)
        {
            foreach (var attrPropDef in attrPropDefs)
            {
                // supply generic type arguments to template
                var sessionGetAttr = _session_GetAttribute
                    .MakeGenericMethod(context.ModelTypeDef, attrPropDef.PropertyType);

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
                        ? context.ModelTypeDef
                        : null)); // invoke session.GetAttribute(..)

                proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>
            }
        }

        private void WeaveMetaFieldInitializers(
            ModelWeavingContext context,
            ILProcessor proc,
            IEnumerable<PropertyDefinition> metaPropDefs)
        {
            foreach (var def in metaPropDefs)
            {
                // supply generic type arguments to template
                var sessionGetAttr = _session_GetMeta.MakeGenericMethod(context.ModelTypeDef, def.PropertyType);

                var backingField = def.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {def?.FullName}");
                }

                var propAttr = def.CustomAttributes.GetAttribute(Constants.Attributes.Meta);
                var metaName = propAttr.ConstructorArguments
                                   .Select(x => x.Value as string)
                                   .SingleOrDefault() ?? def.Name.Camelize();

                proc.Emit(OpCodes.Ldarg_0);

                proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
                proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
                proc.Emit(OpCodes.Ldarg_0); // load 'this'
                proc.Emit(OpCodes.Ldstr, metaName); // load attrName onto stack
                proc.Emit(OpCodes.Callvirt, context.ImportReference(
                    sessionGetAttr,
                    def.PropertyType.IsGenericParameter
                        ? context.ModelTypeDef
                        : null)); // invoke session.GetMeta(..)

                proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>
            }
        }
    }
}
