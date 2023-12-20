using CMS.Core;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Represents the Notifications module metadata.
    /// </summary>
    public class NotificationsModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NotificationsModuleMetadata()
            : base(ModuleName.NOTIFICATIONS)
        {
            RootPath = "~/CMSModules/Notifications/";
        }
    }
}