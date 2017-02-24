using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class CasePlanModelMapper : TitanData<CasePlanModel>
    {
        public CasePlanModelMapper()
        {
            OfType("case-plan-model");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.IsAutoComplete);
        }
    }
}