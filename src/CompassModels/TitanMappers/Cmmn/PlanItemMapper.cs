using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class PlanItemMapper : TitanData<PlanItem>
    {
        public PlanItemMapper()
        {
            OfType("case-stage");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.State);
        }
    }
}