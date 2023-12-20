using CMS.Base;
using CMS.Core;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// A service managing the URL processor for the current HTTP request.
    /// </summary>
    internal class CurrentUrlProcessorService : ICurrentUrlProcessorService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Name of the request item that stores the processor that validated the current HTTP request.
        /// </summary>
        internal const string REQUEST_PROCESSOR = "Kentico.VirtualContextUrlRewriter.RequestProcessor";


        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentUrlProcessorService"/> class.
        /// </summary>
        public CurrentUrlProcessorService()
            : this (Service.Resolve<IHttpContextAccessor>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentUrlProcessorService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        internal CurrentUrlProcessorService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// Gets the processor that validated the request in the given HTTP context items.
        /// </summary>
        public IVirtualContextUrlProcessor GetProcessor()
        {
            var httpContext = httpContextAccessor.HttpContext;
            return httpContext.Items[REQUEST_PROCESSOR] as IVirtualContextUrlProcessor;
        }


        /// <summary>
        /// Sets the given <paramref name="processor" /> into the given request items.
        /// </summary>
        /// <param name="processor">The URL processor to set.</param>
        public void SetProcessor(IVirtualContextUrlProcessor processor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            httpContext.Items[REQUEST_PROCESSOR] = processor;
        }
    }
}
