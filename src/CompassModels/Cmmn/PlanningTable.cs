using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Planning is a run-time effort.
    /// A PlanningTable defines the scope of planning, in terms of identifying a sub-set of PlanItemDefinitions that can be considered for planning in a certain context.
    /// </summary>
    [Model("case-planning-table")]
    public class PlanningTable : TableItem
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// A list of TableItem objects (see 5.4.9.1) available for planning.
        /// </summary>
        [HasMany]
        public ICollection<TableItem> TableItems { get; set; }

        [HasMany]
        public ICollection<ApplicabilityRule> ApplicabilityRules { get; set; }

        [HasMany]
        public ICollection<Role> AuthorizedRoles { get; set; }
    }

    /// <summary>
    /// A TableItem might be a DiscretionaryItem or a PlanningTable.
    /// </summary>
    public interface TableItem : CmmnElement
    {
        /// <summary>
        /// Zero or more ApplicabilityRule objects.
        /// </summary>
        ICollection<ApplicabilityRule> ApplicabilityRules { get; set; }

        /// <summary>
        /// References to zero or more Role objects that are authorized to plan based on the TableItem.
        /// </summary>
        ICollection<Role> AuthorizedRoles { get; set; }
    }

    /// <summary>
    /// A DiscretionaryItem identifies a PlanItemDefinition, of which instances can be planned, to the “discretion” of a Case worker that is involved in planning,
    /// which instances are planned into the context that is implied by the PlanningTable that contains the DiscretionaryItem, either directly, or via a nested PlanningTable.
    /// </summary>
    [Model("case-planning-table-discretionary-item")]
    public class DiscretionaryItem : TableItem
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the DiscretionaryItem.
        /// </summary>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// Defines the PlanItemDefinition associated with the DiscretionaryItem and which is the basis for planning.
        /// The Definition of a DiscretionaryItem MUST represent a Task or a PlanFragment(or Stage).
        /// A Discretionary Item defined in a Planning Table of a Stage MUST refer a PlanItemDefinition in that stage or in any parent stage of that stage.
        /// A Discretionary Item defined in the Planning Table of a Human Task MUST refer a PlanItemDefinition contained in the parent stage of the Human Task or in any parent stage of that Human Task.
        /// </summary>
        [HasOne]
        public PlanItemDefinition Definition { get; set; }

        /// <summary>
        /// An optional PlanItemControl object. The PlanItemControl object controls aspects of the behavior of instances that are planned via the DiscretionaryItem.
        /// If the itemControl attribute is specified, it MUST overwrite the value of attribute defaultControl of the DiscretionaryItem associated PlanItemDefinition.
        /// </summary>
        [HasOne]
        public ItemControl ItemControl { get; set; }

        /// <summary>
        /// Zero or more EntryCriterion that represent the DiscretionaryItem’s entry criteria.
        /// </summary>
        [HasMany]
        public ICollection<EntryCriterion> EntryCriterions { get; set; }

        /// <summary>
        /// Zero or more ExitCriterion that represent the DiscretionaryItem’s exit criteria.
        /// A DiscretionaryItem that is defined by a Task that is non-blocking (isBlocking set to FALSE) MUST NOT have exitCreterion.
        /// </summary>
        [HasMany]
        public ICollection<ExitCriterion> ExitCriterions { get; set; }

        [HasMany]
        public ICollection<ApplicabilityRule> ApplicabilityRules { get; set; }

        [HasMany]
        public ICollection<Role> AuthorizedRoles { get; set; }
    }
}