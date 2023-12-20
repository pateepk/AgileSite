using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.ImportExport;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Abstract report control.
    /// </summary>
    public class AbstractReportControl : InlineUserControl
    {
        #region "Variables"

        private Guid mSavedReportGuid = Guid.Empty;
        private HitsIntervalEnum mRangeInterval = HitsIntervalEnum.None;

        private DataRow mReportParameters;
        private DataRow mContextParameters;
        private QueryDataParameters mAllParameters;
        private MacroResolver mContextResolver;

        private Unit mTableFirstColumnWidth = Unit.Empty;
        private bool mEnableExport = true;
        private bool mEnableSubscription = true;
        private bool mSendOnlyNonEmptyDataSource = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Saved report guid.
        /// </summary>
        public Guid SavedReportGuid
        {
            get
            {
                return mSavedReportGuid;
            }
            set
            {
                mSavedReportGuid = value;
            }
        }


        /// <summary>
        /// Colors assigned to series.
        /// </summary>
        public Dictionary<string, Color> Colors
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether control is used in live site.
        /// </summary>
        public override bool IsLiveSite
        {
            get;
            set;
        }


        /// <summary>
        /// Connection string for query. Used in database separation.
        /// </summary>
        public virtual String ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Report's subscription site ID (used in automatic generation 
        /// </summary>
        public int ReportSubscriptionSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true export is allowed
        /// </summary>
        public virtual bool EnableExport
        {
            get
            {
                return mEnableExport;
            }
            set
            {
                mEnableExport = value;
            }
        }


        /// <summary>
        /// If true, subscription is enabled
        /// </summary>
        public virtual bool EnableSubscription
        {
            get
            {
                return mEnableSubscription;
            }
            set
            {
                mEnableSubscription = value;
            }
        }


        /// <summary>
        /// Computex width if percent width is set.
        /// </summary>
        public virtual int ComputedWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition for reports (replace  ##WHERE##)
        /// </summary>
        public string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// First width of the column
        /// </summary>
        public Unit TableFirstColumnWidth
        {
            get
            {
                return mTableFirstColumnWidth;
            }
            set
            {
                mTableFirstColumnWidth = value;
            }
        }


        /// <summary>
        /// Return loaded dataset 
        /// </summary>
        public DataSet ReportDataSet
        {
            get;
            private set;
        }


        /// <summary>
        /// Special macros used for report macro resolving
        /// </summary>
        public string[,] DynamicMacros
        {
            get;
            set;
        }


        /// <summary>
        /// Order by clause for reports (replace  ##ORDERBY##)
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Selected columns for reports (replace ##COLUMNS##)
        /// </summary>
        public string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Top N specification for reports (replace where ##TOPN##)
        /// </summary>
        public int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// If true, control is rendered for email.
        /// </summary>
        public virtual bool EmailMode
        {
            get;
            set;
        }


        /// <summary>
        /// Width of chart image in percent versus displayable zone.
        /// </summary>
        public int GraphImageWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Name for report caching.
        /// </summary>
        public string CacheItemName
        {
            get;
            set;
        }


        /// <summary>
        /// Get(set) selected interval of graph type (year,month,..)
        /// </summary>
        public String SelectedInterval
        {
            get;
            set;
        }


        /// <summary>
        /// Minutes for cache image holding.
        /// </summary>
        public int CacheMinutes
        {
            get;
            set;
        }


        /// <summary>
        /// Saved report ID.
        /// </summary>
        public int SavedReportID
        {
            get;
            set;
        }


        /// <summary>
        /// Report parameters.
        /// </summary>
        public virtual DataRow ReportParameters
        {
            get
            {
                return mReportParameters;
            }
            set
            {
                mReportParameters = value;
                mAllParameters = null;
            }
        }


        /// <summary>
        /// Context parameters.
        /// </summary>
        public virtual DataRow ContextParameters
        {
            get
            {
                return mContextParameters ?? (mContextParameters = GetContextParametersDataRow());
            }
            set
            {
                mContextParameters = value;
                mAllParameters = null;
            }
        }


        /// <summary>
        /// If true, only reports with non empty datasource is sent  (used in subscriptions).
        /// </summary>
        public virtual bool SendOnlyNonEmptyDataSource
        {
            get
            {
                return mSendOnlyNonEmptyDataSource;
            }
            set
            {
                mSendOnlyNonEmptyDataSource = value;
            }
        }


        /// <summary>
        /// All parameters.
        /// </summary>
        public virtual QueryDataParameters AllParameters
        {
            get
            {
                if (mAllParameters == null)
                {
                    // Apply 'range' time filters
                    ApplyTimeParameters();

                    mAllParameters = ConvertDataRowsToParams(ContextParameters, ReportParameters);
                }

                return mAllParameters;
            }
            set
            {
                mAllParameters = value;
            }
        }


        /// <summary>
        /// Query text.
        /// </summary>
        public virtual string QueryText
        {
            get;
            set;
        }


        /// <summary>
        /// Interval of time range.
        /// </summary>
        public HitsIntervalEnum RangeInterval
        {
            get
            {
                return mRangeInterval;
            }
            set
            {
                mRangeInterval = value;
            }
        }


        /// <summary>
        /// Value of range interval.
        /// </summary>
        public int RangeValue
        {
            get;
            set;
        }


        /// <summary>
        /// Query is stored procedure.
        /// </summary>
        public virtual bool QueryIsStoredProcedure
        {
            get;
            set;
        }


        /// <summary>
        /// Web part context resolver.
        /// </summary>
        public MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    mContextResolver = MacroContext.CurrentResolver.CreateChild();

                    // Set the additional data
                    mContextResolver.SetNamedSourceData(AllParameters.ToDictionary(p => p.Name, p => p.Value), isPrioritized: false);
                }
                return mContextResolver;
            }
        }


        /// <summary>
        /// Inidcates if CSS classes should be rendered.
        /// </summary>
        public bool RenderCssClasses
        {
            get;
            set;
        }


        /// <summary>
        /// Report item name used for caching.
        /// </summary>
        public string ReportItemName
        {
            get;
            set;
        }


        /// <summary>
        /// Item type (graph,table or value).
        /// </summary>
        public ReportItemType ItemType
        {
            get;
            set;
        }


        /// <summary>
        /// CacheDependencies.
        /// </summary>
        public string CacheDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// Report subscription information - used for sending reports by email.
        /// </summary>
        public ReportSubscriptionInfo SubscriptionInfo
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public AbstractReportControl()
        {
            IsLiveSite = false;
        }


        /// <summary>
        /// Loads default parameters from dataset definition.
        /// </summary>
        /// <param name="datasetXml">Dataset XML</param>
        /// <param name="datasetXmlSchema">Dataset XML Schema</param>
        public void LoadDefaultParameters(string datasetXml, string datasetXmlSchema)
        {
            // Check whether scheme and data are defined
            if (!String.IsNullOrEmpty(datasetXml) && !String.IsNullOrEmpty(datasetXmlSchema))
            {
                // Create dataset object
                DataSet ds = new DataSet();

                // Load schnema
                StringReader sr = new StringReader(datasetXmlSchema);
                ds.ReadXmlSchema(sr);

                // Load data
                ds.TryReadXml(datasetXml);

                // Check whether dataset contains at least one row
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    ReportParameters = ds.Tables[0].Rows[0];
                }
            }
        }


        /// <summary>
        /// Sets FromDate and ToDate by given values (computed from actual datetime).
        /// </summary>
        public void ApplyTimeParameters()
        {
            if ((ReportParameters != null) && (RangeValue != 0))
            {
                if (!ReportParameters.Table.Columns.Contains("FromDate"))
                {
                    ReportParameters.Table.Columns.Add("FromDate", typeof(DateTime));
                }

                if (!ReportParameters.Table.Columns.Contains("ToDate"))
                {
                    ReportParameters.Table.Columns.Add("ToDate", typeof(DateTime));
                }

                ReportParameters["FromDate"] = ComputeDate();
                ReportParameters["ToDate"] = DateTime.Now;
            }
        }


        /// <summary>
        /// Register JS function for opening subscription dialog.
        /// </summary>
        /// <param name="reportID">Report ID</param>
        /// <param name="itemIDType">Type of item IO parameter (reportid,tableid,valueid)</param>
        /// <param name="itemID">Item ID (report,table,value)</param>
        /// <param name="menuCont">Control for menu container</param>
        public void RegisterSubscriptionScript(int reportID, string itemIDType, int itemID, ContextMenuContainer menuCont)
        {
            if (EmailMode)
            {
                // Don't register script for email mode
                return;
            }

            // Show enable subscription only for authenticated users with subscribe or modify permissions
            var cui = MembershipContext.AuthenticatedUser;
            EnableSubscription &= (AuthenticationHelper.IsAuthenticated() && (cui.IsAuthorizedPerResource("cms.reporting", "subscribe") || cui.IsAuthorizedPerResource("cms.reporting", "modify")));

            if (EnableSubscription || EnableExport)
            {
                ScriptHelper.RegisterDialogScript(Page);

                // Register context menu
                RegisterContextMenu(menuCont);

                // Register script for subscription
                String interval = SelectedInterval;
                if (String.IsNullOrEmpty(interval) || (interval == "None"))
                {
                    interval = HitsIntervalEnumFunctions.HitsConversionToString(RangeInterval);
                }
                String path = IsLiveSite ? "~/CMSModules/Reporting/LiveDialogs/EditSubscription.aspx" : "~/CMSModules/Reporting/Dialogs/EditSubscription.aspx";
                String subscriptionScript = String.Format("function Process_Context_Menu_{4}(){{modalDialog('{0}?reportid={1}&parameters={2}&interval={3}&{5}={6}','Subscription',{7},{8}); return false;}}",
                    ResolveUrl(path), reportID, AnalyticsHelper.GetQueryStringParameters(ReportParameters), interval, ClientID, itemIDType, itemID,
                                AnalyticsHelper.SUBSCRIPTION_WINDOW_WIDTH, AnalyticsHelper.SUBSCRIPTION_WINDOW_HEIGHT);
                ScriptHelper.RegisterClientScriptBlock(Page, typeof(String), "Subscription" + ClientID, ScriptHelper.GetScript(subscriptionScript));
            }
        }


        /// <summary>
        /// Gets the data from the database.
        /// </summary>
        public virtual DataSet LoadData()
        {
            // Prepare the query
            DataSet ds = null;

            using (var cs = new CachedSection<DataSet>(ref ds, CacheMinutes, true, CacheItemName, "reporting", ReportItemName, ReportInfoProvider.ReportItemTypeToString(ItemType)))
            {
                if (cs.LoadData)
                {
                    // Get the data
                    ds = LoadDataInternal();

                    // Add to the cache
                    if (cs.Cached)
                    {
                        //extract reportItemName from report;itemname string
                        string[] parts = ReportItemName.Split(new char[] { ';' });
                        string itemName = String.Empty;
                        if (parts.Length == 2)
                        {
                            itemName = parts[1];
                        }

                        // Prepare the cache dependency
                        string cacheDependency = String.Empty;

                        switch (ItemType)
                        {
                            case ReportItemType.Table:
                                cacheDependency = "reporting.reporttable|byname|" + itemName;
                                break;

                            case ReportItemType.Value:
                                cacheDependency = "reporting.reportvalue|byname|" + itemName;
                                break;

                            case ReportItemType.Graph:
                                cacheDependency = "reporting.reportgraph|byname|" + itemName;
                                break;
                        }

                        string[] dependencies = new string[] { cacheDependency, CacheDependencies };

                        cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                    }

                    cs.Data = ds;
                }
            }

            ReportDataSet = ds;

            return ds;
        }


        /// <summary>
        /// Export the selected dataset
        /// </summary>
        public void ProcessExport(String displayName)
        {
            if (EnableExport && !EmailMode)
            {
                // If export allowed test postback with control unique ID
                string argument = Request[Page.postEventArgumentID];
                string target = Request[Page.postEventSourceID];
                if (target == UniqueID)
                {
                    // Send dataset to response stream
                    var deh = new DataExportHelper(ReportDataSet);
                    deh.FileName = displayName;
                    deh.ExportData(DataExportHelper.GetDataExportFormatEnum(argument), Page.Response);
                }
            }
        }


        /// <summary>
        /// Registers context menu container for control
        /// </summary>
        /// <param name="menuCont">Container menu object</param>
        public void RegisterContextMenu(ContextMenuContainer menuCont)
        {
            string exportScript =
"function Report_ContextMenu_" + ClientID + @"(format, param)
{
    // Disable process indicator
    
    // Hide loading control
    window.noProgress = true;

    // No postback for subscription
    if (format == 'subscription') {
        Process_Context_Menu_" + ClientID + @"();
    }
    else {
        " + ControlsHelper.GetPostBackEventReference(this, "##PARAM##", false, false).Replace("'##PARAM##'", "format") + @";
    }

    // Remove event target manually
    var theForm = document.forms[0];
    if (!theForm) {
        theForm = document.form;
    }

    if (theForm) {
        theForm.__EVENTTARGET.value = '';
    }
}";

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ClientID + "_exportScript", ScriptHelper.GetScript(exportScript));

            menuCont.MenuControlPath = "~/CMSAdminControls/ContextMenus/ReportContextMenu.ascx";
            menuCont.MenuID = ClientID + "report_context_menu";
            menuCont.ParentElementClientID = ClientID;

            string enableExport = EnableExport ? "enableexport" : String.Empty;
            string enableSubscription = EnableSubscription ? "enablesubscription" : String.Empty;
            menuCont.MenuParameter = String.Format("'{0};{1};{2}'", ItemType.ToString(), enableExport, enableSubscription);
            menuCont.EnableMouseClick = true;
            menuCont.MouseButton = MouseButtonEnum.Right;
            menuCont.VerticalPosition = VerticalPositionEnum.Cursor;
            menuCont.HorizontalPosition = HorizontalPositionEnum.Cursor;
            menuCont.Enabled = true;
        }


        /// <summary>
        /// Sets the default dynamic macros for general charts (used in reports collecting all intervals in one report (weeks,years,days,...))
        /// </summary>
        /// <param name="interval">Selected interval (days,weeks...)</param>
        public void SetDefaultDynamicMacros(int interval)
        {
            SetDefaultDynamicMacros((HitsIntervalEnum)interval);
        }


        /// <summary>
        /// Sets the default dynamic macros for general charts (used in reports collecting all intervals in one report (weeks,years,days,...))
        /// </summary>
        /// <param name="interval">Selected interval (days,weeks...)</param>
        public void SetDefaultDynamicMacros(String interval)
        {
            SetDefaultDynamicMacros(HitsIntervalEnumFunctions.StringToHitsConversion(interval));
        }


        /// <summary>
        /// Sets the default dynamic macros for general charts (used in reports collecting all intervals in one report (weeks,years,days,...))
        /// </summary>
        /// <param name="interval">Selected interval (days,weeks...)</param>
        public void SetDefaultDynamicMacros(HitsIntervalEnum interval)
        {
            string intervalName = String.Empty;
            string tableName = String.Empty;
            string xAxisTitle = String.Empty;
            string xAxisFormat = String.Empty;
            string columns = String.Empty;
            string xValueFormat = String.Empty;

            switch (interval)
            {
                case HitsIntervalEnum.Year:
                    intervalName = "year";
                    tableName = "Analytics_YearHits";
                    xAxisTitle = GetString("reports_general.label_years");
                    xAxisFormat = "{yyyy}";
                    columns = "[Date]";
                    xValueFormat = "{yyyy}";
                    break;

                case HitsIntervalEnum.Month:
                    intervalName = "month";
                    tableName = "Analytics_MonthHits";
                    xAxisTitle = GetString("reports_general.label_months");
                    xAxisFormat = "y";
                    columns = "[Date]";
                    xValueFormat = "{y}";
                    break;

                case HitsIntervalEnum.Week:
                    intervalName = "week";
                    tableName = "Analytics_WeekHits";
                    xAxisTitle = GetString("reports_general.label_weeks");
                    columns = "CONVERT (NVARCHAR(2),DATEPART(wk,[Date]))+'/'+CONVERT (NVARCHAR(4),DATEPART(YEAR,[Date]))";
                    break;


                case HitsIntervalEnum.Day:
                    intervalName = "day";
                    tableName = "Analytics_DayHits";
                    xAxisTitle = GetString("reports_general.label_days");
                    xAxisFormat = "d";
                    columns = "[Date]";
                    xValueFormat = "{dddd, MMMM d, yyyy}";
                    break;

                case HitsIntervalEnum.Hour:
                    intervalName = "hour";
                    tableName = "Analytics_HourHits";
                    xAxisTitle = GetString("reports_general.label_hours");
                    xAxisFormat = "g";
                    columns = "[Date]";
                    xValueFormat = "{dddd, MMMM d, yyyy HH:mm}";
                    break;
            }

            // Resolve data macros
            string[,] dynamicMacros = new string[6, 2];
            dynamicMacros[0, 0] = "Interval";
            dynamicMacros[0, 1] = intervalName;
            dynamicMacros[1, 0] = "AnalyticsTable";
            dynamicMacros[1, 1] = tableName;
            dynamicMacros[2, 0] = "XAxisTitle";
            dynamicMacros[2, 1] = xAxisTitle;
            dynamicMacros[3, 0] = "XAxisFormat";
            dynamicMacros[3, 1] = xAxisFormat;
            dynamicMacros[4, 0] = "columns";
            dynamicMacros[4, 1] = columns;
            dynamicMacros[5, 0] = "xValueFormat";
            dynamicMacros[5, 1] = xValueFormat;

            DynamicMacros = dynamicMacros;
        }


        /// <summary>
        /// Reload data ready to override.
        /// </summary>
        /// <param name="forceLoad">Force reload data</param>
        public virtual void ReloadData(bool forceLoad)
        {
            // Do nothing in base class
        }


        /// <summary>
        /// Check if control is part of givven report.
        /// </summary>
        public virtual bool IsValid(ReportInfo report)
        {
            return true;
        }


        /// <summary>
        /// Resolves the string macros.
        /// </summary>
        /// <param name="inputText">Input text</param>
        public virtual string ResolveMacros(string inputText)
        {
            return ContextResolver.ResolveMacros(inputText);
        }


        /// <summary>
        /// Clear context resolver.
        /// </summary>
        public virtual void ClearResolver()
        {
            mContextResolver = null;
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Checks if current user may see report output based on report access settings.
        /// </summary>
        /// <param name="reportInfo">ReportInfo object</param>
        /// <returns>True if report is sent via email, report access is allowed for all or the access is for authenticated and current user is authenticated.</returns>
        protected bool CheckReportAccess(ReportInfo reportInfo)
        {
            return (EmailMode || ((reportInfo != null) && reportInfo.ReportAccess == ReportAccessEnum.All) || AuthenticationHelper.IsAuthenticated());
        }


        /// <summary>
        /// Check if subscribed user has valid permission for the subscription and subscription is allowed on all levels (report/report part/control).
        /// </summary>
        /// <param name="reportInfo">ReportInfo object</param>
        /// <param name="controlSubscriptionEnabled">Indicates if report control allows subscription</param>
        protected bool CheckEmailModeSubscription(ReportInfo reportInfo, bool controlSubscriptionEnabled)
        {
            if (!EmailMode)
            {
                return true;
            }

            if ((SubscriptionInfo == null) || (reportInfo == null))
            {
                return false;
            }

            string siteName = SiteInfoProvider.GetSiteName(ReportSubscriptionSiteID);
            UserInfo subscriber = UserInfoProvider.GetUserInfo(SubscriptionInfo.ReportSubscriptionUserID);
            // Check if subscriber is granted with subscribe or modify permission
            bool hasPermission = UserSecurityHelper.IsAuthorizedPerResource("cms.reporting", "subscribe", siteName, subscriber) || UserSecurityHelper.IsAuthorizedPerResource("cms.reporting", "modify", siteName, subscriber);

            return (hasPermission && EnableSubscription && controlSubscriptionEnabled && reportInfo.ReportEnableSubscription);
        }


        /// <summary>
        /// Initializes report control parameters (<see cref="ReportParameters"/> and <see cref="AllParameters"/> properties).
        /// </summary>
        /// <param name="reportParams">Parameters from report info object</param>
        protected void InitParameters(string reportParams)
        {
            // Set default parametrs directly if not set
            if (ReportParameters == null)
            {
                FormInfo fi = new FormInfo(reportParams);
                ReportParameters = fi.GetDataRow(false);
                fi.LoadDefaultValues(ReportParameters, true);
            }

            // If used via widget - this function ensure showing specific interval from actual time (f.e. last 6 weeks)
            ApplyTimeParameters();

            // Only use base parameters in case of stored procedure
            if (QueryIsStoredProcedure)
            {
                AllParameters = ConvertDataRowsToParams(ReportParameters);
            }
        }


        /// <summary>
        /// Initializes macro resolver (data from <see cref="AllParameters"/> and <see cref="DynamicMacros"/> properties).
        /// </summary>
        protected void InitResolver()
        {
            // Resolve parameters in query
            ContextResolver.SetNamedSourceData(AllParameters.ToDictionary(p => p.Name, p => p.Value), isPrioritized: false);

            // Resolve dynamic data macros
            if (DynamicMacros != null)
            {
                for (int i = 0; i <= DynamicMacros.GetUpperBound(0); i++)
                {
                    ContextResolver.SetNamedSourceData(DynamicMacros[i, 0], DynamicMacros[i, 1]);
                }
            }
        }

        #endregion


        #region "Private methods"

        private static DataRow GetContextParametersDataRow()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("CMSContextCurrentUserID", typeof(int));
            dt.Columns.Add("CMSContextCurrentUserName", typeof(string));
            dt.Columns.Add("CMSContextCurrentUserDisplayName", typeof(string));

            dt.Columns.Add("CMSContextCurrentSiteID", typeof(int));
            dt.Columns.Add("CMSContextCurrentSiteName", typeof(string));
            dt.Columns.Add("CMSContextCurrentSiteDisplayName", typeof(string));

            dt.Columns.Add("CMSContextCurrentDomain", typeof(string));
            dt.Columns.Add("CMSContextCurrentTime", typeof(DateTime));
            dt.Columns.Add("CMSContextCurrentURL", typeof(string));

            dt.Columns.Add("CMSContextCurrentNodeID", typeof(string));
            dt.Columns.Add("CMSContextCurrentCulture", typeof(string));
            dt.Columns.Add("CMSContextCurrentDocumentID", typeof(string));

            dt.Columns.Add("CMSContextCurrentAliasPath", typeof(string));
            dt.Columns.Add("CMSContextCurrentDocumentName", typeof(string));
            dt.Columns.Add("CMSContextCurrentDocumentNamePath", typeof(string));


            // Get context objects
            SiteInfo currentSite = SiteContext.CurrentSite;
            CurrentUserInfo currentUser = MembershipContext.AuthenticatedUser;
            PageInfo currentPageInfo = DocumentContext.CurrentPageInfo;
            DocumentEngine.TreeNode editedDocument = DocumentContext.EditedDocument;

            // Create the parameters
            DataRow dr = dt.NewRow();

            // Current user values
            if (currentUser != null)
            {
                dr["CMSContextCurrentUserID"] = currentUser.UserID;
                dr["CMSContextCurrentUserName"] = currentUser.UserName;
                dr["CMSContextCurrentUserDisplayName"] = currentUser.FullName;
            }

            // Current site values
            if (currentSite != null)
            {
                dr["CMSContextCurrentSiteID"] = currentSite.SiteID;
                dr["CMSContextCurrentSiteName"] = currentSite.SiteName;
                dr["CMSContextCurrentSiteDisplayName"] = currentSite.DisplayName;
                dr["CMSContextCurrentDomain"] = currentSite.DomainName;
            }

            dr["CMSContextCurrentTime"] = DateTime.Now;

            // URL
            if (HttpContext.Current != null)
            {
                dr["CMSContextCurrentURL"] = RequestContext.RawURL;
            }

            // Current page values
            if (currentPageInfo != null)
            {
                dr["CMSContextCurrentNodeID"] = currentPageInfo.NodeID;
                dr["CMSContextCurrentCulture"] = currentPageInfo.DocumentCulture;
                dr["CMSContextCurrentDocumentID"] = currentPageInfo.DocumentID;
                dr["CMSContextCurrentAliasPath"] = currentPageInfo.NodeAliasPath;
                dr["CMSContextCurrentDocumentName"] = currentPageInfo.GetDocumentName();
                dr["CMSContextCurrentDocumentNamePath"] = currentPageInfo.DocumentNamePath;
            }
            else if (editedDocument != null)
            {
                // If exists edited document info use if (form tab)
                dr["CMSContextCurrentNodeID"] = editedDocument.NodeID;
                dr["CMSContextCurrentCulture"] = editedDocument.DocumentCulture;
                dr["CMSContextCurrentDocumentID"] = editedDocument.DocumentID;
                dr["CMSContextCurrentAliasPath"] = editedDocument.NodeAliasPath;
                dr["CMSContextCurrentDocumentName"] = editedDocument.GetDocumentName();
                dr["CMSContextCurrentDocumentNamePath"] = editedDocument.DocumentNamePath;
            }

            // Add new row
            dt.Rows.Add(dr);
            dt.AcceptChanges();

            return dt.Rows[0];
        }


        /// <summary>
        /// Computes date for given time interval (from actual date).
        /// </summary>
        private DateTime ComputeDate()
        {
            int value = RangeValue - 1;
            switch (RangeInterval)
            {
                case HitsIntervalEnum.Year:
                    return DateTime.Now.AddYears(-value);

                case HitsIntervalEnum.Month:
                    return DateTime.Now.AddMonths(-value);

                case HitsIntervalEnum.Week:
                    return DateTime.Now.AddDays(-value * 7);

                case HitsIntervalEnum.Day:
                    return DateTime.Now.AddDays(-value);

                case HitsIntervalEnum.Hour:
                    return DateTime.Now.AddHours(-value);
            }

            return DateTime.Now;
        }


        /// <summary>
        /// Load data from DB.
        /// </summary>
        private DataSet LoadDataInternal()
        {
            // Prepare the query
            string query = ResolveMacros(QueryText);

            // Resolve query macros (##WHERE##,..)
            query = new QueryMacros()
            {
                Where = WhereCondition,
                OrderBy = OrderBy,
                TopN = TopN,
                Columns = Columns
            }.ResolveMacros(query);

            QueryParameters queryObj = new QueryParameters(query, AllParameters, QueryIsStoredProcedure ? QueryTypeEnum.StoredProcedure : QueryTypeEnum.SQLQuery, false);
            queryObj.ConnectionStringName = ConnectionString;

            // For default connection string use connection string from settings
            if (String.IsNullOrEmpty(queryObj.ConnectionStringName))
            {
                queryObj.ConnectionStringName = ReportHelper.GetDefaultReportConnectionString();
            }

            return ConnectionHelper.ExecuteQuery(queryObj);
        }


        /// <summary>
        /// Converts source parameters to query data parameters with type convert (e.g.: "true" as boolean type etc.).
        /// </summary>
        /// <param name="sourceDataRows">Source array of DataRows</param>
        private static QueryDataParameters ConvertDataRowsToParams(params DataRow[] sourceDataRows)
        {
            QueryDataParameters parameters = new QueryDataParameters();

            foreach (var row in sourceDataRows.Where(i => i != null))
            {
                for (var i = 0; i < row.Table.Columns.Count; i++)
                {
                    parameters.Add(row.Table.Columns[i].ColumnName, row[i]);
                }
            }

            return parameters;
        }

        #endregion
    }
}
