using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Services.SparseFieldsets
{
    public class SparseFieldsetService : ISparseFieldsetService
    {
        private IModelRegistry ModelRegistry { get; }

        public SparseFieldsetService(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
        }

        public Task<Url> BuildSparseFieldsetUrl(Type modelType, string url)
        {
            var fieldsetMap = new Dictionary<string, List<string>>();
            ExtractFieldsetsType(
                modelType,
                modelType.Name.ToLower(),
                modelType.Name.ToLower(),
                new List<string>(),
                fieldsetMap,
                true);

            foreach (var fieldset in fieldsetMap)
            {
                url = url.SetQueryParam($"fields[{fieldset.Key}]", string.Join(",", fieldset.Value));
            }

            return Task.FromResult((Url)url);
        }

        private void ExtractFieldsetsType(Type modelType, string rltnName, string currentLevel, List<string> nodeMap, IDictionary<string, List<string>> fieldsetMap, bool topLevel = false)
        {
            if (fieldsetMap == null)
            {
                fieldsetMap = new Dictionary<string, List<string>>();
            }

            if (!fieldsetMap.ContainsKey(rltnName))
            {
                fieldsetMap.Add(rltnName, new List<string>());
            }

            nodeMap.Add(currentLevel);

            var attributes = ModelRegistry.GetAttributeConfigs(modelType)
                .Select(x => x.AttributeName)
                .ToList();
            if (attributes != null && attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    var attributeList = fieldsetMap[rltnName];
                    if (!attributeList.Contains(attribute))
                    {
                        var attributeInnerList = attributeList;
                        attributeInnerList.Add(attribute);
                        fieldsetMap[rltnName] = attributeInnerList;
                    }
                }
            }

            var collectionConfiguration = ModelRegistry.GetHasManyConfigs(modelType)
                .Select(x => x)
                .ToList();
            if (collectionConfiguration != null && collectionConfiguration.Any())
            {
                foreach (var hasManyConfiguration in collectionConfiguration)
                {
                    if (!nodeMap.Select(x => x.Split('.'))
                            .Any(x => x.Contains(hasManyConfiguration.HasManyType.Name.ToLower()))
                        && hasManyConfiguration.Eager)
                    {
                        ExtractFieldsetsType(
                            hasManyConfiguration.HasManyType,
                            hasManyConfiguration.RelationshipName,
                            $"{currentLevel}.{hasManyConfiguration.HasManyType.Name.ToLower()}",
                            nodeMap,
                            fieldsetMap);
                    }
                }
            }

            var singleConfiguration = ModelRegistry.GetHasOneConfigs(modelType)
                .Select(x => x)
                .ToList();
            if (singleConfiguration != null && singleConfiguration.Any())
            {
                foreach (var hasOneConfiguration in singleConfiguration)
                {
                    if (!nodeMap.Select(x => x.Split('.'))
                            .Any(x => x.Contains(hasOneConfiguration.HasOneType.Name.ToLower()))
                        && hasOneConfiguration.Eager)
                    {
                        ExtractFieldsetsType(
                            hasOneConfiguration.HasOneType,
                            hasOneConfiguration.RelationshipName,
                            $"{currentLevel}.{hasOneConfiguration.HasOneType.Name.ToLower()}",
                            nodeMap,
                            fieldsetMap);
                    }
                }
            }
        }
    }
}
