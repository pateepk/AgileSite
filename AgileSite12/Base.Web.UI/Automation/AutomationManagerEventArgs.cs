using CMS.Automation;
using CMS.WorkflowEngine;
using CMS.Membership;
using CMS.DataEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Manager event arguments
    /// </summary>
    public class AutomationManagerEventArgs : SimpleObjectManagerEventArgs
    {
        #region "Properties"

        /// <summary>
        /// State object
        /// </summary>
        public AutomationStateInfo StateObject
        {
            get;
            set;
        }


        /// <summary>
        /// Automation process
        /// </summary>
        public WorkflowInfo Process
        {
            get;
            set;
        }


        /// <summary>
        /// Original step
        /// </summary>
        public WorkflowStepInfo OriginalStep
        {
            get;
            private set;
        }


        /// <summary>
        /// Current step
        /// </summary>
        public WorkflowStepInfo CurrentStep
        {
            get
            {
                if (InfoObject != null)
                {
                    UserInfo user = UserInfoProvider.GetUserInfo(CMSActionContext.CurrentUser.UserID);
                    AutomationManager am = AutomationManager.GetInstance(user);
                    return am.GetStepInfo(InfoObject, StateObject);
                }

                return null;
            }
        }


        /// <summary>
        /// Context of the save action. When the object was saved as a part of another action.
        /// </summary>
        public string SaveActionContext
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="originalStep">Original step</param>
        /// <param name="mode">Manager mode</param>
        /// <param name="action">Action name</param>
        public AutomationManagerEventArgs(BaseInfo infoObj, AutomationStateInfo stateObj, WorkflowStepInfo originalStep, FormModeEnum mode, string action)
            : base(infoObj, mode)
        {
            StateObject = stateObj;
            IsValid = true;
            OriginalStep = originalStep;
            ActionName = action;
        }

        #endregion
    }
}
