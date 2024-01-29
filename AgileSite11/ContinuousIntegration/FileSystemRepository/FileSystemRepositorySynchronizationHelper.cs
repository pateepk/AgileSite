using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

using CMS.Base;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Contains methods necessary for file system repository access synchronization.
    /// </summary>
    internal class FileSystemRepositorySynchronizationHelper : AbstractHelper<FileSystemRepositorySynchronizationHelper>
    {
        #region "Public methods"

        /// <summary>
        /// Gets <see cref="MutexSecurity"/> to be used when creating named mutexes for synchronization of file system repository operations.
        /// The returned security object must be usable in the <see cref="Mutex"/> constructor call.
        /// </summary>
        /// <returns>Mutex security object to be used for file system repository synchronization mutexes.</returns>
        /// <remarks>
        /// The default implementation returns security object containing single access rule which grants <see cref="MutexRights.FullControl"/> to everyone.
        /// </remarks>
        public static MutexSecurity GetMutexSecurity()
        {
            return HelperObject.GetMutexSecurityInternal();
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Gets <see cref="MutexSecurity"/> to be used when creating named mutexes for synchronization of file system repository operations.
        /// The returned security object must be usable in the <see cref="Mutex(bool, string, out bool, MutexSecurity)"/> constructor call.
        /// </summary>
        /// <returns>Mutex security object to be used for file system repository synchronization mutexes.</returns>
        /// <remarks>
        /// The default implementation returns security object containing single access rule which grants <see cref="MutexRights.FullControl"/> to everyone.
        /// </remarks>
        protected MutexSecurity GetMutexSecurityInternal()
        {
            var identityEveryone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var accessRule = new MutexAccessRule(identityEveryone, MutexRights.FullControl, AccessControlType.Allow);
            var security = new MutexSecurity();
            security.AddAccessRule(accessRule);

            return security;
        }

        #endregion
    }
}
