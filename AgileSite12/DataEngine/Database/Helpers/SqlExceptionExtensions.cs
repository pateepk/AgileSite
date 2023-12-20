using System.Data.SqlClient;

namespace CMS.DataEngine
{
    /// <summary>
    /// Extension methods for <see cref="SqlException"/> class.
    /// </summary>
    public static class SqlExceptionExtensions
    {
        /// <summary>
        /// Error code of exception which indicates a database deadlock
        /// </summary>
        private const int SQL_EXCEPTION_DEADLOCK_ERROR_CODE = 1205;


        /// <summary>
        /// Returns true when given <see cref="SqlException"/> is caused by deadlock.
        /// </summary>
        public static bool HasDeadlockOccured(this SqlException exception)
        {
            if (exception == null)
            {
                return false;
            }

            return exception.Number == SQL_EXCEPTION_DEADLOCK_ERROR_CODE;
        }
    }
}
