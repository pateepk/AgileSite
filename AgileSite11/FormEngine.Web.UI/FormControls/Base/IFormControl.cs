namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form control interface.
    /// </summary>
    public interface IFormControl
    {
        /// <summary>
        /// Field info object.
        /// </summary>
        FormFieldInfo FieldInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Parent form.
        /// </summary>
        BasicForm Form
        {
            get;
            set;
        }


        /// <summary>
        /// Gets ClientID of the control from which the Value is retrieved or 
        /// null if such a control can't be specified.
        /// </summary>
        string ValueElementID
        {
            get;
        }


        /// <summary>
        /// Gets or sets field value. You need to override this method to make the control work properly with the form.
        /// </summary>
        object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Returns value prepared for validation.
        /// </summary>
        object ValueForValidation
        {
            get;
        }


        /// <summary>
        /// Gets the display name of the value item. Returns null if display name is not available.
        /// </summary>
        string ValueDisplayName
        {
            get;
        }


        /// <summary>
        /// Returns true if the control has value, if false, the value from the control should not be used within the form to update the data
        /// </summary>
        bool HasValue
        {
            get;
        }


        /// <summary>
        /// Validation error string shown when the control is not valid.
        /// </summary>
        string ValidationError
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        bool IsValid();


        /// <summary>
        /// Returns an array of values of any other fields returned by the control.
        /// </summary>
        /// <remarks>It returns an array where first dimension is attribute name and the second dimension is its value.</remarks>
        object[,] GetOtherValues();


        /// <summary>
        /// Loads the other fields values to the state of the form control
        /// </summary>
        void LoadOtherValues();


        /// <summary>
        /// Helper property to use custom parameter in form control.
        /// </summary>
        object FormControlParameter
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the control and all its child controls are displayed.
        /// </summary>
        bool Visible
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the control can respond to user interaction.
        /// </summary>
        bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Loads control value
        /// </summary>
        /// <param name="value">Value to load</param>
        void LoadControlValue(object value);
    }
}