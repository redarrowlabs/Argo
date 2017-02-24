using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class CaseMapper : TitanData<Case>
    {
        public CaseMapper()
        {
            OfType("case");
            // only use [Property]s here.
            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.Roles);
            WithAttribute(x => x.CasePlanModel);
        }
    }
}
