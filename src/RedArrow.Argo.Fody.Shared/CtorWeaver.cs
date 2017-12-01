using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddStaticCtor(ModelWeavingContext context)
        {
            var ctor = context.ModelTypeDef.GetStaticConstructor();

            var include = GetIncludePath(context);

            if (ctor == null)
            {
                ctor = new MethodDefinition(
                    ".cctor",
                    MethodAttributes.Private |
                    MethodAttributes.HideBySig |
                    MethodAttributes.Static |
                    MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName,
                    TypeSystem.Void);

                context.Methods.Add(ctor);

                var proc = ctor.Body.GetILProcessor();

                proc.Emit(OpCodes.Ldstr, include);
                proc.Emit(OpCodes.Stsfld, context.IncludePathField);
                proc.Emit(OpCodes.Ret);
            }
            else
            {
                var proc = ctor.Body.GetILProcessor();
                var originalFirst = ctor.Body.Instructions[0];

                proc.InsertBefore(originalFirst, proc.Create(OpCodes.Ldstr, include));
                proc.InsertBefore(originalFirst, proc.Create(OpCodes.Stsfld, context.IncludePathField));
            }

            context.ModelTypeDef.IsBeforeFieldInit = false;
        }
    }
}