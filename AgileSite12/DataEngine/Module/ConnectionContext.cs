using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Connection context.
    /// </summary>
    public class ConnectionContext : AbstractContext<ConnectionContext>, INotCopyThreadItem
    {
        private IDataConnection mCurrentScopeConnection;
        private CMSConnectionScope mCurrentConnectionScope;


        /// <summary>
        /// Current DB connection to use within current connection scope.
        /// </summary>
        public static IDataConnection CurrentScopeConnection
        {
            get
            {
                return Current.mCurrentScopeConnection;
            }
            set
            {
                Current.mCurrentScopeConnection = value;
            }
        }


        /// <summary>
        /// Request connection scope.
        /// </summary>
        public static CMSConnectionScope CurrentConnectionScope
        {
            get
            {
                return Current.mCurrentConnectionScope;
            }
            set
            {
                Current.mCurrentConnectionScope = value;
            }
        }


        /// <summary>
        /// Ensures the <see cref="CMSConnectionScope"/> for current thread.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns><c>true</c> if <see cref="CMSConnectionScope"/> was created for current thread; otherwise <c>false</c></returns>
        /// <remarks>Thread scope should be created only once per thread lifetime.</remarks>
        public static bool EnsureThreadScope(string connectionString)
        {
            CMSConnectionScope scope = CurrentConnectionScope;
            if (scope != null)
            {
                return false;
            }

            scope = new CMSConnectionScope(connectionString);
            scope.Connection.KeepOpen = ConnectionHelper.KeepContextConnectionOpen;
            CurrentConnectionScope = scope;

            return true;
        }


        /// <summary>
        /// Disposes the <see cref="CMSConnectionScope"/> for current thread.
        /// </summary>
        public static void DisposeThreadScope()
        {
            var scope = CurrentConnectionScope;
            if (scope != null)
            {
                scope.Connection.KeepOpen = false;
                scope.Connection.Close();

                scope.Dispose();

                CurrentConnectionScope = null;
            }
        }
    }
}