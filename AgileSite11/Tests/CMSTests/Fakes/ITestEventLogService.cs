using CMS.Core;

namespace CMS.Tests
{
    interface ITestEventLogService : IEventLogService
    {
        FakeEventLogProvider TestsEventLogProvider
        {
            get;
            set;
        }
    }
}
