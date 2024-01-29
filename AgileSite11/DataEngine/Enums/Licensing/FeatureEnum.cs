using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Feature types.
    /// </summary>
    public enum FeatureEnum
    {
        /// <summary>
        /// Unknown feature.
        /// </summary>
        [EnumStringRepresentation("Unknown")]
        Unknown,

        /// <summary>
        /// Document level permissions.
        /// </summary>
        [EnumStringRepresentation("DocumentLevelPermissions")]
        DocumentLevelPermissions,

        /// <summary>
        /// BizForms.
        /// </summary>
        [EnumStringRepresentation("BizForms")]
        BizForms,

        /// <summary>
        /// Workflow versioning.
        /// </summary>
        [EnumStringRepresentation("WorkflowVersioning")]
        WorkflowVersioning,

        /// <summary>
        /// Advanced workflow.
        /// </summary>
        [EnumStringRepresentation("AdvancedWorkflow")]
        AdvancedWorkflow,

        /// <summary>
        /// Marketing automation.
        /// </summary>
        [EnumStringRepresentation("MarketingAutomation")]
        MarketingAutomation,

        /// <summary>
        /// Forums.
        /// </summary>
        [EnumStringRepresentation("Forums")]
        Forums,

        /// <summary>
        /// Newsletters.
        /// </summary>
        [EnumStringRepresentation("Newsletters")]
        Newsletters,

        /// <summary>
        /// Multilingual.
        /// </summary>
        [EnumStringRepresentation("Multilingual")]
        Multilingual,

        /// <summary>
        /// Staging.
        /// </summary>
        [EnumStringRepresentation("Staging")]
        Staging,

        /// <summary>
        /// Ecommerce.
        /// </summary>
        [EnumStringRepresentation("Ecommerce")]
        Ecommerce,

        /// <summary>
        /// Webfarm.
        /// </summary>
        [EnumStringRepresentation("Webfarm")]
        Webfarm,

        /// <summary>
        /// Polls.
        /// </summary>
        [EnumStringRepresentation("Polls")]
        Polls,

        /// <summary>
        /// Web analytics.
        /// </summary>
        [EnumStringRepresentation("WebAnalytics")]
        WebAnalytics,

        /// <summary>
        /// Blogs.
        /// </summary>
        [EnumStringRepresentation("Blogs")]
        Blogs,

        /// <summary>
        /// Event manager.
        /// </summary>
        [EnumStringRepresentation("EventManager")]
        EventManager,

        /// <summary>
        /// Global administrators.
        /// </summary>
        [EnumStringRepresentation("Administrators")]
        Administrators,

        /// <summary>
        /// Editors.
        /// </summary>
        [EnumStringRepresentation("Editors")]
        Editors,

        /// <summary>
        /// Site members.
        /// </summary>
        [EnumStringRepresentation("SiteMembers")]
        SiteMembers,

        /// <summary>
        /// Documents.
        /// </summary>
        [EnumStringRepresentation("Documents")]
        Documents,

        /// <summary>
        /// Subscribers.
        /// </summary>
        [EnumStringRepresentation("Subscribers")]
        Subscribers,

        /// <summary>
        /// Banned IP.
        /// </summary>
        [EnumStringRepresentation("BannedIP")]
        BannedIP,

        /// <summary>
        /// Community.
        /// </summary>
        [EnumStringRepresentation("Groups")]
        Groups,

        /// <summary>
        /// Custom Tables.
        /// </summary>
        [EnumStringRepresentation("CustomTables")]
        CustomTables,

        /// <summary>
        /// Friends.
        /// </summary>
        [EnumStringRepresentation("Friends")]
        Friends,

        /// <summary>
        /// Notifications.
        /// </summary>
        [EnumStringRepresentation("Notifications")]
        Notifications,

        /// <summary>
        /// Message boards.
        /// </summary>
        [EnumStringRepresentation("MessageBoards")]
        MessageBoards,

        /// <summary>
        /// User contributions.
        /// </summary>
        [EnumStringRepresentation("UserContributions")]
        UserContributions,

        /// <summary>
        /// Windows live id.
        /// </summary>
        [EnumStringRepresentation("WindowsLiveID")]
        WindowsLiveID,

        /// <summary>
        /// On-line users.
        /// </summary>
        [EnumStringRepresentation("OnlineUsers")]
        OnlineUsers,

        /// <summary>
        /// Messaging.
        /// </summary>
        [EnumStringRepresentation("Messaging")]
        Messaging,

        /// <summary>
        /// Active directory import tool.
        /// </summary>
        [EnumStringRepresentation("ADImportTool")]
        ADImportTool,

        /// <summary>
        /// OpenID.
        /// </summary>
        [EnumStringRepresentation("OpenID")]
        OpenID,

        /// <summary>
        /// WebDav.
        /// </summary>
        [EnumStringRepresentation("WebDav")]
        WebDav,

        /// <summary>
        /// Membership.
        /// </summary>
        [EnumStringRepresentation("Membership")]
        Membership,

        /// <summary>
        /// Facebook connect.
        /// </summary>
        [EnumStringRepresentation("FaceBookConnect")]
        FaceBookConnect,

        /// <summary>
        /// Linked in.
        /// </summary>
        [EnumStringRepresentation("LinkedIn")]
        LinkedIn,

        /// <summary>
        /// System integration bus.
        /// </summary>
        [EnumStringRepresentation("IntegrationBus")]
        IntegrationBus,

        /// <summary>
        /// Object versioning.
        /// </summary>
        [EnumStringRepresentation("ObjectVersioning")]
        ObjectVersioning,

        /// <summary>
        /// Health monitoring.
        /// </summary>
        [EnumStringRepresentation("HealthMonitoring")]
        HealthMonitoring,

        /// <summary>
        /// Lead scoring
        /// </summary>
        [EnumStringRepresentation("LeadScoring")]
        LeadScoring,

        /// <summary>
        /// A/B Testing
        /// </summary>
        [EnumStringRepresentation("ABTesting")]
        ABTesting,

        /// <summary>
        /// MV testing
        /// </summary>
        [EnumStringRepresentation("MVTesting")]
        MVTesting,

        /// <summary>
        /// Campaign and conversions
        /// </summary>
        [EnumStringRepresentation("CampaignAndConversions")]
        CampaignAndConversions,

        /// <summary>
        /// Content personalization
        /// </summary>
        [EnumStringRepresentation("ContentPersonalization")]
        ContentPersonalization,

        /// <summary>
        /// Multiple SMTP servers
        /// </summary>
        [EnumStringRepresentation("MultipleSMTPServers")]
        MultipleSMTPServers,

        /// <summary>
        /// Scheduler windows service
        /// </summary>
        [EnumStringRepresentation("SchedulerWinService")]
        SchedulerWinService,

        /// <summary>
        /// Translation services
        /// </summary>
        [EnumStringRepresentation("TranslationServices")]
        TranslationServices,

        /// <summary>
        /// Chat module.
        /// </summary>
        [EnumStringRepresentation("Chat")]
        Chat,

        /// <summary>
        /// SalesForce connector
        /// </summary>
        [EnumStringRepresentation("SalesForce")]
        SalesForce,

        /// <summary>
        /// Banner management
        /// </summary>
        [EnumStringRepresentation("BannerManagement")]
        BannerManagement,

        /// <summary>
        /// Contact management database separation.
        /// </summary>
        [EnumStringRepresentation("DBSeparation")]
        DBSeparation,

        /// <summary>
        /// Device profiles
        /// </summary>
        [EnumStringRepresentation("DeviceProfiles")]
        DeviceProfiles,

        /// <summary>
        /// Newsletter A/B Testing
        /// </summary>
        [EnumStringRepresentation("NewsletterABTesting")]
        NewsletterABTesting,

        /// <summary>
        /// Newsletter tracking (open e-mail, click through and bounces)
        /// </summary>
        [EnumStringRepresentation("NewsletterTracking")]
        NewsletterTracking,

        /// <summary>
        /// Publishing to Facebook and Twitter social networks
        /// </summary>
        [EnumStringRepresentation("SocialMarketing")]
        SocialMarketing,
        
        /// <summary>
        /// Providing access to social media analytics information
        /// </summary>
        [EnumStringRepresentation("SocialMarketingInsights")]
        SocialMarketingInsights,

        /// <summary>
        /// Displaying content to visitors based on Personas
        /// </summary>
        [EnumStringRepresentation("Personas")]
        Personas,

        /// <summary>
        /// SharePoint integration
        /// </summary>
        [EnumStringRepresentation("SharePoint")]
        SharePoint,


        /// <summary>
        /// Continuous integration
        /// </summary>
        [EnumStringRepresentation("ContinuousIntegration")]
        ContinuousIntegration,


        /// <summary>
        /// Contact management with basic functionality
        /// </summary>
        [EnumStringRepresentation("SimpleContactManagement")]
        SimpleContactManagement,


        /// <summary>
        /// Contact management for EMS - full functionality
        /// </summary>
        [EnumStringRepresentation("FullContactManagement")]
        FullContactManagement,


        /// <summary>
        /// Data protection
        /// </summary>
        [EnumStringRepresentation("DataProtection")]
        DataProtection
    }
}