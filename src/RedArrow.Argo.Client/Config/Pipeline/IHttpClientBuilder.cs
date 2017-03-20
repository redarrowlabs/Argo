using System;
using System.Net.Http;

namespace RedArrow.Argo.Client.Config.Pipeline
{
	public interface IHttpClientBuilder
	{
		/// <summary>
		/// Defines a type of <see cref="DelegatingHandler"/> to be added
		/// to the pipeline.  Optionally, provide any constructor arguments required to activate
		/// an instance of <see cref="THandler"/>.  Handlers are executed in the same order they are
		/// added o the IHttpClientBuilder pipeline.
		/// </summary>
		/// <typeparam name="THandler">The type of <see cref="DelegatingHandler"/></typeparam>
		/// <param name="args">Any constructor arguments required to activate the handler</param>
		/// <returns>The current <see cref="IHttpClientBuilder"/>, for chaining purposes</returns>
		IHttpClientBuilder Use<THandler>(params object[] args)
			where THandler : DelegatingHandler;
		
		/// <summary>
		/// Defines the "final" or "last" <see cref="HttpMessageHandler"/> in the pipeline.
		/// If the last handler in the pipeline is never defined, a <see cref="HttpClientHandler"/>
		/// is used by default.
		/// </summary>
		/// <typeparam name="THandler">The type of <see cref="HttpMessageHandler"/> that will
		/// interact with the server</typeparam>
		/// <param name="args">Any constructor arguments required to activate the handler</param>
		/// <returns>The current <see cref="IHttpClientBuilder"/>, for chaining purposes</returns>
		IHttpClientBuilder UseFinal<THandler>(params object[] args)
			where THandler : HttpMessageHandler;
		
		/// <summary>
		/// Configures the final <see cref="HttpMessageHandler"/> in the pipeline
		/// with the given action.
		/// </summary>
		/// <param name="configure">An action used to configure the final <see cref="HttpMessageHandler"/>
		/// when the pipeline is built</param>
		/// <returns>The current <see cref="IHttpClientBuilder"/>, for chaining purposes</returns>
		IHttpClientBuilder ConfigureFinal(Action<HttpMessageHandler> configure);
	}
}
