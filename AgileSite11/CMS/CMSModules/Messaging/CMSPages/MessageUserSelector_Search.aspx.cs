using System;
using CMS.Helpers;
using CMS.UIControls;


public partial class CMSModules_Messaging_CMSPages_MessageUserSelector_Search : LivePage
{
    protected override void OnPreInit(EventArgs e)
    {
        // Validate hash and redirect to access denied page if not valid
        QueryHelper.ValidateHash(name: "hash", settings: new HashSettings { HashSalt = "CMSPages|MessageUserSelector|Search", Redirect = true });

        base.OnPreInit(e);
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        // Add styles
        RegisterDialogCSSLink();
    }
}