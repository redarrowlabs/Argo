using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class ExitCriterionMapper : TitanData<ExitCriterion>
    {
        public ExitCriterionMapper()
        {
            OfType("case-criterion-exit");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
        }
    }
}