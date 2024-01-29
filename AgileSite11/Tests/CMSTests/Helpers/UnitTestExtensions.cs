using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Extension methods for tests
    /// </summary>
    public static class UnitTestExtensions
    {
        /// <summary>
        /// Gets the unique SQL query mark for a unit test
        /// </summary>
        private static string GetTestQueryMark()
        {
            return String.Format("/* TEST - {0} */", Guid.NewGuid());
        }


        /// <summary>
        /// Fakes the execution of a query
        /// </summary>
        /// <param name="query">Query to fake</param>
        /// <param name="execution">Execution code</param>
        /// <param name="cancel">True when query execution should be canceled</param>
        public static BeforeConditionalEvent<ExecuteQueryEventArgs<DataSet>> FakeExecution(this IDataQuery query, Action<ExecuteQueryEventArgs<DataSet>> execution, bool cancel = true)
        {
            if (query.Parameters == null)
            {
                query.Parameters = new QueryDataParameters();
            }
            var mark = Environment.NewLine + GetTestQueryMark();

            // Insert mark
            query.Parameters.QueryAfter += mark;

            var result = SqlEvents.ExecuteQuery
                .AddBefore()
                .When(args => args.Query.Text.Contains(mark))
                .Call(args =>
                {
                    // Remove mark
                    args.Query.Text = args.Query.Text.Substring(0, args.Query.Text.IndexOf(mark));
                    execution(args);
                });

            if (cancel)
            {
                result.ThenCancel();
            }

            return result;
        }
    }
}
