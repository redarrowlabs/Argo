using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Case is a top-level concept that combines all elements that constitute a Case model.
    /// </summary>
    [Model("case")]
    public class Case : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the Case
        /// </summary>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// This attribute lists the Role objects associated with the Case.
        /// These Roles are specific to the Case, and are not known outside the context of the Case.
        /// </summary>
        [Property]
        public ICollection<Role> Roles { get; set; }

        /// <summary>
        /// One CaseFile object. Every Case MUST be associated with exactly one CaseFile.
        /// </summary>
        [HasOne]
        public CaseFile CaseFileModel { get; set; }

        /// <summary>
        /// The plan model of the Case. Every Case MUST be associated with exactly one plan model. It is defined by association to Stage.
        /// </summary>
        [HasOne]
        public CasePlanModel CasePlanModel { get; set; }
        /*
        /// <summary>
        /// Input Parameters of the Case. A Case might have input Parameters so that it can be called from outside, e.g., by other Cases.
        /// </summary>
        public ICollection<CaseParameter> Inputs{ get; set; }

        /// <summary>
        /// Output Parameters of the Case. A Case might have output parameters so that it can return a result to e.g., a calling Case.
        /// </summary>
        public ICollection<CaseParameter> Outputs { get; set; }
        */
    }
}
