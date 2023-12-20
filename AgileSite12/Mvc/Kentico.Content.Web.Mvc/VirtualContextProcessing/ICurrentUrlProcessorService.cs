using CMS;
using CMS.Core;

using Kentico.Content.Web.Mvc;

[assembly: RegisterImplementation(typeof(ICurrentUrlProcessorService), typeof(CurrentUrlProcessorService), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// A service managing the URL processor for the current HTTP request.
    /// </summary>
    internal interface ICurrentUrlProcessorService
    {
        /// <summary>
        /// Gets the processor that validated the request in the given HTTP context items.
        /// </summary>
        IVirtualContextUrlProcessor GetProcessor();


        /// <summary>
        /// Sets the given <paramref name="processor" /> into the given request items.
        /// </summary>
        /// <param name="processor">The URL processor to set.</param>
        void SetProcessor(IVirtualContextUrlProcessor processor);
    }
}
