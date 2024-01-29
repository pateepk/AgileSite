namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for initializing default CSS class for basic form.
    /// </summary>
    public class TablePerCategoryLayoutCssClass : AbstractFormStyleConfiguration
    {
        /// <summary>
        /// Sets default CSS class for current layout.
        /// </summary>
        /// <param name="formStyle">Form style configuration</param>
        public TablePerCategoryLayoutCssClass(IFormStyleConfiguration formStyle)
            : base(formStyle)
        {
            GroupCssClass = "EditingFormCategoryTable";
            FieldGroupCaptionCssClass = "EditingFormCategoryRow";
            FieldCssClass = "EditingFormRow";
            FieldCaptionCellCssClass = "FieldLabel";
        }
    }
}
