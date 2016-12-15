using Newtonsoft.Json;

namespace RedArrow.Jsorm.Client.JsonModels
{
    public class ErrorSource : JModel
    {
        internal ErrorSource() { }

        [JsonProperty("pointer", NullValueHandling = NullValueHandling.Ignore)]
        public string Pointer { get; set; }

        [JsonProperty("parameter", NullValueHandling = NullValueHandling.Ignore)]
        public string Parameter { get; set; }
    }
}