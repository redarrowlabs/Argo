using RedArrow.Jsorm.Map;
using RedArrow.Jsorm.Sample.Models;

namespace RedArrow.Jsorm.Sample.Maps
{
    public class PatientMap : ResourceMap<Patient>
    {
        public PatientMap()
        {
            Id(x => x.Id);

            Attribute(x => x.FirstName);
            Attribute(x => x.LastName);

	        Attribute(x => x.Age);
	        Attribute(x => x.AccountBalance);

            HasOne(x => x.Provider);
        }
    }
}