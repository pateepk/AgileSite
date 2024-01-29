using System;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Registered external edit control
    /// </summary>
    internal class RegisteredExternalEditControl
    {
        #region "Properties"

        /// <summary>
        /// Control condition, gets the item extension as parameter
        /// </summary>
        public Func<string, string, bool> Condition
        {
            get;
            set;
        }


        /// <summary>
        /// Control definition
        /// </summary>
        public ControlDefinition Control
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Control definition</param>
        /// <param name="condition">Control condition, gets the item extension as parameter</param>
        public RegisteredExternalEditControl(ControlDefinition control, Func<string, string, bool> condition)
        {
            Control = control;
            Condition = condition;
        }

        #endregion
    }
}