using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(GiftCardInfo.OBJECT_TYPE, typeof(GiftCardCloneSettings))]

namespace CMS.Ecommerce.Web.UI
{
    internal class GiftCardCloneSettings : CloneSettingsControl
    {
        public override string ExcludedChildTypes
        {
            get { return GiftCardCouponCodeInfo.OBJECT_TYPE; }
        }


        public override bool DisplayControl
        {
            get { return false; }
        }
    }
}