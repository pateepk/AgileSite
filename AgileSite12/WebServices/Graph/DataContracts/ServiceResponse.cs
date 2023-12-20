using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.WebServices
{
    /// <summary>
    /// Service general response.
    /// </summary>
    [DataContract]
    public class ServiceResponse
    {
        #region "Data member properties"

        /// <summary>
        /// Status code
        /// </summary>
        [DataMember]
        public ResponseStatusEnum StatusCode
        {
            get;
            set;
        }


        /// <summary>
        /// Status message
        /// </summary>
        [DataMember]
        public string StatusMessage
        {
            get;
            set;
        }


        /// <summary>
        /// ScreenLock interval
        /// </summary>
        [DataMember]
        public int ScreenLockInterval
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServiceResponse()
        {
            StatusCode = ResponseStatusEnum.None;
        }


        /// <summary>
        /// Simple parametric constructor
        /// </summary>
        /// <param name="statusCode">Status code</param>
        public ServiceResponse(ResponseStatusEnum statusCode)
        {
            StatusCode = statusCode;
            ScreenLockInterval = SecurityHelper.GetSecondsToShowScreenLockAction(SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Parametric constructor
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="statusMessage">Status message</param>
        public ServiceResponse(ResponseStatusEnum statusCode, string statusMessage)
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage;
            ScreenLockInterval = SecurityHelper.GetSecondsToShowScreenLockAction(SiteContext.CurrentSiteName);
        }

        #endregion
    }


    /// <summary>
    /// Generic service response
    /// </summary>
    /// <typeparam name="TData">Type of payload</typeparam>
    [DataContract]
    public class ServiceResponse<TData> : ServiceResponse
    {
        #region "Data member properties"

        /// <summary>
        /// Payload
        /// </summary>
        [DataMember]
        public TData Data
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServiceResponse()
        {
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="nonGeneric">Source</param>
        public ServiceResponse(ServiceResponse nonGeneric)
            : this(nonGeneric.StatusCode, nonGeneric.StatusMessage)
        {
        }


        /// <summary>
        /// Parametric constructor for OK cases
        /// </summary>
        /// <param name="data">Data of response</param>
        public ServiceResponse(TData data)
            : base(ResponseStatusEnum.OK)
        {
            Data = data;
        }


        /// <summary>
        /// Parametric constructor for status response only
        /// </summary>
        /// <param name="statusCode">Status code</param>
        public ServiceResponse(ResponseStatusEnum statusCode)
            : base(statusCode)
        {
        }


        /// <summary>
        /// Parametric constructor for error cases
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="statusMessage">Status message</param>
        public ServiceResponse(ResponseStatusEnum statusCode, string statusMessage)
            : base(statusCode, statusMessage)
        {
        }

        
        /// <summary>
        /// Parametric constructor for error cases
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="statusMessage">Status message</param>
        /// <param name="data">Data of response</param>
        public ServiceResponse(ResponseStatusEnum statusCode, string statusMessage, TData data)
            : base(statusCode, statusMessage)
        {
            Data = data;
        }

        #endregion
    }
}
