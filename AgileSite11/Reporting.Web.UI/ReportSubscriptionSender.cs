using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.WebAnalytics;

using SystemIO = System.IO;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Class for subscription email sender
    /// </summary>
    public class ReportSubscriptionSender : ITask
    {
        #region "Variables"

        /// <summary>
        /// Indicates for how long will be subscription email css file cached (one week)
        /// </summary>
        public const int EMAIL_CSS_FILE_CACHE_MINUTES = 60 * 24 * 7;

        private static String mPath = String.Empty;
        private static ReportSubscriptionPage mReportPage;
        private static Regex mInlineImageRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regex object for search inline images in report.
        /// </summary>
        private static Regex InlineImageRegex
        {
            get
            {
                return mInlineImageRegex ?? (mInlineImageRegex = RegexHelper.GetRegex("<img\\s*src=\"\\S*fileguid=(\\S*)\" />"));
            }
        }


        /// <summary>
        /// Page object used for render without HTTP context
        /// </summary>
        private static ReportSubscriptionPage ReportPage
        {
            get
            {
                return mReportPage ?? (mReportPage = new ReportSubscriptionPage());
            }
        }


        /// <summary>
        /// Path for default CSS styles document
        /// </summary>
        public static String Path
        {
            get
            {
                if (String.IsNullOrEmpty(mPath))
                {
                    mPath = HostingEnvironment.MapPath("~/CMSModules/Reporting/CMSPages/ReportSubscription.css");
                }

                return mPath;
            }
            set
            {
                mPath = value;
            }
        }

        #endregion


        #region "ITask members"

        /// <summary>
        /// Executes the logprocessor action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                SendSubscriptions();
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("Reporting", "ReportSubscriptionSender", e);
                return e.Message;
            }

            return null;
        }

        #endregion


        #region "Methods"

        private void SendSubscriptions()
        {
            var subscriptions = ReportSubscriptionInfoProvider.GetSubscriptions()
                                                              .WhereEquals("ReportSubscriptionEnabled", 1)
                                                              .WhereLessThan("ReportSubscriptionNextPostDate", DateTime.Now)
                                                              .TypedResult;
            string defaultCss = String.Empty;

            if (subscriptions.Any())
            {
                // Try to read default css file
                try
                {
                    using (var cs = new CachedSection<string>(ref defaultCss, EMAIL_CSS_FILE_CACHE_MINUTES, true, null, "reportsubscriptioncssfile", Path))
                    {
                        if (cs.LoadData)
                        {
                            // Read the content
                            using (StreamReader sr = StreamReader.New(Path))
                            {
                                defaultCss = sr.ReadToEnd();
                            }

                            // Add to the cache
                            if (cs.Cached)
                            {
                                cs.CacheDependency = CacheHelper.GetFileCacheDependency(Path);
                            }

                            cs.Data = defaultCss;
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Report subscription sender", "EXCEPTION", ex);
                }

                foreach (var subscription in subscriptions)
                {
                    // Empty content from previous subscriptions
                    string content = String.Empty;

                    // Check whether subscription condition is defined
                    if (MacroProcessor.ContainsMacro(subscription.ReportSubscriptionCondition))
                    {
                        // Resolve subscription condition and if is not match -> do not continue
                        if (!ValidationHelper.GetBoolean(MacroResolver.Resolve(subscription.ReportSubscriptionCondition), false))
                        {
                            continue;
                        }
                    }

                    // Read report info
                    ReportInfo ri = ReportInfoProvider.GetReportInfo(subscription.ReportSubscriptionReportID);
                    if (ri == null)
                    {
                        continue;
                    }

                    if (!ri.ReportEnableSubscription)
                    {
                        continue;
                    }

                    String siteName = SiteInfoProvider.GetSiteName(subscription.ReportSubscriptionSiteID);
                    const string TEMPLATE_NAME = "Report_subscription_template";

                    // Check for template
                    EmailTemplateInfo eti = EmailTemplateProvider.GetEmailTemplate(TEMPLATE_NAME, siteName);
                    if (eti == null)
                    {
                        EventLogProvider.LogEvent(EventType.ERROR, "Administration", "SENDEMAIL", String.Format(ResHelper.GetString("subscription.emailtemplatenotfound"), TEMPLATE_NAME), String.Empty, 0, String.Empty, 0, String.Empty, String.Empty);
                        return;
                    }

                    // Load subscriptions parameters
                    FormInfo fi = new FormInfo(ri.ReportParameters);
                    DataRow data = fi.GetDataRow(false);
                    fi.LoadDefaultValues(data, true);
                    ReportHelper.ApplySubscriptionParameters(subscription, data, true);

                    bool isItem = false;

                    // Manage solo graph subscription
                    if (subscription.ReportSubscriptionGraphID != 0)
                    {
                        isItem = true;

                        ReportGraphInfo rgi = ReportGraphInfoProvider.GetReportGraphInfo(subscription.ReportSubscriptionGraphID);
                        if ((rgi != null) && ValidationHelper.GetBoolean(rgi.GraphSettings["SubscriptionEnabled"], true))
                        {
                            AbstractReportControl control;
                            if (rgi.GraphIsHtml)
                            {
                                control = ReportPage.LoadUserControl("~/CMSModules/Reporting/Controls/HtmlBarGraph.ascx") as AbstractReportControl;
                            }
                            else
                            {
                                control = ReportPage.LoadUserControl("~/CMSModules/Reporting/Controls/ReportGraph.ascx") as AbstractReportControl;
                            }

                            content = RenderControlToString(control, ri.ReportName + "." + rgi.GraphName, data, subscription);
                        }
                    }

                    // Table subscription
                    if (subscription.ReportSubscriptionTableID != 0)
                    {
                        isItem = true;

                        ReportTableInfo rti = ReportTableInfoProvider.GetReportTableInfo(subscription.ReportSubscriptionTableID);
                        if ((rti != null) && ValidationHelper.GetBoolean(rti.TableSettings["SubscriptionEnabled"], true))
                        {
                            AbstractReportControl control = ReportPage.LoadUserControl("~/CMSModules/Reporting/Controls/ReportTable.ascx") as AbstractReportControl;
                            content = RenderControlToString(control, ri.ReportName + "." + rti.TableName, data, subscription);
                        }
                    }

                    // Value subscription
                    if (subscription.ReportSubscriptionValueID != 0)
                    {
                        isItem = true;

                        ReportValueInfo rvi = ReportValueInfoProvider.GetReportValueInfo(subscription.ReportSubscriptionValueID);
                        if ((rvi != null) && ValidationHelper.GetBoolean(rvi.ValueSettings["SubscriptionEnabled"], true))
                        {
                            AbstractReportControl control = ReportPage.LoadUserControl("~/CMSModules/Reporting/Controls/ReportValue.ascx") as AbstractReportControl;
                            content = RenderControlToString(control, ri.ReportName + "." + rvi.ValueName, data, subscription);
                        }
                    }

                    if (!isItem)
                    {
                        // Whole report is subscribed
                        IDisplayReport displayReport = ReportPage.LoadUserControl("~/CMSModules/Reporting/Controls/DisplayReport.ascx") as IDisplayReport;
                        displayReport.ReportName = ri.ReportName;
                        displayReport.EmailMode = true;
                        displayReport.ReportParameters = data;
                        displayReport.LoadFormParameters = false;
                        displayReport.RenderCssClasses = true;
                        displayReport.SendOnlyNonEmptyDataSource = subscription.ReportSubscriptionOnlyNonEmpty;
                        displayReport.ReportSubscriptionSiteID = subscription.ReportSubscriptionSiteID;
                        ((AbstractReportControl)displayReport).SubscriptionInfo = subscription;

                        String interval = subscription.ReportSubscriptionSettings["reportinterval"] as String;
                        displayReport.SetDefaultDynamicMacros(interval);

                        // Handle exception to allow send another subscriptions in dataset
                        try
                        {
                            content = displayReport.RenderToString(subscription.ReportSubscriptionSiteID);
                        }
                        catch (Exception e)
                        {
                            EventLogProvider.LogException("Reporting", "ReportSubscriptionSender", e, 0, "Report name: " + ri.ReportName);
                        }

                    }

                    // If any content rendered, send email
                    if (content != String.Empty)
                    {
                        SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                        EmailFormatEnum format = EmailHelper.GetEmailFormat(siteName);

                        // Create new email message
                        EmailMessage em = new EmailMessage();

                        // Plain text is not allowed for whole reports
                        em.EmailFormat = isItem ? EmailFormatEnum.Default : EmailFormatEnum.Html;
                        em.From = String.IsNullOrEmpty(eti.TemplateFrom) ? EmailHelper.Settings.NotificationsSenderAddress(siteName) : eti.TemplateFrom;
                        em.Recipients = subscription.ReportSubscriptionEmail;
                        em.Subject = subscription.ReportSubscriptionSubject;
                        em.BccRecipients = eti.TemplateBcc;
                        em.CcRecipients = eti.TemplateCc;

                        MacroResolver resolver = CreateSubscriptionMacroResolver(ri, subscription, si, em.Recipients, defaultCss, content);
                        String html = resolver.ResolveMacros(eti.TemplateText);
                        String plain = resolver.ResolveMacros(eti.TemplatePlainText);

                        // Add graph images as inline attachments
                        Dictionary<string, byte[]> dict = AbstractStockHelper<RequestStockHelper>.GetItem(ri.ReportName) as Dictionary<string, byte[]>;
                        if (dict != null)
                        {
                            foreach (String name in dict.Keys)
                            {
                                String itemName = name.Substring(1);
                                byte[] arr = dict[name];

                                // Table "plaintext" attachment
                                if (name.StartsWith("t", StringComparison.OrdinalIgnoreCase))
                                {
                                    em.Attachments.Add(CreateAttachment(itemName + ".csv", MediaTypeNames.Text.Plain, false, Guid.Empty, new SystemIO.MemoryStream(arr)));
                                }
                                else if (name.StartsWith("g", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Graph attachment
                                    Guid guid = Guid.NewGuid();

                                    bool insertInlineAtt = (format != EmailFormatEnum.PlainText);
                                    String imgText = insertInlineAtt ? "<img src=\"cid:" + guid + "\" alt=\"inlineimage\">" : String.Format(ResHelper.GetString("reportsubscription.attachment_img"), itemName);
                                    html = html.Replace("##InlineImage##" + itemName + "##InlineImage##", imgText);
                                    plain = plain.Replace("##InlineImage##" + itemName + "##InlineImage##", imgText);

                                    var ms = new SystemIO.MemoryStream(arr);

                                    // For 'both' mode (plain text and html) add inline and classic attachments
                                    if (format == EmailFormatEnum.Both)
                                    {
                                        em.Attachments.Add(CreateAttachment(itemName + ".png", "image/png", false, guid, ms));
                                        em.Attachments.Add(CreateAttachment(itemName + ".png", "image/png", true, guid, ms));
                                    }
                                    else
                                    {
                                        em.Attachments.Add(CreateAttachment(itemName + ".png", "image/png", insertInlineAtt, guid, ms));
                                    }
                                }
                            }

                            AbstractStockHelper<RequestStockHelper>.Remove(ri.ReportName);
                        }

                        // Create email body
                        em.Body = URLHelper.MakeLinksAbsolute(html);
                        em.PlainTextBody = URLHelper.MakeLinksAbsolute(plain);

                        // Add attachments and send e-mail
                        EmailHelper.ResolveMetaFileImages(em, eti.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

                        EmailSender.SendEmail(siteName, em);

                        // Store new next run time to subscription object
                        subscription.ReportSubscriptionNextPostDate = SchedulingHelper.GetNextTime(SchedulingHelper.DecodeInterval(subscription.ReportSubscriptionInterval), subscription.ReportSubscriptionNextPostDate);
                        subscription.ReportSubscriptionLastPostDate = DateTime.Now;

                        using (CMSActionContext context = new CMSActionContext())
                        {
                            // Don't log send time in event log
                            context.LogEvents = false;
                            context.TouchParent = false;

                            ReportSubscriptionInfoProvider.SetReportSubscriptionInfo(subscription);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Creates attachment for email 
        /// </summary>
        /// <param name="name">Name of the attachment</param>
        /// <param name="mediaType">Type of the attachment (img, text)</param>
        /// <param name="isInline">Indicates whether this attachment is added as inline</param>
        /// <param name="guid">Guid for inline attachment (is ignored for non inline attachments)</param>
        /// <param name="ms">Memory stream with attachment data</param>
        private EmailAttachment CreateAttachment(String name, String mediaType, bool isInline, Guid guid, SystemIO.MemoryStream ms)
        {
            EmailAttachment att = new EmailAttachment(ms, name);

            if (isInline)
            {
                att.ContentDisposition.Inline = true;
                att.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                att.ContentId = guid.ToString();
            }

            att.ContentType.Name = name;
            att.ContentType.MediaType = mediaType;
            att.TransferEncoding = EmailHelper.TransferEncoding;

            return att;
        }


        /// <summary>
        /// Renders control to string 
        /// </summary>
        /// <param name="ctrl">Control to render</param>
        /// <param name="itemName">Repor item name</param>
        /// <param name="data">Datarow with report parameters</param>
        /// <param name="rsi">Subscription object</param>
        private String RenderControlToString(AbstractReportControl ctrl, String itemName, DataRow data, ReportSubscriptionInfo rsi)
        {
            try
            {
                ctrl.EmailMode = true;
                ctrl.Parameter = itemName;
                ctrl.RenderCssClasses = true;
                ctrl.ReportParameters = data;
                ctrl.ReportSubscriptionSiteID = rsi.ReportSubscriptionSiteID;
                ctrl.SendOnlyNonEmptyDataSource = rsi.ReportSubscriptionOnlyNonEmpty;
                ctrl.SubscriptionInfo = rsi;

                String interval = rsi.ReportSubscriptionSettings["reportinterval"] as String;
                ctrl.SetDefaultDynamicMacros(HitsIntervalEnumFunctions.StringToHitsConversion(interval));

                // Change siteID from context to subscription site id
                ctrl.AllParameters.Add("CMSContextCurrentSiteID", rsi.ReportSubscriptionSiteID);

                ctrl.ReloadData(true);

                // Render control to string
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                Html32TextWriter writer2 = new Html32TextWriter(sw);
                ctrl.RenderControl(writer2);
                return sb.ToString();
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("Reporting", "ReportSubscriptionSender", e, 0, "Report item name:" + itemName);
                return String.Empty;
            }
        }


        /// <summary>
        /// Creates macro resolver for report subscription
        /// </summary>
        /// <param name="ri">Report info</param>
        /// <param name="rsi">Report subscription info</param>
        /// <param name="si">Site info</param>
        /// <param name="rec">Recipient email</param>
        /// <param name="defaultCss">Report default CSS</param>
        /// <param name="content">Report content</param>
        public static MacroResolver CreateSubscriptionMacroResolver(ReportInfo ri, ReportSubscriptionInfo rsi, SiteInfo si, String rec, String defaultCss = null, String content = null)
        {
            // Add object report and report subscription to macro resolver
            MacroResolver resolver = MacroResolver.GetInstance();
            resolver.SetNamedSourceData("Report", ri);
            resolver.SetNamedSourceData("ReportSubscription", rsi);

            // Add solo parameters
            var namedData = new Dictionary<string, object>
            {
                { "CurrentEmail", HttpUtility.UrlEncode(rec) }
            };

            if (si != null)
            {
                var domain = GetSiteDomain(si);

                // Add scheme to domain
                namedData.Add("SiteDomain", domain);

                if (rsi != null)
                {
                    namedData.Add("UnsubscriptionLink", $"{domain}/CMSModules/Reporting/CMSPages/Unsubscribe.aspx?guid={rsi.ReportSubscriptionGUID}&email={HttpUtility.UrlEncode(rec)}");
                }
            }

            namedData.Add("ItemName", GetItemName(rsi));
            namedData.Add("defaultsubscriptioncss", defaultCss);
            namedData.Add("subscriptionbody", content);

            resolver.SetNamedSourceData(namedData, false);

            return resolver;
        }


        private static string GetSiteDomain(SiteInfo siteInfo)
        {
            // Get site domain
            var domain = siteInfo.DomainName.TrimEnd('/');

            // If context defined
            if (HttpContext.Current != null)
            {
                // Test if domain contains app path
                if (!domain.Contains("/"))
                {
                    // Get virtual path and ensure '/'
                    String appPath = SystemContext.ApplicationPath;
                    if (!String.IsNullOrEmpty(appPath))
                    {
                        // Add path to domain
                        domain += "/" + appPath.Trim('/');
                    }
                }
            }

            return RequestContext.CurrentScheme + "://" + domain;
        }


        /// <summary>
        /// Adds data to collection in HTTP context items
        /// </summary>
        /// <param name="key">Key in http context</param>
        /// <param name="item">Key in item's collections(graph,table,..) </param>
        /// <param name="data">Data to store</param>
        public static void AddToRequest(String key, String item, byte[] data)
        {
            if (AbstractStockHelper<RequestStockHelper>.Contains(key))
            {
                Dictionary<string, byte[]> arr = AbstractStockHelper<RequestStockHelper>.GetItem(key) as Dictionary<string, byte[]>;
                arr?.Add(item, data);
                AbstractStockHelper<RequestStockHelper>.Add(key, arr);
            }
            else
            {
                Dictionary<string, byte[]> arr = new Dictionary<string, byte[]>();
                arr.Add(item, data);
                AbstractStockHelper<RequestStockHelper>.Add(key, arr);
            }
        }


        /// <summary>
        /// Creates type and name string identification of subscription item.
        /// </summary>
        /// <param name="rsi">Report subscription object</param>        
        public static String GetItemName(ReportSubscriptionInfo rsi)
        {
            String displayName = String.Empty;
            String type = String.Empty;

            if (rsi.ReportSubscriptionGraphID != 0)
            {
                ReportGraphInfo rgi = ReportGraphInfoProvider.GetReportGraphInfo(rsi.ReportSubscriptionGraphID);
                if (rgi != null)
                {
                    displayName = rgi.GraphDisplayName;
                    type = ResHelper.GetString("ReportItemType.graph").ToLowerInvariant();
                }
            }

            if (rsi.ReportSubscriptionTableID != 0)
            {
                ReportTableInfo rti = ReportTableInfoProvider.GetReportTableInfo(rsi.ReportSubscriptionTableID);
                if (rti != null)
                {
                    displayName = rti.TableDisplayName;
                    type = ResHelper.GetString("ReportItemType.table").ToLowerInvariant();
                }
            }

            if (rsi.ReportSubscriptionValueID != 0)
            {
                ReportValueInfo rvi = ReportValueInfoProvider.GetReportValueInfo(rsi.ReportSubscriptionValueID);
                if (rvi != null)
                {
                    displayName = rvi.ValueDisplayName;
                    type = ResHelper.GetString("ReportItemType.value").ToLowerInvariant();
                }
            }

            if (displayName != String.Empty)
            {
                return String.Format(ResHelper.GetString("reportsubscription.itemnameformat"), type, displayName);
            }

            return String.Empty;
        }


        /// <summary>
        /// Resolves metafiles in the given report HTML. Returns HTML with resolved metafiles
        /// </summary>
        /// <param name="reportName">Report name</param>
        /// <param name="html">Report HTML</param>
        public static string ResolveMetaFiles(string reportName, string html)
        {
            // Find all inline images
            var coll = InlineImageRegex.Matches(html);

            foreach (Match m in coll)
            {
                var guid = ValidationHelper.GetGuid(m.Groups[1], Guid.Empty);
                if (guid != Guid.Empty)
                {
                    // For all found images, find according metafile
                    var mfi = MetaFileInfoProvider.GetMetaFileInfo(guid, null, true);
                    if (mfi != null)
                    {
                        // If metafile found add binary representation to inline attachments
                        AddToRequest(reportName, "g" + guid, mfi.MetaFileBinary);

                        html = html.Replace(m.Value, "##InlineImage##" + guid + "##InlineImage##");
                    }
                }
            }

            return html;
        }

        #endregion
    }
}
