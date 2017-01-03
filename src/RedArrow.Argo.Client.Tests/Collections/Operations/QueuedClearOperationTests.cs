using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Collections.Operations;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Collections.Operations
{
    public class QueuedClearOperationTests
    {
        [Fact]
        public void ctor__Given_NullRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedClearOperation(null));
        }

        [Fact]
        public void ctor__Given_EmptyRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedClearOperation(string.Empty));
        }

        [Fact]
        public void ctor__Given_WhitespaceRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedClearOperation(" "));
        }

        [Theory, AutoData]
        public void Patch__Given_PatchContext__Then_RemoveAll
            (Guid parentId, Guid[] itemIds)
        {
            var rltnName = "items";
            var itemType = "item";

            var patchContext = new PatchContext(new Resource
            {
                Id = parentId,
                Type = "parent",
                Relationships = new Dictionary<string, Relationship>
                {
                    {
                        rltnName, new Relationship
                        {
                            Data = JToken.FromObject(itemIds.Select(x => new ResourceIdentifier {Id = x, Type = itemType}))
                        }
                    }
                }
            });

            var subject = new QueuedClearOperation(rltnName);

            subject.Patch(patchContext);

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(0, rltn.Count);
        }
    }
}
