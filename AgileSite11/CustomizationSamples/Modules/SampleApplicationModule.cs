using CMS;
using CMS.Base;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(SampleApplicationModule))]

/// <summary>
/// Sample module with custom action within application pre-initialization phase
/// </summary>
internal class SampleApplicationModule : Module
{
    public SampleApplicationModule()
        : base("SampleApplicationModule")
    {
    }


    protected override void OnPreInit()
    {
        base.OnPreInit();

        // Initialize the connection string programmatically
        SettingsHelper.ConnectionStrings.SetConnectionString("CMSConnectionString", "<enter connection string>");
    }
}