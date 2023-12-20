using System;
using System.Collections.Specialized;

namespace CMS.Base
{
    /// <summary>
    /// Defines implementation capable of reading the HTTP values sent by a client during a Web request.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets the <see cref="IRequestContext" /> instance of the current request.
        /// </summary>
        IRequestContext RequestContext { get; }


        /// <summary>
        /// Gets the complete URL of the current request.
        /// </summary>
        string RawUrl { get; }


        /// <summary>
        /// Gets information about the URL of the current request.
        /// </summary>
        Uri Url { get; }


        /// <summary>
        /// Gets the collection of HTTP headers that were sent by the client.
        /// </summary>
        NameValueCollection Headers { get; }


        /// <summary>
        /// Gets a collection of Web server variables.
        /// </summary>
        NameValueCollection ServerVariables { get; }


        /// <summary>
        /// Gets the collection of HTTP query-string variables.
        /// </summary>
        NameValueCollection QueryString { get; }


        /// <summary>
        /// Gets the collection of cookies that were sent by the client.
        /// </summary>
        IHttpCookieCollection Cookies { get; }


        /// <summary>
        /// Gets the collection of form variables that were sent by the client.
        /// </summary>
        NameValueCollection Form { get; }


        /// <summary>
        /// Gets the HTTP data-transfer method (such as <see langword="GET" />, <see langword="POST" />, or <see langword="HEAD" />) that was used by the client.
        /// </summary>
        string HttpMethod { get; }


        /// <summary>
        /// Gets information about the requesting client's browser capabilities.
        /// </summary>
        IBrowser Browser { get; }


        /// <summary>
        /// Gets the complete user-agent string of the client.
        /// </summary>
        string UserAgent { get; }


        /// <summary>
        /// Gets information about the URL of the client request that linked to the current URL.
        /// </summary>
        Uri UrlReferrer { get; }


        /// <summary>
        /// Gets a sorted array of client language preferences.
        /// </summary>
        string[] UserLanguages { get; }


        /// <summary>
        /// Gets the virtual path of the application root and makes it relative by using the tilde (~) notation for the application root (as in "~/page.aspx").
        /// </summary>
        string AppRelativeCurrentExecutionFilePath { get; }


        /// <summary>
        /// Gets a value that indicates whether the HTTP connection uses secure sockets (HTTPS protocol).
        /// </summary>
        bool IsSecureConnection { get; }


        /// <summary>
        /// Gets the IP host address of the client.
        /// </summary>
        string UserHostAddress { get; }


        /// <summary>
        /// Gets the physical file-system path of the current application's root directory.
        /// </summary>
        string PhysicalApplicationPath { get; }
    }
}
