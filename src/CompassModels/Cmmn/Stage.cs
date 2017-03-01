using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Serves as building block for Case(instance) plans.
    /// Unlike PlanFragments (that are not Stages), Stages do have run-time representations in a Case (instance) plan.
    /// Instances of Stages are tracked through the CMMN-defined Stage lifecycle. Stages may be considered “episodes” of a Case, though Case models allow for defining Stages that can be planned in parallel also.
    /// </summary>
    [Model("case-stage")]
    public class Stage : PlanFragment
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// This attribute controls completion of the Stage. If FALSE, a Stage requires a user to manually complete it.
        /// </summary>
        [Property]
        public bool IsAutoComplete { get; set; }

        /// <summary>
        /// Defines the (optional) PlanningTable of the Stage.
        /// </summary>
        [HasOne]
        public PlanningTable PlanningTable { get; set; }

        /// <summary>
        /// This attribute lists the PlanItemDefinition objects available in the Stage, and its nested Stages.
        /// ExitCriterion of a Stage MUST refer to Sentries that are contained by that Stage.
        /// </summary>
        [Property]
        public ICollection<PlanItemDefinition> PlanItemDefinitions { get; set; }

        /// <summary>
        /// Define zero or more ExitCriterion that serve as the exit criteria for the Stage.
        /// Only the Stage that is referenced by the Case as its casePlanningModel can have exitCriteria.
        /// Note that it is only useful for that Stage to directly have exitCriteria, as it cannot be further nested in other Stages
        /// </summary>
        [Property]
        public ICollection<Sentry> ExitCriteria { get; set; }

        [Property]
        public string Name { get; set; }

        /// <summary>
        /// The PlanItems that are contained by the PlanFragment.
        /// </summary>
        [HasOne]
        public DefaultControl DefaultControl { get; set; }

        /// <summary>
        /// The PlanItems that are contained by the PlanFragment.
        /// </summary>
        [Property]
        public ICollection<PlanItem> PlanItems { get; set; }

        /// <summary>
        /// The Sentry(ies) contained by the PlanFragment.
        /// </summary>
        [Property]
        public ICollection<Sentry> Sentrys { get; set; }
    }

    /// <summary>
    /// A PlanFragment is a set of PlanItems, possibly dependent on each other, and that often occur in Case plans in combination, representing a pattern.
    /// </summary>
    public interface PlanFragment : PlanItemDefinition
    {
        /// <summary>
        /// The PlanItems that are contained by the PlanFragment.
        /// </summary>
        ICollection<PlanItem> PlanItems { get; set; }

        /// <summary>
        /// The Sentry(ies) contained by the PlanFragment.
        /// </summary>
        ICollection<Sentry> Sentrys { get; set; }
    }

    /// <summary>
    /// A PlanItem object is a use of a PlanItemDefinition element in a PlanFragment (or Stage).
    /// 
    /// This required a separate class, PlanItem, that refers to PlanItemDefinition.
    /// Multiple PlanItems might refer to the same PlanItemDefinition.
    /// A PlanItemDefinition is (re-)used in multiple PlanFragments(or Stages) when these PlanFragments(or Stages) contain PlanItems that refer to or(“use”) that same PlanItemDefinition.
    /// </summary>
    public class PlanItem : CmmnElementInstance
    {
        public Guid Id { get; set; }

        public CmmnState State { get; set; }

        /// <summary>
        /// The name of the PlanItem object. This attribute supersedes the attribute of the corresponding PlanItemDefinition element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Reference to the corresponding PlanItemDefinition object.
        /// For every PlanItem object, there MUST be exactly one PlanItemDefinition object.
        /// 
        /// DefinitionRef MUST NOT represent the Stage that is the casePlanModel of the Case.
        /// 
        /// DefinitionRef MUST NOT represent a PlanFragment that is not a Stage.
        /// 
        /// This implies that a PlanFragment, not being a Stage, cannot be used as PlanItem inside a PlanFragment or Stage.As PlanItems may refer to a PlanItemDefinition that is a Stage, Stages can be nested.
        /// A Stage is said to be “nested” in another Stage, when the Stage is the PlanItemDefinition of a PlanItem that is contained in that other Stage, either directly, or recursively through even other Stages.
        /// 
        /// DefinitionRef of a PlanItem that is contained by a Stage MUST NOT be that Stage or any Stage in which that Stage is nested.
        /// 
        /// A PlanItem can only refer to a PlanItemDefinition that is defined in the same Stage than the PlanItem or in one of its parents.
        /// When the PlanItem is contained in a PlanFragment, the PlanItemDefinition of that PlanItem MUST be contained by the parent Stage of the Plan Fragment or by a direct of indirect parent Stage of that Plan Fragment.
        /// </summary>
        public PlanItemDefinition PlanItemDefinition { get; set; }

        /// <summary>
        /// The PlanItemControl controls aspects of the behavior of instances of the PlanItem object.
        /// 
        /// If a PlanItemControl object is specified for a PlanItem, then it MUST overwrite the PlanItemControl object of the associated PlanItemDefinition element.
        /// Otherwise, the behavior of the PlanItem object is specified by the PlanItemControl object of its associated PlanItemDefinition.
        /// </summary>
        public ItemControl ItemControl { get; set; }

        /// <summary>
        /// Zero or more EntryCriterion for that PlanItem. A PlanItem that is defined by an EventListener MUST NOT have entryCriteriaRefs.
        /// </summary>
        public ICollection<EntryCriterion> EntryCriteria { get; set; }

        //public ICollection<EntryCriterion> EntryCriterions { get; set; }

        /// <summary>
        /// Zero or more ExitCriterion for that PlanItem.
        /// A PlanItem that is defined by an EventListener or Milestone MUST NOT have exitCriteriaRefs.
        /// A PlanItem that is defined by a Task that is non-blocking (isBlocking set to FALSE) MUST NOT have exitCriteriaRefs.
        /// </summary>
        public ICollection<ExitCriterion> ExitCriteria { get; set; }

        //public ICollection<ExitCriterion> ExitCriterions { get; set; }
    }
}