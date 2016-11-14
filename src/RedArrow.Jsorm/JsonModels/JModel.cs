using Newtonsoft.Json;

namespace RedArrow.Jsorm.JsonModels
{
    public abstract class JModel
    {
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}