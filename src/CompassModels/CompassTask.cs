using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Forms;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassTask
    {
        public const string TITAN_TYPE = "compass-task";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [Property]
        public ICollection<UserGuid> Assignees { get; set; }

        [Property]
        public ICollection<Role> Roles { get; set; }

        [HasOne]
        public FormDefinitionEntity FormDefinition { get; set; }

        [Property]
        public CmmnState State { get; set; }

        [Property]
        public IDictionary<string, string> VariableState { get; set; }

        [HasMany]
        public ICollection<CompassMapping> InputMappings { get; set; }

        [HasMany]
        public ICollection<CompassMapping> OutputMappings { get; set; }
    }
}