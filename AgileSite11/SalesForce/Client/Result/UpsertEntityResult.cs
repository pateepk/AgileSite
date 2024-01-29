using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{
    
    /// <summary>
    /// Represents a result of the upsert entity command.
    /// </summary>
    public sealed class UpsertEntityResult : IEntityCommandResult
    {

        #region "Private members"

        private WebServiceClient.UpsertResult mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the identifier of the upserted entity, if the operation was successful.
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
        /// Gets the value indicating whether the entity was updated, and not created.
        /// </summary>
        public bool IsUpdate
        {
            get
            {
                return !mSource.created;
            }
        }

        /// <summary>
        /// Gets a list of errors, if the operation was not successful.
        /// </summary>
        public IEnumerable<Error> Errors
        {
            get
            {
                if (mSource.errors == null)
                {
                    return Enumerable.Empty<Error>();
                }

                return mSource.errors.Select(x => new Error(x));
            }
        }

        #endregion

        #region "Constructors"

        internal UpsertEntityResult(WebServiceClient.UpsertResult source)
        {
            mSource = source;
        }

        #endregion

    }

}