using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassStage
    {
        public const string TITAN_TYPE = "compass-stage";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [Property]
        public string DisplayName { get; set; }

        [Property]
        public CmmnState State { get; set; }

        [HasMany]
        public ICollection<CompassTask> Tasks { get; set; }
    }
}