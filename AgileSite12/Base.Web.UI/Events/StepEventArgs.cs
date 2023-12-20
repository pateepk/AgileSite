using System;

using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Wizard step event arguments
    /// </summary>
    public class StepEventArgs : CMSEventArgs, ISimpleDataContainer
    {
        #region "Variables"

        private StringSafeDictionary<object> properties = new StringSafeDictionary<object>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value from/to object collection
        /// </summary>
        /// <param name="columnName">Property name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Total number of steps
        /// </summary>
        public int Steps
        {
            get;
            protected set;
        }


        /// <summary>
        /// Current step index indexed from 0
        /// </summary>
        public int CurrentStep
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the current operation should be canceled (refers to the validation and finish event)
        /// </summary>
        public bool CancelEvent
        {
            get;
            set;
        }


        /// <summary>
        /// If true, current step is skipped (use in the load event)
        /// </summary>
        public bool Skip 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Gets or sets the URL which is used after final step
        /// </summary>
        public string FinalStepUrl
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the back button is hidden (use in the load event)
        /// </summary>
        public bool HideBackButton 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Text for the next button (use in the load event)
        /// </summary>
        public string NextButtonText 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Text for the back button (use in the load event)
        /// </summary>
        public string BackButtonText
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the next button should be hidden in current step, there is no next action (use in the load event)
        /// </summary>
        public bool HideNextButton 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Gets the value that indicates whether current step is first step
        /// </summary>
        public bool IsFirstStep
        {
            get
            {
                return (CurrentStep == 0);
            }
        }


        /// <summary>
        /// Gets the value that indicates whether current step is last step
        /// </summary>
        public bool IsLastStep
        {
            get
            {
                return (CurrentStep == (Steps - 1));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="steps">Total number of steps</param>
        /// <param name="currentStep">Current step number</param>
        public StepEventArgs(int steps, int currentStep)
        {
            Steps = steps;
            CurrentStep = currentStep;
        }

        
        /// <summary>
        /// Gets the value from the object collection
        /// </summary>
        /// <param name="columnName">Property name</param>
        public object GetValue(string columnName)
        {
            return properties[columnName];
        }


        /// <summary>
        /// Sets the value to the object collection
        /// </summary>
        /// <param name="columnName">Property name</param>
        /// <param name="value">Value</param>
        public bool SetValue(string columnName, object value)
        {
            properties[columnName] = value;
            return true;
        }
        
        #endregion
    }
}
