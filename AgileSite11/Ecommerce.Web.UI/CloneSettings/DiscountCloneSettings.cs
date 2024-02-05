using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(DiscountInfo.OBJECT_TYPE, typeof(DiscountCloneSettings))]

namespace CMS.Ecommerce.Web.UI
{
    internal class DiscountCloneSettings : CloneSettingsControl
    {
        public override string ExcludedChildTypes
        {
            get { return CouponCodeInfo.OBJECT_TYPE; }
        }


        public override bool DisplayControl
        {
            get { return false; }
        }
    }
}
