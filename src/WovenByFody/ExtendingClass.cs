using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class ExtendingClass : BaseClass
    {
        [Property]
        public string FirstName { get; set; }
    }
}
