using System;

namespace CMS.Tests
{
    /// <summary>
    /// Indicates that database is shared for all isolated integration tests that are in same class.
    /// (ie. Database is created before first test run and it is deleted after last test run)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SharedDatabaseForAllTestsAttribute : Attribute
    {
    }
}
