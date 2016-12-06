﻿using System;
using System.Collections.Generic;
using RedArrow.Jsorm.Attributes;

namespace WovenByTest
{
    [Model("integration-test-provider")]
    public class Provider
    {
        [Id]
        public Guid Id { get; protected set; }

        [Property("first-name")]
        public string FirstName { get; set; }

        [Property("last-name")]
        public string LastName { get; set; }

        [HasMany]
        public IEnumerable<Patient> Patients { get; set; }
    }
}