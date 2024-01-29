using System;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.Reporting
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class ReportingResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mReportingResolver = null;

        #endregion


        /// <summary>
        /// Returns reporting e-mail template macro resolver.
        /// </summary>
        public static MacroResolver ReportingResolver
        {
            get
            {
                if (mReportingResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Report", ModuleManager.GetReadOnlyObject(ReportInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("ReportSubscription", ModuleManager.GetReadOnlyObject(ReportSubscriptionInfo.OBJECT_TYPE));

                    resolver.SetNamedSourceData("SubscriptionBody", String.Empty);
                    resolver.SetNamedSourceData("DefaultSubscriptionCSS", String.Empty);
                    resolver.SetNamedSourceData("UnsubscriptionLink", String.Empty);
                    resolver.SetNamedSourceData("ItemName", String.Empty);

                    mReportingResolver = resolver;
                }

                return mReportingResolver;
            }
        }
    }
}