using System;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a deleted entity.
    /// </summary>
    public sealed class GetDeletedEntitiesResultItem
    {

        #region "Private members"

        private WebServiceClient.DeletedRecord mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the identifier of the deleted entity.
        /// </summary>
        public string EntityId
        {
            get
            {
                return mSource.id;
            }
        }

        /// <summary>
        /// Gets the date and time when the entity was deleted.
        /// </summary>
        public DateTime DeletionDateTimeUtc
        {
            get
            {
                return mSource.deletedDate.ToUniversalTime();
            }
        }

        #endregion

        #region "Constructors"

        internal GetDeletedEntitiesResultItem(WebServiceClient.DeletedRecord source)
        {
            mSource = source;
        }

        #endregion

    }

}