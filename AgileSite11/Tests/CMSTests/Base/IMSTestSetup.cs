using System;
using System.Linq;
using System.Text;

namespace CMS.Tests
{
    /// <summary>
    /// Interface that define methods required for test initialization of MSTests.
    /// </summary>
    internal interface IMSTestSetup
    {
        /// <summary>
        /// Initializes the test class for all MSTests.
        /// </summary>
        /// <remarks>
        /// Calling this method in NUnit tests causes duplicate initialization.
        /// </remarks>

        void InitializeTestClass();


        /// <summary>
        /// Cleans up the class after all MSTests run.
        /// </summary>
        /// <remarks>
        /// Calling this method in NUnit tests causes duplicate initialization.
        /// </remarks>
        void CleanUpTestClass();
    }
}
