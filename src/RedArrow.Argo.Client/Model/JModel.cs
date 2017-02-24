using Newtonsoft.Json;

namespace RedArrow.Argo.Client.Model
{
    public abstract class JModel
    {
        internal JModel() { }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}