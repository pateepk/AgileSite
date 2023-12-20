using CMS.Base;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Arguments of event represented by <see cref="SaveBizFormItemHandler"/>.
    /// </summary>
    public class SaveBizFormItemEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Form item which is being inserted or updated.
        /// </summary>
        public BizFormItem FormItem { get; set; }
    }
}
