using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedArrow.Argo.Client.JsonModels;

namespace RedArrow.Argo.Client.Services.Includes
{
    public interface IIncludeResourcesService
    {
        Task<IDictionary<string, IEnumerable<Resource>>> Process(Type modelType, object model);
    }
}
