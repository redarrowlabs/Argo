using Newtonsoft.Json;

namespace RedArrow.Argo.Client.JsonModels
{
    internal abstract class JModel
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