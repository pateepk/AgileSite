using Kentico.PageBuilder.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.Widgets
{
    /// <summary>
    /// Form widget properties
    /// </summary>
    public class FormWidgetProperties : IWidgetProperties
    {
        /// <summary>
        /// Name of the selected form
        /// </summary>
        public string SelectedForm { get; set; }
    }
}