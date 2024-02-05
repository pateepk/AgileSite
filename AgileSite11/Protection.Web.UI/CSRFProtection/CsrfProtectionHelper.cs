using System.Web.UI;

using CMS.Base;

namespace CMS.Protection.Web.UI
{
    /// <summary>
    /// Helper class for CSRF protection module.
    /// </summary>
    internal class CsrfProtectionHelper : AbstractHelper<CsrfProtectionHelper>
    {
        /// <summary>
        /// Register hidden field on the page.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="hiddenFieldName">Hidden field name</param>
        /// <param name="hiddenFieldValue">Hidden field value</param>
        public static void RegisterHiddenField(Page page, string hiddenFieldName, string hiddenFieldValue)
        {
            HelperObject.RegisterHiddenFieldInternal(page, hiddenFieldName, hiddenFieldValue);
        }


        /// <summary>
        /// Register hidden field on the page.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="hiddenFieldName">Hidden field name</param>
        /// <param name="hiddenFieldValue">Hidden field value</param>
        protected virtual void RegisterHiddenFieldInternal(Page page, string hiddenFieldName, string hiddenFieldValue)
        {
            page.ClientScript.RegisterHiddenField(hiddenFieldName, hiddenFieldValue);
        }
    }
}
