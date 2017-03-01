using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class PlanningTableMapper : TitanData<PlanningTable>
    {
        public PlanningTableMapper()
        {
            OfType("case-planning-table");

            WithId(x => x.Id);
        }
    }
}