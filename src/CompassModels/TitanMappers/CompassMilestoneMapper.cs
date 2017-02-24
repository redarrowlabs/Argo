using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassMilestoneMapper : TitanData<CompassMilestone>
    {
        public CompassMilestoneMapper()
        {
            OfType(CompassMilestone.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.State);
        }
    }
}
