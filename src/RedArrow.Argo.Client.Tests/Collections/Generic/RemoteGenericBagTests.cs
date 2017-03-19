using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Collections.Generic;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Client.Tests.Extensions;
using RedArrow.Argo.Client.Tests.Session.Models;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Collections.Generic
{
	public class RemoteGenericBagTests
	{
		[Theory, AutoData]
		public void Ctor__Given_Items__When__Then_SetItemIds()
		{
			var uri = "http://www.test.com/";

			var items = Enumerable.Range(0, 3)
				.Select(i => new BasicModel())
				.ToArray();

			var modelRegistry = CreateModelRegistry(typeof(ComplexModel), typeof (BasicModel));

			var mockRequestBuilder = new Mock<IHttpRequestBuilder>();

			var cachedItems = new List<object>();
			var mockCacheProvider = new Mock<ICacheProvider>();
			mockCacheProvider
				.Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<BasicModel>()))
				.Callback<Guid, object>((id, m) =>
				{
					cachedItems.Add(m);
					mockCacheProvider
						.Setup(y => y.Retrieve<BasicModel>(id))
						.Returns((BasicModel) m);
				});
			mockCacheProvider
				.SetupGet(x => x.Items)
				.Returns(() => cachedItems);

			var expectedRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));
			var mockHandler = new MockRequestHandler();
			mockHandler.Setup(
				new Uri(uri),
				request =>
				{
					Assert.Same(expectedRequest, request);
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(JsonConvert.SerializeObject(new ResourceRootCollection
						{
							Data = Enumerable.Empty<Resource>()
						}))
					});
				});

			var session = CreateSession(
				mockHandler,
				mockRequestBuilder.Object,
				mockCacheProvider.Object,
				modelRegistry);

			var parent = session.ManageModel(new ComplexModel());

			mockRequestBuilder
				.Setup(x => x.GetRelated(parent.Id, modelRegistry.GetResourceType<ComplexModel>(), "test"))
				.Returns(expectedRequest);
			
			var subject = new RemoteGenericBag<BasicModel>(session, parent, "test", items);
			
			Assert.True(subject.Any());
			Assert.Equal(3, subject.Count());

			Assert.All(items, item =>
			{
				Assert.NotEqual(Guid.Empty, item.Id);
				Assert.Contains(item, subject);
			});
		}

		private static IModelRegistry CreateModelRegistry(params Type[] types)
		{
			return new ModelRegistry(types.Select(x => new ModelConfiguration(x)));
		}

		private static Client.Session.Session CreateSession(
			HttpMessageHandler mockHandler = null,
			IHttpRequestBuilder requestBuilder = null,
			ICacheProvider cacheProvider = null,
			IModelRegistry modelRegistry = null)
		{
			if (mockHandler == null)
			{
				mockHandler = new HttpClientHandler();
			}

			return new Client.Session.Session(
				() => new HttpClient(mockHandler),
				requestBuilder ?? Mock.Of<IHttpRequestBuilder>(),
				cacheProvider ?? Mock.Of<ICacheProvider>(),
				modelRegistry ?? Mock.Of<IModelRegistry>());
		}
	}
}
