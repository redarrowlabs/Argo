using System.Collections.Generic;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Forms
{
    public class FieldEntity
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// TODO enumerated strings pattern
        /// </summary>
        public string Type { get; set; }

        public string DefaultValue { get; set; }

        public IEnumerable<SelectOptionEntity> Options { get; set; }
    }
}
