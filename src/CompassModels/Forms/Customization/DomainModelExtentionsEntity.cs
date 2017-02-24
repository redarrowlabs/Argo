using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Forms.Customization
{
    /// <summary>
    /// Allows a form to persist data that isn't a property on a specific domain model.
    ///
    /// That is, this allows a tenant to have limited control over customizing the domain model itself.
    /// </summary>
    [Model("domain-model-extension")]
    public class DomainModelExtentionsEntity
    {
        public const string TITAN_TYPE = "domain-model-extension";

        /// <summary>
        /// Note: for now, probably have to set this equal to the case file it's related to, since I can't look up
        /// an object by something other than it's id.
        /// </summary>
        [Id]
        public Guid Id { get; set; }

        [Property]
        public Guid CaseFileId { get; set; } // But eventually, I'd look it up like this.

        /// <summary>
        /// Note: case-file is the only top-level model related to a case file.
        /// </summary>
        [Property]
        public string ExtendsModelType { get; set; } = "case-file";

        [Property]
        public IList<CustomPropertyEntity> Extensions { get; set; }
    }
}
