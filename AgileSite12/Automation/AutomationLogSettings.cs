using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Class for automation history log settings.
    /// </summary>
    public class AutomationLogSettings : WorkflowLogSettings
    {
        #region "Properties"

        /// <summary>
        /// State object ID
        /// </summary>
        public int StateObjectID
        {
            get;
            set;
        }
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="stateId">State object ID</param>
        public AutomationLogSettings(string objectType, int objectId, int stateId) :
            base(objectType, objectId)
        {
            StateObjectID = stateId;
        }

        #endregion
    }
}