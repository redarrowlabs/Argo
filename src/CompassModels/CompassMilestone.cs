using System;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassMilestone
    {
        public const string TITAN_TYPE = "compass-milestone";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [Property]
        public CmmnState State { get; set; }
        
        /*
         * Note: may want to add roles on here as well. This would allow different roles to interpret
         * the state/progress of the case differently. e.g. patient might just see:
         * 
         * enrollment-pending, enrollment-complete
         * 
         * vs care team seeing
         * 
         * enrollment-submitted, enrollment-pending-approval, enrollment-complete
         * 
         */
    }
}
