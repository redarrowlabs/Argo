using Newtonsoft.Json;

namespace RedArrow.Argo.Client.Model
{
    public abstract class JModel
    {
        internal JModel()
        {
        }

        public virtual string ToJson(JsonSerializerSettings jsonSettings)
        {
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.SerializeObject(this, Formatting.None, jsonSettings);
        }
    }
}