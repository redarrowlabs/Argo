using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Cmmn
{
    public class DefinitionsMapper : TitanData<Definitions>
    {
        public DefinitionsMapper()
        {
            OfType("case");

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.Author);
            WithAttribute(x => x.CreationDate);
        }
    }
}