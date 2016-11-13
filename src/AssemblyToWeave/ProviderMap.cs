﻿using RedArrow.Jsorm.Map;
using RedArrow.Jsorm.Map.Id.Generator;

namespace AssemblyToWeave
{
    public class ProviderMap : ResourceMap<Provider>
    {
        public ProviderMap()
        {
            Id(x => x.Id);

            Attribute(x => x.FirstName, "providerFirstName");
            Attribute(x => x.LastName, "providerLastName");

            HasMany(x => x.Patients);
        }
    }
}