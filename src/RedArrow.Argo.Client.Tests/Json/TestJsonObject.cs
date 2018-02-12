using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Client.Tests.Json
{
    public class TestJsonObject
    {
        public string Null
        {
            get => null;
            set => value = null;
        }
        public string String { get; set; }
        public Guid Guid { get; set; }
        public int Int { get; set; }
        public Decimal Decimal { get; set; }
        public double Double { get; set; }
        public byte[] Bytes { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan TimeSpan { get; set; }

        public TestJsonObjectChild Child { get; set; }
        public ICollection<TestJsonObjectChild> OtherChildren { get; set; }
    }

    public class TestJsonObjectChild
    {
        public string Null {
            get => null;
            set => value = null;
        }
        public string String { get; set; }
        public Guid Guid { get; set; }
        public int Int { get; set; }
        public Decimal Decimal { get; set; }
        public double Double { get; set; }
        public byte[] Bytes { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
}
