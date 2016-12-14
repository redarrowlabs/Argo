﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.JsonModels
{
    internal abstract class BaseResourceRoot<TData> : JModel, IMetaDecorated
    {
        [JsonProperty("jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> JsonApi { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public TData Data { get; set; }

        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Error> Errors { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Resource> Included { get; set; }

        public static ResourceRootSingle FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ResourceRootSingle>(json);
        }
    }
}