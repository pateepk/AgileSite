using System;
using System.Web.UI;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Custom event arguments used for control registration in BasicForm.
    /// </summary>
    public class ControlRegistrationEventArgs : EventArgs
    {
        /// <summary>
        /// Control being registered.
        /// </summary>
        public Control ChildControl { get; private set; }


        /// <summary>
        /// Field name for the control.
        /// </summary>
        public string FieldName { get; private set; }


        /// <summary>
        /// Default constructor for event arguments used in control registration process.
        /// </summary>
        /// <param name="childControl">Child control being registered</param>
        /// <param name="fieldName">Field name for the control</param>
        public ControlRegistrationEventArgs(Control childControl, string fieldName)
        {
            ChildControl = childControl;
            FieldName = fieldName;
        }
    }
}
