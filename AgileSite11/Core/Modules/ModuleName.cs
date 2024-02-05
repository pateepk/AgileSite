using System;

namespace CMS.Core
{
    /// <summary>
    /// Constants for module names
    /// </summary>
    public static class ModuleName
    {
        #region "Constants"

        /// <summary>
        /// CMS.
        /// </summary>
        public const string CMS = "CMS";

        /// <summary>
        /// Custom system module
        /// </summary>
        public const string CUSTOMSYSTEM = "CMS.CustomSystemModule";

        /// <summary>
        /// Content.
        /// </summary>
        public const string CONTENT = "CMS.Content";

        /// <summary>
        /// Continuous integration
        /// </summary>
        public const string CONTINUOUSINTEGRATION = "CMS.ContinuousIntegration";

        /// <summary>
        /// Event log.
        /// </summary>
        public const string EVENTLOG = "CMS.EventLog";

        /// <summary>
        /// Emails.
        /// </summary>
        [Obsolete("Use " + nameof(EMAILENGINE) + " instead.", true)]
        public const string EMAIL = "CMS.Email";

        /// <summary>
        /// Email engine.
        /// </summary>
        public const string EMAILENGINE = "CMS.EmailEngine";

        /// <summary>
        /// Portal engine.
        /// </summary>
        public const string PORTALENGINE = "CMS.PortalEngine";

        /// <summary>
        /// Categories.
        /// </summary>
        public const string CATEGORIES = "CMS.Categories";

        /// <summary>
        /// Blogs.
        /// </summary>
        public const string BLOGS = "CMS.Blog";

        /// <summary>
        /// Ecommerce.
        /// </summary>
        public const string ECOMMERCE = "CMS.Ecommerce";

        /// <summary>
        /// Event manager.
        /// </summary>
        public const string EVENTMANAGER = "CMS.EventManager";

        /// <summary>
        /// Forums.
        /// </summary>
        public const string FORUMS = "CMS.Forums";

        /// <summary>
        /// Friends.
        /// </summary>
        public const string FRIENDS = "CMS.Friends";

        /// <summary>
        /// Message board.
        /// </summary>
        public const string MESSAGEBOARD = "CMS.MessageBoards";

        /// <summary>
        /// Messaging.
        /// </summary>
        public const string MESSAGING = "CMS.Messaging";

        /// <summary>
        /// Newsletter.
        /// </summary>
        public const string NEWSLETTER = "CMS.Newsletter";

        /// <summary>
        /// Notifications.
        /// </summary>
        public const string NOTIFICATIONS = "CMS.Notifications.Web.UI";

        /// <summary>
        /// Polls.
        /// </summary>
        public const string POLLS = "CMS.Polls";

        /// <summary>
        /// Permissions.
        /// </summary>
        public const string PERMISSIONS = "CMS.Permissions";

        /// <summary>
        /// Personas (a.k.a. Persona Based Recommendations).
        /// </summary>
        public const string PERSONAS = "CMS.Personas";

        /// <summary>
        /// Reporting.
        /// </summary>
        public const string REPORTING = "CMS.Reporting";

        /// <summary>
        /// Synchronization.
        /// </summary>
        public const string SYNCHRONIZATION = "CMS.Synchronization";

        /// <summary>
        /// Synchronization engine.
        /// </summary>
        public const string SYNCHRONIZATIONENGINE = "CMS.SynchronizationEngine";

        /// <summary>
        /// Scoring.
        /// </summary>
        public const string SCORING = "CMS.Scoring";

        /// <summary>
        /// Staging.
        /// </summary>
        public const string STAGING = "CMS.Staging";

        /// <summary>
        /// Staging WSE3 communication service.
        /// </summary>
        public const string WSE3SERVICE = "CMS.WSE3Service";

        /// <summary>
        /// UI Personalization.
        /// </summary>
        public const string UIPERSONALIZATION = "CMS.UIPersonalization";

