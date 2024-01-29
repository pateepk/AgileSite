using System;
using System.Collections.Generic;

using CMS.IO;

using AngleSharp.Dom;
using AngleSharp.Dom.Events;
using AngleSharp.Dom.Html;

namespace CMS.Newsletters.Internal
{
    /// <summary>
    /// Parses input HTML code to DOM structure and provides basic operations with it.
    /// </summary>
    public sealed class EmailHtmlModifier : IEmailHtmlModifier
    {
        #region "Private variables"

        private readonly bool isFragment;
        private readonly IHtmlDocument htmlDocument;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Errors occurred during parsing input HTML code.
        /// </summary>
        public ICollection<string> ParsingErrors { get; } = new List<string>();

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor. If any errors occurred while parsing input <paramref name="htmlCode"/>, they will be stored in <see cref="ParsingErrors"/>.
        /// </summary>
        /// <param name="htmlCode">Input HTML code.</param>
        /// <param name="isFragment">Indicates if the input HTML code is just a fragment, not complete document.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="htmlCode"/> is null or empty.</exception>
        public EmailHtmlModifier(string htmlCode, bool isFragment)
        {
            if (String.IsNullOrEmpty(htmlCode))
            {
                throw new ArgumentException($"The {nameof(htmlCode)} must not be null or empty string", nameof(htmlCode));
            }

            this.isFragment = isFragment;
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            parser.Context.ParseError += ParseError;
            htmlDocument = parser.Parse(htmlCode);
        }


        /// <summary>
        /// Appends element to body with specific attributes and inner text.
        /// </summary>
        /// <param name="elementName">HTML element name.</param>
        /// <param name="attributes">Attributes collection.</param>
        /// <param name="innerText">Inner text of element.</param>
        public void AppendElementToBody(string elementName, Dictionary<string, string> attributes = null, string innerText = null)
        {
            if (isFragment)
            {
                throw new NotSupportedException("Element cannot be appended to a body for a HTML fragment.");
            }

            var element = CreateElement(elementName, attributes, innerText);

            htmlDocument.Body.Append(element);
        }


        /// <summary>
        /// Appends element to head with specific attributes and inner text.
        /// </summary>
        /// <param name="elementName">HTML element name.</param>
        /// <param name="attributes">Attributes collection.</param>
        /// <param name="innerText">Inner text of element.</param>
        public void AppendElementToHead(string elementName, Dictionary<string, string> attributes = null, string innerText = null)
        {
            if (isFragment)
            {
                throw new NotSupportedException("Element cannot be appended to a head for a HTML fragment.");
            }

            var element = CreateElement(elementName, attributes, innerText);

            htmlDocument.Head.Append(element);
        }


        /// <summary>
        /// Renders back result HTML including all modifications.
        /// </summary>
        public string GetHtml()
        {
            var writer = new StringWriter();
            var formatter = new AngleSharp.Xml.XmlMarkupFormatter();


            if (isFragment)
            {
                // Return only inner HTML of the body, if the input is HTML fragment
                htmlDocument.Body.ChildNodes.ToHtml(writer, formatter);
            }
            else
            {
                // Need to call ToHtml() in order to include <!DOCTYPE> declaration in the result HTML
                htmlDocument.ToHtml(writer, formatter);
            }

            return writer.ToString();
        }


        /// <summary>
        /// Ensures that the <c>a</c> elements are not clickable. 
        /// </summary>
        public void DisableLinks()
        {
            var links = htmlDocument.QuerySelectorAll("a");

            foreach (var link in links)
            {
                link.SetAttribute("onclick", "return false");
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Constructs and returns element with optional attributes defined.
        /// </summary>
        private IElement CreateElement(string elementName, Dictionary<string, string> attributes = null, string innerText = null)
        {
            var element = htmlDocument.CreateElement(elementName);

            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    element.SetAttribute(attribute.Key, attribute.Value);
                }
            }
            if (!String.IsNullOrEmpty(innerText))
            {
                element.TextContent = innerText;
            }

            return element;
        }


        /// <summary>
        /// Adds entry to <see cref="ParsingErrors"/> collection when some HTML DOM error occurs.
        /// </summary>
        private void ParseError(object sender, Event ev)
        {
            var htmlError = ev as HtmlErrorEvent;

            if (htmlError != null)
            {
                ParsingErrors.Add(htmlError.Message);
            }
        }

        #endregion
    }
}
