using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class ProcessTaskMapper : TitanData<ProcessTask>
    {
        public ProcessTaskMapper()
        {
            OfType("case-task-process");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.IsBlocking);
        }
    }
}