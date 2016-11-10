using System;

namespace RedArrow.Jsorm.Core.Map.Id.Generator
{
    public interface IIdentifierGenerator
    {
        Guid Generate();
    }
}