using System.Collections.Specialized;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Defines implementation capable of preparing web response on the server.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the output that is returned to the client.
        /// </summary>
        int StatusCode { get; set; }


        /// <summary>
        /// Gets or sets the encoding for the header of the current response.
        /// </summary>
        Encoding HeaderEncoding { get; }


        /// <summary>
        /// Gets the response cookie collection.
        /// </summary>
        IHttpCookieCollection Cookies { get; }


        /// <summary>
        /// Gets the collection of response headers.
        /// </summary>
        NameValueCollection Headers { get; }


        /// <summary>
        /// Gets the caching policy (such as expiration time, privacy settings, and vary clauses) of the current Web page.
        /// </summary>
        ICache Cache { get; }


        /// <summary>
        /// Gets or sets the value of the HTTP <see langword="Location" /> header.
        /// </summary>
        string RedirectLocation { get; set; }


        /// <summary>
        /// Sends all currently buffered output to the client, stops execution of the requested process.
        /// </summary>
        void End();


        /// <summary>
        /// Adds an HTTP header to the current response.
        /// </summary>
        /// <param name="name">The name of the HTTP header to add <paramref name="value" /> to.</param>
        /// <param name="value">The string to add to the header.</param>
        void AddHeader(string name, string value);


        /// <summary>
        /// Updates an existing cookie in the cookie collection.
        /// </summary>
        /// <param name="cookie">The cookie in the collection to be updated.</param>
        void SetCookie(IHttpCookie cookie);


        /// <summary>
        /// Clears all headers and content output from the current response.
        /// </summary>
        void Clear();


        /// <summary>
        /// Writes the specified string to the HTTP response output stream.
        /// </summary>
        /// <param name="content">The string to write to the HTTP output stream.</param>
        void Write(string content);


        /// <summary>
        /// Performs a permanent redirect from the requested URL to the specified URL, and provides the option to complete the response.
        /// </summary>
        /// <param name="url">The location to which the request is redirected.</param>
        /// <param name="endResponse"><see langword="true" /> to terminate the response.</param>
        void RedirectPermanent(string url, bool endResponse);


        /// <summary>
        /// Redirects a request to the specified URL and specifies whether execution of the current process should terminate.
        /// </summary>
        /// <param name="url">The target location.</param>
        /// <param name="endResponse"><see langword="true" /> to terminate the response.</param>
        void Redirect(string url, bool endResponse);
    }
}
