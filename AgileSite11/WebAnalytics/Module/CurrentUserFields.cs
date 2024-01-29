using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.WebAnalytics;

[assembly: RegisterExtension(typeof(CurrentUserFields), typeof(CurrentUserInfo))]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Wrapper class to provide fields from System.Math namespace in the MacroEngine.
    /// </summary>
    internal class CurrentUserFields : MacroFieldContainer
    {
        /// <summary>
        /// Registers the math fields.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            RegisterField(new MacroField("IsNewVisitor", () => AnalyticsContext.IsNewVisitor));
            RegisterField(new MacroField("IsReturningVisitor", () => AnalyticsContext.IsReturningVisitor));
        }
    }
}
