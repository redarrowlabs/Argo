using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Model
{
    public interface IMetaDecorated
    {
        IDictionary<string, JToken> Meta { get; set; }
    }
}