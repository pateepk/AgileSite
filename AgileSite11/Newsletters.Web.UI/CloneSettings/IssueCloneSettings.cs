using CMS.DataEngine;
using CMS.Newsletters;
using CMS.Newsletters.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(IssueInfo.OBJECT_TYPE, typeof(IssueCloneSettings))]

namespace CMS.Newsletters.Web.UI
{
    internal class IssueCloneSettings : CloneSettingsControl
    {
        public override string ExcludedChildTypes
        {
            get { return string.Join(";", LinkInfo.OBJECT_TYPE, OpenedEmailInfo.OBJECT_TYPE); }
        }


        public override bool DisplayControl
        {
            get { return false; }
        }


        public override bool IsValid(CloneSettings settings)
        {
            if (InfoToClone.GetBooleanValue("IssueIsABTest", false))
            {
                if (!settings.IncludeChildren || (settings.MaxRelativeLevel == 0))
                {
                    // It is not possible to clone ABTest issue without its children, 
                    // because children are variants of the AB Tests and it makes no sense to clone without variants
                    ShowError(GetString("newsletters.cannotcloneabtestissuewithoutchildren"));

                    return false;
                }
            }

            return true;
        }
    }
}
