using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class OutputCaseParameterMapper : TitanData<OutputCaseParameter>
    {
        public OutputCaseParameterMapper()
        {
            OfType("case-parameter-output");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
        }
    }
}