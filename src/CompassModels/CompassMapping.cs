using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassMapping
    {
        public const string TITAN_TYPE = "case-mapping";

        [Id]
        public Guid Id { get; set; }

        [HasOne]
        public CompassCaseFileItem CaseFileItem { get; set; }

        /// <summary>
        /// Gets the CaseFileItem property (path)
        /// </summary>
        [Property]
        public string CaseFileItemProperty { get; set; }

        /// <summary>
        /// Gets the Task VariableState path
        /// </summary>
        [Property]
        public string TaskVariableName { get; set; }
    }
}