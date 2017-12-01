using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace RedArrow.Argo.Client.Exceptions
{
    public class ArgoResponseException : Exception
    {
        /// <summary>
        ///     Gets the content of a HTTP response message.
        /// </summary>
        public string ResponseContent { get; }
        /// <summary>
        ///     Gets the collection of HTTP response headers.
        /// </summary>
        public IDictionary<string, string> ResponseHeaders { get; }
        /// <summary>
        ///     Gets the reason phrase which typically is sent by servers together with
        ///     the status code.
        /// </summary>
        public string ResponseReasonPhrase { get; }
        /// <summary>
        ///     Gets the status code of the HTTP response.
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; }

        public ArgoResponseException(HttpResponseMessage response)
        {
            ResponseStatusCode = response.StatusCode;
            ResponseReasonPhrase = response.ReasonPhrase;
            ResponseHeaders = response.Headers?.ToDictionary(h => h.Key, h => string.Join(",", h.Value));

            try
            {
                ResponseContent = response.Content?.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                ResponseContent = "Could not read the response content: " + e.Message;
            }
        }

        /* Intentially not including the response body because it could include PHI
         * which should not be sent to SEQ */
        public override string Message => $"Response status code was unexpected: {(int)ResponseStatusCode}";
    }
}
