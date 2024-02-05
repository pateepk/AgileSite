using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.Messaging
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class MessagingResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mMessagingResolver = null;

        #endregion


        /// <summary>
        /// Returns messaging e-mail template macro resolver.
        /// </summary>
        public static MacroResolver MessagingResolver
        {
            get
            {
                if (mMessagingResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Sender", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Recipient", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Message", ModuleManager.GetReadOnlyObject("messaging.message"));

                    RegisterStringValues(resolver, new [] { "LogonUrl" });

                    mMessagingResolver = resolver;
                }

                return mMessagingResolver;
            }
        }
    }
}