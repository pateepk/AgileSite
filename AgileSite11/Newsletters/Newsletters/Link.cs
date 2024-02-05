using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Represents link in web applications. Generally content of anchor hypertext reference ("a href").
    /// </summary>
    internal class Link
    {
        private readonly string mLink;
            

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="link">Link as string</param>
        public Link(string link)
        {
            mLink = link;
        }


        /// <summary>
        /// Returns true if link is relative. 
        /// Link is relative if it contains protocol.
        /// </summary>
        /// <example>
        /// <code>
        /// var relativeLink = new Link("/path?query=string#anchor");
        /// var absoluteLink = new Link("http://www.example.com/path?query=string#anchor");
        /// 
        /// // true
        /// relativeLink.IsRelative();
        /// 
        /// // false
        /// absoluteLink.IsRelative();
        /// </code>
        /// <remarks>
        /// Uses <see cref="URLHelper.GetProtocol(string)"/> internally for obtaining protocol of the <see cref="Link"/>.
        /// </remarks>
        /// </example>
        public bool IsRelative()
        {
            return String.IsNullOrEmpty(URLHelper.GetProtocol(mLink));
        }


        /// <summary>
        /// Creates link which contains only domain (protocol, port, application path, query string and anchor are removed). 
        /// </summary>
        /// <example>
        /// <code>
        /// var link = new Link("http://www.example.com:80/path?query=string#anchor");
        /// 
        /// //www.example.com
        /// string linkDomain = link.GetDomain();
        /// </code>
        /// </example>
        /// <remarks>
        /// Uses <see cref="URLHelper.GetDomain(string)"/> for extracting the domain and <see cref="URLHelper.RemovePort(string)"/> for removing port from the link.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Link refers to a relative address, for which it is not possible to retrieve its domain</exception>
        /// <returns>Domain of the original<see cref="Link"/></returns>
        public string GetDomain()
        {
            if (IsRelative())
            {
                throw new InvalidOperationException("[Link.GetDomain]: Domain cannot be obtained for relative links.");
            }

            return URLHelper.RemovePort(URLHelper.GetDomain(mLink));
        }


        /// <summary>
        /// Returns true if query string contains parameter given by name <paramref name="paramName" />.
        /// </summary>
        /// <example>
        /// <code>
        /// var link = new Link("http://www.example.com/path?query=string#anchor");
        /// 
        /// // true
        /// bool contains = link.HasParameter("query");
        /// 
        /// // false
        /// bool doesNotContain = link.HasParameter("anotherQuery");
        /// </code>
        /// </example>
        /// <remarks>
        /// Uses <see cref="URLHelper.GetQueryValue(string, string)"/> method internally.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="paramName"/> is null</exception>
        /// <returns><c>true</c> if the specified <paramref name="paramName"/> is present in the query string;; otherwise, <c>false</c>.</returns>
        public bool HasParameter(string paramName)
        {
            if (paramName == null)
            {
                throw new ArgumentNullException("paramName");
            }

            return !string.IsNullOrEmpty(URLHelper.GetQueryValue(mLink, paramName));
        }


        /// <summary>
        /// Creates new <see cref="Link"/> and appends given <paramref name="queryString" /> to the existing query string of current object.
        /// Anchor is kept in place.
        /// </summary>
        /// <example>
        /// <code>
        /// var link = new Link("http://www.example.com/path?query=string#anchor");
        /// 
        /// var resultLink = link.AppendQueryString("query2=string2&amp;query3=string3");
        /// 
        /// // http://www.example.com/path?query=string&amp;query2=string2&amp;query3=string3#anchor
        /// resultLink.ToString();
        /// </code>
        /// </example>
        /// <remarks>
        /// Uses <see cref="URLHelper.AppendQuery(string, string)"/> method for the appending query logic.
        /// </remarks>
        /// <returns>New <see cref="Link"/> object with appended <paramref name="queryString"/></returns>
        public Link AppendQueryString(string queryString)
        {
            var anchor = GetAnchor();
            return new Link(URLHelper.AppendQuery(RemoveAnchor().ToString(), queryString) + anchor);
        }


        /// <summary>
        /// Creates link without anchor.
        /// </summary>
        /// <example>
        /// <code>
        /// var link = new Link("http://www.example.com/path?query=string#anchor");
        /// 
        /// var resultLink = link.RemoveAnchor();
        /// 
        /// // http://www.example.com/path?query=string
        /// resultLink.ToString();
        /// </code>
        /// </example>
        /// <returns>New <see cref="Link"/> object without the link anchor</returns>
        private Link RemoveAnchor()
        {
            var result = mLink;

            int index = result.IndexOfCSafe('#');
            if (index > 0)
            {
                result = result.Substring(0, index);
            }
            
            return new Link(result);
        }


        /// <summary>
        /// Returns link anchor.
        /// </summary>
        /// <example>
        /// <code>
        /// var link = new Link("http://www.example.com/path?query=string#some-anchor");
        /// // some-anchor
        /// string anchor = link.GetAnchor();
        /// </code>
        /// </example>
        private string GetAnchor()
        {
            int anchorIndex = mLink.LastIndexOf('#');

            if (anchorIndex > 0)
            {
                return mLink.Substring(anchorIndex);
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns a string that represents the current <see cref="Link"/>.
        /// </summary>
        public override string ToString()
        {
            return mLink;
        }


        /// <summary>
        /// Determines whether the specified <paramref name="obj"/> is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns><c>true</c> if the specified <paramref name="obj"/> is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Link)obj);
        }
        

        /// <summary>
        /// Determines whether the specified <paramref name="other"/> is equal to the current <see cref="Link"/>.
        /// </summary>
        /// <param name="other">The <see cref="Link"/> to compare with the current <see cref="Link"/>. </param>
        /// <returns><c>true</c> if the specified <paramref name="other"/> is equal to the current <see cref="Link"/>; otherwise, <c>false</c>.</returns>
        protected bool Equals(Link other)
        {
            return mLink == other.mLink;
        }
        

        /// <summary>
        /// Serves as the hash function for the <see cref="Link"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>0</c> if the <see cref="Link"/> was initialized with empty <see cref="String"/>.
        /// </remarks>
        /// <returns>A hash code for the current <see cref="Link"/>.</returns>
        public override int GetHashCode()
        {
            return (mLink != null ? mLink.GetHashCode() : 0);
        }
    }
}