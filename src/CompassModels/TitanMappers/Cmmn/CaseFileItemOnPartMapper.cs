using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class CaseFileItemOnPartMapper : TitanData<CaseFileItemOnPart>
    {
        public CaseFileItemOnPartMapper()
        {
            OfType("case-sentry-onpart-case-file-item");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.StandardEvent);
        }
    }
}