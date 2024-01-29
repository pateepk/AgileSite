using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(RuleInfo.OBJECT_TYPE, typeof(RuleCloneSettings))]

namespace CMS.ContactManagement.Web.UI
{
    internal class RuleCloneSettings : CloneSettingsControl
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
