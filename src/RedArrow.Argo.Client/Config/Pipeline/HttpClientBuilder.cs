using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Config.Pipeline
{
	public class HttpClientBuilder : IHttpClientBuilder
	{
		private ICollection<HandlerDefinition> HandlerDefs { get; } =
			new List<HandlerDefinition>();
		
		private HandlerDefinition FinalHandlerDef { get; set; }
		private ICollection<Action<HttpMessageHandler>> FinalHandlerConfigurators { get; } =
			new List<Action<HttpMessageHandler>>();

		public IHttpClientBuilder Use<THandler>(params object[] args)
			where THandler : DelegatingHandler
		{
			HandlerDefs.Add(new HandlerDefinition {HandlerType = typeof (THandler), CtorArgs = args});
			return this;
		}

		public IHttpClientBuilder UseRequestHandler<THandler>(params object[] args)
			where THandler : HttpMessageHandler
		{
			FinalHandlerDef = new HandlerDefinition {HandlerType = typeof(THandler), CtorArgs = args};
			return this;
		}

		public IHttpClientBuilder ConfigureRequestHandler(Action<HttpMessageHandler> configure)
		{
			FinalHandlerConfigurators.Add(configure);
			return this;
		}

		internal HttpMessageHandler Build()
		{
			var root = FinalHandlerDef != null
				? (HttpMessageHandler) Activator.CreateInstance(FinalHandlerDef.HandlerType, FinalHandlerDef.CtorArgs)
				: new HttpClientHandler();

			FinalHandlerConfigurators.Each(x => x(root));

			return HandlerDefs
				.Reverse()
				.Select(x => (DelegatingHandler) Activator.CreateInstance(x.HandlerType, x.CtorArgs))
				.Aggregate(root, (current, next) =>
				{
					next.InnerHandler = current;
					return next;
				});
		}

		private class HandlerDefinition
		{
			public Type HandlerType { get; set; }
			public object[] CtorArgs { get; set; }
		}
	}
}
