using System;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// Base class for CMMN Elements.
    /// </summary>
    public interface CmmnElement
    {
        /// <summary>
        /// Titan Data Id
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Titan Data DataType
        /// </summary>
        //public string DataType { get; set; }
    }
}