using Newtonsoft.Json;

namespace RedArrow.Argo.Client.JsonModels
{
    internal class ErrorLink : JModel
    {
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public Link About { get; set; }
    }
}