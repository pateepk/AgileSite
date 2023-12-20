using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules.Internal;
using CMS.Search;

[assembly: RegisterModule(typeof(MembershipModule))]

namespace CMS.Membership
{
    /// <summary>
    /// Represents the Membership module.
    /// </summary>
    public class MembershipModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for password.
        /// </summary>
        public const string PASSWORD_EMAIL_TEMPLATE_TYPE_NAME = "password";


        /// <summary>
        /// Name of email template type for registration.
        /// </summary>
        public const string REGISTRATION_EMAIL_TEMPLATE_TYPE_NAME = "registration";


        /// <summary>
        /// Name of email template type for registration approval.
        /// </summary>
        public const string REGISTRATION_APPROVAL_EMAIL_TEMPLATE_TYPE_NAME = "registrationapproval";


        /// <summary>
        /// Name of email template type for membership registration.
        /// </summary>
        public const string MEMBERSHIP_REGISTRATION_EMAIL_TEMPLATE_TYPE_NAME = "membershipregistration";


        /// <summary>
        /// Name of email template type for membership expiration.
        /// </summary>
        public const string MEMBERSHIP_EXPIRATION_EMAIL_TEMPLATE_TYPE_NAME = "membershipexpiration";


        /// <summary>
        /// Name of email template type for membership change password.
        /// </summary>
        public const string MEMBERSHIP_CHANGE_PASSWORD_EMAIL_TEMPLATE_TYPE_NAME = "membershipchangepassword";
        
        
        /// <summary>
        /// Name of email template type for membership unlock account.
        /// </summary>
        public const string MEMBERSHIP_UNLOCK_ACCOUNT_EMAIL_TEMPLATE_TYPE_NAME = "membershipunlockaccount";


        /// <summary>
        /// Name of email template type for forgotten password.
        /// </summary>
        public const string FORGOTTEN_PASSWORD_EMAIL_TEMPLATE_TYPE_NAME = "forgottenpassword";

        #endregion


        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public MembershipModule()
            : base(new MembershipModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            Service.Use<IAuthenticationService, AuthenticationService>();
            Service.Use<IWindowsTokenRoleService, WindowsTokenRoleService>();
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Init synchronization
            MembershipSynchronization.Init();
            SessionSynchronization.Init();

            // Init handlers
            MembershipHandlers.Init();
            EventLogDataHandler.Init();

            // Import export handlers
            InitImportExport();

            RegisterContext<MembershipContext>("MembershipContext");

            InitMacros();

            MembershipCounters.RegisterPerformanceCounters();

            // Register task aliases
            RegisterTaskAliases();

            SearchIndexers.RegisterIndexer<UserSearchIndexer>(UserInfo.OBJECT_TYPE);

            // Register function providing user for module installation process
            ModuleInstallerConfiguration.ModuleInstallerUserProvider = () => UserInfoProvider.AdministratorUser;
        }


        /// <summary>
        /// Initializes import/export handlers
        /// </summary>
        private static void InitImportExport()
        {
            AvatarExport.Init();
            AvatarImport.Init();

            ImportSpecialActions.Init();
            ExportSpecialActions.Init();
        }


        /// <summary>
        /// Initializes the Membership macros
        /// </summary>
        private static void InitMacros()
        {
            var r = MacroContext.GlobalResolver;

            r.SetNamedSourceData("Gender", new EnumDataContainer(typeof(UserGenderEnum)), false);
            r.SetNamedSourceData("UserPrivilegeLevelEnum", new EnumDataContainer(typeof(UserPrivilegeLevelEnum)), false);

            r.AddSourceAlias("User", "MembershipContext.AuthenticatedUser");

            r.SetNamedSourceDataCallback("URLReferrer", (x) => CookieHelper.GetValue(CookieName.UrlReferrer), false);
            r.SetNamedSourceDataCallback("PreferredCulture", (x) => LocalizationContext.PreferredCultureCode, false);
            r.SetNamedSourceDataCallback("PreferredUICulture", (x) => MembershipContext.AuthenticatedUser.PreferredUICultureCode, false);

            MacroResolverStorage.RegisterResolver("ForgottenPasswordResolver", () => MembershipResolvers.GetForgottenPasswordResolver(null, String.Empty, String.Empty));
            MacroResolverStorage.RegisterResolver("MembershipChangePasswordResolver", () => MembershipResolvers.GetMembershipChangePasswordResolver(null, String.Empty, String.Empty));
            MacroResolverStorage.RegisterResolver("MembershipPasswordResetConfirmationResolver", () => MembershipResolvers.GetMembershipPasswordResetConfirmationResolver(null));
            MacroResolverStorage.RegisterResolver("MembershipExpirationResolver", () => MembershipResolvers.GetMembershipExpirationResolver(null, null));
            MacroResolverStorage.RegisterResolver("MembershipRegistrationResolver", () => MembershipResolvers.GetMembershipRegistrationResolver(null, String.Empty));
            MacroResolverStorage.RegisterResolver("MembershipUnlockAccountResolver", () => MembershipResolvers.GetMembershipUnlockAccountResolver(null, String.Empty, String.Empty));
            MacroResolverStorage.RegisterResolver("PasswordResolver", () => MembershipResolvers.GetPasswordResolver(null, String.Empty));
            MacroResolverStorage.RegisterResolver("RegistrationApprovalResolver", () => MembershipResolvers.GetRegistrationApprovalResolver(null, String.Empty));
            MacroResolverStorage.RegisterResolver("RegistrationResolver", () => MembershipResolvers.GetRegistrationResolver(null));
        }


        /// <summary>
        /// Registers tasks aliases for the membership scheduled tasks
        /// </summary>
        private static void RegisterTaskAliases()
        {
            ClassHelper.RegisterAliasFor<MembershipReminder>("CMS.Scheduler", "CMS.Scheduler.MembershipReminder");
            ClassHelper.RegisterAliasFor<DeleteNonActivatedUser>("CMS.Scheduler", "CMS.Scheduler.DeleteNonActivatedUser");

            ClassHelper.RegisterAliasFor<RemoveExpiredSessions>("CMS.CMSHelper", "CMS.CMSHelper.RemoveExpiredSessions");
            ClassHelper.RegisterAliasFor<UpdateDatabaseSession>("CMS.CMSHelper", "CMS.CMSHelper.UpdateDatabaseSession");
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            UserInfoProvider.ClearLicenseValues(logTasks);
            UserInfo.TYPEINFO.InvalidateAllObjects(logTasks);
        }

        #endregion
    }
}