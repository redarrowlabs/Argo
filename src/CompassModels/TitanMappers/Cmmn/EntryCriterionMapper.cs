using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class EntryCriterionMapper : TitanData<EntryCriterion>
    {
        public EntryCriterionMapper()
        {
            OfType("case-criterion-entry");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
        }
    }
}