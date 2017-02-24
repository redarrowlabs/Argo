using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Forms;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class EntryPointDefinition
    {
        public const string TITAN_TYPE = "entry-point";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Title { get; set; }

        [Property]
        public string CaseType { get; set; }

        [Property]
        public ICollection<Role> Roles { get; set; }

        [HasMany]
        public ICollection<CompassMapping> OutputMappings { get; set; }

        [HasOne]
        public FormDefinitionEntity FormDef { get; set; }
    }
}
