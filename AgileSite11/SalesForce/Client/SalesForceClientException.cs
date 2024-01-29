using System;
using System.ServiceModel;

using CMS.SalesForce.WebServiceClient;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce client exception.
    /// </summary>
    public class SalesForceClientException : SalesForceException
    {

        #region "Private members"

        private string mCode;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the exception code.
        /// </summary>
        public string Code
        {
            get
            {
                return mCode;
            }
        }

        #endregion

        #region "Constructors"

        internal SalesForceClientException(ApiFault error, FaultException innerException) : base(error.exceptionMessage, innerException)
        {
            mCode = Enum.GetName(error.exceptionCode.GetType(), error.exceptionCode);
        }

        #endregion

    }

}