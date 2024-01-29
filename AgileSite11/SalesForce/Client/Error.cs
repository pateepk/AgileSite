using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce integration API error.
    /// </summary>
    public sealed class Error
    {

        #region "Private members"

        private WebServiceClient.Error mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the error status code.
        /// </summary>
        public StatusCode StatusCode
        {
            get
            {
                return (StatusCode)Enum.ToObject(typeof(StatusCode), mSource.statusCode);
            }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message
        {
            get
            {
                return mSource.message;
            }
        }

        /// <summary>
        /// Gets a collection of attribute names that are related to this error.
        /// </summary>
        public IEnumerable<string> AttributeNames
        {
            get
            {
                return mSource.fields.AsEnumerable();
            }
        }

        #endregion

        #region "Constructors"

        internal Error(WebServiceClient.Error source)
        {
            mSource = source;
        }

        #endregion

    }

}