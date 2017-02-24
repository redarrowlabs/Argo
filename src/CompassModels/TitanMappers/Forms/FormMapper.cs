using RedArrow.Compass.CareTeam.CaseManagement.Model.Forms;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Forms
{
    public class FormMapper : TitanData<FormDefinitionEntity>
    {
        public FormMapper()
        {
            OfType(FormDefinitionEntity.TITAN_TYPE);

            WithId(x => x.Id);

            WithAttribute(x => x.Title);
            WithAttribute(x => x.Name);
            WithAttribute(x => x.Sections);
            WithAttribute(x => x.FieldValidations);
        }
    }
}