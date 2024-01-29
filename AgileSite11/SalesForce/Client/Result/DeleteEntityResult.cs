using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a result of the delete entity command.
    /// </summary>
    public sealed class DeleteEntityResult
    {

        #region "Private members"

        private WebServiceClient.DeleteResult mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the identifier of the deleted entity, if the operation was successful.
        /// </summary>
        public string EntityId
        {
            get
            {
                return mSource.id;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return mSource.success;
            }
        }

        /// <summary>
        /// Gets a list of errors, if the operation was not successful.
        /// </summary>
        public IEnumerable<Error> Errors
        {
            get
            {
                return mSource.errors.Select(x => new Error(x));
            }
        }

        #endregion

        #region "Constructors"

        internal DeleteEntityResult(WebServiceClient.DeleteResult source)
        {
            mSource = source;
        }

        #endregion

    }

}