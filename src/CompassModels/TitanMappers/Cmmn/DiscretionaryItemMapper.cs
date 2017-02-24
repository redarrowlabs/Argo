using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class DiscretionaryItemMapper : TitanData<DiscretionaryItem>
    {
        public DiscretionaryItemMapper()
        {
            OfType("case-planning-table-discretionary-item");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
        }
    }
}