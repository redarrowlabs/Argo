using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers
{
    public class CompassCasePlanModelMapper : TitanData<CompassCasePlanModel>
    {
        public CompassCasePlanModelMapper()
        {
            OfType(CompassCasePlanModel.TITAN_TYPE);

            WithId(x => x.Id);
            WithAttribute(x => x.State);
            WithAttribute(x => x.VariableState);
        }
    }
}
