using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Model
{
    public interface IMetaDecorated
    {
        JObject Meta { get; set; }
    }
}