using RedArrow.Compass.CareTeam.CaseManagement.Model.Forms.Customization;
using RedArrow.Titan.Sdk.Model.Data;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.TitanMappers.Forms.Customization
{
    public class DomainModelExtensionsMapper : TitanData<DomainModelExtentionsEntity>
    {
        public DomainModelExtensionsMapper()
        {
            OfType(DomainModelExtentionsEntity.TITAN_TYPE);

            WithId(x => x.Id);

            WithAttribute(x => x.CaseFileId);
            WithAttribute(x => x.ExtendsModelType);
            WithAttribute(x => x.Extensions);
        }
    }
}