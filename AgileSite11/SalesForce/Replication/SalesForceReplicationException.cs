using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a replication exception that aggregates multiple errors.
    /// </summary>
    public sealed class SalesForceReplicationException : SalesForceException
    {

        #region "Private members"

        private string[] mErrors;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets an enumerable collection of replication errors.
        /// </summary>
        public IEnumerable<string> Errors
        {
            get
            {
                return mErrors.AsEnumerable();
            }
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the replication exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="errors">The enumerable collection of replication errors.</param>
        public SalesForceReplicationException(string message, IEnumerable<string> errors) : base(message)
        {
            mErrors = errors.ToArray();
        }

        #endregion

    }

}