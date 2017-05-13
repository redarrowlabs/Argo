using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class AllPropertyTypes
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public Guid GuidProperty { get; set; }

        [Property]
        public string StringProperty { get; set; }

        [Property]
        public int IntProperty { get; set; }

        [Property]
        public bool BoolProperty { get; set; }

        [Property]
        public Boolean BooleanProperty { get; set; }

        [Property]
        public uint UIntProperty { get; set; }

        [Property]
        public short ShortProperty { get; set; }

        [Property]
        public ushort UShortProperty { get; set; }

        [Property]
        public char CharProperty { get; set; }

        [Property]
        public byte ByteProperty { get; set; }

        [Property]
        public sbyte SByteProperty { get; set; }

        [Property]
        public long LongProperty { get; set; }

        [Property]
        public ulong ULongProperty { get; set; }

        [Property]
        public decimal DecimalProperty { get; set; }

        [Property]
        public double DoubleProperty { get; set; }

        [Property]
        public float FloatProperty { get; set; }

        [Property]
        public object ObjectProperty { get; set; }

        [Property]
        public DateTime DateTimeProperty { get; set; }

        [Property]
        public string[] StringArrayProperty { get; set; }
    }
}