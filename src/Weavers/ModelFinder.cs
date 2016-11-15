using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private IEnumerable<TypeDefinition> _modelTypeDefs;

        private void FindModels()
        {
            _modelTypeDefs = ModuleDefinition.Types
                .Where(x => x.HasCustomAttributes)
                .Where(x => x.CustomAttributes.ContainsAttribute(Constants.Attributes.Model))
                .ToArray();

            LogInfo("Jsorm scanner discovered model types:");
            foreach (var model in _modelTypeDefs)
            {
                LogInfo($"\t{model.FullName}");
            }
        }
    }
}