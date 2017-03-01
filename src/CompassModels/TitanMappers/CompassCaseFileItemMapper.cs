using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassCaseFileItemMapper : TitanData<CompassCaseFileItem>
    {
        public CompassCaseFileItemMapper()
        {
            OfType(CompassCaseFileItem.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.State);
            WithAttribute(x => x.Uri);
            WithAttribute(x => x.ResourceType);
            WithAttribute(x => x.TitanDataType);
        }
    }
}
