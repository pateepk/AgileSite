using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Extended XMLRepeater control that provides RSS support.
    /// </summary>
    [ToolboxData("<{0}:RSSRepeater runat=server></{0}:RSSRepeater>")]
    public class RSSRepeater : XMLRepeater
    {
        #region "Private variables"

        private string mHeaderXML;
        private string mFooterXML;

        private static bool? mUseAtomLink;

        #endregion


        #region "Public properties"

        /// <summary>
        /// URL title of the feed.
        /// </summary>
        public string FeedTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Link to feed.
        /// </summary>
        public string FeedLink
        {
            get;
            set;
        }


        /// <summary>
        /// Description of the feed.
        /// </summary>
        public string FeedDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Language of the feed. If the value is empty the content culture will be used.
        /// </summary>
        public string FeedLanguage
        {
            get;
            set;
        }


        /// <summary>
        /// Custom feed header XML which is generated before feed items. If the value is empty default header for RSS feed is generated.
        /// </summary>
        public override string HeaderXML
        {
            get
            {
                if (string.IsNullOrEmpty(mHeaderXML))
                {
                    var stringBuilder = new StringBuilder();

                    // Write RSS header
                    if (UseAtomLink)
                    {
                        stringBuilder.AppendLine("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">");
                    }
                    else
                    {
                        stringBuilder.AppendLine("<rss version=\"2.0\">");
                    }
                    stringBuilder.AppendLine("<channel>");
                    if (UseAtomLink)
                    {
                        stringBuilder.AppendLine("<atom:link href=\"" + HTMLHelper.EncodeForHtmlAttribute(RequestContext.CurrentURL) + "\" rel=\"self\" type=\"application/rss+xml\"/>");
                    }
                    stringBuilder.AppendLine("<title><![CDATA[" + FeedTitle + "]]></title>");
                    stringBuilder.AppendLine("<link><![CDATA[" + FeedLink + "]]></link>");
                    stringBuilder.AppendLine("<description><![CDATA[" + FeedDescription + "]]></description>");
                    if (!string.IsNullOrEmpty(FeedLanguage))
                    {
                        stringBuilder.AppendLine("<language><![CDATA[" + FeedLanguage + "]]></language>");
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return mHeaderXML;
                }
            }
            set
            {
                mHeaderXML = value;
            }
        }


        /// <summary>
        /// Custom feed footer XML which is generated after feed items. If the value is empty default footer for RSS feed is generated.
        /// </summary>
        public override string FooterXML
        {
            get
            {
                if (string.IsNullOrEmpty(mFooterXML))
                {
                    var stringBuilder = new StringBuilder();

                    // Write RSS footer
                    stringBuilder.AppendLine("</channel>");
                    stringBuilder.AppendLine("</rss>");
                    return stringBuilder.ToString();
                }
                else
                {
                    return mFooterXML;
                }
            }
            set
            {
                mFooterXML = value;
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates if the "atom:link" element should be used in the generated RSS feed.
        /// Default is true.
        /// </summary>
        public static bool UseAtomLink
        {
            get
            {
                if (mUseAtomLink == null)
                {
                    mUseAtomLink = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseAtomLinkInGeneratedRss"], true);
                }
                return mUseAtomLink.Value;
            }
            set
            {
                mUseAtomLink = value;
            }
        }

        #endregion
    }
}