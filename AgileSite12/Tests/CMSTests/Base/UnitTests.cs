using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;

using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Base class for unit tests.
    /// </summary>
    [Category.Unit]
    public class UnitTests : AutomatedTestsWithData
    {
        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected UnitTests()
        {
        }


        /// <summary>
        /// Initializes the test
        /// </summary>
        [SetUp]
        public void UnitTestsSetUp()
        {
            // Fake derives from simple data class, but doesn't touch the database.
            DataClassFactory.ChangeDefaultDataClassTypeTo<FakeSimpleDataClass>();

            QueryInfoProvider.UseAutomaticQueriesAsPrimary = true;

            AppCore.PreInit();

            Fake<QueryInfo, QueryInfoProvider>().WithData();
            Fake<EventLogInfo>();
        }


        /// <summary>
        /// Resets all fakes
        /// </summary>
        public override void ResetAllFakes()
        {
            QueryInfoProvider.UseAutomaticQueriesAsPrimary = false;

            base.ResetAllFakes();
        }
        
        #endregion
    }
}
