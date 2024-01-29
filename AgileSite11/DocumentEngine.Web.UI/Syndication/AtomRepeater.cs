using System;
using System.Text;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Extended XMLRepeater control that provides Atom support.
    /// </summary>
    [ToolboxData("<{0}:AtomRepeater runat=server></{0}:AtomRepeater>")]
    public class AtomRepeater : XMLRepeater
    {
        #region "Private variables"

        private Guid mFeedGUID = Guid.Empty;
        private DateTime mFeedUpdated = DateTimeHelper.ZERO_TIME;

        private string mHeaderXML;
        private string mFooterXML;

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
        /// Description/Subtitle of the feed.
        /// </summary>
        public string FeedSubtitle
        {
            get;
            set;
        }


        /// <summary>
        /// The unique identifier of the feed. If the value is empty, Guid.Empty will be used.
        /// </summary>
        public Guid FeedGUID
        {
            get
            {
                return mFeedGUID;
            }
            set
            {
                mFeedGUID = value;
            }
        }


        /// <summary>
        /// Last significant modification date of the feed.
        /// </summary>
        public DateTime FeedUpdated
        {
            get
            {
                return mFeedUpdated;
            }
            set
            {
                mFeedUpdated = value;
            }
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
        /// Author name of the feed.
        /// </summary>
        public string FeedAuthorName
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
        /// Custom feed header XML which is generated before feed items. If the value is empty default header for Atom feed is generated.
        /// </summary>
        public override string HeaderXML
        {
            get
            {
                if (string.IsNullOrEmpty(mHeaderXML))
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    // Write Atom header
                    string langAttribut = string.IsNullOrEmpty(FeedLanguage) ? "" : "xml:lang=\"" + FeedLanguage + "\"";
                    stringBuilder.AppendLine("<feed xmlns=\"http://www.w3.org/2005/Atom\" " + langAttribut + ">");
                    stringBuilder.AppendLine("<title><![CDATA[" + FeedTitle + "]]></title>");
                    if (!string.IsNullOrEmpty(FeedSubtitle))
                    {
                        stringBuilder.AppendLine("<subtitle><![CDATA[" + FeedSubtitle + "]]></subtitle>");
                    }
                    stringBuilder.AppendLine(HTMLHelper.GetLink(UrlResolver.ResolveUrl(FeedLink), "application/atom+xml", "self", null, FeedTitle));
                    stringBuilder.AppendLine("<updated>" + FeedUpdated.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzzz") + "</updated>");
                    stringBuilder.AppendLine("<author><name><![CDATA[" + FeedAuthorName + "]]></name></author>");
                    stringBuilder.AppendLine("<id>urn:uuid:" + FeedGUID + "</id>");
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
        /// Custom feed footer XML which is generated after feed items. If the value is empty default footer for Atom feed is generated.
        /// </summary>
        public override string FooterXML
        {
            get
            {
                if (string.IsNullOrEmpty(mFooterXML))
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    // Write Atom footer
                    stringBuilder.AppendLine("</feed>");
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

        #endregion
    }
}