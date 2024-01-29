using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a result of the get deleted entities command.
    /// </summary>
    public sealed class GetDeletedEntitiesResult
    {

        #region "Private members"

        private WebServiceClient.GetDeletedResult mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the timestamp of the last deleted entity.
        /// </summary>
        public DateTime MaxDateTimeUtc
        {
            get
            {
                return mSource.earliestDateAvailable.ToUniversalTime();
            }
        }

        /// <summary>
        /// Gets the timestamp of the last deleted entity in the requested time window.
        /// </summary>
        public DateTime MaxWindowDateTimeUtc
        {
            get
            {
                return mSource.latestDateCovered.ToUniversalTime();
            }
        }

        /// <summary>
        /// Gets a list of entities that were deleted in the requested time window.
        /// </summary>
        public IEnumerable<GetDeletedEntitiesResultItem> Items
        {
            get
            {
                return mSource.deletedRecords.Select(x => new GetDeletedEntitiesResultItem(x));
            }
        }

        #endregion

        #region "Constructors"

        internal GetDeletedEntitiesResult(WebServiceClient.GetDeletedResult source)
        {
            mSource = source;
        }

        #endregion

    }

}