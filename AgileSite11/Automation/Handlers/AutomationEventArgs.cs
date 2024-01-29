using CMS.DataEngine;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Automation event arguments
    /// </summary>
    public class AutomationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Edited version of the object
        /// </summary>
        public BaseInfo InfoObject
        {
            get;
            set;
        }      
        
        
        /// <summary>
        /// State object
        /// </summary>
        public AutomationStateInfo StateObject
        {
            get;
            set;
        }
        

        /// <summary>
        /// Previous step
        /// </summary>
        public WorkflowStepInfo PreviousStep
        {
            get;
            set;
        }
    }
}