        /// <summary>
        /// Web analytics.
        /// </summary>
        public const string WEBANALYTICS = "CMS.WebAnalytics";

        /// <summary>
        /// Globalization
        /// </summary>
        public const string GLOBALIZATION = "CMS.Globalization";

        /// <summary>
        /// Site.
        /// </summary>
        public const string SITE = "CMS.Site";

        /// <summary>
        /// Custom tables.
        /// </summary>
        public const string CUSTOMTABLES = "CMS.CustomTables";

        /// <summary>
        /// Search
        /// </summary>
        public const string SEARCHLUCENE3 = "CMS.Search.Lucene3";

        /// <summary>
        /// Search
        /// </summary>
        public const string SEARCH = "CMS.Search";

        /// <summary>
        /// Search
        /// </summary>
        public const string SEARCHTEXTEXTRACTORS = "CMS.Search.TextExtractors";

        /// <summary>
        /// Search Lucene
        /// </summary>
        public const string SEARCHLUCENE = "CMS.Search.Lucene";

        /// <summary>
        /// Modules
        /// </summary>
        public const string MODULES = "CMS.Modules";

        /// <summary>
        /// Membership
        /// </summary>
        public const string MEMBERSHIP = "CMS.Membership";

        /// <summary>
        /// Macro engine
        /// </summary>
        public const string MACROENGINE = "CMS.MacroEngine";

        /// <summary>
        /// Protection.
        /// </summary>
        public const string PROTECTION = "CMS.Protection";

        /// <summary>
        /// Relationships.
        /// </summary>
        public const string RELATIONSHIPS = "CMS.Relationships";

        /// <summary>
        /// Banner management
        /// </summary>
        public const string BANNERMANAGEMENT = "CMS.BannerManagement";

        /// <summary>
        /// Taxonomy
        /// </summary>
        public const string TAXONOMY = "CMS.Taxonomy";

        /// <summary>
        /// Web farm sync.
        /// </summary>
        public const string WEBFARMSYNC = "CMS.WebFarmSyncHelper";

        /// <summary>
        /// Community.
        /// </summary>
        public const string COMMUNITY = "CMS.Community";

        /// <summary>
        /// Media library.
        /// </summary>
        public const string MEDIALIBRARY = "CMS.MediaLibrary";

        /// <summary>
        /// Biz Forms.
        /// </summary>
        public const string BIZFORM = "CMS.Form";

        /// <summary>
        /// Groups.
        /// </summary>
        public const string GROUPS = "CMS.Groups";

        /// <summary>
        /// Tree engine.
        /// </summary>
        public const string DOCUMENTENGINE = "CMS.DocumentEngine";

        /// <summary>
        /// Workflow engine.
        /// </summary>
        public const string WORKFLOWENGINE = "CMS.Workflow";

        /// <summary>
        /// Automation engine.
        /// </summary>
        public const string AUTOMATION = "CMS.Automation";

        /// <summary>
        /// Form engine.
        /// </summary>
        public const string FORMENGINE = "CMS.FormEngine";

        /// <summary>
        /// Form engine web UI.
        /// </summary>
        public const string FORMENGINEWEBUI = "CMS.FormEngine.Web.UI";

        /// <summary>
        /// Scheduler.
        /// </summary>
        public const string SCHEDULER = "CMS.Scheduler";
        
        /// <summary>
        /// Windows services engine.
        /// </summary>
        public const string WINSERVICEENGINE = "CMS.WinServiceEngine";

        /// <summary>
        /// Settings provider.
        /// </summary>
        public const string BASE = "CMS.Base";

        /// <summary>
        /// File manager.
        /// </summary>
        public const string FILEMANAGER = "CMS.FileManager";

        /// <summary>
        /// Localization.
        /// </summary>
        public const string LOCALIZATION = "CMS.Localization";

        /// <summary>
        /// Licenses.
        /// </summary>
        public const string LICENSE = "CMS.License";

        /// <summary>
        /// Global helper
        /// </summary>
        public const string HELPERS = "CMS.Helpers";

