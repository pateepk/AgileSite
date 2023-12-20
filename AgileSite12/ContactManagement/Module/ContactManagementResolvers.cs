using CMS.Activities;
using CMS.Automation;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    internal class ContactManagementResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mAutomationSimpleResolver = null;
        private static MacroResolver mAutomationEmailResolver = null;
        private static MacroResolver mScoringResolver = null;
        private static MacroResolver mContactResolver = null;
        private static MacroResolver mContactActivityResolver = null;
        private static MacroResolver mContactScoreResolver = null;
        private static MacroResolver mVariantResolver = null;

        #endregion


        /// <summary>
        /// Returns simple automation macro resolver with Contact field.
        /// </summary>
        public static MacroResolver AutomationSimpleResolver
        {
            get
            {
                if (mAutomationSimpleResolver == null)
                {
                    MacroResolver resolver = WorkflowResolvers.WorkflowSimpleResolver.CreateChild();

                    resolver.SetNamedSourceData("Contact", ModuleManager.GetReadOnlyObject(ContactInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("AutomationState", ModuleManager.GetReadOnlyObject(AutomationStateInfo.OBJECT_TYPE));
                    resolver.AddSourceAlias("ActivityMacroOnlyForNonContentOnlySites", "");

                    mAutomationSimpleResolver = resolver;
                }

                return mAutomationSimpleResolver;
            }
        }


        /// <summary>
        /// Returns automation e-mail macro resolver with On-line marketing fields.
        /// </summary>
        public static MacroResolver AutomationResolver
        {
            get
            {
                if (mAutomationSimpleResolver == null)
                {
                    MacroResolver resolver = AutomationSimpleResolver.CreateChild();

                    resolver.SetNamedSourceData("Score", ModuleManager.GetReadOnlyObject(ScoreInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Activity", ModuleManager.GetReadOnlyObject(ActivityInfo.OBJECT_TYPE));
                    resolver.AddSourceAlias("ActivityMacroOnlyForNonContentOnlySites", "");

                    mAutomationEmailResolver = resolver;
                }

                return mAutomationEmailResolver;
            }
        }


        /// <summary>
        /// Returns scoring e-mail template macro resolver.
        /// </summary>
        public static MacroResolver ScoringResolver
        {
            get
            {
                if (mScoringResolver == null)
                {
                    MacroResolver resolver = ContactResolver.CreateChild();

                    resolver.SetNamedSourceData("Score", ModuleManager.GetReadOnlyObject("om.score"));

                    resolver.SetNamedSourceData("ScoreValue", string.Empty);

                    mScoringResolver = resolver;
                }

                return mScoringResolver;
            }
        }


        /// <summary>
        /// Returns contact resolver.
        /// </summary>
        public static MacroResolver ContactResolver
        {
            get
            {
                if (mContactResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Contact", ModuleManager.GetReadOnlyObject("OM.Contact"));
                    resolver.AddSourceAlias("ActivityMacroOnlyForNonContentOnlySites", "");

                    mContactResolver = resolver;
                }

                return mContactResolver;
            }
        }


        /// <summary>
        /// Returns contact activity resolver (for trigger macro).
        /// </summary>
        public static MacroResolver ContactActivityResolver
        {
            get
            {
                if (mContactActivityResolver == null)
                {
                    MacroResolver resolver = ContactResolver.CreateChild();
                    resolver.SetNamedSourceData("Activity", ModuleManager.GetReadOnlyObject("OM.Activity"));
                    resolver.AddSourceAlias("ActivityMacroOnlyForNonContentOnlySites", "");

                    mContactActivityResolver = resolver;
                }

                return mContactActivityResolver;
            }
        }


        /// <summary>
        /// Returns contact score resolver (for trigger macro).
        /// </summary>
        public static MacroResolver ContactScoreResolver
        {
            get
            {
                if (mContactScoreResolver == null)
                {
                    MacroResolver resolver = ContactResolver.CreateChild();
                    resolver.SetNamedSourceData("Score", ModuleManager.GetReadOnlyObject("OM.Score"));

                    mContactScoreResolver = resolver;
                }

                return mContactScoreResolver;
            }
        }


        /// <summary>
        /// Returns variant resolver.
        /// </summary>
        public static MacroResolver VariantResolver
        {
            get
            {
                if (mVariantResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.PrioritizeProperty("CurrentUser");
                    resolver.PrioritizeProperty("ContactManagementContext");
                    resolver.PrioritizeProperty("CurrentDevice");
                    resolver.AddSourceAlias("ActivityMacroOnlyForNonContentOnlySites", "");

                    mVariantResolver = resolver;
                }

                return mVariantResolver;
            }
        }
    }
}