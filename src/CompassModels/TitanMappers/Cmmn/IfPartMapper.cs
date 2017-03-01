using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class IfPartMapper : TitanData<IfPart>
    {
        public IfPartMapper()
        {
            OfType("case-sentry-ifpart");

            WithId(x => x.Id);
            WithAttribute(x => x.Type);
        }
    }
}