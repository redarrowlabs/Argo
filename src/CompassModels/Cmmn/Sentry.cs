using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// A Sentry “watches out” for important situations to occur (or “events”), which influence the further proceedings of a Case(and hence their name).
    /// A Sentry is a combination of an “event and/or condition.” When the event is received, a condition might be applied to evaluate whether the event has effect or not.
    /// </summary>
    [Model("case-sentry")]
    public class Sentry : CmmnElementInstance
    {
        [Id]
        public Guid Id { get; set; }
        [Property]
        public CmmnState State { get; set; }

        [Property]
        public string Name { get; set; }

        /// <summary>
        /// The Sentry OnPart addresses the “event” aspect of a Sentry.
        /// </summary>
        [Property]
        public ICollection<OnPart> OnParts { get; set; }

        /// <summary>
        /// The IfPart of a Sentry is used to specify an (optional) condition.
        /// </summary>
        [Property]
        public IfPart IfPart { get; set; }
    }

    /// <summary>
    /// The IfPart of a Sentry is used to specify an (optional) condition.
    /// </summary>
    [Model("case-sentry-ifpart")]
    public class IfPart : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// The caseFileItem that serves as starting point for evaluation of the Expression that is specified by the condition of the IfPart.
        /// If not specified, evaluation starts at the CaseFile object that is referenced by the Case as its caseFileModel.
        /// </summary>
        [HasOne]
        public CaseFileItem Context { get; set; }

        /// <summary>
        /// A condition that is defined as Expression. The Expression MUST evaluate to boolean.
        /// </summary>
        public Expression<Func<CaseFileItem, bool>> Condition { get; set; }

        /* [Property] */
        public string Type => GetType().FullName;
    }

    /// <summary>
    /// The Sentry OnPart addresses the “event” aspect of a Sentry.
    /// </summary>
    public interface OnPart : CmmnElement
    {
        string Name { get; set; }
    }

    [Model("case-sentry-onpart-case-file-item")]
    public class CaseFileItemOnPart : OnPart
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        /// <summary>
        /// Reference to a CaseFileItem.
        /// If the associated CaseFileItem is undergoing the state transition as specified by attribute standardEvent, the OnPart MUST occur
        /// </summary>
        [HasOne]
        public CaseFileItem Source { get; set; }

        /// <summary>
        /// Reference to a state transition in the CaseFileItem lifecyle
        /// </summary>
        [Property]
        public CaseFileItemTrasition StandardEvent { get; set; }
    }

    [Model("case-sentry-onpart-plan-item")]
    public class PlanItemOnPart : OnPart
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        /// <summary>
        /// Reference to a PlanItem.
        /// If the associated PlanItem is undergoing a state transition as specified by attribute standardEvent, the OnPart MUST occur
        /// </summary>
        [HasOne]
        public PlanItem Source { get; set; }

        /// <summary>
        /// Reference to a state transition in the lifecycle of a Stage, Task, EventListener, or Milestone.
        /// If Source is a Stage or Task, StandardEvent must denote a transition of the CMMN-defined lifecycle of Stage/Task.
        /// If Source is a EventListener or Milestone, StandardEvent must denote a transition of the CMMN-defined lifecycle of EventListener/Milestone.
        /// </summary>
        [Property]
        public PlanItemTrasition StandardEvent { get; set; }

        /// <summary>
        /// A reference to an ExitCriterion.
        /// It enforces that the PlanItemOnPart of the Sentry occurs when the PlanItem that is referenced by sourceRef transits by the specified exitCriterion due to the Sentry that is refers being satisfied.
        /// If specified, exitCriterionRef MUST referred to an ExitCriterion that is contained PlanItem referred by the sourceRef of the PlanItemOnPart.
        /// </summary>
        [HasOne]
        public ExitCriterion ExitCriterion { get; set; }
    }
}