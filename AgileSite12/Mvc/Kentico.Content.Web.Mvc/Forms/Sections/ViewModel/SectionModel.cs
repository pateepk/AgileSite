using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// View model for rendering form sections.
    /// </summary>
    public class SectionModel
    {
        /// <summary>
        /// Configuration for the form fields rendering.
        /// </summary>
        public FormFieldRenderingConfiguration RenderingConfiguration { get; set; }


        /// <summary>
        /// Ordered list of form components rendered for each zone.
        /// </summary>
        public IList<IList<FormComponent>> ZonesContent { get; set; }


        /// <summary>
        /// Creates instance of view model for section.
        /// </summary>
        public SectionModel()
        {
            ZonesContent = new List<IList<FormComponent>>();
        }
    }
}
