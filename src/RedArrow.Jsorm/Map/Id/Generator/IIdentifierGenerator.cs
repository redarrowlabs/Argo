using System;

namespace RedArrow.Jsorm.Map.Id.Generator
{
    public interface IIdentifierGenerator
    {
        Guid Generate();
    }
}