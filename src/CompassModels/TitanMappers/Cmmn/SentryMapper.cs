using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class SentryMapper : TitanData<Sentry>
    {
        public SentryMapper()
        {
            OfType("case-sentry");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.State);
        }
    }
}