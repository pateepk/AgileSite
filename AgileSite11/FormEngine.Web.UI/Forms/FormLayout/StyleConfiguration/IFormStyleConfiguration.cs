namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Interface describing form style configuration options.
    /// </summary>
    public interface IFormStyleConfiguration
    {
        /// <summary>
        /// CSS class which will be used to wrap form control.
        /// </summary>
        string FormCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the whole field (usually row).
        /// </summary>
        string FieldCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the whole field group with heading.
        /// </summary>
        string GroupCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the whole field group.
        /// </summary>
        string FieldGroupCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the field group caption.
        /// </summary>
        string FieldGroupCaptionCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field control cell.
        /// </summary>
        string FieldValueCellCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field label cell.
        /// </summary>
        string FieldCaptionCellCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field label.
        /// </summary>
        string FieldCaptionCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field error label cell.
        /// </summary>
        string FieldErrorCellCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field error label.
        /// </summary>
        string FieldErrorLabelCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class that is added to editing form control after its validation failed.
        /// </summary>
        string FieldErrorCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field visibility cell.
        /// </summary>
        string FieldVisibilityCellCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the individual field visibility control.
        /// </summary>
        string FieldVisibilityCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class that is added to container that is wrapping content after text (only if content before text is empty).
        /// </summary>
        string ExplanationTextCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the form button.
        /// </summary>
        string FormButtonCssClass
        {
            get;
        }


        /// <summary>
        /// CSS class for the form button panel.
        /// </summary>
        string FormButtonPanelCssClass
        {
            get;
        }
    }
}
