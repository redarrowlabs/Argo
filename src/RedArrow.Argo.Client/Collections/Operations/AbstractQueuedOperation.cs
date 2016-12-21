using System;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public abstract class AbstractQueuedOperation : IQueuedOperation
    {
        protected string RltnName { get; }

        protected AbstractQueuedOperation(string rltnName)
        {
            RltnName = rltnName;
        }

        public abstract void Patch(PatchContext patchContext);

        protected JArray GetRelationshipData(PatchContext patchContext)
        {
            var rltn = patchContext.GetRelationship(RltnName);
            if (rltn == null)
            {
                rltn = new Relationship();
                patchContext.SetRelationship(RltnName, rltn);
            }

            var jData = rltn.Data as JArray;
            if (jData == null)
            {
                jData = new JArray();
                rltn.Data = jData;
            }
            return jData;
        }
    }
}
