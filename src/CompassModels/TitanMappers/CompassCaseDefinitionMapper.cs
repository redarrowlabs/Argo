using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassCaseDefinitionMapper : TitanData<CompassCaseDefinition>
    {
        public CompassCaseDefinitionMapper()
        {
            OfType(CompassCaseDefinition.TITAN_TYPE);

            WithId(x => x.Id);
        }
    }
}
