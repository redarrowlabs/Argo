using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// All information, or references to information, that is required as context for managing a Case, is defined by a CaseFile.
    /// </summary>
    [Model("case-file")]
    public class CaseFile : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }
        
        /// <summary>
        /// A CaseFile MUST contain at least one CaseFileItem.
        /// </summary>
        [HasMany]
        public ICollection<CaseFileItem> CaseFileItems { get; set; }
    }
}
