namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for initializing default CSS class for basic form.
    /// </summary>
    public class SingleTableLayoutStyle : AbstractFormStyleConfiguration
    {
        /// <summary>
        /// Sets default CSS class for current layout.
        /// </summary>
        /// <param name="formStyle">Form style configuration</param>
        public SingleTableLayoutStyle(IFormStyleConfiguration formStyle)
            : base(formStyle)
        {
            // Set bootstrap classes and append CMS-specific classes
            FormCssClass = "FormPanel";
            GroupCssClass = "EditingFormCategoryRow";
            FieldCaptionCellCssClass = "FieldLabel";
        }
    }
}
