using System.Collections.Generic;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.Widgets
{
    /// <summary>
    /// Form widget view model
    /// </summary>
    public class FormWidgetViewModel
    {
        /// <summary>
        /// Name of the currently selected form.
        /// </summary>
        public string FormName { get; set; }


        /// <summary>
        /// Configuration of selected form.
        /// </summary>
        public FormBuilderConfiguration FormConfiguration { get; set; }


        /// <summary>
        /// Forms belonging to the current site.
        /// </summary>
        public List<SelectListItem> SiteForms { get; set; }


        /// <summary>
        /// Form components of the currently selected form.
        /// </summary>
        public List<FormComponent> FormComponents { get; set; }


        /// <summary>
        /// Generated prefix for unique form field identifiers.
        /// </summary>
        public string FormPrefix { get; set; }


        /// <summary>
        /// Text of the submit button.
        /// </summary>
        public string SubmitButtonText { get; set; }


        /// <summary>
        /// Image source of the submit button.
        /// </summary>
        public string SubmitButtonImage { get; set; }


        /// <summary>
        /// Indicates whether the form widget can be submitted, or is intended just for viewing.
        /// </summary>
        public bool IsFormSubmittable { get; set; } = true;
        
    }
}