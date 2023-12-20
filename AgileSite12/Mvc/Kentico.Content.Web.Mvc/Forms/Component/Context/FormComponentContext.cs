namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a base class for form components contexts.
    /// </summary>
    /// <seealso cref="BizFormComponentContext"/>
    /// <seealso cref="PropertiesPanelComponentContext"/>
    public abstract class FormComponentContext
    {
        /// <summary>
        /// Indicates whether the component is rendered in a context in which it can be submitted, or is intended just for viewing.
        /// </summary>
        public bool FormIsSubmittable { get; set; } = true;


        /// <summary>
        /// Copies values of this object into <paramref name="targetObject"/>.
        /// </summary>
        public void CopyTo(FormComponentContext targetObject)
        {
            targetObject.FormIsSubmittable = FormIsSubmittable;
        }
    }
}
