using System.Web.Mvc;

using CMS.ContactManagement;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines context for customizable model binding.
    /// </summary>
    public class FormBuilderModelBindingContext : ModelBindingContext
    {
        /// <summary>
        /// Gets or sets filter used during model binding.
        /// </summary>
        public BizFormItem ExistingItem
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the current contact.
        /// </summary>
        public ContactInfo Contact
        {
            get;
            set;
        }
    }
}
