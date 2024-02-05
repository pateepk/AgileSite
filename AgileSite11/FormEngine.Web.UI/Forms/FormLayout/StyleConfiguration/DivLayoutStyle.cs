namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for initializing default CSS class for basic form.
    /// </summary>
    public class DivLayoutStyle : AbstractFormStyleConfiguration
    {
        /// <summary>
        /// Sets default CSS class for current layout.
        /// </summary>
        /// <param name="formStyle">Form style configuration</param>
        public DivLayoutStyle(IFormStyleConfiguration formStyle)
            : base(formStyle)
        {
            FormCssClass = "form-horizontal";

            GroupCssClass = "editing-form-category";
            FieldGroupCssClass = "editing-form-category-fields";
            FieldGroupCaptionCssClass = "editing-form-category-caption";

            FieldCssClass = "form-group";

            FieldCaptionCellCssClass = "editing-form-label-cell";
            FieldCaptionCssClass = "control-label editing-form-label";

            FieldValueCellCssClass = "editing-form-value-cell";
            ExplanationTextCssClass = "explanation-text";

            FormButtonPanelCssClass = "form-group form-group-submit FieldPanel";
        }
    }
}
