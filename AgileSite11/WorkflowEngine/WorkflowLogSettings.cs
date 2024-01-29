using System;

using CMS.Membership;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class for workflow history log settings.
    /// </summary>
    public class WorkflowLogSettings
    {
        #region "Properties"

        /// <summary>
        /// Version history ID
        /// </summary>
        public int VersionHistoryId
        {
            get;
            set;
        }


        /// <summary>
        /// Source workflow step
        /// </summary>
        public WorkflowStepInfo SourceStep
        {
            get;
            set;
        }


        /// <summary>
        /// Target workflow step
        /// </summary>
        public WorkflowStepInfo TargetStep
        {
            get;
            set;
        }


        /// <summary>
        /// Comment
        /// </summary>
        public string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if step was rejected.
        /// </summary>
        public bool Rejected
        {
            get;
            set;
        }


        /// <summary>
        /// Date and time of the action.
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }


        /// <summary>
        /// User info for the action.
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object ID
        /// </summary>
        public int ObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// Transition type
        /// </summary>
        public WorkflowTransitionTypeEnum TransitionType
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
        public WorkflowLogSettings(string objectType, int objectId)
        {
            ObjectID = objectId;
            ObjectType = objectType;
            Time = DateTime.Now;
        }

        #endregion
    }
}