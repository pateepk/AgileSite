using System.Collections.Generic;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Model used for drop-down inline editor.
    /// </summary>
    public class DropdownEditorViewModel
    {
        /// <summary>
        /// Name of the widget property to edit.
        /// </summary>
        public string PropertyName { get; set; }


        /// <summary>
        /// All options of the selector.
        /// </summary>
        public List<SelectListItem> Options { get; set; }


        /// <summary>
        /// Selected option.
        /// </summary>
        public string Selected { get; set; }


        /// <summary>
        /// Key used to display localized label for the drop-down.
        /// </summary>
        public string LabelKey { get; set; }


        /// <summary>
        /// Key used to display localized message when no options are available.
        /// </summary>
        public string NoOptionsMessageKey { get; set; }
    }
}