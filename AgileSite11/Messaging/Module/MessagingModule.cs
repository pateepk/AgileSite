using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;
using CMS.Messaging;

[assembly: RegisterModule(typeof(MessagingModule))]

namespace CMS.Messaging
{
    /// <summary>
    /// Represents the Messaging module.
    /// </summary>
    public class MessagingModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for general purposes.
        /// </summary>
        public const string MESSAGING_EMAIL_TEMPLATE_TYPE_NAME = "messaging";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public MessagingModule()
            : base(new MessagingModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ExtendList<MacroResolverStorage, MacroResolver>.With("MessagingResolver").WithLazyInitialization(() => MessagingResolvers.MessagingResolver);
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("IsInIgnoreList", IsInIgnoreList);
            RegisterCommand("IsInContactList", IsInContactList);
            RegisterCommand("AddToIgnoreList", AddToIgnoreList);
            RegisterCommand("AddToContactList", AddToContactList);
        }


        /// <summary>
        /// Add user to contact list
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object AddToContactList(object[] parameters)
        {
            int ownerId = (int)parameters[0];
            int userId = (int)parameters[1];

            ContactListInfoProvider.AddToContactList(ownerId, userId);

            return null;
        }


        /// <summary>
        /// Add user to ignore list
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object AddToIgnoreList(object[] parameters)
        {
            int ownerId = (int)parameters[0];
            int userId = (int)parameters[1];

            IgnoreListInfoProvider.AddToIgnoreList(ownerId, userId);

            return null;
        }


        /// <summary>
        /// Check whether user is in contact list
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object IsInContactList(object[] parameters)
        {
            int ownerId = (int)parameters[0];
            int userId = (int)parameters[1];

            return ContactListInfoProvider.IsInContactList(ownerId, userId);
        }


        /// <summary>
        /// Check whether user is in ignore list
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object IsInIgnoreList(object[] parameters)
        {
            int ownerId = (int)parameters[0];
            int userId = (int)parameters[1];

            return IgnoreListInfoProvider.IsInIgnoreList(ownerId, userId);
        }
    }
}