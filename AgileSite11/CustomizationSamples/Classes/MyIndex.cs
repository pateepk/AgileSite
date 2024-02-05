using CMS;
using CMS.EventLog;
using CMS.Search;

[assembly: RegisterCustomClass("Custom.MyIndex", typeof(MyIndex))]

/// <summary>
/// Sample search index class.
/// </summary>
public class MyIndex : ICustomSearchIndex
{
    /// <summary>
    /// Implementation of rebuild method.
    /// </summary>
    /// <param name="srchInfo">Search index info</param>
    public void Rebuild(SearchIndexInfo srchInfo)
    {
        EventLogProvider.LogEvent(EventType.INFORMATION, "MyCustomIndex", "Rebuild", "The index is building", null);
    }
}
