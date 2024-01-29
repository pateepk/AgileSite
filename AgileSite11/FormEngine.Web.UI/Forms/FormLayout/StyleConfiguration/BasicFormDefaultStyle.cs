namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Default CSS classes used by basic form unless layout overrides them.
    /// </summary>
    public class BasicFormDefaultStyle : AbstractFormStyleConfiguration
    {
        /// <summary>
        /// Initializes default CSS classes for basic form.
        /// </summary>
        public BasicFormDefaultStyle()
            : base(null)
        {
            FormCssClass = "FormPanel";

            FormButtonCssClass = "FormButton";

            FieldCaptionCellCssClass = "EditingFormLabelCell";
            FieldCaptionCssClass = "EditingFormLabel";

            FieldErrorCellCssClass = "EditingFormErrorCell";
            FieldErrorLabelCssClass = "EditingFormErrorLabel";

            FieldValueCellCssClass = "EditingFormValueCell";
            ExplanationTextCssClass = "ExplanationText";

            FieldVisibilityCellCssClass = "EditingFormVisibilityCell";
            FieldVisibilityCssClass = "EditingFormVisibility";
        }
    }
}
