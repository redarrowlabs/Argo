using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    /// <summary>
    /// The containing Stage for the Compass CareTeam Case
    /// </summary>
    [Model(TITAN_TYPE)]
    public class CompassCasePlanModel
    {
        public const string TITAN_TYPE = "compass-case-plan-model";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public CmmnState State { get; set; }

        [HasMany]
        public ICollection<CompassMilestone> Milestones { get; set; }

        [HasMany]
        public ICollection<CompassStage> Stages { get; set; }

        [Property]
        public IDictionary<string, string> VariableState { get; set; }

        [HasMany]
        public ICollection<CompassMapping> InputMappings { get; set; }

        [HasMany]
        public ICollection<CompassMapping> OutputMappings { get; set; }
    }
}