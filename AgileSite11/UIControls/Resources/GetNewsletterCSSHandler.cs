using System;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Routing.Web;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterHttpHandler("CMSPages/GetNewsletterCSS.aspx", typeof(GetNewsletterCSSHandler), Order = 1)]

namespace CMS.UIControls
{
    /// <summary>
    /// Returns newsletter stylesheet stored in a database.
    /// Usage: ~/CMSPages/GetNewsletterCSS.aspx?newslettertemplatename=newsletterTemplateName
    /// </summary>
    internal class GetNewsletterCSSHandler : AdvancedGetFileHandler
    {
        #region "Variables"

        private CMSOutputResource outputFile;
        private string newsletterTemplateName;
        
        /// <summary>
        /// Sets to false to disable the client caching.
        /// </summary>
        private readonly bool useClientCache = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the process allows cache.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                if (mAllowCache == null)
                {
                    // By default, cache for the newsletter CSS is always enabled (even outside of the live site)
                    mAllowCache = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAlwaysCacheNewsletterCSS"], true) || IsLiveSite;
                }

                return mAllowCache.Value;
            }
            set
            {
                mAllowCache = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Processes the handler request
        /// </summary>
        /// <param name="context">Handler context</param>
        protected override void ProcessRequestInternal(HttpContextBase context)
        {
            // Check the site
            if (string.IsNullOrEmpty(CurrentSiteName))
            {
                throw new Exception("[GetCSS.aspx]: Site not running.");
            }

            newsletterTemplateName = QueryHelper.GetString("newslettertemplatename", string.Empty);
            string cacheKey = string.Format("getnewslettercss|{0}|{1}", SiteContext.CurrentSiteName, newsletterTemplateName);

            // Try to get data from cache
            using (var cs = new CachedSection<CMSOutputResource>(ref outputFile, CacheMinutes, true, cacheKey))
            {
                if (cs.LoadData)
                {
                    // Process the file
                    ProcessStylesheet();

                    // Ensure the cache settings
                    if ((outputFile != null) && cs.Cached)
                    {
                        // Add cache dependency
                        var cd = CacheHelper.GetCacheDependency(new[]
                        {
                            "newsletter.emailtemplate|byname|" + newsletterTemplateName.ToLowerCSafe()
                        });

                        // Cache the data
                        cs.CacheDependency = cd;
                    }

                    cs.Data = outputFile;
                }
            }

            if (outputFile != null)
            {
                // Send the data
                SendFile(outputFile);
            }
        }


        /// <summary>
        /// Processes the stylesheet.
        /// </summary>
        protected void ProcessStylesheet()
        {
            // Newsletter template stylesheet
            if (!string.IsNullOrEmpty(newsletterTemplateName))
            {
                // Get the template (do not use EmailTemplateInfoProvider because CMS.Newsletter is a separable module)
                var newsletterEmailTemplate = ProviderHelper.GetInfoByName(PredefinedObjectType.NEWSLETTERTEMPLATE, newsletterTemplateName, CurrentSite.SiteID);

                if (newsletterEmailTemplate != null)
                {
                    var style = ValidationHelper.GetString(newsletterEmailTemplate.GetValue("TemplateStylesheetText"), "");
                    var templateName = ValidationHelper.GetString(newsletterEmailTemplate.GetValue("TemplateName"), "");

                    // Create the output file
                    outputFile = new CMSOutputResource
                    {
                        Name = RequestContext.URL.ToString(),
                        Data = HTMLHelper.ResolveCSSUrls(style, SystemContext.ApplicationPath),
                        Etag = templateName
                    };
                }
            }
        }


        /// <summary>
        /// Sends the given file within response.
        /// </summary>
        /// <param name="file">File to send</param>
        protected void SendFile(CMSOutputResource file)
        {
            // Clear response.
            CookieHelper.ClearResponseCookies();
            Response.Clear();

            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            // Send the file
            if ((file != null) && (file.Data != null))
            {
                // Prepare response
                Response.ContentType = "text/css; charset=utf-8";

                // Client caching - only on the live site
                if (useClientCache && AllowCache && CacheHelper.CacheImageEnabled(CurrentSiteName) && ETagsMatch(file.Etag, file.LastModified))
                {
                    RespondNotModified(file.Etag);
                    return;
                }

                if (useClientCache && AllowCache && CacheHelper.CacheImageEnabled(CurrentSiteName))
                {
                    SetTimeStamps(file.LastModified);

                    Response.Cache.SetETag(file.Etag);
                }

                // Add the file data
                Response.Write(file.Data);
            }

            CompleteRequest();
        }

        #endregion
    }
}