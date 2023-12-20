using System.Collections.Generic;

using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// View model for rendering an automatically generated form.
    /// </summary>
    public sealed class GeneratedFormViewModel
    {
        /// <summary>
        /// Gets or sets a collection of <see cref="FormComponent"/>s that will be rendered in the automatically generated form.
        /// </summary>
        public IEnumerable<FormComponent> FormComponents { get; set; }
    }
}
