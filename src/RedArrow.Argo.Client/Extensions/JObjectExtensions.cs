using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Extensions
{
    public static class JObjectExtensions
    {
        public static void SetMetaValue(this JObject meta, string metaName, object value)
        {
            var path = metaName.Split('.');
            var nextMeta = meta;
            // Navigate or build the object structure to the desired Meta
            for (int i = 0; i < path.Length - 1; i++)
            {
                var segment = path[i];
                if (nextMeta.TryGetValue(segment, out var subMeta))
                {
                    // Navigate to the next node
                    nextMeta = (JObject)subMeta;
                }
                else
                {
                    // Create a new node and navigate to it
                    var newMeta = new JObject();
                    nextMeta[segment] = newMeta;
                    nextMeta = newMeta;
                }
            }
            // Set the Meta at the terminal node
            var lastSegment = path[path.Length - 1];
            nextMeta[lastSegment] = JToken.FromObject(value);
        }
    }
}
