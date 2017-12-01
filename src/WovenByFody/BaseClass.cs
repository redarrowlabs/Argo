using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    public abstract class BaseClass
    {
        [Id]
        public Guid Id { get; set; }
    }
}
