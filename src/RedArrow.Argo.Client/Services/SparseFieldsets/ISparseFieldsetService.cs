using System;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Flurl.Shared;

namespace RedArrow.Argo.Client.Services.SparseFieldsets
{
    public interface ISparseFieldsetService
    {
        Task<Url> BuildSparseFieldsetUrl(Type modelType, string url);
    }
}
