using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassCaseFile
    {
        public const string TITAN_TYPE = "compass-case-file";

        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// A CaseFile MUST contain at least one CaseFileItem.
        /// </summary>
        [HasMany]
        public ICollection<CompassCaseFileItem> CaseFileItems { get; set; }
    }
}