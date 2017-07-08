using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class IdPropertyNotNamedId
    {
        [Id]
        public Guid NotNamedId { get; set; }
    }
}
