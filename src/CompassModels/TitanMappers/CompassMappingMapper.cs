using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassMappingMapper : TitanData<CompassMapping>
    {
        public CompassMappingMapper()
        {
            OfType(CompassMapping.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.CaseFileItemProperty);
            WithAttribute(x => x.TaskVariableName);
        }
    }
}
