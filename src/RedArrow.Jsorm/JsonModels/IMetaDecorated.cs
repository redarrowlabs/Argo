using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RedArrow.Jsorm.JsonModels
{
    internal interface IMetaDecorated
    {
        IDictionary<string, JToken> Meta { get; set; }
    }
}