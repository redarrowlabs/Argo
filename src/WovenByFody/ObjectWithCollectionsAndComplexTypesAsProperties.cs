﻿using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{

    public class InnerObject
    {
        public string Value { get; set; }
    }

    [Model("test-property-collection")]
    public class ObjectWithCollectionsAndComplexTypesAsProperties
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// Tests storing a collection as a Property, rather than HasMany
        /// </summary>
        [Property]
        public ICollection<InnerObject> TestingDescriptions { get; set; }

        /// <summary>
        /// Tests storing a complext type as a Property, rather than HasOne
        /// </summary>
        [Property]
        public InnerObject TestingDescription { get; set; }
    }
}
