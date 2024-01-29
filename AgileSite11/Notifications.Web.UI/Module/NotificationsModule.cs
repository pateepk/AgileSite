using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Notifications.Web.UI;

[assembly: RegisterModule(typeof(NotificationsModule))]

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Represents the Notifications module.
    /// </summary>
    public class NotificationsModule : Module
    {
        internal const string NOTIFICATION = "##NOTIFICATION##";


        /// <summary>
        /// Default constructor
        /// </summary>
        public NotificationsModule()
            : base(new NotificationsModuleMetadata())
        {
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("RaiseEvent", RaiseEvent);
        }


        /// <summary>
        /// Raises a notification event
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object RaiseEvent(object[] parameters)
        {
            var eventSource = (string)parameters[0];
            var eventCode = (string)parameters[1];
            var eventObjectId = (int)parameters[2];
            var eventData1 = (string)parameters[3];
            var eventData2 = (string)parameters[4];
            var siteId = (int)parameters[5];
            var where = (string)parameters[6];
            object resolverData = parameters[7];
            var resolverSpecialMacros = (IDictionary<string, object>)parameters[8];

            CMSNotificationGateway.RaiseEvent(eventSource, eventCode, eventObjectId, eventData1, eventData2, siteId, where, resolverData, resolverSpecialMacros);

            return null;
        }
    }
}