        /// <summary>
        /// Data engine.
        /// </summary>
        public const string DATAENGINE = "CMS.DataEngine";

        /// <summary>
        /// Online marketing.
        /// </summary>
        public const string ONLINEMARKETING = "CMS.OnlineMarketing";

        /// <summary>
        /// Contact management.
        /// </summary>
        public const string CONTACTMANAGEMENT = "CMS.ContactManagement";

        /// <summary>
        /// A/B tests.
        /// </summary>
        public const string ABTEST = "CMS.ABTest";

        /// <summary>
        /// MVT tests.
        /// </summary>
        public const string MVTEST = "CMS.MVTest";

        /// <summary>
        /// Content personalization.
        /// </summary>
        public const string CONTENTPERSONALIZATION = "CMS.ContentPersonalization";

        /// <summary>
        /// WebDAV.
        /// </summary>
        public const string WEBDAV = "CMS.WebDAV";

        /// <summary>
        /// Import/export module.
        /// </summary>
        public const string IMPORTEXPORT = "CMS.ImportExport";

        /// <summary>
        /// URL rewriting module.
        /// </summary>
        public const string URLREWRITING = "CMS.URLRewritingEngine";

        /// <summary>
        /// Social media module
        /// </summary>
        public const string SOCIALMEDIA = "CMS.SocialMedia";

        /// <summary>
        /// Translation services module
        /// </summary>
        public const string TRANSLATIONSERVICES = "CMS.TranslationServices";

        /// <summary>
        /// Translation services module
        /// </summary>
        public const string CHAT = "CMS.Chat";

        /// <summary>
        /// Abuse report module
        /// </summary>
        public const string ABUSEREPORT = "CMS.AbuseReport";

        /// <summary>
        /// Bad words module
        /// </summary>
        public const string BADWORDS = "CMS.Badwords";

        /// <summary>
        /// IO
        /// </summary>
        public const string IO = "CMS.IO";

        /// <summary>
        /// Design
        /// </summary>
        public const string DESIGN = "CMS.Design";

        /// <summary>
        /// Social marketing
        /// </summary>
        public const string SOCIALMARKETING = "CMS.SocialMarketing";

        /// <summary>
        /// SharePoint
        /// </summary>
        public const string SHAREPOINT = "CMS.SharePoint";

        /// <summary>
        /// Online forms.
        /// </summary>
        public const string ONLINEFORMS = "CMS.OnlineForms";

        /// <summary>
        /// Online forms web UI.
        /// </summary>
        public const string ONLINEFORMSWEBUI = "CMS.OnlineForms.Web.UI";

        /// <summary>
        /// Integration with Strands Recommender.
        /// </summary>
        public const string STRANDSRECOMMENDER = "CMS.StrandsRecommender";

        /// <summary>
        /// Output filer.
        /// </summary>
        public const string OUTPUTFILTER = "CMS.OutputFilter";

        /// <summary>
        /// Issues.
        /// </summary>
        public const string ISSUES = "CMS.Issues";

        /// <summary>
        /// Device profiles
        /// </summary>
        public const string DEVICEPROFILES = "CMS.DeviceProfiles";

        /// <summary>
        /// Widgets
        /// </summary>
        public const string WIDGETS = "CMS.Widgets";

        /// <summary>
        /// Windows Identity Foundation Integration
        /// </summary>
        public const string WIFINTEGRATION = "CMS.WIFIntegration";

        /// <summary>
        /// Web API
        /// </summary>
        public const string WEBAPI = "CMS.WebApi";

        /// <summary>
        /// Application Dashboard
        /// </summary>
        public const string APPLICATIONDASHBOARD = "CMS.ApplicationDashboard.Web.UI";

        /// <summary>
        /// Routing
        /// </summary>
        public const string ROUTING = "CMS.Routing";
        
        /// <summary>
        /// Activities.
        /// </summary>
        public const string ACTIVITIES = "CMS.Activities";

        #endregion
    }
}
