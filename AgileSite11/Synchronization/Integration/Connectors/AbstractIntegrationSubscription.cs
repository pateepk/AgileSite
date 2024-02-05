using CMS.DataEngine;
using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class representing generic subscription object.
    /// </summary>
    public abstract class AbstractIntegrationSubscription
    {
        #region "Constants"

        /// <summary>
        /// Used for subscribing to global objects.
        /// </summary>
        public const string GLOBAL_OBJECTS = "##GLOBAL##";

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of connector the subscription is attached to
        /// </summary>
        public abstract string ConnectorName
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Determines whether given CMS object and task type match the subscription
        /// </summary>
        /// <param name="obj">CMS object object to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if the info object and task correspond with subscription settings</returns>
        public abstract bool IsMatch(ICMSObject obj, TaskTypeEnum taskType, ref TaskProcessTypeEnum taskProcessType);

        #endregion
    }
}