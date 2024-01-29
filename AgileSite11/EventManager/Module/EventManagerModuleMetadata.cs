using CMS.Core;

namespace CMS.EventManager
{
    /// <summary>
    /// Represents the Event Manager module metadata.
    /// </summary>
    public class EventManagerModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EventManagerModuleMetadata()
            : base(ModuleName.EVENTMANAGER)
        {
            RootPath = "~/CMSModules/EventLog/";
        }
    }
}