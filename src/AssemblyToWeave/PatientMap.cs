using RedArrow.Jsorm.Map;

namespace AssemblyToWeave
{
    public class PatientMap : ResourceMap<Patient>
    {
        public PatientMap()
        {
            Id(x => x.Id);

            Attribute(x => x.FirstName);
            Attribute(x => x.LastName);

            HasOne(x => x.Provider);
        }
    }
}