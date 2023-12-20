using System;

using CMS.Base;
using CMS.Core;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Module enabling cross origin resource sharing (CORS) with domains bound to current site.
    /// When request is sent from current site's domain, puts corresponding headers into response to enable CORS.
    /// </summary>
    internal static class KenticoCorsModule
    {
        private static CorsConfiguration configuration;


        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="corsConfiguration"/> is <c>null</c>.</exception>
        public static void Initialize(CorsConfiguration corsConfiguration)
        {
            configuration = corsConfiguration ?? throw new ArgumentNullException(nameof(corsConfiguration));

            RequestEvents.PreSendRequestHeaders.Execute += PreSendRequestHeaders_Execute;
            RequestEvents.Begin.Execute += Begin_Execute;
        }


        /// <summary>
        /// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET
        /// responds to a request. 
        /// </summary>
        private static void Begin_Execute(object sender, EventArgs e)
        {
            var httpContext = Service.Resolve<IHttpContextAccessor>().HttpContext;
            if (httpContext == null)
            {
                return;
            }

            HandlePreflightRequest(httpContext, new CorsContextFactory());
        }


        /// <summary>
        /// Ends request and stops execution in case of the preflight request. 
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="corsContextFactory">CorsContext Factory</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="httpContext"/> or <paramref name="corsContextFactory"/> is <c>null</c>.</exception>
        internal static void HandlePreflightRequest(IHttpContext httpContext, CorsContextFactory corsContextFactory)
        {
            httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            corsContextFactory = corsContextFactory ?? throw new ArgumentNullException(nameof(corsContextFactory));

            var corsRequestContext = corsContextFactory.GetCorsRequestContext(httpContext);
            if (corsRequestContext.IsPreflight && corsRequestContext.IsCurrentSiteOrigin)
            {
                var corsResponseContext = corsContextFactory.GetCorsResponseContext(httpContext);
                corsResponseContext.EndResponse();
            }
        }


        /// <summary>
        /// Executes just before ASP.NET sends HTTP headers to the client.
        /// </summary>
        private static void PreSendRequestHeaders_Execute(object sender, EventArgs e)
        {
            var httpContext = Service.Resolve<IHttpContextAccessor>().HttpContext;
            if (httpContext == null)
            {
                return;
            }

            SetHeaderToAllowCurrentSiteOrigin(httpContext, new CorsContextFactory());
        }


        /// <summary>
        /// Sets corresponding response headers taking into account the origin and preflight.
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="corsContextFactory">CorsContext Factory</param>
        /// <exception cref="ArgumentNullException">Throws when<paramref name="httpContext"/> or <paramref name="corsContextFactory"/> is <c>null</c>.</exception>
        internal static void SetHeaderToAllowCurrentSiteOrigin(IHttpContext httpContext, CorsContextFactory corsContextFactory)
        {
            httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            corsContextFactory = corsContextFactory ?? throw new ArgumentNullException(nameof(corsContextFactory));

            var corsRequestContext = corsContextFactory.GetCorsRequestContext(httpContext);
            if (corsRequestContext.IsCurrentSiteOrigin)
            {
                var corsResponseContext = corsContextFactory.GetCorsResponseContext(httpContext);
                corsResponseContext.AllowOrigin = corsRequestContext.Origin;
                corsResponseContext.AllowCredentials = configuration.AllowCredentials;
                
                if (corsRequestContext.IsPreflight)
                {
                    corsResponseContext.AllowMethods = configuration.AllowMethods;
                    corsResponseContext.AllowHeaders = configuration.AllowHeaders;
                    corsResponseContext.MaxAge = configuration.MaxAge;
                }
            }
        }
    }
}
