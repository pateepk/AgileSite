using CMS;
using CMS.EventLog;
using CMS.SiteProvider;

[assembly: RegisterCustomProvider(typeof(CustomSiteInfoProvider))]

/// <summary>
/// Sample custom user info provider, does log an event upon the user update.
/// </summary>
public class CustomSiteInfoProvider : SiteInfoProvider
{
    /// <summary>
    /// Sets the specified site data.
    /// </summary>
    /// <param name="siteInfoObj">New site info data</param>
    protected override void SetInfo(SiteInfo siteInfoObj)
    {
        base.SetInfo(siteInfoObj);

        // Log the event that the site was updated
        EventLogProvider.LogEvent(EventType.INFORMATION, "MyCustomSiteInfoProvider", "SetSiteInfo", "The site was updated", null);
    }
}