using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Forms
{
    [Model(TITAN_TYPE)]
    public class FormDefinitionEntity
    {
        public const string TITAN_TYPE = "from-definition";

        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Title { get; set; }

        [Property]
        public string Name { get; set; }

        [Property]
        public IEnumerable<FormSectionEntity> Sections { get; set; }

        [Property]
        public Dictionary<string, ValidationEntity> FieldValidations { get; set; }
    }
}
