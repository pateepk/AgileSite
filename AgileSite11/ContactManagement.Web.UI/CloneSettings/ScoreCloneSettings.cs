using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(ScoreInfo.OBJECT_TYPE, typeof(ScoreCloneSettings))]

namespace CMS.ContactManagement.Web.UI
{
    internal class ScoreCloneSettings : CloneSettingsControl
    {
        public override string ExcludedOtherBindingTypes
        {
            get { return ScoreContactRuleInfo.OBJECT_TYPE; }
        }


        public override bool DisplayControl
        {
            get { return false; }
        }
    }
}
