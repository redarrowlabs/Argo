using System;
using System.Linq.Expressions;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    public interface Rule : CmmnElement
    {
        /// <summary>
        /// The name of the rule.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The caseFileItem that serves as starting point for evaluation of the Expression that is specified by the condition of the Rule.
        /// If not specified, evaluation starts at the CaseFile object that is referenced by the Case as its caseFileModel.
        /// </summary>
        CaseFileItem Context { get; set; }

        /// <summary>
        /// The Expression that serves as condition of the Rule. Must evaluate to a boolean.
        /// </summary>
        Expression<Func<CaseFileItem, bool>> Condition { get; set; }
    }

    /// <summary>
    /// A RepetitionRule specifies under which conditions Tasks, Stages, and Milestones will have repetitions.
    /// Each repetition is a new instance of it.
    /// The first instantiation is not considered a repetition.
    /// The trigger for the repetition is a Sentry, that is referenced as entry criterion, being satisfied, whereby an OnPart of that Sentry occurs.
    /// </summary>
    [Model("repetition-case-rule")]
    public class RepetitionRule : Rule
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [HasOne]
        public CaseFileItem Context { get; set; }

        public Expression<Func<CaseFileItem, bool>> Condition { get; set; }

        /* [Property] */
        public string Type => GetType().FullName;
    }

    /// <summary>
    /// A RequiredRule specifies under which conditions Tasks, Stages, and Milestones will be “required” to complete or terminate before their containing Stage can complete.
    /// </summary>
    [Model("required-case-rule")]
    public class RequiredRule : Rule
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [HasOne]
        public CaseFileItem Context { get; set; }

        public Expression<Func<CaseFileItem, bool>> Condition { get; set; }

        /* [Property] */
        public string Type => GetType().FullName;
    }

    /// <summary>
    /// A ManualActivationRule specifies under which conditions Tasks and Stages, once enabled, start manually or automatically.
    /// </summary>
    [Model("manual-case-rule")]
    public class ManualActivationRule : Rule
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [HasOne]
        public CaseFileItem Context { get; set; }

        public Expression<Func<CaseFileItem, bool>> Condition { get; set; }

        /* [Property] */
        public string Type => GetType().FullName;
    }

    /// <summary>
    /// ApplicabilityRules are used to specify whether a TableItem is “applicable” (“eligible,” “available”) for planning,
    ///  based conditions that are evaluated over information in the CaseFile.
    /// </summary>
    [Model("applicability-case-rule")]
    public class ApplicabilityRule : Rule
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [HasOne]
        public CaseFileItem Context { get; set; }

        public Expression<Func<CaseFileItem, bool>> Condition { get; set; }

        /* [Property] */
        public string Type => GetType().FullName;
    }
}
