using System.Collections.Generic;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Forms
{
    public class FieldGroupEntity
    {
        public string Title { get; set; }

        public bool AddMany { get; set; }

        public IEnumerable<FieldEntity> Fields { get; set; }

    }
}
