using System.Collections.Generic;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Forms
{
    public class FormSectionEntity
    {
        public string Title { get; set; }

        public IEnumerable<FieldGroupEntity> FieldGroups { get; set; }
    }
}
