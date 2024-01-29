using System.Collections.Generic;

namespace CMS.Newsletters.Internal
{
    /// <summary>
    /// Parses input HTML code to DOM structure and provides basic operations with it.
    /// </summary>
    internal interface IEmailHtmlModifier
    {
        /// <summary>
        /// Appends element to body with specific attributes and inner text.
        /// </summary>
        /// <param name="elementName">HTML element name.</param>
        /// <param name="attributes">Attributes collection.</param>
        /// <param name="innerText">Inner text of element.</param>
        void AppendElementToBody(string elementName, Dictionary<string, string> attributes = null, string innerText = null);


        /// <summary>
        /// Appends element to head with specific attributes and inner text.
        /// </summary>
        /// <param name="elementName">HTML element name.</param>
        /// <param name="attributes">Attributes collection.</param>
        /// <param name="innerText">Inner text of element.</param>
        void AppendElementToHead(string elementName, Dictionary<string, string> attributes = null, string innerText = null);


        /// <summary>
        /// Ensures that the <c>a</c> elements are not clickable. 
        /// </summary>
        void DisableLinks();


        /// <summary>
        /// Renders back result HTML including all modifications.
        /// </summary>
        string GetHtml();
    }
}