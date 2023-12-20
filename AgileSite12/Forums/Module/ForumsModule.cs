using System;

using CMS;
using CMS.DataEngine;
using CMS.Forums;
using CMS.MacroEngine;
using CMS.Base;
using CMS.Search;
using CMS.Search.Internal;
using CMS.Forums.Internal;

[assembly: RegisterModule(typeof(ForumsModule))]

namespace CMS.Forums
{
    /// <summary>
    /// Represents the Forums module.
    /// </summary>
    public class ForumsModule : Module
    {
        #region "Constants"

        internal const string FORUMS = "##FORUMS##";


        /// <summary>
        /// Name of email template type for forum.
        /// </summary>
        public const string FORUM_EMAIL_TEMPLATE_TYPE_NAME = "forum";


        /// <summary>
        /// Name of email template type for forum subscription.
        /// </summary>
        public const string FORUM_SUBSCRIPTION_EMAIL_TEMPLATE_TYPE_NAME = "forumsubscribtion"; // the typo comes from EmailTemplateTypeEnum...

        #endregion
        


        /// <summary>
        /// Default constructor
        /// </summary>
        public ForumsModule()
            : base(new ForumsModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ForumsSynchronization.Init();

            InitImportExport();
            InitMacros();

            SearchIndexers.RegisterIndexer<ForumSearchIndexer>(ForumInfo.OBJECT_TYPE);
            SearchablesRetrievers.Register<ForumSearchablesRetriever>(ForumInfo.OBJECT_TYPE);

            ForumHandlers.Init();
        }


        /// <summary>
        /// Initializes Import/Export handlers
        /// </summary>
        private static void InitImportExport()
        {
            // Import export handlers
            ForumExport.Init();
            ForumImport.Init();
            ImportSpecialActions.Init();
        }


        /// <summary>
        /// Initializes the macro engine from the module
        /// </summary>
        private static void InitMacros()
        {
            RegisterContext<ForumContext>();

            ExtendList<MacroResolverStorage, MacroResolver>.With("ForumResolver").WithLazyInitialization(() => ForumsResolvers.ForumResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("ForumSubscribtionResolver").WithLazyInitialization(() => ForumsResolvers.ForumSubscribtionResolver);
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("GetDocumentForumsCount", GetDocumentForumsCount);
            RegisterCommand("AddForumModerator", AddForumModerator);
            RegisterCommand("RemoveForumModerator", RemoveForumModerator);
            RegisterCommand("GetForumInfo", GetForumInfo);
            RegisterCommand("GetForumPostInfo", GetForumPostInfo);
            RegisterCommand("GetForumGroupInfo", GetForumGroupInfo);
            RegisterCommand("GetForumPostUrl", GetForumPostUrl);
        }


        /// <summary>
        /// Get number of forums for selected document
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object GetDocumentForumsCount(object[] parameters)
        {
            int documentId = (int)parameters[0];

            return ForumInfoProvider.GetForumsCount(documentId);
        }


        /// <summary>
        /// Adds the moderator to the forum
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object AddForumModerator(object[] parameters)
        {
            int userId = (int)parameters[0];
            int forumId = (int)parameters[1];

            ForumModeratorInfoProvider.AddModeratorToForum(userId, forumId);

            return null;
        }


        /// <summary>
        /// Remove the moderator from the forum
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object RemoveForumModerator(object[] parameters)
        {
            int userId = (int)parameters[0];
            int forumId = (int)parameters[1];

            ForumModeratorInfoProvider.RemoveModeratorFromForum(userId, forumId);

            return null;
        }
        

        /// <summary>
        /// Returns forum info by forum id
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static ForumInfo GetForumInfo(object[] parameters)
        {
            ForumInfo forumInfo = ForumInfoProvider.GetForumInfo(Convert.ToInt32(parameters[0]));
            if (forumInfo != null)
            {
                forumInfo.SetValue("ForumBaseUrl", forumInfo.ForumBaseUrl);
            }

            return forumInfo;
        }


        /// <summary>
        /// Returns forum post info by forum post id
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static ForumPostInfo GetForumPostInfo(object[] parameters)
        {
            return ForumPostInfoProvider.GetForumPostInfo(Convert.ToInt32(parameters[0]));
        }


        /// <summary>
        /// Returns forum group info by forum group id
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static ForumGroupInfo GetForumGroupInfo(object[] parameters)
        {
            return ForumGroupInfoProvider.GetForumGroupInfo(Convert.ToInt32(parameters[0]));
        }


        /// <summary>
        /// Returns post URL
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetForumPostUrl(object[] parameters)
        {
            return ForumPostInfoProvider.GetPostURL(parameters[0] as string, Convert.ToInt32(parameters[1]), Convert.ToBoolean(parameters[2]));
        }
    }
}