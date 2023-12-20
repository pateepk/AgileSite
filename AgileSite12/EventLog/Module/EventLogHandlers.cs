using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.EventLog
{
    /// <summary>
    /// Event log handlers
    /// </summary>
    internal class EventLogHandlers
    {
        /// <summary>
        /// Initializes the synchronization handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += SettingsKeyChange;

            ApplicationEvents.Initialized.Execute += LogApplicationStart;
            ApplicationEvents.End.Execute += LogApplicationEnd;

            ObjectEvents.Insert.Before += InsertObject;
            ObjectEvents.Update.Before += UpdateObject;
            ObjectEvents.Delete.Before += DeleteObject;
        }


        /// <summary>
        /// Logs the application start to event log
        /// </summary>
        private static void LogApplicationStart(object sender, EventArgs e)
        {
            EventLogProvider.LogApplicationStart();
        }


        /// <summary>
        /// Logs the application end
        /// </summary>
        private static void LogApplicationEnd(object sender, EventArgs e)
        {
            // Log the application end
            EventLogProvider.LogApplicationEnd();
        }


        /// <summary>
        /// Actions that execute upon object deletion
        /// </summary>
        [ExcludeFromDebug]
        private static void DeleteObject(object sender, ObjectEventArgs e)
        {
            var obj = e.Object;

            // Log the object deletion
            EventLogHelper.LogDelete(obj);
        }


        /// <summary>
        /// Actions that execute upon object insert
        /// </summary>
        [ExcludeFromDebug]
        private static void InsertObject(object sender, ObjectEventArgs e)
        {
            var obj = e.Object;

            // Log the object update
            EventLogHelper.LogInsert(obj);
        }


        /// <summary>
        /// Actions that execute upon object update
        /// </summary>
        [ExcludeFromDebug]
        private static void UpdateObject(object sender, ObjectEventArgs e)
        {
            var obj = e.Object;

            // Log the object update
            EventLogHelper.LogUpdate(obj);
        }


        /// <summary>
        /// Handles settings key change
        /// </summary>
        private static void SettingsKeyChange(object sender, SettingsKeyChangedEventArgs e)
        {
            switch (e.KeyName.ToLowerCSafe())
            {
                case "cmslogsize":
                    // Clear the hashtables for the event log if related settings key changed
                    ProviderHelper.ClearHashtables(EventLogInfo.OBJECT_TYPE, true);
                    break;

                case "cmsuseeventloglistener":
                    // Handle registration of event log listener
                    EventLogProvider.ToggleDefaultEventLogListenerRegistration(e.KeyValue.ToBoolean(false));
                    break;
            }
        }
    }
}
