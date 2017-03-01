using RedArrow.Argo.Attributes;

//using RedArrow.Compass.CareTeam.Contracts.Profile;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// CaseRoles authorize case workers or teams of case workers to perform HumanTasks, plan based on DiscretionaryItems, and raise user events (by triggering UserEventListeners).
    /// </summary>
    public class Role
    {
        /// <summary>
        /// The name of the Role
        /// </summary>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// The Case that contains the caseRoles
        /// </summary>
        // public Case Case { get; set; } // ~todo~ 

        /***** Compass Extension *****/

        // ~todo~ UserProfile is in contracts and this is persistence (circular ref).

//        [HasMany]
//        public ICollection<UserProfile> Participants { get; set; }
    }
}