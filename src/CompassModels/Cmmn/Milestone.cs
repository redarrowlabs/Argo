using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// A Milestone is a PlanItemDefinition that represents an achievable target, defined to enable evaluation of progress of the Case.
    /// No work is directly associated with a Milestone, but completion of set of tasks or the availability of key deliverables(information in the CaseFile) typically leads to achieving a Milestone.
    /// </summary
    [Model("case-milestone")]
    public class Milestone : PlanItemDefinition
    {
        [Id]
        public Guid Id { get; set; }
        [Property]
        public string Name { get; set; }
        [HasOne]
        public DefaultControl DefaultControl { get; set; }

    }
}
