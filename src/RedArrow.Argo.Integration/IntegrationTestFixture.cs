﻿using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using RedArrow.Argo.TestUtils.XUnitSink;
using Serilog;
using Xunit.Abstractions;

namespace RedArrow.Argo.Integration
{
    public class IntegrationTestFixture : IDisposable
    {
        public string Host = "http://titan-test.centralus.cloudapp.azure.com/api";
        public string AccessToken { get; }

		public IntegrationTestFixture()
		{
			using (var authClient = new HttpClient { BaseAddress = new Uri($"{Host}/security/") })
			{
				var reqBody = new StringContent(JsonConvert.SerializeObject(new
				{
					accountId = Guid.Parse("10000000-0000-0000-0000-000000000000"),
					applicationId = Guid.Parse("10000000-0000-0000-0000-200000000000"),
					email = "redarrowqa+account1user1@gmail.com",
					password = "Testing1234"
				}), Encoding.UTF8, "application/json");
				var response = authClient.PostAsync("authenticate", reqBody).GetAwaiter().GetResult();

				response.EnsureSuccessStatusCode();

				var responseContentStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				dynamic resContent = JsonConvert.DeserializeObject(responseContentStr);
				AccessToken = resContent.accessToken;
			}
		}

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
    }
}