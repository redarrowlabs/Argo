using Newtonsoft.Json;
using RedArrow.Argo.TestUtils.XUnitSink;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using Xunit.Abstractions;

namespace RedArrow.Argo.TestUtils
{
    public class IntegrationTestFixture : IDisposable
    {
        public static Lazy<string> AccessToken { get; } = new Lazy<string>(GetAccessToken);

        public static string Host { get; } = "https://test.redarrow.io/api";

        public void ConfigureLogging(ITestOutputHelper outputHelper)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.XunitTestOutput(outputHelper)
                .CreateLogger();
        }

        public void Dispose()
        {
        }

        private static string GetAccessToken()
        {
            using (var authClient = new HttpClient { BaseAddress = new Uri($"{Host}/security/") })
            {
                var reqBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    accountId = Guid.Parse("10000000-0000-0000-0000-000000000000"),
                    applicationId = Guid.Parse("10000000-0000-0000-0000-200000000000"),
                    email = "redarrowqa+adminboth@gmail.com",
                    password = "Testing1234"
                }), Encoding.UTF8, "application/json");
                var response = authClient.PostAsync("authenticate", reqBody).GetAwaiter().GetResult();

                response.EnsureSuccessStatusCode();

                var responseContentStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic resContent = JsonConvert.DeserializeObject(responseContentStr);
                return resContent.accessToken;
            }
        }
    }
}