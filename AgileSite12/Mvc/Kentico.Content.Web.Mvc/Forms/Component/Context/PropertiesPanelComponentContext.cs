using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Context for form components being rendered in the properties panel of the Form builder application.
    /// </summary>
    public class PropertiesPanelComponentContext : FormComponentContext
    {
        /// <summary>
        /// Gets or sets the biz form.
        /// </summary>
        public BizFormInfo BizFormInfo { get; set; }


        /// <summary>
        /// Gets or sets the name of the form field the properties panel is rendered for.
        /// </summary>
        public string FieldName { get; set; }
    }
}
