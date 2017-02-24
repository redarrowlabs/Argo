using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// A CaseFile consists of CaseFileItems. A CaseFileItem may represent a piece of information of any nature, ranging from unstructured to structured,
    /// and from simple to complex, which information can be defined based on any information modeling “language.”
    /// A CaseFileItem can be anything from a folder or document stored in CMIS, an entire folder hierarchy referring or containing other CaseFileItems,
    /// or simply an XML document with a given structure.
    /// The structure, as well as the “language” (or format) to define the structure, is defined by the associated CaseFileItemDefinition.
    /// This may include the definition of properties(“metadata”) of a CaseFileItem.
    /// If the internal content of the CaseFileItem is known, an XML Schema, describing the CaseFileItem, may be imported.
    /// 
    /// CaseFileItems can be organized into arbitrary hierarchies of CaseFileItems either by containment or by reference.
    /// </summary>
    [Model("case-file-item")]
    public class CaseFileItem : CmmnElementInstance
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public CmmnState State { get; set; }

        /// <summary>
        /// The name of the CaseFileItem
        /// </summary>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// A reference to the CaseFileItemDefinition. Every CaseFileItem MUST be associated to exactly one CaseFileItemDefinition.
        /// </summary>
        [HasOne]
        public CaseFileItemDefinition CaseFileItemDefinition { get; set; }

        /// <summary>
        /// The multiplicity specifies the number of potential instances of this CaseFileItem in the context of a particular Case instance.
        /// </summary>
        [Property]
        public Multiplicity Multiplicity { get; set; }

        /// <summary>
        /// Zero or more children of the CaseFileItem. The children objects are contained by the CaseFileItem.
        /// CANNOT be recursive.
        /// </summary>
        [HasOne]
        public Children Children { get; set; }

        /// <summary>
        /// Zero or one parent of the CaseFileItem.
        /// </summary>
        [HasOne]
        public CaseFileItem Parent { get; set; }

        /// <summary>
        /// Zero or more references to target CaseFileItems.
        /// CANNOT be recursive.
        /// </summary>
        [HasMany]
        public ICollection<CaseFileItem> TargetRefs { get; set; }

        /// <summary>
        /// Zero or one source CaseFileItem. For reference hierarchies of a CaseFileItem, souceRef refers to the source of the CaseFileItem.
        /// If CaseFileItem b is a targetRef of CaseFileItem a, then sourceRef of CaseFileItem b is a.
        /// </summary>
        [HasMany]
        public ICollection<CaseFileItem> SourceRefs { get; set; }

        /***** Compass Implementation *****/
        [Property]
        public Guid TitanResourceId { get; set; }
        [Property]
        public string TitanResourceDataType { get; set; }
    }

    /// <summary>
    /// CaseFileItemDefinition elements specify the structure of a CaseFileItem.
    /// TODO NOTE: We break away from CMMN here in lieu of XML implementation
    /// </summary>
    [Model("case-file-item-definition")]
    public class CaseFileItemDefinition : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        /// <summary>
        /// Definition type of the CaseFileItem.
        /// (Folder, Document, Relationship, XML Schema Element, XML Schema Complex Type, XML Schema Simple Type, Unknown, Unspecified)
        /// TODO NOTE: We break away from CMMN here in lieu of XML implementation (URI)
        /// </summary>
        [Property]
        public string DefinitionType { get; set; }

        /// <summary>
        /// Concrete structure of the definition entity.
        /// The View Form definition.
        /// TODO NOTE: We break away from CMMN here in lieu of XML implementation (QName)
        /// </summary>
        [Property]
        public string Structure { get; set; }

        /// <summary>
        /// Zero or more Property objects
        /// </summary>
        [HasMany]
        public ICollection<Property> Properties { get; set; }
    }

    /// <summary>
    /// Property MAY complement CaseFileItemDefinitions.
    /// </summary>
    [Model("case-file-item-definition-property")]
    public class Property : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the attribute
        /// </summary>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// The type of the attribute.
        /// (string, boolean, integer, float, double, duration, dateTime, time, date, gYearMonth, gYear, gMonthDay, gDay, gMonth, hexBinary, base64Binary, anyURI, QName, decimal, Unspecified)
        /// TODO NOTE: We break away from CMMN here in lieu of XML implementation (URI)
        /// </summary>
        [Property]
        public string Type { get; set; }
    }

    public class Children : CmmnElement
    {
        [Id]
        public Guid Id { get; set; }

        [HasMany]
        public ICollection<CaseFileItem> CaseFileItems { get; set; }
    }
}
