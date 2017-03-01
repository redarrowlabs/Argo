namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Base class for CMMN instance Elements.
    /// Case, EventListener, Milestone, Stage, Task
    /// </summary>
    public interface CmmnElementInstance : CmmnElement
    {
        CmmnState State { get; set; }
    }
}
