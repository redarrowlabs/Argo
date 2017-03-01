using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// PlanItemControls define aspects of control of instances of Tasks, Stages, EventListeners, and Milestones.
    /// Under which conditions will Tasks and Stages, once enabled, start manually or automatically.
    /// Under which conditions will Tasks, Stages, and Milestones be “required” to complete before their containing Stage can complete.
    /// Under which conditions will Tasks, Stages, and Milestones need to be repeated.
    /// 
    /// NOTE
    /// A PlanItemControl that is the defaultControl of an EventListener or Milestone,
    /// or that is the itemControl of a PlanItem or DiscretionaryItem that is defined by an EventListener or Milestone, MUST NOT contain a ManualActivationRule.
    /// 
    /// NOTE
    /// A PlanItemControl that is the defaultControl of an EventListener,
    /// or that is the itemControl of a PlanItem or DiscretionaryItem that is defined by an EventListener, MUST NOT contain a RequiredRule.
    /// 
    /// NOTE
    /// A PlanItemControl that is the defaultControl of an EventListener,
    /// or that is the itemControl of a PlanItem that is defined by an EventListener, MUST NOT contain a RepetitionRule.
    /// 
    /// NOTE
    /// A PlanItem that has a PlanItemControl that contains a RepetitionRule, MUST have either an entry criterion that refers to a Sentry that has at least one OnPart
    /// or no entry criteria at all. (This is because the concept of “repetition” depends on the semantics of Sentries with OnParts 
    /// </summary>
    public interface PlanItemControl : CmmnElement
    {
        /// <summary>
        /// A RepetitionRule comprises of an Expression that MUST evaluate to boolean.
        /// If no RepetitionRule object is specified, the default is FALSE.
        /// </summary>
        RepetitionRule RepetitionRule { get; set; }

        /// <summary>
        /// A RequiredRule comprises of an Expression that MUST evaluate to boolean.
        /// If no RequiredRule is specified, the default is FALSE.
        /// </summary>
        RequiredRule RequiredRule { get; set; }

        /// <summary>
        /// A ManualActivationRule comprises of an Expression that MUST evaluate to boolean.
        /// If no ManualActivationRule is specified, then the default is considered TRUE.
        /// </summary>
        ManualActivationRule ManualActivationRule { get; set; }
    }

    /// <summary> 
    /// NOTE
    /// A PlanItemControl that is the defaultControl of an EventListener or Milestone,
    /// or that is the itemControl of a PlanItem or DiscretionaryItem that is defined by an EventListener or Milestone, MUST NOT contain a ManualActivationRule.
    /// 
    /// NOTE
    /// A PlanItemControl that is the defaultControl of an EventListener,
    /// or that is the itemControl of a PlanItem or DiscretionaryItem that is defined by an EventListener, MUST NOT contain a RequiredRule.
    /// 
    /// NOTE
    /// A PlanItemControl that is the defaultControl of an EventListener,
    /// or that is the itemControl of a PlanItem that is defined by an EventListener, MUST NOT contain a RepetitionRule.
    /// </summary>
    [Model("case-plan-item-control-default")]
    public class DefaultControl : PlanItemControl
    {
        [Id]
        public Guid Id { get; set; }
        [HasOne]
        public RepetitionRule RepetitionRule { get; set; }
        [HasOne]
        public RequiredRule RequiredRule { get; set; }
        [HasOne]
        public ManualActivationRule ManualActivationRule { get; set; }
    }

    /// <summary>
    /// NOTE
    /// A PlanItem that has a PlanItemControl that contains a RepetitionRule, MUST have either an entry criterion that refers to a Sentry that has at least one OnPart
    /// or no entry criteria at all. (This is because the concept of “repetition” depends on the semantics of Sentries with OnParts 
    /// </summary>
    [Model("case-plan-item-control-item")]
    public class ItemControl : PlanItemControl
    {
        [Id]
        public Guid Id { get; set; }
        [HasOne]
        public RepetitionRule RepetitionRule { get; set; }
        [HasOne]
        public RequiredRule RequiredRule { get; set; }
        [HasOne]
        public ManualActivationRule ManualActivationRule { get; set; }
    }
}