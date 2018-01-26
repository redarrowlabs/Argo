using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddResourceIdentifierProperty(ModelWeavingContext context)
        {
            context.ResourcePropDef = AddAutoProperty("__argo__generated_Resource", context);
        }

        private PropertyDefinition AddAutoProperty(string propertyName, ModelWeavingContext context)
        {
            var backingField = new FieldDefinition(
                $"<{propertyName}>k__BackingField",
                FieldAttributes.Private,
                context.ImportReference(_resourceIdentifierTypeDef));
            backingField.CustomAttributes.Add(new CustomAttribute(context.ImportReference(_compilerGeneratedAttribute)));
            backingField.CustomAttributes.Add(new CustomAttribute(context.ImportReference(_debuggerBrowsableAttribute))
            {
                ConstructorArguments =
                {
                    new CustomAttributeArgument(
                        context.ImportReference(_debuggerBrowsableStateTypeDef),
                        DebuggerBrowsableState.Collapsed)
                }
            });

            var getter = new MethodDefinition(
                $"get_{propertyName}",
                MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                context.ImportReference(_resourceIdentifierTypeDef))
            {
                SemanticsAttributes = MethodSemanticsAttributes.Getter
            };

            var getterProc = getter.Body.GetILProcessor();
            getterProc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            getterProc.Emit(OpCodes.Ldfld, backingField); // load <__argo__generated_Resource>k__BackingField onto stack
            getterProc.Emit(OpCodes.Ret); // return this.<__argo__generated_Resource>k__BackingField;

            var setter = new MethodDefinition(
                $"set_{propertyName}",
                MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                context.ImportReference(TypeSystem.Void))
            {
                SemanticsAttributes = MethodSemanticsAttributes.Setter,
                Parameters =
                {
                    new ParameterDefinition(
                        "value",
                        ParameterAttributes.None,
                        context.ImportReference(_resourceIdentifierTypeDef))
                }
            };

            var setterProc = setter.Body.GetILProcessor();
            setterProc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            setterProc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
            setterProc.Emit(OpCodes.Stfld, backingField); // this.<__argo__generated_Resource>k__BackingField = value;
            setterProc.Emit(OpCodes.Ret); // return;

            var propDef = new PropertyDefinition(
                propertyName,
                PropertyAttributes.None,
                context.ImportReference(_resourceIdentifierTypeDef))
            {
                GetMethod = getter,
                SetMethod = setter
            };

            context.Fields.Add(backingField);
            context.Methods.Add(getter);
            context.Methods.Add(setter);
            context.Properties.Add(propDef);

            return propDef;
        }

        private void AddSessionManagedProperty(ModelWeavingContext context)
        {
            var propertyName = "__argo__generated_SessionManaged";

            var getter = new MethodDefinition(
                $"get_{propertyName}",
                MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                context.ImportReference(typeof(bool)))
            {
                SemanticsAttributes = MethodSemanticsAttributes.Getter
            };

            var proc = getter.Body.GetILProcessor();

            proc.Body.Variables.Add(new VariableDefinition(context.ImportReference(typeof(bool))));

            var load0 = proc.Create(OpCodes.Ldc_I4_0);
            var storeLoc0 = proc.Create(OpCodes.Stloc_0);

            var loadRet = proc.Create(OpCodes.Ldloc_0);

            proc.Emit(OpCodes.Nop);

            //====== this.__argo__generated_session != null
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldfld, context.SessionField);
            proc.Emit(OpCodes.Brfalse_S, load0);
            //=============================================

            //====== !this.__argo__generated_session.Disposed
            proc.Emit(OpCodes.Ldarg_0); // push 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // push 'this.__argo__generated_session' onto stack
            proc.Emit(OpCodes.Callvirt, context.ImportReference(_session_DisposedGetter)); // push 'Disposed' value onto stack
            proc.Emit(OpCodes.Ldc_I4_0);
            proc.Emit(OpCodes.Ceq);
            proc.Emit(OpCodes.Br_S, storeLoc0);
            //===============================================

            proc.Append(load0);
            proc.Append(storeLoc0);
            proc.Emit(OpCodes.Br_S, loadRet);
            proc.Append(loadRet);
            proc.Emit(OpCodes.Ret);

            context.SessionManagedProperty = new PropertyDefinition(
                propertyName,
                PropertyAttributes.None,
                context.ImportReference(typeof(bool)))
            {
                GetMethod = getter
            };

            context.Methods.Add(getter);
            context.Properties.Add(context.SessionManagedProperty);
        }
    }
}