using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace RedArrow.Jsorm.Sample
{
    public class IntegrationTestFixture : IDisposable
    {
        public string AccessToken { get; }

        public IntegrationTestFixture()
        {
            using (var authClient = new HttpClient { BaseAddress = new Uri("http://localhost:8081/api/") })
            {
                var reqBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    accountId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    applicationId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    email = "bengen@redarrowlabs.com",
                    password = "Bananaphone1"
                }), Encoding.UTF8, "application/json");
                var response = authClient.PostAsync("auth/login", reqBody).GetAwaiter().GetResult();

                response.EnsureSuccessStatusCode();

                var responseContentStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic resContent = JsonConvert.DeserializeObject(responseContentStr);
                AccessToken = resContent.token;
            }
        }

        public void Dispose()
        {
        }
    }
}