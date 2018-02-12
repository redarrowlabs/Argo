using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Json
{
    public class JsonDiff
    {
        private IEqualityComparer<JToken> Comparer { get; }
        public JsonDiff()
        {
            Comparer = new LooseJsonEqualityComparer();
        }
        
        /// <summary>
        /// Using the update as a template, remove any properties equivalent to the original
        /// </summary>
        /// <param name="original"></param>
        /// <param name="update"></param>
        /// <returns>The smallest possible patch</returns>
        public JToken ReducePatch(JToken original, JToken update)
        {
            /* Trim any equivalent values from the patch.
             * We're NOT removing/nulling values that exist in the original resource,
             * but not in the new resource.  This trim is coarse-grained to avoid
             * recursion and provide the safest update (in regard to array updates). */

            // if not exists in original and null in model, then remove from patch
            // if original same as model including nulls, then remove from patch
            // if exists in original and null in model, then include in patch
            // if original is different than model, then include in patch
            if (original == null || original.Type == JTokenType.Null)
            {
                // Respond with the full update, or remove from patch
                return update?.Type == JTokenType.Null ? null : update;
            }
            if (update == null || update.Type == JTokenType.Null)
            {
                // Original was not null, so we're nulling the value in the patch
                return JValue.CreateNull();
            }
            if (update.Type == JTokenType.Property)
            {
                // Do not pass properties, just JObjects
                throw new ArgumentException($"Cannot diff type {update.Type}");
            }
            if (update.Type == JTokenType.Object)
            {
                // Create a new object that only contains the differences between the original and update
                var originalObj = (JObject) original;
                var patchObject = new JObject();
                foreach (var updatedProp in ((JObject)update).Properties())
                {
                    originalObj.TryGetValue(updatedProp.Name, out var originalPropValue);
                    var diff = ReducePatch(originalPropValue, updatedProp.Value);
                    if (diff != null)
                    {
                        patchObject.Add(updatedProp.Name, diff);
                    }
                }
                return patchObject.HasValues
                    ? patchObject
                    : null;
            }

            // Arrays and any JValues
            return Comparer.Equals(original, update)
                ? null
                : update;
        }
    }
}
