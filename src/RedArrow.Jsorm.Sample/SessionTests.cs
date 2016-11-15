using AssemblyToWeave;
using RedArrow.Jsorm.Config;
using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace RedArrow.Jsorm.Sample
{
    public class SessionTests
    {
        [Fact, Category("Integration")]
        public async Task GetPatient()
        {
            var token = "13f131e74cf3e1c579b38011b091c3fc";

            var sessionFactory = Fluently.Configure()
                .Remote()
                    .Configure(x => x.BaseAddress = new Uri("http://titan-test.centralus.cloudapp.azure.com/resources/write/api"))
                    .Configure(x => x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token))
                .Models(x => x.AddFromAssemblyOf<Patient>())
                .BuildSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                var test = new Patient();
                var patient = new Patient
                {
                    FirstName = "Brian",
                    LastName = "Engen"
                };

                patient = await session.Create(patient);

                Assert.NotEqual(Guid.Empty, patient.Id);
            }
        }
    }
}