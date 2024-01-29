using CMS.Base;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for Set attribute value
    /// </summary>
    public class ContactPropertyAction : ContactAutomationAction
    {
        #region "Parameters"

        /// <summary>
        /// Gets property name
        /// </summary>
        protected virtual string PropertyName
        {
            get
            {
                return GetResolvedParameter<string>("PropertyName", null);
            }
        }


        /// <summary>
        /// Gets property value
        /// </summary>
        protected virtual string PropertyValue
        {
            get
            {
                return GetResolvedParameter<string>("PropertyValue", null);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Execute
        /// </summary>
        public override void Execute()
        {
            if ((Contact != null) && !string.IsNullOrEmpty(PropertyName))
            {
                using (new CMSActionContext
                {
                    // Set evaluation context culture to the default one cause macros are being evaluated in en-us context (mainly datetime issue)
                    ThreadCulture = CultureHelper.EnglishCulture
                })
                {
                    Contact.SetValue(PropertyName, PropertyValue);
                    Contact.Update();
                }
            }
        }

        #endregion
    }
}
