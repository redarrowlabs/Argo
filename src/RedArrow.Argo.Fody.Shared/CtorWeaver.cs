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
            // Ctor(Guid id, IModelSession session)
            // {
            //   Id = id;
            //   _argo_generated_session = session;
            //  }
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                TypeSystem.Void);

            ctor.Parameters.Add(
                new ParameterDefinition(
                    "id",
                    ParameterAttributes.None,
                    context.ImportReference(typeof(Guid))));
            ctor.Parameters.Add(
                new ParameterDefinition(
                    "session",
                    ParameterAttributes.None,
                    context.ImportReference(_sessionTypeDef)));

            var objectCtor = context.ImportReference(TypeSystem.Object.Resolve().GetConstructors().First());

            var proc = ctor.Body.GetILProcessor();

            // public Patient(Guid id, IModelSession session)
            // {
            //   this.Id = id;
            //   this._argo_generated_session = session;
            // }
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Call, objectCtor); // call base ctor on 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'id' onto stack
            proc.Emit(OpCodes.Callvirt, context.IdPropDef.SetMethod); // this.Id = id;
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_2); // load 'session' onto stack
            proc.Emit(OpCodes.Stfld, context.SessionField); // this.__argo__generated_session = session;

            // this._attrBackingField = this.__argo__generated_session.GetAttribute
            WeaveAttributeFieldInitializers(context, proc, context.MappedAttributes);

            proc.Emit(OpCodes.Ret); // return

            context.Methods.Add(ctor);
        }

        private void WeaveAttributeFieldInitializers(ModelWeavingContext context, ILProcessor proc, IEnumerable<PropertyDefinition> attrPropDefs)
        {
            var sessionGetAttrGeneric = _sessionTypeDef
                   .Methods
                   .SingleOrDefault(x => x.Name == "GetAttribute");

            foreach (var attrPropDef in attrPropDefs)
            {
                // supply generic type arguments to template
                var sessionGetAttr = sessionGetAttrGeneric.MakeGenericMethod(context.ModelTypeRef, attrPropDef.PropertyType);

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
                proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
                proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
                proc.Emit(OpCodes.Callvirt, context.ImportReference(
                    sessionGetAttr,
                    attrPropDef.PropertyType.IsGenericParameter
                        ? context.ModelTypeRef
                        : null)); // invoke session.GetAttribute(..)

                proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>
            }
        }
    }
}