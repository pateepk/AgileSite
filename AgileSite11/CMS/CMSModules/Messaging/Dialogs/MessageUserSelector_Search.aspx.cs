using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.UIControls;


public partial class CMSModules_Messaging_Dialogs_MessageUserSelector_Search : CMSPage
{
    protected override void OnPreInit(EventArgs e)
    {
        // Validate hash and redirect to access denied page if not valid
        QueryHelper.ValidateHash(name: "hash", settings: new HashSettings { HashSalt = "Dialogs|MessageUserSelector|Search", Redirect = true });

        base.OnPreInit(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);
    }
}