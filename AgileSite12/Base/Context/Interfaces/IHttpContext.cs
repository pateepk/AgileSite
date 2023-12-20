using System.Collections;
using System.Security.Principal;

namespace CMS.Base
{
    /// <summary>
    /// Encapsulates all HTTP-specific information about an individual HTTP request.
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// Gets a key/value collection that can be used to organize and share data during an HTTP request.
        /// </summary>
        IDictionary Items { get; }


        /// <summary>
        /// Gets the <see cref="IRequest" /> object for the current HTTP request.
        /// </summary>
        IRequest Request { get; }


        /// <summary>
        /// Gets the <see cref="IResponse" /> object for the current HTTP response.
        /// </summary>
        IResponse Response { get; }


        /// <summary>
        /// Gets the <see cref="IServer" /> object that provides methods used in processing Web requests on server.
        /// </summary>
        IServer Server { get; }


        /// <summary>
        /// Gets the <see cref="ISession" /> object for the current HTTP request.
        /// </summary>
        ISession Session { get; }


        /// <summary>
        /// Gets the <see cref="IPrincipal" /> object for the current HTTP request.
        /// </summary>
        IPrincipal User { get; }


        /// <summary>
        /// Gets the <see cref="IHttpApplication" /> object for the current HTTP request.
        /// </summary>
        IHttpApplication ApplicationInstance { get; }
    }
}