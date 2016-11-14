using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AssemblyToWeave;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Jsorm.Config;
using Xunit;

namespace RedArrow.Jsorm.Sample
{
	public class SessionTests
	{
		public async Task GetPatient
			(Guid patientId)
		{
			var sessionFactory = Fluently.Configure()
				.Host(ConfigureHttpClient)
				.Mappings(x => x.ResourceMaps.AddFromAssemblyOf<PatientMap>())
				.BuildSessionFactory();

			using (var session = sessionFactory.CreateSession())
			{
				var patient = await session.GetModel<Patient>(patientId);
			}
		}

		private void ConfigureHttpClient(HttpClient client)
		{
			
		}
	}
}
