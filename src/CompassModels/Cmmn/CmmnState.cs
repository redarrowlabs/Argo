namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    public enum CmmnState
    {
        /// <summary>
        /// Indicates behavior is being executed in the instance.
        /// </summary>
        Active,
        /// <summary>
        /// The instance is waiting for a Sentry to become TRUE or for an event to occur, so that the instance can progress to its primary purpose (e.g., become Active or Enabled).
        /// </summary>
        Available,
        /// <summary>
        /// Terminal state. There is no activity (no behavior being executed) in the Case instance, and further planning in the Case's casePlanModel is not permitted. This state is only available for the outermost Stage instance implementing the Case's casePlanModel.
        /// </summary>
        Closed,
        /// <summary>
        /// Semi-terminal state for Case instance, but terminal state for all other EventListener, Milestone, Stage, or Task instances. There is no activity (no behavior being executed) in the element. A Case instance could transition back to Active by engaging in planning at the outermost Stage instance implementing the Case's casePlanModel.
        /// </summary>
        Completed,
        /// <summary>
        /// Semi-terminal state. Indicates a Case worker (human) decision to disable the instance, because it may not be required for the Case instance at hand.
        /// </summary>
        Disabled,
        /// <summary>
        /// The instance is waiting for a Case worker (human) decision to become Active or Disabled.
        /// </summary>
        Enabled,
        /// <summary>
        /// Semi-terminal state. This state indicates an exception or software failure.
        /// </summary>
        Failed,
        /// <summary>
        /// Indicates a Case worker (human) decision to temporary suspend work on an Active instance. There is no activity (no behavior being executed) in the instance, but a Case worker (human) could move the instance back to an Active state.
        /// </summary>
        Suspended,
        /// <summary>
        /// Terminal state. Indicates termination by an exit criteria or a Case worker (human) decision to terminate an Active instance.
        /// </summary>
        Terminated
    }
}