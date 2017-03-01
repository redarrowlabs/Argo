using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// The Definitions class is the outermost containing object for all CMMNElements.
    /// It defines the scope of visibility and the namespace for all contained elements.
    /// The interchange of CMMN files will always be through one or more Definitions.
    /// </summary>
    [Model("definitions")]
    public class Definitions : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the Definitions object.
        /// </summary>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// This attribute identifies the author of the CMMN model file.
        /// </summary>
        [Property]
        public string Author { get; set; }

        /// <summary>
        /// This attribute identifies the creation date of the CMMN model file.
        /// </summary>
        [Property]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// This attribute is used for the definition of CaseFileItem elements and makes those definitions available to use by elements within this Definitions.
        /// A Definition object that contains a Case MUST contain the CaseFileItemDefinitions of the CaseFileItems in the CaseFile of that Case.
        /// </summary>
        [HasMany]
        public ICollection<CaseFileItemDefinition> CaseFileItemDefinitions { get; set; }

        /// <summary>
        /// This attribute is used to define Cases and make them available for use by elements within this Definitions.
        /// </summary>
        [HasMany]
        public ICollection<Case> Cases { get; set; }

        /***** More CMMN Models available *****/
        /*
        /// <summary>
        /// This attribute identifies the namespace associated with the Definitions objects and follows the convention established by XML Schema.
        /// TODO (URI)
        /// </summary>
        public string TargetNamespace { get; set; }

        /// <summary>
        /// The expression language used for this Definitions object. The default is “http://www.w3.org/1999/XPath.”
        /// This value MAY be overridden on each individual Expression.The language MUST be specified in a URI format.
        /// TODO (URI)
        /// </summary>
        public string ExpressionLanguage { get; set; }

        /// <summary>
        /// This attribute identifies the tool that is exporting the CMMN model file.
        /// </summary>
        public string Exporter { get; set; }

        /// <summary>
        /// This attribute identifies the version of the tool that is exporting the CMMN model file.
        /// </summary>
        public string ExporterVersion { get; set; }
        */
    }
}
