using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(SKUInfo.OBJECT_TYPE_SKU, typeof(SKUCloneSettings))]

namespace CMS.Ecommerce.Web.UI
{
    internal class SKUCloneSettings : CloneSettingsControl
    {
        public override bool DisplayControl
        {
            get { return false; }
        }


        public override string ExcludedOtherBindingTypes
        {
            get
            {
                return BundleInfo.OBJECT_TYPE + ";" + WishlistItemInfo.OBJECT_TYPE;
            }
        }
    }
}
