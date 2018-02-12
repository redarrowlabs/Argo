using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Json
{
    public class LooseJsonEqualityComparerTests
    {
        [Theory, AutoData]
        public void Equals__Given_IsEqual__Then_True(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.True(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_String__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.String = Guid.NewGuid().ToString();
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_Guid__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.Guid = Guid.NewGuid();
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_Decimal__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.Decimal = 999.999m;
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_DateTime__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.DateTime = DateTime.UtcNow;
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_SingleChild__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.Child.Guid = Guid.NewGuid();
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_OneOfChildren__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.OtherChildren.First().String = Guid.NewGuid().ToString();
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_RemovedChildren__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.OtherChildren.Remove(obj.OtherChildren.First());
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }

        [Theory, AutoData]
        public void Equals__Given_NotEqual_AddedChildren__Then_False(TestJsonObject obj)
        {
            // Assemble
            var token1 = JObject.FromObject(obj);
            obj.OtherChildren.Add(new TestJsonObjectChild());
            var token2 = JObject.Parse(JsonConvert.SerializeObject(obj));

            var subject = new LooseJsonEqualityComparer();

            // Act
            var result = subject.Equals(token1, token2);

            // Assert
            Assert.False(result);
            Assert.False(JToken.DeepEquals(token1, token2));
        }
    }
}
