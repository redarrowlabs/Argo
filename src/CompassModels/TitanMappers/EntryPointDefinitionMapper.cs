using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class EntryPointDefinitionMapper : TitanData<EntryPointDefinition>
    {
        public EntryPointDefinitionMapper()
        {
            OfType(EntryPointDefinition.TITAN_TYPE);

            WithId(x => x.Id);

            WithAttribute(x => x.Title);
            WithAttribute(x => x.CaseType);
            WithAttribute(x => x.Roles);
        }
    }
}
