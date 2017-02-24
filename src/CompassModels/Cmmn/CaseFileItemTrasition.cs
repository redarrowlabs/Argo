
namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    public enum CaseFileItemTrasition
    {
        /// <summary>
        /// A new child CaseFileItem has been added to an existing CaseFileItem. The lifecycle state remains Available.
        /// </summary>
        AddChild,
        /// <summary>
        /// A new reference to a CaseFileItem has been added to a CaseFileItem. The lifecycle state remains Available.
        /// </summary>
        AddReference,
        /// <summary>
        /// A CaseFileItem transitions from the initial state to Available.
        /// </summary>
        Create,
        /// <summary>
        /// A CaseFileItem transitions from Available to Discarded.
        /// </summary>
        Delete,
        /// <summary>
        /// A child CaseFileItem has been removed from a CaseFileItem. The lifecycle state remains Available.
        /// </summary>
        RemoveChild,
        /// <summary>
        /// A reference to a CaseFileItem has been removed from a CaseFileItem. The lifecycle state remains Available.
        /// </summary>
        RemoveReference,
        /// <summary>
        /// The content of a CaseFileItem has been replaced. The lifecycle state remains Available.
        /// </summary>
        Replace,
        /// <summary>
        /// The CaseFileItem has been updated. The lifecycle state remains Available.
        /// </summary>
        Update
    }
}
