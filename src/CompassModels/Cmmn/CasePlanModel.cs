using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Very top level container for the Case Plan.
    /// </summary>
    [Model("case-plan-model")]
    public class CasePlanModel : Stage, CmmnElementInstance
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public CmmnState State { get; set; }
    }
}