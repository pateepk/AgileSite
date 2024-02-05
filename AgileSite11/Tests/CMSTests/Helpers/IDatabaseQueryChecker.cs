using System.Data;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Common interface for checking database calls.
    /// </summary>
    public interface IDatabaseQueryChecker
    {
        /// <summary>
        /// Allows to check database query call.
        /// </summary>
        void Check(object sender, ExecuteQueryEventArgs<DataSet> args);
    }
}