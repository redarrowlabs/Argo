using System;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Json;
using System.Linq;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Json
{
    public class JsonDiffTests
    {
        [Theory, AutoData]
        public void ReducePatch__Given_AllEqual__Then_Null(TestJsonObject obj)
        {
            // Assemble
            var original = JObject.FromObject(obj);
            var update = JObject.FromObject(obj);
            var subject = new JsonDiff();

            // Act
            var result = subject.ReducePatch(original, update);

            // Assert
            Assert.Null(result);
        }

        [Theory, AutoData]
        public void ReducePatch__Given_SinglePropertyNotEqual__Then_PatchProperty(TestJsonObject obj)
        {
            // Assemble
            var original = JObject.FromObject(obj);
            obj.String = "Updated Value";
            var update = JObject.FromObject(obj);
            var subject = new JsonDiff();
            
            // Act
            var result = (JObject)subject.ReducePatch(original, update);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(obj.String, result["String"].Value<string>());
            // Other properties should not need patching
            Assert.Single(result);
        }

        [Theory, AutoData]
        public void ReducePatch__Given_ChildPropertyNotEqual__Then_PatchProperty(TestJsonObject obj)
        {
            // Assemble
            var original = JObject.FromObject(obj);
            obj.Child.String = "Updated Value";
            var update = JObject.FromObject(obj);
            var subject = new JsonDiff();

            // Act
            var result = (JObject) subject.ReducePatch(original, update);

            // Assert
            Assert.NotNull(result);
            // Values should be targeted
            Assert.Equal(obj.Child.String, result[nameof(obj.Child)]["String"].Value<string>());
            // Other properties should not need patching
            Assert.Single(result);
        }

        [Theory, AutoData]
        public void ReducePatch__Given_ArrayPropertyNotEqual__Then_PatchEntireArray(TestJsonObject obj)
        {
            // Assemble
            var original = JObject.FromObject(obj);
            obj.OtherChildren.First().String = "Updated Value";
            var update = JObject.FromObject(obj);
            var subject = new JsonDiff();

            // Act
            var result = (JObject)subject.ReducePatch(original, update);

            // Assert
            Assert.NotNull(result);

            // All values should be included in array patch
            Assert.Equal(obj.OtherChildren.Count, result[nameof(obj.OtherChildren)].Count());
            var otherChildObj = obj.OtherChildren.First();
            var otherChildJson = result[nameof(obj.OtherChildren)][0];
            Assert.Equal(otherChildObj.String, otherChildJson["String"].Value<string>());
            Assert.Equal(otherChildObj.Guid, otherChildJson["Guid"].Value<Guid>());
            Assert.Equal(otherChildObj.Int, otherChildJson["Int"].Value<int>());
            Assert.Equal(otherChildObj.Decimal, otherChildJson["Decimal"].Value<decimal>());
            Assert.Equal(otherChildObj.Double, otherChildJson["Double"].Value<double>());
            Assert.Equal(otherChildObj.DateTime, otherChildJson["DateTime"].Value<DateTime>());
            // Other properties should not need patching
            Assert.Single(result);
        }
    }
}
