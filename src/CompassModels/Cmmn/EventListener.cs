using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// an event is something that “happens” during the course of a Case.
    /// Events may trigger, for example, the enabling, activation, and termination of Stages and Tasks, or the achievement of Milestones.
    /// </summary>
    public interface EventListener : PlanItemDefinition
    {
    }

    /// <summary>
    /// A UserEventListener is a PlanItemDefinition, which instances are used to catch events that are raised by a user,
    /// which events are used to influence the proceeding of the Case directly, instead of indirectly via impacting information in the CaseFile.
    /// A UserEventListener enables direct interaction of a user with the Case.
    /// </summary>
    [Model("case-event-listener-user")]
    public class UserEventListener : EventListener
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// The Roles that are authorized to raise the user event.
        /// </summary>
        [HasMany]
        public ICollection<Role> AuthorizedRoles { get; set; }

        [Property]
        public string Name { get; set; }
        [HasOne]
        public DefaultControl DefaultControl { get; set; }
    }
}
