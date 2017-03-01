namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    public enum PlanItemTrasition
    {
        /// <summary>
        /// The casePlanModel transitions from Completed, Terminated, Failed, or Suspended to Closed
        /// </summary>
        Close,
        /// <summary>
        /// The casePlanModel, Stage, or Task transitions from Active to Completed.
        /// </summary>
        Complete,
        /// <summary>
        /// The casePlanModel transitions from the initial state to Active.
        /// The PlanItem transitions from the initial state to Available.
        /// </summary>
        Create,
        /// <summary>
        /// The Stage or Task transitions from Enabled to Disabled.
        /// </summary>
        Disable,
        /// <summary>
        /// The Stage or Task transitions from Available to Enabled.
        /// </summary>
        Enable,
        /// <summary>
        /// The Stage or Task transitions from Available, Enabled, Disabled, Active, Failed, or Suspended to Terminated.
        /// </summary>
        Exit,
        /// <summary>
        /// The Stage or Task transitions from Active to Failed.
        /// </summary>
        Fault,
        /// <summary>
        /// The Stage or Task transitions from Enabled to Active.
        /// </summary>
        ManualStart,
        /// <summary>
        /// The EventListener or Milestone transitions from Available to Completed.
        /// </summary>
        Occur,
        /// <summary>
        /// The Stage or Task transitions from Suspended to Available, Enabled, Disabled, or Active depending on its state before it was suspended.
        /// </summary>
        ParentResume,
        /// <summary>
        /// The Stage or Task transitions from Available, Enabled, Disabled, or Active to Suspended.
        /// </summary>
        ParentSuspend,
        /// <summary>
        /// The casePlanModel transitions from Completed, Terminated, Failed, or Suspended to Active. The PlanItem transitions from Failed to Active.
        /// </summary>
        Reactivate,
        /// <summary>
        /// The Stage or Task transitions from Disabled to Enabled.
        /// </summary>
        Reenable,
        /// <summary>
        /// The Task or Stage transitions from Suspended to Active. The EventListener or Milestone transitions from Suspended to Available.
        /// </summary>
        Resume,
        /// <summary>
        /// The Stage or Task transitions from Available to Active.
        /// </summary>
        Start,
        /// <summary>
        /// The casePlanModel, Stage, or Task transitions from Active to Suspended. The EventListener or Milestone transitions from Available to Suspended.
        /// </summary>
        Suspend,
        /// <summary>
        /// The casePlanModel, Stage, or Task transitions from Active to Terminated. The EventListener or Milestone transitions from Available to Terminated.
        /// </summary>
        Terminate
    }
}