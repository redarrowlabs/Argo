using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedArrow.Jsorm.Extensions;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private void WeaveHasManys(ModelWeavingContext context)
        {

            if (_session_GetGenericEnumerable == null
             || _session_SetGenericEnumerable == null
             || _session_GetGenericCollection == null
             || _session_SetGenericCollection == null)
            {
                throw new Exception("Jsorm relationship weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasManys)
            {
                var propertyTypeRef = propertyDef.PropertyType;
                var propertyTypeDef = propertyTypeRef.Resolve();
                
                MethodReference setRltnMethRef;

                if (propertyTypeDef.FullName == _genericIEnumerableTypeDef.FullName)
                {
                    setRltnMethRef = _session_SetGenericEnumerable;
                }
                else if (propertyTypeDef.FullName == _genericICollectionTypeDef.FullName)
                {
                    setRltnMethRef = _session_SetGenericCollection;
                }
                else
                {
                    throw new Exception($"Jsorm encountered a HasMany relationship on non IEnumerable<T> or ICollection<T> property {propertyDef.FullName}");
                }
                
                // get the backing field
                var backingField = propertyDef.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {propertyDef.FullName}");
                }

                // find the rltnName, if there is one
                var propAttr = propertyDef.CustomAttributes.GetAttribute(Constants.Attributes.HasMany);
                var rltnName = propAttr.ConstructorArguments
                    .Where(x => x.Type == TypeSystem.String)
                    .Select(x => x.Value as string)
                    .SingleOrDefault() ?? propertyDef.Name.Camelize();

                // find property generic element type
                var elementTypeDef = ((GenericInstanceType) propertyTypeRef).GenericArguments.First().Resolve();

                LogInfo($"\tWeaving {propertyDef} => {rltnName}");
                
                WeaveRltnSetter(context, backingField, propertyDef, elementTypeDef, setRltnMethRef, rltnName);
            }
        }

        private void WeaveRltnSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition rltnPropDef,
            TypeDefinition elementTypeDef,
            MethodReference sessionSetRltnGeneric,
            string rltnName)
        {
            // supply generic type arguments to template
            var sessionSetRltn = sessionSetRltnGeneric.MakeGenericMethod(
                context.ModelTypeRef,
                elementTypeDef);

            rltnPropDef.SetMethod.Body.Instructions.Clear();

            // set
            // {
            //     if (this.__jsorm__generated_session != null)
            //     {
            //         this.<[PropName]>k__BackingField = this.__jsorm__generated_session.Set<[ModelType], [ElementType]>(this.Id, "[RltnName]", this.<[PropName]>k__BackingField);
            //     }
            //     else
            //     {
            //         this.<[PropName]>k__BackingField = value;
            //     }
            // }
            var proc = rltnPropDef.SetMethod.Body.GetILProcessor();

            var endif = proc.Create(OpCodes.Ldarg_0);
            var ret = proc.Create(OpCodes.Ret);
            
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            
            // if __jsorm__generated_session == null
            proc.Emit(OpCodes.Brfalse_S, endif);

            proc.Emit(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
            proc.Emit(OpCodes.Ldstr, rltnName); // load attrName onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value'
            proc.Emit(OpCodes.Callvirt, context.ImportReference(
                sessionSetRltn,
                rltnPropDef.PropertyType.IsGenericParameter
                    ? context.ModelTypeRef
                    : null)); // invoke session.GetReference(..)

            proc.Emit(OpCodes.Stfld, backingField);

            proc.Emit(OpCodes.Br_S, ret);
            // else
            proc.Append(endif); // load 'this' onto stack to reference session field
            //proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldarg_1);
            proc.Emit(OpCodes.Stfld, backingField);

            proc.Append(ret);
        }
    }
}