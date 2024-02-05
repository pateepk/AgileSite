using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Atom Feed - Generates link to Atom feed.
    /// </summary>
    [ToolboxData("<{0}:AtomFeed runat=server></{0}:AtomFeed>")]
    public class AtomFeed : PlaceHolder
    {
        #region "Variables"

        /// <summary>
        /// Default querystring parameter name.
        /// </summary>
        private string mQueryStringKey = "atom";

        /// <summary>
        /// Indicates whether control was binded.
        /// </summary>
        private bool binded = false;

        private string mLinkText = string.Empty;
        private string mLinkIcon = string.Empty;
        private string mFeedTitle = string.Empty;
        private string mFeedLink = string.Empty;
        private string mFeedSubtitle = string.Empty;
        private string mFeedAuthorName = string.Empty;
        private string mFeedLanguage = string.Empty;
        private DateTime mFeedUpdated = DateTimeHelper.ZERO_TIME;
        private Guid mFeedGUID = Guid.Empty;

        private readonly AtomRepeater repeaterElem = new AtomRepeater();

        private bool mEnableAutodiscovery = true;
        private bool mStopProcessing = false;

        private string feed = null;

        #endregion


        #region "Stop processing"

        /// <summary>
        /// Stops further processing.
        /// </summary>
        public bool StopProcessing
        {
            get
            {
                return mStopProcessing;
            }
            set
            {
                mStopProcessing = value;
                repeaterElem.StopProcessing = value;
            }
        }

        #endregion


        #region "Atom Feed Properties"

        /// <summary>
        /// Querystring key which is used for Atom feed identification on a page with multiple Atom feeds.
        /// </summary>
        public string QueryStringKey
        {
            get
            {
                return mQueryStringKey;
            }
            set
            {
                mQueryStringKey = value;
            }
        }


        /// <summary>
        /// Feed name to identify this feed on a page with multiple feeds. If the value is empty the GUID of the web part instance will be used by default.
        /// </summary>
        public string FeedName
        {
            get
            {
                return repeaterElem.FeedName;
            }
            set
            {
                repeaterElem.FeedName = value;
            }
        }


        /// <summary>
        /// Text for the feed link.
        /// </summary>
        public string LinkText
        {
            get
            {
                return mLinkText;
            }
            set
            {
                mLinkText = value;
            }
        }


        /// <summary>
        /// Icon which will be displayed in the feed link.
        /// </summary>
        public string LinkIcon
        {
            get
            {
                return mLinkIcon;
            }
            set
            {
                mLinkIcon = value;
            }
        }


        /// <summary>
        /// Indicates if the Atom feed is automatically discovered by the browser.
        /// </summary>
        public bool EnableAutodiscovery
        {
            get
            {
                return mEnableAutodiscovery;
            }
            set
            {
                mEnableAutodiscovery = value;
            }
        }


        /// <summary>
        /// Custom feed header XML which is generated before feed items. If the value is empty default header for Atom feed is generated.
        /// </summary>
        public string HeaderXML
        {
            get
            {
                return repeaterElem.HeaderXML;
            }
            set
            {
                repeaterElem.HeaderXML = value;
            }
        }


        /// <summary>
        /// Custom feed footer XML which is generated after feed items. If the value is empty default footer for Atom feed is generated.
        /// </summary>
        public string FooterXML
        {
            get
            {
                return repeaterElem.FooterXML;
            }
            set
            {
                repeaterElem.FooterXML = value;
            }
        }

        #endregion


        #region "Atom Repeater properties"

        /// <summary>
        /// Atom repeater control.
        /// </summary>
        public AtomRepeater AtomRepeaterControl
        {
            get
            {
                return repeaterElem;
            }
        }


        /// <summary>
        /// URL title of the feed.
        /// </summary>
        public string FeedTitle
        {
            get
            {
                return mFeedTitle;
            }
            set
            {
                mFeedTitle = value;
                repeaterElem.FeedTitle = value;
            }
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
                repeaterElem.FeedGUID = value;
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
                repeaterElem.FeedUpdated = value;
            }
        }


        /// <summary>
        /// Link to feed.
        /// </summary>
        public string FeedLink
        {
            get
            {
                return mFeedLink;
            }
            set
            {
                mFeedLink = value;
                repeaterElem.FeedLink = value;
            }
        }


        /// <summary>
        /// Author name of the feed.
        /// </summary>
        public string FeedAuthorName
        {
            get
            {
                return mFeedAuthorName;
            }
            set
            {
                mFeedAuthorName = value;
                repeaterElem.FeedAuthorName = value;
            }
        }


        /// <summary>
        /// Description of the feed.
        /// </summary>
        public string FeedSubtitle
        {
            get
            {
                return mFeedSubtitle;
            }
            set
            {
                mFeedSubtitle = value;
                repeaterElem.FeedSubtitle = value;
            }
        }


        /// <summary>
        /// Language of the feed. If the value is empty the content culture will be used.
        /// </summary>
        public string FeedLanguage
        {
            get
            {
                return mFeedLanguage;
            }
            set
            {
                mFeedLanguage = value;
                repeaterElem.FeedLanguage = value;
            }
        }

        #endregion


        #region "Cache properties"

        /// <summary>
        /// Gets or sets the cache item name.
        /// </summary>
        public string CacheItemName
        {
            get
            {
                return repeaterElem.CacheItemName;
            }
            set
            {
                repeaterElem.CacheItemName = value;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        public string CacheDependencies
        {
            get
            {
                return repeaterElem.CacheDependencies;
            }
            set
            {
                repeaterElem.CacheDependencies = value;
            }
        }


        /// <summary>
        /// Gets or sets the cache minutes.
        /// </summary>
        public int CacheMinutes
        {
            get
            {
                return repeaterElem.CacheMinutes;
            }
            set
            {
                repeaterElem.CacheMinutes = value;
            }
        }

        #endregion


        #region "Datasource properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                return repeaterElem.DataSourceName;
            }
            set
            {
                repeaterElem.DataSourceName = value;
            }
        }


        /// <summary>
        /// Control with data source.
        /// </summary>
        public CMSBaseDataSource DataSourceControl
        {
            get
            {
                return repeaterElem.DataSourceControl;
            }
            set
            {
                repeaterElem.DataSourceControl = value;
            }
        }


        /// <summary>
        /// Indicates whether data binding should be performed by default.
        /// </summary>
        public bool DataBindByDefault
        {
            get
            {
                return repeaterElem.DataBindByDefault;
            }
            set
            {
                repeaterElem.DataBindByDefault = value;
            }
        }

        #endregion


        #region "Transformation properties"

        /// <summary>
        /// Gets or sets ItemTemplate property.
        /// </summary>
        public string TransformationName
        {
            get
            {
                return repeaterElem.TransformationName;
            }
            set
            {
                repeaterElem.TransformationName = value;
            }
        }

        #endregion


        #region "Overidden methods"

        /// <summary>
        /// Overrides load event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing)
            {
                repeaterElem.StopProcessing = true;
            }
            else
            {
                // Reload data
                if (!binded && !string.IsNullOrEmpty(FeedName))
                {
                    ReloadData(false);
                }

                if (string.IsNullOrEmpty(feed))
                {
                    repeaterElem.StopProcessing = true;

                    string absoluteUrl = ResolveUrl(FeedLink);

                    if (EnableAutodiscovery)
                    {
                        // Add link to page header
                        string link = HTMLHelper.GetLink(absoluteUrl, "application/atom+xml", "alternate", null, FeedTitle);
                        Page.AddToHeader(link);
                    }

                    Controls.Clear();

                    if (!string.IsNullOrEmpty(LinkIcon))
                    {
                        HyperLink lnkFeedImg = new HyperLink();
                        lnkFeedImg.ID = "lnkFeedImg";
                        lnkFeedImg.NavigateUrl = absoluteUrl;
                        lnkFeedImg.EnableViewState = false;
                        lnkFeedImg.CssClass = "FeedLink";

                        Image imgFeed = new Image();
                        imgFeed.ID = "imgFeed";
                        imgFeed.ImageUrl = UIHelper.GetImageUrl(Page, LinkIcon);
                        imgFeed.AlternateText = FeedTitle;
                        imgFeed.EnableViewState = false;
                        imgFeed.CssClass = "FeedIcon";

                        lnkFeedImg.Controls.Add(imgFeed);
                        Controls.Add(lnkFeedImg);
                    }

                    if (!string.IsNullOrEmpty(LinkText))
                    {
                        HyperLink lnkFeedText = new HyperLink();
                        lnkFeedText.ID = "lnkFeedText";
                        lnkFeedText.NavigateUrl = absoluteUrl;
                        lnkFeedText.EnableViewState = false;
                        lnkFeedText.CssClass = "FeedLink";

                        Label ltlFeed = new Label();
                        ltlFeed.ID = "ltlFeed";
                        ltlFeed.EnableViewState = false;
                        ltlFeed.Text = HTMLHelper.HTMLEncode(LinkText);
                        ltlFeed.CssClass = "FeedCaption";

                        lnkFeedText.Controls.Add(ltlFeed);
                        Controls.Add(lnkFeedText);
                    }
                }
            }
        }


        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        public void ReloadData(bool forceReload)
        {
            if (StopProcessing)
            {
                repeaterElem.StopProcessing = true;
            }
            else
            {
                if (!binded || forceReload)
                {
                    string decodedUrl = HttpUtility.HtmlDecode(RequestContext.CurrentURL);
                    feed = ValidationHelper.GetString(URLHelper.GetUrlParameter(decodedUrl, QueryStringKey), string.Empty);

                    if (string.IsNullOrEmpty(feed))
                    {
                        repeaterElem.StopProcessing = true;
                    }
                    else if (feed.ToLowerCSafe() == FeedName.ToLowerCSafe())
                    {
                        // Get absolute URL
                        string url = URLHelper.GetAbsoluteUrl(FeedLink);
                        // Encode query string
                        url = HTMLHelper.EncodeForHtmlAttribute(url);

                        // Cache properties
                        repeaterElem.CacheItemName = CacheItemName;
                        repeaterElem.CacheDependencies = CacheDependencies;
                        repeaterElem.CacheMinutes = CacheMinutes;

                        // Transformation properties
                        repeaterElem.TransformationName = TransformationName;

                        // Datasource properties
                        repeaterElem.DataSourceName = DataSourceName;
                        repeaterElem.DataSourceControl = DataSourceControl;

                        // Feed properties
                        repeaterElem.FeedTitle = FeedTitle;
                        repeaterElem.FeedSubtitle = FeedSubtitle;
                        repeaterElem.FeedLanguage = FeedLanguage;
                        repeaterElem.FeedName = FeedName;
                        repeaterElem.FeedLink = url;

                        Controls.Add(repeaterElem);
                    }
                    binded = true;
                }
            }
        }

        #endregion
    }
}
