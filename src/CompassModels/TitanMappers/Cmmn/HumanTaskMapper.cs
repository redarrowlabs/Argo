using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class HumanTaskMapper : TitanData<HumanTask>
    {
        public HumanTaskMapper()
        {
            OfType("case-task-human");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.IsBlocking);
        }
    }
}