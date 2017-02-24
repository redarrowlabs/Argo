using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassTaskMapper : TitanData<CompassTask>
    {
        public CompassTaskMapper()
        {
            OfType(CompassTask.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.Assignees);
            WithAttribute(x => x.Roles);
            WithAttribute(x => x.State);
            WithAttribute(x => x.VariableState);
        }
    }
}
