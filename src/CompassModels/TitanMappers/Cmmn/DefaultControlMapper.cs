using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class DefaultControlMapper : TitanData<DefaultControl>
    {
        public DefaultControlMapper()
        {
            OfType("case-plan-item-control-default");

            WithId(x => x.Id);
        }
    }
}