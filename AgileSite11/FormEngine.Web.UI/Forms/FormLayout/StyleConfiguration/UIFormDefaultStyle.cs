namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Default CSS classes used by UI form unless layout overrides them.
    /// </summary>
    public class UIFormDefaultStyle : AbstractFormStyleConfiguration
    {
        /// <summary>
        /// Initializes default CSS classes for UI form.
        /// </summary>
        public UIFormDefaultStyle()
            : base(null)
        {
            FieldCaptionCellCssClass = "FieldLabel";
            FieldErrorLabelCssClass = "FormErrorLabel";

            ExplanationTextCssClass = "ExplanationText";
        }
    }
}
