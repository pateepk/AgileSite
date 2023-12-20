namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// View model for rendering Form builder page.
    /// </summary>
    public class FormBuilderPage
    {
        /// <summary>
        /// Id of the displayed form.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Collection of identifiers of form components that should be available in form builder.
        /// </summary>
        public string AvailableFormBuilderComponents { get; set; }


        /// <summary>
        /// Indicates that maximum allowed number of forms is exceeded in current license.
        /// </summary>
        public bool FormsLimitExceeded
        {
            get;
            set;
        }
    }
}
