using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class CaseFileMapper : TitanData<CaseFile>
    {
        public CaseFileMapper()
        {
            OfType("case-file");

            WithId(x => x.Id);
        }
    }
}