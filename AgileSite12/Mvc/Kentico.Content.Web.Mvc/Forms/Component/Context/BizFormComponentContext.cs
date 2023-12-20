using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Context for form components being rendered as a representation of a biz form.
    /// </summary>
    public class BizFormComponentContext : FormComponentContext
    {
        /// <summary>
        /// Gets or sets the biz form.
        /// </summary>
        public BizFormInfo FormInfo { get; set; }


        /// <summary>
        /// An event raised upon saving the corresponding <see cref="BizFormItem"/>. The before and after phases can be used to perform
        /// additional actions when saving the item.
        /// </summary>
        public readonly SaveBizFormItemHandler SaveBizFormItem = new SaveBizFormItemHandler { Name = "BizFormComponentContext.SavingBizFormItem" };

        
        /// <summary>
        /// Copies values of this object into <paramref name="targetObject"/>.
        /// Does not copy <see cref="SaveBizFormItem"/> event handlers.
        /// </summary>
        public void CopyTo(BizFormComponentContext targetObject)
        {
            base.CopyTo(targetObject);

            targetObject.FormInfo = FormInfo;
        }
    }
}
