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
		        .Where(HasIdAttribute)
		        .ToArray();

	        LogInfo("Jsorm scanner discovered model types:");
	        foreach (var model in _modelTypeDefs)
	        {
		        LogInfo($"\t{model.FullName}");
	        }
        }

        private bool HasIdAttribute(TypeDefinition type)
        {
	        return type.Properties
		        .Any(t => t.CustomAttributes
			        .Any(a => a.Constructor.DeclaringType.FullName == "RedArrow.Jsorm.Attributes.IdAttribute"));
        }
    }
}