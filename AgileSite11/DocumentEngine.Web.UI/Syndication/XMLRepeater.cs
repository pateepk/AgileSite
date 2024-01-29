using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Xml;

using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;
using MemoryStream = System.IO.MemoryStream;
using SeekOrigin = System.IO.SeekOrigin;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Extended Repeater control that produces XML output.
    /// </summary>
    [ToolboxData("<{0}:XMLRepeater runat=server></{0}:XMLRepeater>")]
    public class XMLRepeater : BasicRepeater
    {
        #region "Variables"

        /// <summary>
        /// Indicates whether control was binded.
        /// </summary>
        private bool binded;

        /// <summary>
        /// Content type of response.
        /// </summary>
        private string mContentType = "application/xml";

        /// <summary>
        /// Content encoding.
        /// </summary>
        private Encoding mEncoding = Encoding.UTF8;

        /// <summary>
        /// Base properties for caching purposes.
        /// </summary>
        private readonly CMSBaseProperties mProperties = new CMSBaseProperties();

        #endregion


        #region "Behavior properties"

        /// <summary>
        /// Indicates whether the data should be loaded to the load event instead of default init event.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether the data should be loaded to the load event instead of default init event.")]
        public bool DelayedLoading
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DelayedLoading"], false);
            }
            set
            {
                ViewState["DelayedLoading"] = value;
            }
        }


        /// <summary>
        /// Indicates whether data binding should be performed by default.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates whether data binding should be performed by default.")]
        public new bool DataBindByDefault
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["CMSDataBindByDefault"], true);
            }
            set
            {
                ViewState["CMSDataBindByDefault"] = value;
            }
        }


        /// <summary>
        /// Stops further processing.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Stop processing.")]
        public bool StopProcessing
        {
            get
            {
                return mProperties.StopProcessing;
            }
            set
            {
                mProperties.StopProcessing = value;
            }
        }

        #endregion


        #region "Transformation properties"

        /// <summary>
        /// Transformation name in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation.")]
        public string TransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["TransformationName"], string.Empty);
            }
            set
            {
                ViewState["TransformationName"] = value;
            }
        }

        #endregion


        #region "Cache properties"

        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        [Category("Behavior"), DefaultValue(0), Description("Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.")]
        public int CacheMinutes
        {
            get
            {
                return mProperties.CacheMinutes;
            }
            set
            {
                mProperties.CacheMinutes = value;
            }
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        /// <remarks>
        /// By setting this name dynamically, you can achieve caching based on URL parameter or some other variable - simply put the value of the parameter to the CacheItemName property. If no value is set, the control stores its content to the item named "URL|ControlID".
        /// </remarks>
        [Category("Behavior"), DefaultValue(""), Description("Name of the cache item the control will use.")]
        public string CacheItemName
        {
            get
            {
                return mProperties.CacheItemName;
            }
            set
            {
                mProperties.CacheItemName = value;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Cache dependencies, each cache dependency on a new line.")]
        public string CacheDependencies
        {
            get
            {
                return mProperties.CacheDependencies;
            }
            set
            {
                mProperties.CacheDependencies = value;
            }
        }

        #endregion


        #region "XML properties"

        /// <summary>
        /// XML which is rendered before repeater items.
        /// </summary>
        public virtual string HeaderXML
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HeaderXML"], "<root>");
            }
            set
            {
                ViewState["HeaderXML"] = value;
            }
        }


        /// <summary>
        /// XML which is rendered after repeater items.
        /// </summary>
        public virtual string FooterXML
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FooterXML"], "</root>");
            }
            set
            {
                ViewState["FooterXML"] = value;
            }
        }


        /// <summary>
        /// Content type of response.
        /// </summary>
        public string ContentType
        {
            get
            {
                return mContentType;
            }
            set
            {
                mContentType = value;
            }
        }


        /// <summary>
        /// Content encoding. (UTF8 by default).
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return mEncoding;
            }
            set
            {
                mEncoding = value;
            }
        }


        /// <summary>
        /// Name of feed for caching purposes.
        /// </summary>
        public string FeedName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FeedName"], string.Empty);
            }
            set
            {
                ViewState["FeedName"] = value;
            }
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Ensures default data binding 
        /// </summary>
        /// <param name="loadPhase">Indicates whether Init is call from Load event</param>
        protected override void InitControl(bool loadPhase)
        {
            // Check for context (due to VS design mode)
            if (Context == null)
            {
                return;
            }

            // if control is in stop processing mode, disable default binding
            if (!binded && (!DelayedLoading || loadPhase))
            {
                base.InitControl(loadPhase);
            }

            // Do not call base method for delayed loading
            // base.InitControl();
        }


        /// <summary>
        /// Overrides render event.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[XMLRepeater: " + ClientID + "]");
                return;
            }
            base.Render(writer);
        }


        /// <summary>
        /// Overrides databind event.
        /// </summary>
        public override void DataBind()
        {
            // Apply transformations if they exist
            if (!String.IsNullOrEmpty(TransformationName))
            {
                ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName);
            }
            base.DataBind();
        }


        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        public override void ReloadData(bool forceReload)
        {
            if (StopProcessing)
            {
                // Do nothing
            }
            else
            {
                // Disable logging page views
                RequestContext.LogPageHit = false;

                if (!binded || forceReload)
                {
                    if (DataSourceControl != null)
                    {
                        HttpResponse response = Context.Response;

                        // Render XML
                        response.Clear();
                        response.ClearContent();
                        response.ContentType = ContentType;
                        response.ContentEncoding = Encoding;

                        string output = null;

                        // Get hash code from XML to get shorter cache item name
                        string xmlHash = (HeaderXML + FooterXML).GetHashCode().ToString();

                        // Get cache item name from datasource control
                        string dsCacheItemName = CacheHelper.GetCacheItemName(DataSourceControl.CacheItemName, "datasource", CacheHelper.BaseCacheKey, DataSourceControl.ClientID, DataSourceControl.WhereCondition, DataSourceControl.OrderBy);

                        // Try to get data from cache
                        using (var cs = new CachedSection<string>(ref output, CacheMinutes, true, CacheItemName, "xmlfeed", dsCacheItemName, FeedName, TransformationName, xmlHash))
                        {
                            if (cs.LoadData)
                            {
                                // Bind data source
                                DataSource = DataSourceControl.LoadData(forceReload);
                                DataBind();

                                StringBuilder stringBuilder = new StringBuilder();
                                StringWriter textWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);

                                // Create XML root element
                                MemoryStream ms = new MemoryStream();
                                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, Encoding))
                                {
                                    // Write root element
                                    xmlTextWriter.WriteStartDocument();
                                    // Flush streams
                                    xmlTextWriter.Flush();
                                    // Read rendered data
                                    StreamReader reader = StreamReader.New(xmlTextWriter.BaseStream);
                                    xmlTextWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                                    // Append data to result data
                                    stringBuilder.Append(reader.ReadToEnd());
                                    // Close streams
                                    xmlTextWriter.Close();
                                    reader.Close();
                                }

                                // Render header XML
                                stringBuilder.Append(HeaderXML);

                                // Render repeater
                                HtmlTextWriter writer = new HtmlTextWriter(textWriter);
                                RenderChildren(writer);

                                // Render footer XML
                                stringBuilder.Append(FooterXML);

                                // Resolve relative links to absolute links
                                bool linksResolved = false;
                                if ((DataSourceControl != null) && !string.IsNullOrEmpty(DataSourceControl.SiteName))
                                {
                                    SiteInfo si = SiteInfoProvider.GetSiteInfo(DataSourceControl.SiteName);
                                    if (si != null)
                                    {
                                        URLHelper urlHelper = new URLHelper();
                                        output = urlHelper.MakeLinksAbsolute(stringBuilder.ToString(), si.DomainName);
                                        linksResolved = true;
                                    }
                                }

                                if (!linksResolved)
                                {
                                    output = URLHelper.MakeLinksAbsolute(stringBuilder.ToString());
                                }

                                // Close text writer
                                textWriter.Flush();
                                textWriter.Close();

                                // Save the result to the cache
                                if (cs.Cached)
                                {
                                    cs.CacheDependency = GetCacheDependency();
                                }

                                cs.Data = output;
                            }
                        }

                        // Write rendered code to output
                        response.Write(output);

                        // End response
                        RequestHelper.EndResponse();
                        binded = true;
                    }
                }
            }
        }

        #endregion


        #region "Caching methods"

        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependendencies()
        {
            return null;
        }


        /// <summary>
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            // Process the dependencies
            string dep = CacheHelper.GetCacheDependencies(CacheDependencies, GetDefaultCacheDependendencies());

            return CacheHelper.GetCacheDependency(dep);
        }

        #endregion
    }
}