using CMS;
using CMS.MacroEngine;
using CMS.Membership;

[assembly: RegisterExtension(typeof(CurrentUserFields), typeof(CurrentUserInfo))]

namespace CMS.Membership
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

            RegisterField(new MacroField("IsAuthenticated", () => AuthenticationHelper.IsAuthenticated()));
        }
    }
}