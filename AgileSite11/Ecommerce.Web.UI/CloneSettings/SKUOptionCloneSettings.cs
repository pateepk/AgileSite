using CMS.UIControls;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;

[assembly: RegisterCustomCloneSettingsControl(SKUInfo.OBJECT_TYPE_OPTIONSKU, typeof(SKUOptionCloneSettings))]

namespace CMS.Ecommerce.Web.UI
{
    internal class SKUOptionCloneSettings : CloneSettingsControl
    {
        public override bool DisplayControl
        {
            get { return false; }
        }


        public override string ExcludedOtherBindingTypes
        {
            get
            {
                return SKUInfo.OBJECT_TYPE_OPTIONSKU + ";" + VariantOptionInfo.OBJECT_TYPE + ";" + SKUAllowedOptionInfo.OBJECT_TYPE;
            }
        }
    }
}
