using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassCaseFileMapper : TitanData<CompassCaseFile>
    {
        public CompassCaseFileMapper()
        {
            OfType(CompassCaseFile.TITAN_TYPE);

            WithId(x => x.Id);
        }
    }
}
