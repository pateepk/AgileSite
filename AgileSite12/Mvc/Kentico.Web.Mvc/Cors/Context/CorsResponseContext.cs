using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides access to CORS-related informations of the response.
    /// </summary>
    internal sealed class CorsResponseContext : ICorsResponseContext
    {
        private readonly IHttpContext httpContext;


        /// <summary>
        /// Gets HTTP-response information. 
        /// </summary>
        private IResponse HttpResponse => httpContext.Response;


        /// <summary>
        /// Gets or sets the Access-Control-Allow-Origin response header. 
        /// </summary>
        public string AllowOrigin
        {
            get
            {
                return HttpResponse.Headers[CorsConstants.ACCESS_CONTROL_ALLOW_ORIGIN];
            }
            set
            {
                SetOrReplaceHeader(CorsConstants.ACCESS_CONTROL_ALLOW_ORIGIN, value);
            }
        }


        /// <summary>
        /// Gets or sets the Access-Control-Allow-Methods response header values.
        /// </summary>
        public IEnumerable<string> AllowMethods
        {
            get
            {
                return HttpResponse.Headers[CorsConstants.ACCESS_CONTROL_ALLOW_METHODS]
                    ?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                SetOrReplaceHeader(CorsConstants.ACCESS_CONTROL_ALLOW_METHODS,
                    String.Join(",", value ?? Enumerable.Empty<string>()));
            }
        }


        /// <summary>
        /// Gets or sets the Access-Control-Allow-Headers response header values.
        /// </summary>
        public IEnumerable<string> AllowHeaders
        {
            get
            {
                return HttpResponse.Headers[CorsConstants.ACCESS_CONTROL_ALLOW_HEADERS]
                    ?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                SetOrReplaceHeader(CorsConstants.ACCESS_CONTROL_ALLOW_HEADERS,
                    String.Join(",", value ?? Enumerable.Empty<string>()));
            }
        }


        /// <summary>
        /// Gets or sets the Access-Control-Allow-Credentials response header.
        /// </summary>
        public bool AllowCredentials
        {
            get
            {
                return ValidationHelper.GetBoolean(HttpResponse.Headers[CorsConstants.ACCESS_CONTROL_ALLOW_CREDENTIALS], false);
            }
            set
            {
                if (value)
                {
                    SetOrReplaceHeader(CorsConstants.ACCESS_CONTROL_ALLOW_CREDENTIALS, value);
                }
            }
        }


        /// <summary>
        /// Gets or sets the Access-Control-Max-Age response header.
        /// </summary>
        public int MaxAge
        {
            get
            {
                return ValidationHelper.GetInteger(HttpResponse.Headers[CorsConstants.ACCESS_CONTROL_MAX_AGE], 0);
            }
            set
            {
                SetOrReplaceHeader(CorsConstants.ACCESS_CONTROL_MAX_AGE, value);
            }
        }


        /// <summary>
        /// Creates new instance of <see cref="CorsResponseContext" />.
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="httpContext"/> is <c>null</c>.</exception>
        public CorsResponseContext(IHttpContext httpContext)
        {
            httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            this.httpContext = httpContext;
        }


        /// <summary>
        /// Sets OK (200) status code, stops execution of the request
        /// and sends all currently buffered output to the client.
        /// </summary>
        public void EndResponse()
        {
            HttpResponse.StatusCode = 200;
            httpContext.ApplicationInstance.CompleteRequest();
        }


        /// <summary>
        /// Sets or replace response header determined by given <paramref name="headerName"/>.
        /// </summary>
        /// <param name="headerName">Header name</param>
        /// <param name="headerValue">Header value</param>
        private void SetOrReplaceHeader(string headerName, object headerValue)
        {
            var stringHeaderValue = headerValue?.ToString();
            if (String.IsNullOrEmpty(stringHeaderValue))
            {
                return;
            }

            if (HttpResponse.Headers[headerName] == null)
            {
                // AppendHeader has some additional logic, so using this instead adding header directly using indexer
                HttpResponse.AddHeader(headerName, stringHeaderValue);
            }
            else
            {
                // Header is already present so we can directly overwrite it
                HttpResponse.Headers[headerName] = stringHeaderValue;
            }
        }
    }
}
