using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a result of the get updated entities command.
    /// </summary>
    public sealed class GetUpdatedEntitiesResult
    {

        #region "Private members"

        private WebServiceClient.GetUpdatedResult mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the timestamp of the last updated entity in the requested time window.
        /// </summary>
        public DateTime MaxWindowDateTimeUtc
        {
            get
            {
                return mSource.latestDateCovered.ToUniversalTime();
            }
        }

        /// <summary>
        /// Gets a list of entities that were updated in the requested time window.
        /// </summary>
        public IEnumerable<string> EntityIds
        {
            get
            {
                return mSource.ids.AsEnumerable();
            }
        }

        #endregion

        #region "Constructors"

        internal GetUpdatedEntitiesResult(WebServiceClient.GetUpdatedResult source)
        {
            mSource = source;
        }

        #endregion

    }

}