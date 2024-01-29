using System;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Operation type enumeration.
    /// </summary>
    public class SystemTaskType
    {
        /// <summary>
        /// Restart application operation.
        /// </summary>
        public const string RestartApplication = "RESTARTAPPLICATION";


        /// <summary>
        /// Clears web farm context information.
        /// </summary>
        public const string ClearWebFarmContext = "CLEARWEBFARMCONTEXT";
    }
}