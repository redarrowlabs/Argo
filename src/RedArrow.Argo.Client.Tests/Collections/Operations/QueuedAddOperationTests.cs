using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Collections.Operations;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Session.Patch;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Collections.Operations
{
    public class QueuedAddOperationTests
    {
        [Fact]
        public void ctor__Given_EmptyItemId__Then_ThrowEx()
        {
            Assert.Throws<ArgumentException>(() => new QueuedAddOperation("items", Guid.Empty, "item"));
        }

        [Fact]
        public void ctor__Given_NullRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedAddOperation(null, Guid.NewGuid(), "item"));
        }

        [Fact]
        public void ctor__Given_EmptyRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedAddOperation(string.Empty, Guid.NewGuid(), "item"));
        }

        [Fact]
        public void ctor__Given_WhitespaceRltnName__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedAddOperation(" ", Guid.NewGuid(), "item"));
        }

        [Fact]
        public void ctor__Given_NullItemType__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedAddOperation("items", Guid.NewGuid(), null));
        }

        [Fact]
        public void ctor__Given_EmptyItemType__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedAddOperation("items", Guid.NewGuid(), string.Empty));
        }

        [Fact]
        public void ctor__Given_WhitespaceItemType__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedAddOperation("items", Guid.NewGuid(), ""));
        }

        [Theory, AutoData]
        public void Patch__Given_ItemIdAndResourceType__Then_AddRelationshipLink
            (Guid parentId, Guid itemId)
        {
            var rltnName = "items";
            var itemType = "item";

            var patchContext = new PatchContext(new Resource { Id = parentId, Type = "parent" });

            var subject = new QueuedAddOperation(rltnName, itemId, itemType);
            
            subject.Patch(patchContext);

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(1, rltn.Count);
            Assert.Equal(itemId, rltn[0]["id"].Value<Guid>());
            Assert.Equal(itemType, rltn[0]["type"].Value<string>());
        }

        [Theory, AutoData]
        public void Patch__Given_ItemIdAndResourceType__When_Multi__Then_AddRelationshipLink
            (Guid parentId, Guid[] itemIds)
        {
            var rltnName = "items";
            var itemType = "item";

            var patchContext = new PatchContext(new Resource { Id = parentId, Type = "parent" });

            var subjects = itemIds.Select(x => new QueuedAddOperation(rltnName, x, itemType)).ToArray();

            foreach(var subject in subjects)
            {
                subject.Patch(patchContext);
            }

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(3, rltn.Count);

            for (var i = 0; i < itemIds.Length; i++)
            {
                Assert.Equal(itemIds[i], rltn[i]["id"].Value<Guid>());
                Assert.Equal(itemType, rltn[i]["type"].Value<string>());
            }
        }

        [Theory, AutoData]
        public void Patch__Given_ItemIdAndResourceType__When_AddItemTwice__Then_AddRelationshipLinkOnce
            (Guid parentId, Guid itemId)
        {
            var rltnName = "items";
            var itemType = "item";

            var patchContext = new PatchContext(new Resource { Id = parentId, Type = "parent" });

            var subjects = new[]
            {
                new QueuedAddOperation(rltnName, itemId, itemType),
                new QueuedAddOperation(rltnName, itemId, itemType) 
            };

            foreach (var subject in subjects)
            {
                subject.Patch(patchContext);
            }

            Assert.NotNull(patchContext.Resource?.Relationships);
            Assert.True(patchContext.Resource.Relationships.ContainsKey(rltnName));
            var rltn = patchContext.Resource.Relationships[rltnName]?.Data as JArray;
            Assert.NotNull(rltn);
            Assert.Equal(1, rltn.Count);
            Assert.Equal(itemId, rltn[0]["id"].Value<Guid>());
            Assert.Equal(itemType, rltn[0]["type"].Value<string>());
        }
    }
}
