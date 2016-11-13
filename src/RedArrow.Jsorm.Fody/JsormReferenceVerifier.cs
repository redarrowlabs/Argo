using System;
using System.Linq;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private void VerifyJsomReference()
        {
            if (ModuleDefinition.AssemblyReferences.All(x => x.Name != "RedArrow.Jsorm"))
            {
                throw new Exception($"{ModuleDefinition.Assembly.Name} must reference RedArrow.Jsorm");
            }
        }
    }
}