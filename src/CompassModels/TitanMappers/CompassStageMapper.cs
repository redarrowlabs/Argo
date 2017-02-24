using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassStageMapper : TitanData<CompassStage>
    {
        public CompassStageMapper()
        {
            OfType(CompassStage.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.State);
        }
    }
}
