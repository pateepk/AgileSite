using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterModule(typeof(ContactManagementModule))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents the Contact Management module.
    /// </summary>
    internal class ContactManagementModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ContactManagementModule()
            : base(new ContactManagementModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            ContactManagementHandlers.Init();
            ContactManagementImportSpecialActions.Init();
            ScoringHandlers.Init();
            ContactGroupHandlers.Init();

            ExtendList<MacroResolverStorage, MacroResolver>.With("AutomationResolver").WithLazyInitialization(() => ContactManagementResolvers.AutomationResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("AutomationSimpleResolver").WithLazyInitialization(() => ContactManagementResolvers.AutomationSimpleResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("ContactActivityResolver").WithLazyInitialization(() => ContactManagementResolvers.ContactActivityResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("ContactResolver").WithLazyInitialization(() => ContactManagementResolvers.ContactResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("ContactScoreResolver").WithLazyInitialization(() => ContactManagementResolvers.ContactScoreResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("ScoringResolver").WithLazyInitialization(() => ContactManagementResolvers.ScoringResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("VariantResolver").WithLazyInitialization(() => ContactManagementResolvers.VariantResolver);

            // Register contact changes and activities processing
            if (SystemContext.IsWebSite)
            {
                RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => ContactChangeLogWorker.Current.EnsureRunningThread();

                var createContactActionsLogWorker = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSCreateContactActionsLogWorker"], true);
                if (SystemContext.IsCMSRunningAsMainApplication && createContactActionsLogWorker)
                {
                    RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => ContactActionsLogWorker.Current.EnsureRunningThread();
                }
            }

            MacroRuleInfo.TYPEINFO.Events.CheckPermissions.Before += CheckMacroPermissions;
            
            RegisterContactActivityModifier();
            RegisterActivityValidator();
        }


        /// <summary>
        /// Checks permissions for on-line marketing macro rules.
        /// </summary>
        internal void CheckMacroPermissions(object sender, ObjectSecurityEventArgs e)
        {
            var rule = e.Object as MacroRuleInfo;
            var permission = e.Permission;

            if (rule != null)
            {
                // Special permission check for on-line marketing macro rules
                switch (rule.MacroRuleResourceName.ToLowerInvariant())
                {
                    case "cms.onlinemarketing":
                        {
                            switch (permission)
                            {
                                case PermissionsEnum.Read:
                                    e.Result = e.User.IsAuthorizedPerResource("CMS.ContactManagement", "ReadConfiguration", e.SiteName, false).ToAuthorizationResultEnum();

                                    break;

                                case PermissionsEnum.Create:
                                case PermissionsEnum.Delete:
                                case PermissionsEnum.Modify:
                                case PermissionsEnum.Destroy:
                                    e.Result = e.User.IsAuthorizedPerResource("CMS.ContactManagement", "ModifyConfiguration", e.SiteName, false).ToAuthorizationResultEnum();

                                    break;
                            }

                            // Skip default check
                            e.Cancel();
                        }

                        break;
                }
            }
        }


        /// <summary>
        /// Registers all <see cref="IActivityLogValidator"/> available in the current assembly to the <see cref="IActivityLogService"/>.
        /// </summary>
        private void RegisterActivityValidator()
        {
            var activityLogService = Service.Resolve<IActivityLogService>();
            var validator = new ContactActivityLogValidator();
            activityLogService.RegisterValidator(validator);
        }


        private static void RegisterContactActivityModifier()
        {
            var activityLogService = Service.Resolve<IActivityLogService>();
            var currentContactProviderContextDecorator = new CurrentContactProviderContextDecorator(Service.Resolve<ICurrentContactProvider>(), Service.Resolve<IContactProcessingChecker>());
            var modifier = new ContactActivityModifier(currentContactProviderContextDecorator);
            activityLogService.RegisterModifier(modifier);
        }
    }
}
