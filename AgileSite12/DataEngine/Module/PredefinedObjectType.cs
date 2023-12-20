using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Predefined object type constants.
    /// </summary>
    public static class PredefinedObjectType
    {
        #region "Group constants"

        /// <summary>
        /// Document object type - Special constant.
        /// </summary>
        public const string GROUP_DOCUMENTS = "##DOCUMENTS##";


        /// <summary>
        /// All objects object type - Group constant.
        /// </summary>
        public const string GROUP_OBJECTS = "##OBJECTS##";


        /// <summary>
        /// Current group.
        /// </summary>
        public const string COMMUNITY_CURRENT_GROUP = "##CURRENT_GROUP##";

        #endregion


        #region "PortalObjectType"

        /// <summary>
        /// Code name for page templates.
        /// </summary>
        public const string PAGETEMPLATE = "cms.pagetemplate";


        /// <summary>
        /// Code name for page template scope.
        /// </summary>
        public const string PAGETEMPLATESCOPE = "cms.pagetemplatescope";


        /// <summary>
        /// Code name for webpart categories.
        /// </summary>
        public const string WEBPARTCATEGORY = "cms.webpartcategory";


        /// <summary>
        /// Code name for personalization.
        /// </summary>
        public const string PERSONALIZATION = "cms.personalization";


        /// <summary>
        /// Code name for dashboard.
        /// </summary>
        public const string DASHBOARD = "cms.dashboard";


        /// <summary>
        /// Code name for transformations.
        /// </summary>
        public const string TRANSFORMATION = "cms.transformation";

        #endregion


        #region "SiteObjectType"

        /// <summary>
        /// Code name for sites.
        /// </summary>
        public const string SITE = "cms.site";

        /// <summary>
        /// Code name for site culture binding.
        /// </summary>
        public const string CULTURESITE = "cms.culturesite";

        /// <summary>
        /// Code name for users.
        /// </summary>
        public const string USER = "cms.user";
        

        /// <summary>
        /// Code name for user settings.
        /// </summary>
        public const string USERSETTINGS = "cms.usersettings";


        /// <summary>
        /// Code name for permissions.
        /// </summary>
        public const string PERMISSION = "cms.permission";


        /// <summary>
        /// Code name for class (document type) permissions.
        /// </summary>
        public const string CLASSPERMISSION = "cms.classpermission";


        /// <summary>
        /// Code name for document global category.
        /// </summary>
        public const string CATEGORY = "cms.category";


        /// <summary>
        /// User culture binding.
        /// </summary>
        public const string USERCULTURE = "cms.userculture";


        /// <summary>
        /// Code name for resources.
        /// </summary>
        public const string RESOURCE = "cms.resource";


        /// <summary>
        /// Code name for roles.
        /// </summary>
        public const string ROLE = "cms.role";


        /// <summary>
        /// Code name for group roles.
        /// </summary>
        public const string GROUPROLE = "cms.grouprole";


        /// <summary>
        /// Custom table item prefix for object type.
        /// </summary>
        public const string CUSTOM_TABLE_ITEM_PREFIX = "customtableitem.";


        /// <summary>
        /// BizFrom item prefix for object type.
        /// </summary>
        public const string BIZFORM_ITEM_PREFIX = "bizformitem.";

        #endregion


        #region "DocumentObjectType"

        /// <summary>
        /// Object type for document type
        /// </summary>
        public const string DOCUMENTTYPE = "cms.documenttype";

        /// <summary>
        /// Allowed child class bindings.
        /// </summary>
        public const string ALLOWEDCHILDCLASS = "cms.allowedchildclass";

        /// <summary>
        /// Document.
        /// </summary>
        public const string DOCUMENT = "cms.document";

        /// <summary>
        /// Document item prefix for object type.
        /// </summary>
        public const string DOCUMENT_ITEM_PREFIX = "cms.document.";

        /// <summary>
        /// Document-category relationship.
        /// </summary>
        public const string DOCUMENTCATEGORY = "cms.documentcategory";

        /// <summary>
        /// Node.
        /// </summary>
        public const string NODE = "cms.node";

        /// <summary>
        /// Node culture specific data.
        /// </summary>
        public const string DOCUMENTLOCALIZATION = "cms.documentlocalization";

        #endregion


        #region "ForumsObjectType"

        /// <summary>
        /// Code name for forums.
        /// </summary>
        public const string FORUM = "forums.forum";


        /// <summary>
        /// Code name for community group forums.
        /// </summary>
        public const string GROUPFORUM = "forums.groupforum";

        
        /// <summary>
        /// Forum moderator.
        /// </summary>
        public const string FORUMMODERATOR = "forums.forummoderator";


        /// <summary>
        /// Code name for forum posts.
        /// </summary>
        public const string FORUMPOST = "forums.forumpost";

        #endregion


        #region "MediaLibraryObjectType"

        /// <summary>
        /// Code name for media library.
        /// </summary>
        public const string MEDIALIBRARY = "media.library";


        /// <summary>
        /// Code name for media file.
        /// </summary>
        public const string MEDIAFILE = "media.file";

        #endregion


        #region "FormObjectType"

        /// <summary>
        /// Object type for form class
        /// </summary>
        public const string FORMCLASS = "cms.formclass";


        /// <summary>
        /// BizForm role binding.
        /// </summary>
        public const string BIZFORMROLE = "cms.formrole";


        /// <summary>
        /// Alternative form constant.
        /// </summary>
        public const string ALTERNATIVEFORM = "cms.alternativeform";
        
        #endregion


        #region "CommunityObjectType"

        /// <summary>
        /// Code name for general group.
        /// </summary>
        public const string GROUP = "community.group";


        /// <summary>
        /// Code name of group members.
        /// </summary>
        public const string GROUPMEMBER = "community.groupmember";

        #endregion


        #region "EventManagerObjectType"

        /// <summary>
        /// Code name for general group.
        /// </summary>
        public const string EVENTATTENDEE = "cms.eventattendee";

        #endregion


        #region "MessageBoardObjectType"

        /// <summary>
        /// Code name for message board boards.
        /// </summary>
        public const string BOARD = "board.board";


        /// <summary>
        /// Code name for board moderator.
        /// </summary>
        public const string BOARDMODERATOR = "board.moderator";


        /// <summary>
        /// Code name for board message.
        /// </summary>
        public const string BOARDMESSAGE = "board.message";


        /// <summary>
        /// Code name for board subscription.
        /// </summary>
        public const string BOARDSUBSCRIPTION = "board.subscription";

        #endregion


        #region "NewsletterObjectType"

        /// <sumary>
        /// Code name for newsletter.
        /// </sumary>
        public const string NEWSLETTER = "newsletter.newsletter";

        /// <summary>
        /// Code name for newsletter issues.
        /// </summary>
        public const string NEWSLETTERISSUE = "newsletter.issue";


        /// <summary>
        /// Code name for newsletter A/B variants.
        /// </summary>
        public const string NEWSLETTERISSUEVARIANT = "newsletter.issuevariant";


        /// <summary>
        /// Code name for newsletter subscribers.
        /// </summary>
        public const string NEWSLETTERSUBSCRIBER = "newsletter.subscriber";


        /// <summary>
        /// Code name for newsletter email templates.
        /// </summary>
        public const string NEWSLETTERTEMPLATE = "newsletter.emailtemplate";


        /// <summary>
        /// Code name for newsletter unsubscription.
        /// </summary>
        public const string NEWSLETTERUNSUBSCRIPTION = "newsletter.unsubscription";


        /// <summary>
        /// Code name for newsletters contact group.
        /// </summary>
        public const string NEWSLETTERCONTACTGROUP = "newsletter.contactgroupsubscriber";


        /// <summary>
        /// Code name for item in newsletter email queue.
        /// </summary>
        public const string EMAILQUEUEITEM = "newsletter.emails";


        /// <summary>
        /// Code name for subscriber newsletter info.
        /// </summary>
        public const string SUBSCRIBERTONEWSLETTER = "newsletter.subscribernewsletter";

        #endregion


        #region "PollsObjectType"

        /// <summary>
        /// Code name for polls.
        /// </summary>
        public const string POLL = "polls.poll";


        /// <summary>
        /// Code name for poll answers.
        /// </summary>
        public const string POLLANSWER = "polls.pollanswer";

        #endregion


        #region "NotificationObjectType"

        /// <summary>
        /// Notification template code name.
        /// </summary>
        public const string NOTIFICATIONTEMPLATE = "notification.template";

        #endregion


        #region "ReportingObjectType"

        /// <summary>
        /// Code name for report categories.
        /// </summary>
        public const string REPORTCATEGORY = "reporting.reportcategory";


        /// <summary>
        /// Code name for reports.
        /// </summary>
        public const string REPORT = "reporting.report";

        /// <summary>
        /// Code name for report subscription
        /// </summary>
        public const string REPORTSUBSCRIPTION = "reporting.reportsubscription";

        #endregion


        #region "BlogObjectType"

        /// <summary>
        /// Blog comment.
        /// </summary>
        public const string BLOGCOMMENT = "blog.comment";
        
        #endregion


        #region "ECommerceObjectType"

        /// <summary>
        /// Code name for SKUs.
        /// </summary>
        public const string SKU = "ecommerce.sku";

        /// <summary>
        /// Code name for sku option categories.
        /// </summary>
        public const string OPTIONCATEGORY = "ecommerce.optioncategory";


        /// <summary>
        /// Code name for customers.
        /// </summary>
        public const string CUSTOMER = "ecommerce.customer";


        /// <summary>
        /// Code name for orders.
        /// </summary>
        public const string ORDER = "ecommerce.order";


        /// <summary>
        /// Code name for shopping cart.
        /// </summary>
        public const string SHOPPING_CART = "ecommerce.shoppingcart";

        #endregion


        #region "WorkflowObjectType"


        /// <summary>
        /// Workflow step role binding.
        /// </summary>
        public const string WORKFLOWSTEPROLE = "cms.workflowsteprole";


        /// <summary>
        /// Workflow step user binding.
        /// </summary>
        public const string WORKFLOWSTEPUSER = "cms.workflowstepuser";


        /// <summary>
        /// Workflow user binding.
        /// </summary>
        public const string WORKFLOWUSER = "cms.workflowuser";


        /// <summary>
        /// Code name for version history.
        /// </summary>
        public const string VERSIONHISTORY = "cms.versionhistory";

        #endregion


        #region "Marketing automation"

        /// <summary>
        /// Code name for marketing automation process.
        /// </summary>
        public const string AUTOMATIONSTATE = "ma.automationstate";


        /// <summary>
        /// Code name for marketing automation workflow trigger.
        /// </summary>
        public const string AUTOMATIONWORKFLOWTRIGGER = "cms.objectworkflowtrigger";

        #endregion


        #region "Online marketing"

        /// <summary>
        /// Code name for MVT combinations.
        /// </summary>
        public const string MVTCOMBINATION = "om.mvtcombination";


        /// <summary>
        /// Code name for document MVT combinations.
        /// </summary>
        public const string DOCUMENTMVTCOMBINATION = "om.documentmvtcombination";


        /// <summary>
        /// Code name for MVT variants.
        /// </summary>
        public const string MVTVARIANT = "om.mvtvariant";


        /// <summary>
        /// Code name for document MVT variants.
        /// </summary>
        public const string DOCUMENTMVTVARIANT = "om.documentmvtvariant";

        /// <summary>
        /// Code name for the contact.
        /// </summary>
        public const string CONTACT = "om.contact";


        /// <summary>
        /// Code name for the contact group.
        /// </summary>
        public const string CONTACTGROUP = "om.contactgroup";


        /// <summary>
        /// Code name for the contact group member.
        /// </summary>
        public const string CONTACTGROUPMEMBERCONTACT = "om.contactgroupmembercontact";


        /// <summary>
        /// Code name for the score.
        /// </summary>
        public const string SCORE = "om.score";


        /// <summary>
        /// Code name for the content personalization variant.
        /// </summary>
        public const string CONTENTPERSONALIZATIONVARIANT = "om.personalizationvariant";


        /// <summary>
        /// Code name for the document content personalization variant.
        /// </summary>
        public const string DOCUMENTCONTENTPERSONALIZATIONVARIANT = "om.documentpersonalizationvariant";


        /// <summary>
        /// Code name for tests.
        /// </summary>
        public const string ABTEST = "om.abtest";


        /// <summary>
        /// Code name for tests' variants.
        /// </summary>
        public const string ABVARIANT = "om.abvariant";


        /// <summary>
        /// Code name for tests' variants (MVC).
        /// </summary>
        public const string ABVARIANTDATA = "om.abvariantdata";


        /// <summary>
        /// Code name for Activity.
        /// </summary>
        public const string ACTIVITY = "om.activity";


        /// <summary>
        /// Code name for Activity recalculation queue.
        /// </summary>
        public const string ACTIVITYRECALCULATIONQUEUEINFO = "om.activityrecalculationqueue";


        /// <summary>
        /// Code name for Contact changes recalculation queue.
        /// </summary>
        public const string CONTACTCHANGERECALCULATIONQUEUEINFO = "om.contactchangerecalculationqueue";


        /// <summary>
        /// Code name for BizForms.
        /// </summary>
        public const string BIZFORM = "cms.form";
        
        #endregion


        #region "Scheduler"

        /// <summary>
        /// Object's scheduled task.
        /// </summary>
        public const string OBJECTSCHEDULEDTASK = "cms.objectscheduledtask";

        #endregion


        #region "Relationships"


        /// <summary>
        /// Relationship name.
        /// </summary>
        public const string RELATIONSHIPNAME = "cms.relationshipname";

        #endregion


        #region "Personas"

        /// <summary>
        /// Code name for persona.
        /// </summary>
        public const string PERSONA = "personas.persona";

        #endregion


        #region "Custom tables"

        /// <summary>
        /// Object type for custom table class
        /// </summary>
        public const string CUSTOMTABLECLASS = "cms.customtable";

        #endregion


        #region "Data protection"

        /// <summary>
        /// Object type for consent agreement.
        /// </summary>
        public const string CONSENTAGREEMENT = "cms.consentagreement";

        #endregion
    }
}
