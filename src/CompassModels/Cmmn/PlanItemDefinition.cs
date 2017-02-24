
namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// PlanItemDefinition elements define the building blocks from which Case (instance) plans are constructed.
    /// Specialized into: EventListener, Milestone, PlanFragment (and Stage), and Task.
    /// </summary>
    public interface PlanItemDefinition : CmmnElement
    {
        /// <summary>
        /// The name of the PlanItemDefinition
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Element that specifies the default for aspects of control of PlanItemDefinitions.
        /// DefaultControl MUST NOT be specified for the Stage that is referenced by the Case as its casePlanModel.
        /// </summary>
        DefaultControl DefaultControl { get; set; }
    }
}