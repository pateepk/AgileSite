using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(OptionCategoryInfo.OBJECT_TYPE, typeof(OptionCategoryCloneSettings))]

namespace CMS.Ecommerce.Web.UI
{
    internal class OptionCategoryCloneSettings : CloneSettingsControl
    {
        public override bool DisplayControl
        {
            get { return false; }
        }


        public override string ExcludedOtherBindingTypes
        {
            get
            {
                return SKUOptionCategoryInfo.OBJECT_TYPE + ";" + VariantOptionInfo.OBJECT_TYPE + ";" + SKUAllowedOptionInfo.OBJECT_TYPE;
            }
        }
    }
}
