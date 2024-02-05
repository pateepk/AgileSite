using CMS.Core;

namespace CMS.EventLog
{
    /// <summary>
    /// Represents the Event Log module metadata.
    /// </summary>
    public class EventLogModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EventLogModuleMetadata()
            : base(ModuleName.EVENTLOG)
        {
            RootPath = "~/CMSModules/EventManager/";
        }
    }
}