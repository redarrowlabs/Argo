using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class InputCaseParameterMapper : TitanData<InputCaseParameter>
    {
        public InputCaseParameterMapper()
        {
            OfType("case-parameter-input");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
        }
    }
}