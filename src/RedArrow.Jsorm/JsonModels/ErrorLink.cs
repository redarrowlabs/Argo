using Newtonsoft.Json;

namespace RedArrow.Jsorm.JsonModels
{
    public class ErrorLink : JModel
    {
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public Link About { get; set; }
    }
}