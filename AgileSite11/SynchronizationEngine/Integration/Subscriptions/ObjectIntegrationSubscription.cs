using System;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class representing subscription to objects
    /// </summary>
    public class ObjectIntegrationSubscription : BaseIntegrationSubscription
    {
        #region "Properties"

        /// <summary>
        /// Type of object to match
        /// </summary>
        public string ObjectType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Code name of object to match
        /// </summary>
        public string ObjectCodeName
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="connectorName">Name of connector the subscription is attached to</param>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task which to subscribe to</param>
        /// <param name="siteName">Name of site which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        /// <param name="objectType">Type of object which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        /// <param name="objectCodeName">Code name of object which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        public ObjectIntegrationSubscription(string connectorName, TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, string siteName, string objectType, string objectCodeName)
            : base(connectorName, taskProcessType, taskType, siteName)
        {
            ObjectType = objectType;
            ObjectCodeName = objectCodeName;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Determines whether given info object and task type match the subscription
        /// </summary>
        /// <param name="obj">GeneralizedInfo to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if the info object and task correspond with subscription settings</returns>
        public override bool IsMatch(ICMSObject obj, TaskTypeEnum taskType, ref TaskProcessTypeEnum taskProcessType)
        {
            GeneralizedInfo infoObject = obj as GeneralizedInfo;
            if (infoObject != null)
            {
                bool result = base.IsMatch(infoObject, taskType, ref taskProcessType);

                Regex codeNameRegex = GetRegex(ObjectCodeName);
                if (codeNameRegex != null)
                {
                    result &= codeNameRegex.IsMatch(infoObject.ObjectCodeName);
                }
                Regex objectTypeRegex = GetRegex(ObjectType);
                if (objectTypeRegex != null)
                {
                    result &= objectTypeRegex.IsMatch(infoObject.TypeInfo.ObjectType);
                }
                return result;
            }
            return false;
        }

        #endregion
    }
}