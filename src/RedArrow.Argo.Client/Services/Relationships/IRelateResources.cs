using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Services.Relationships
{
    public interface IRelateResources
    {
        IDictionary<string, Relationship> Process(IDictionary<string, IEnumerable<Resource>> included);

        IDictionary<string, Relationship> Process(Type modelType, object model);
    }
}
