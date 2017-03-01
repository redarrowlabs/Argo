using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassCase
    {
        public const string TITAN_TYPE = "compass-case";

        public const string ENROLLMENT_RESOURCE_TYPE = "patient";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [Property]
        public ICollection<UserGuid> Participants { get; set; }

        [HasOne]
        public CompassCaseFile CaseFileModel { get; set; }

        [HasOne]
        public CompassCasePlanModel CasePlanModel { get; set; }
    }
}
