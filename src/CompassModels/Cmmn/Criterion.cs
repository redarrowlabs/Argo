using System;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Represent the condition for a PlanItem to become available or to complete depending on the concrete implementation used.
    /// </summary>
    public interface Criterion : CmmnElement
    {
        /// <summary>
        /// An optional name for this criterion.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Reference a Sentry that represents the PlanItem’s entry or exit criteria.
        /// Criteria of a PlanItem MUST refer to Sentries that are contained by the Stage or PlanFragment that contains that PlanItem.
        /// </summary>
        Sentry Sentry { get; set; }
    }
    
    public class EntryCriterion : Criterion
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Sentry Sentry { get; set; }
    }
    
    public class ExitCriterion : Criterion
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Sentry Sentry { get; set; }
    }
}
