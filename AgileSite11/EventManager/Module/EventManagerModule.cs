using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventManager;
using CMS.MacroEngine;
using CMS.Base;

[assembly: RegisterModule(typeof(EventManagerModule))]

namespace CMS.EventManager
{
    /// <summary>
    /// Represents the Event Manager module.
    /// </summary>
    public class EventManagerModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for booking event.
        /// </summary>
        public const string BOOKING_EVENT_EMAIL_TEMPLATE_TYPE_NAME = "bookingevent";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public EventManagerModule()
            : base(new EventManagerModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            InitImportExport();

            ExtendList<MacroResolverStorage, MacroResolver>.With("BookingResolver").WithLazyInitialization(() => EventManagerResolvers.BookingResolver);
        }


        private static void InitImportExport()
        {
            ImportSpecialActions.Init();
            EventExport.Init();
            EventImport.Init();
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("GetSiteEvent", GetSiteEvent);
        }


        /// <summary>
        /// Gets the site event
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static DataSet GetSiteEvent(object[] parameters)
        {
            int eventId = (int)parameters[0];
            string siteName = (string)parameters[1];
            string columns = (string)parameters[2];

            return EventProvider.GetEvent(eventId, siteName, columns);
        }
    }
}