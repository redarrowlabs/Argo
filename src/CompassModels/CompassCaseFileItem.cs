using System;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Forms;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{
    [Model(TITAN_TYPE)]
    public class CompassCaseFileItem
    {
        public const string TITAN_TYPE = "compass-case-file-item";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public CmmnState State { get; set; }

        [Property]
        public Uri Uri { get; set; }

        [Property]
        public string ResourceType { get; set; }

        /*** Titan/Compass Extensions ***/

        [Property]
        public string TitanDataType { get; set; }

        [HasOne]
        public FormDefinitionEntity FormDefinition { get; set; }
    }
}