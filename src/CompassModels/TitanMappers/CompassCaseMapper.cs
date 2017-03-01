using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassCaseMapper : TitanData<CompassCase>
    {
        public CompassCaseMapper()
        {
            OfType(CompassCase.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.Participants);
        }
    }
}
