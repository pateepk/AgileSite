using System;
using System.Runtime.Serialization;
using System.Security;

using CMS.Helpers;
using CMS.Base;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site deletion object for logging messages to the async control
    /// </summary>
    [Serializable]
    public sealed class SiteDeletionLog : ISerializable, IProgress<SiteDeletionStatusMessage>
    {
        private const string SEPARATOR = "<#>";


        /// <summary>
        /// Keep information about deletion progress.
        /// </summary>
        public string DeletionLog
        {
            get;
            set;
        }


        /// <summary>
        /// Persistent key to store the log.
        /// </summary>
        public string PersistentLogKey
        {
            get;
            set;
        }


        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="statusMessage">Status message</param>
        public void Report(SiteDeletionStatusMessage statusMessage)
        {
            LogDeletionState(statusMessage.Status, statusMessage.Message);
        }


        /// <summary>
        /// Gets progress state.
        /// </summary>
        public LogStatusEnum GetProgressState()
        {
            string[] status = DeletionLog.Split(new[] { SEPARATOR }, StringSplitOptions.None);

            switch (status[0].ToLowerCSafe())
            {
                case "e":
                    return LogStatusEnum.Error;
                case "f":
                    return LogStatusEnum.Finish;
                default:
                    return LogStatusEnum.Info;
            }
        }


        /// <summary>
        /// Logs deletion state.
        /// </summary>
        /// <param name="type">Type of the message</param>
        /// <param name="message">Message to be logged</param>
        public void LogDeletionState(LogStatusEnum type, string message)
        {
            string[] status = DeletionLog.Split(new[] { SEPARATOR }, StringSplitOptions.None);

            // Wrong format of the internal status
            if (status.Length != 4)
            {
                DeletionLog = "F" + SEPARATOR + "" + SEPARATOR + "" + SEPARATOR;
            }

            switch (type)
            {
                case LogStatusEnum.Info:
                    status[0] = "I";
                    status[1] = message + "<br />" + status[1];
                    break;

                case LogStatusEnum.Error:
                    status[0] = "E";
                    status[2] = message + "<br />";
                    break;

                case LogStatusEnum.Warning:
                    status[3] += "<strong>" + ResHelper.GetAPIString("Global.Warning", "WARNING:&nbsp;") + "</strong>" + message + "<br />";
                    break;

                case LogStatusEnum.Finish:
                case LogStatusEnum.UnexpectedFinish:
                    status[0] = "F";
                    status[1] = "<strong>" + message + "</strong><br />" + status[1];
                    break;
            }

            DeletionLog = status[0] + SEPARATOR + status[1] + SEPARATOR + status[2] + SEPARATOR + status[3];
            SavePersistent();
        }


        /// <summary>
        /// Saves the settings object.
        /// </summary>
        private void SavePersistent()
        {
            if (PersistentLogKey == null)
            {
                return;
            }

            try
            {
                PersistentStorageHelper.SetValue(PersistentLogKey, this);
            }
            catch
            {
                // Do not throw exception if save object to persistent storage failed
            }
        }


        /// <summary>
        /// Serialization function.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("DeletionLog", DeletionLog);
            info.AddValue("PersistentLogKey", PersistentLogKey);
        }

        
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        public SiteDeletionLog(SerializationInfo info, StreamingContext ctxt)
        {
            DeletionLog = info.GetString("DeletionLog");
            PersistentLogKey = info.GetString("PersistentLogKey");
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public SiteDeletionLog()
        {
            DeletionLog = "I" + SEPARATOR + "" + SEPARATOR + "" + SEPARATOR;
        }
    }
}
