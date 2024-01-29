using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class containing event data pertaining to an event raised for a form control.
    /// </summary>
    public class FormEngineUserControlEventArgs : CMSEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Gets the name of the column (field) this form control works with.
        /// </summary>
        /// <value>The name of the column</value>
        public string ColumnName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the form control.
        /// </summary>
        /// <value>The form control</value>
        public FormEngineUserControl FormControl
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets the value of a form control.
        /// </summary>
        /// <value>The value</value>
        public object Value
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="FormEngineUserControlEventArgs"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="formControl">The form control</param>
        /// <param name="value">The value</param>
        public FormEngineUserControlEventArgs(string columnName, FormEngineUserControl formControl, object value)
        {
            ColumnName = columnName;
            FormControl = formControl;
            Value = value;
        }

        #endregion
    }
}