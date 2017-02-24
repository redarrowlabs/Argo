using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class ItemControlMapper : TitanData<ItemControl>
    {
        public ItemControlMapper()
        {
            OfType("case-plan-item-control-item");

            WithId(x => x.Id);
        }
    }
}