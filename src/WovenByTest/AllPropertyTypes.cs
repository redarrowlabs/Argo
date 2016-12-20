﻿using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace WovenByTest
{
    [Model]
    public class AllPropertyTypes<T, TRef, TElmnt>
    {
        [Id]
        public Guid Id { get; }

        [Property]
        public Guid GuidProperty { get; set; }

        [Property]
        public string StringProperty { get; set; }

        [Property]
        public int IntProperty { get; set; }

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
        public T GenericProperty { get; set; }

        [HasOne]
        public TRef GenericReferenceProperty { get; set; }

        [HasOne]
        public object ReferenceProperty { get; set; }

        [HasMany]
        public IEnumerable<object> EnumerableProperty { get; set; }

        //[HasMany]
        //public IEnumerable<TElmnt> GenericEnumerableProperty { get; set; }

        [HasMany]
        public ICollection<object> CollectionProperty { get; set; }

        //[HasMany]
        //public ICollection<TElmnt> GenericCollectionProperty { get; set; }
    }
}
