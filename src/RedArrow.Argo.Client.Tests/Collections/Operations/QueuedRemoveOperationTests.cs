using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Collections.Operations;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Session.Patch;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Collections.Operations
{
    public class QueuedRemoveOperationTests
    {
        [Fact]
        public void ctor__Given_EmptyItemId__Then_ThrowEx()
        {
            Assert.Throws<ArgumentException>(() => new QueuedRemoveOperation("items", Guid.Empty));
        }

        [Fact]
        public void ctor__Given_NullRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedRemoveOperation(null, Guid.NewGuid()));
        }

        [Fact]
        public void ctor__Given_EmptyRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedRemoveOperation(string.Empty, Guid.NewGuid()));
        }

        [Fact]
        public void ctor__Given_WhitespaceRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedRemoveOperation(" ", Guid.NewGuid()));
        }

        [Theory, AutoData]
        public void Patch__Given_PatchContext__Then_RemoveRelationshipLink
            (Guid parentId, Guid itemId)
        {
            var rltnName = "items";

            var subject = new QueuedRemoveOperation(rltnName, itemId);

            var patchContext = new PatchContext(new Resource
            {
                Id = parentId,
                Type = "parent",
                Relationships = new Dictionary<string, Relationship>
                {
                    {
                        rltnName, new Relationship
                        {
                            Data = JToken.FromObject(new[]
                            {
                                new ResourceIdentifier {Id = itemId, Type = "item"}
                            })
                        }
                    }
                }
            });

            subject.Patch(patchContext);
            
            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(0, rltn.Count);
        }

        [Theory, AutoData]
        public void Patch__Given_PatchContext__When_MultipleLinks__Then_RemoveRelationshipLink
            (Guid parentId, Guid[] itemIds)
        {
            var rltnName = "items";
            var itemId = itemIds[1];
            var itemType = "item";

            var subject = new QueuedRemoveOperation(rltnName, itemId);

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

            subject.Patch(patchContext);

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(itemIds.Length - 1, rltn.Count);
            Assert.Equal(itemIds[0], rltn[0]["id"].Value<Guid>());
            Assert.Equal(itemType, rltn[0]["type"].Value<string>());
            Assert.Equal(itemIds[2], rltn[1]["id"].Value<Guid>());
            Assert.Equal(itemType, rltn[1]["type"].Value<string>());
        }

        [Theory, AutoData]
        public void Patch__Given_PatchContext__When_LinkExistsMultipleTimes__Then_RemoveRelationshipLink
            (Guid parentId, Guid[] itemIds)
        {
            var rltnName = "items";
            var itemId = itemIds[1];
            var itemType = "item";

            var subject = new QueuedRemoveOperation(rltnName, itemId);

            var patchContext = new PatchContext(new Resource
            {
                Id = parentId,
                Type = "parent",
                Relationships = new Dictionary<string, Relationship>
                {
                    {
                        rltnName, new Relationship
                        {
                            Data = JToken.FromObject(itemIds.Select(x => new ResourceIdentifier {Id = itemId, Type = itemType}))
                        }
                    }
                }
            });

            subject.Patch(patchContext);

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(0, rltn.Count);
        }

        [Theory, AutoData]
        public void Patch__Given_PatchContext__When_LinkNotFound__Then_DoNothing
            (Guid parentId, Guid[] itemIds)
        {
            var rltnName = "items";
            var itemType = "item";

            var subject = new QueuedRemoveOperation(rltnName, Guid.NewGuid());

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

            subject.Patch(patchContext);

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(itemIds.Length, rltn.Count);

            for (var i = 0; i < itemIds.Length; i++)
            {
                Assert.Equal(itemIds[i], rltn[i]["id"].Value<Guid>());
                Assert.Equal(itemType, rltn[i]["type"].Value<string>());
            }
        }
    }
}
