using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(SiteDomainAliasInfo.OBJECT_TYPE, typeof(SiteDomainAliasCloneSettings))]

namespace CMS.UIControls
{
    internal class SiteDomainAliasCloneSettings : CloneSettingsControl
    {
        public override bool DisplayControl
        {
            get { return false; }
        }


        public override bool ValidateCodeName
        {
            get { return false; }
        }
    }
}
