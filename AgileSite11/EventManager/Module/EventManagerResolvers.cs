using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.EventManager
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class EventManagerResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mBookingResolver = null;

        #endregion

        /// <summary>
        /// Booking e-mail template macro resolver.
        /// </summary>
        public static MacroResolver BookingResolver
        {
            get
            {
                if (mBookingResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Register "cms.bookingevent" object if exists
                    if (ModuleManager.GetReadOnlyObject("cms.bookingevent") != null)
                    {
                        resolver.SetNamedSourceData("Event", ModuleManager.GetReadOnlyObject("cms.bookingevent"));
                    }
                    resolver.SetNamedSourceData("Attendee", ModuleManager.GetReadOnlyObject("cms.eventattendee"));

                    // Set additional macros
                    resolver.SetNamedSourceData("EventDateString", string.Empty);

                    mBookingResolver = resolver;
                }

                return mBookingResolver;
            }
        }
    }
}