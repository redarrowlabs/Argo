using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Session.Patch
{
    public class PatchContextTests
    {
        [Fact]
        public void ctor__Given_NullResource__Then_ThrowEx()
        {
            Assert.Throws<ArgumentNullException>(() => new PatchContext(null));
        }

        [Fact]
        public void ctor__Given_Resource__Then_InititializeTransientReferences()
        {
            var resource = new Resource();
            var subject = new PatchContext(resource);
            var result = subject.GetTransientReferences();

            Assert.Same(resource, subject.Resource);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetTransientReferences__Given_InitializedSubject__Then_ReturnReadonlyCopy()
        {
            var subject = new PatchContext(new Resource());

            var result = subject.GetTransientReferences();

            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Throws<NotSupportedException>(() => result.Add("test", Guid.NewGuid()));
        }

        [Fact]
        public void GetAttribute__Given_Resource__When_ResourceHasAttr__Then_ReturnAttrValue()
        {
            var attrName = "test";
            var attrValue = Guid.NewGuid();

            var resource = new Resource
            {
                Attributes = JObject.FromObject(new Dictionary<string, object> {{attrName, attrValue}})
            };

            var subject = new PatchContext(resource);

            var result = subject.GetAttribute<Guid>(attrName);

            Assert.Equal(attrValue, result);
        }

        [Fact]
        public void GetAttribute__Given_Resource__When_ResourceDoesntHaveAttr__Then_ReturnDefault()
        {
            var attrName = "test";

            var resource = new Resource();

            var subject = new PatchContext(resource);

            var result = subject.GetAttribute<Guid>(attrName);

            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public void GetAttribute__Given_Resource__When_ResourceDoesntHaveNullableAttr__Then_ReturnNull()
        {
            var attrName = "test";

            var resource = new Resource();

            var subject = new PatchContext(resource);

            var result = subject.GetAttribute<string>(attrName);

            Assert.Null(result);
        }

        [Theory, AutoData]
        public void SetAttriute__Given_Resource__When_ResourceHasAttrValue__Then_ReplaceValue
            (string attrValue, string newAttrValue)
        {
            var attrName = "test";

            var resource = new Resource
            {
                Attributes = JObject.FromObject(new Dictionary<string, object> {{attrName, attrValue}})
            };

            var subject = new PatchContext(resource);

            var initialResult = subject.GetAttribute<string>(attrName);
            Assert.Equal(attrValue, initialResult);

            subject.SetAttriute(attrName, newAttrValue);
            var newResult = subject.GetAttribute<string>(attrName);
            Assert.Equal(newAttrValue, newResult);

        }
    }
}
