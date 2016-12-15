using Newtonsoft.Json;

namespace RedArrow.Jsorm.Client.JsonModels
{
    public class ErrorLink : JModel
    {
        internal ErrorLink() { }

        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public Link About { get; set; }
    }
}