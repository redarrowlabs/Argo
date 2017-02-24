using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class UserEventListenerMapper : TitanData<UserEventListener>
    {
        public UserEventListenerMapper()
        {
            OfType("case-event-listener-user");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            //WithAttribute(x => x.State);
        }
    }
}