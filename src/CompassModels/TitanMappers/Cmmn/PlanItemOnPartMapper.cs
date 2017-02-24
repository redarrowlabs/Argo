using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class PlanItemOnPartMapper : TitanData<PlanItemOnPart>
    {
        public PlanItemOnPartMapper()
        {
            OfType("case-sentry-onpart-plan-item");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.StandardEvent);
        }
    }
}