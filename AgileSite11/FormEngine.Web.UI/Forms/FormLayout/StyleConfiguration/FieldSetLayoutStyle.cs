namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for initializing default CSS class for basic form.
    /// </summary>
    public class FieldSetLayoutStyle : AbstractFormStyleConfiguration
    {
        /// <summary>
        /// Sets default CSS class for current layout.
        /// </summary>
        /// <param name="formStyle">Form style configuration</param>
        public FieldSetLayoutStyle(IFormStyleConfiguration formStyle)
            : base(formStyle)
        {
            FormCssClass = "FormPanel";
            GroupCssClass = "EditingFormFieldSet";
            FieldCaptionCellCssClass = "FieldLabel";
            FormButtonPanelCssClass = "FieldPanel";
        }
    }
}
