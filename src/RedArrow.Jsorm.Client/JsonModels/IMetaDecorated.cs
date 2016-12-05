using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.JsonModels
{
    internal interface IMetaDecorated
    {
        IDictionary<string, JToken> Meta { get; set; }
    }
}