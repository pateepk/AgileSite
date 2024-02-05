using System.Data;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Extension methods for <see cref="ExecuteQueryEventArgs{DataSet}"/> class.
    /// </summary>
    internal static class ExecuteQueryEventArgsExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when given <paramref name="args"/> has equal query text as given <paramref name="expectedText"/> and equal parameters as given <paramref name="expectedParameters"/>.
        /// Returns <c>false</c> otherwise.
        /// </summary>
        public static bool IsQueryMatch(this ExecuteQueryEventArgs<DataSet> args, string expectedText, QueryDataParameters expectedParameters)
        {
            return IsQueryTextMatch(expectedText, args.Query.Text) && IsQueryParametersMatch(expectedParameters, args.Query.Params);
        }


        /// <summary>
        /// Returns <c>true</c> when given <paramref name="args"/> has equal query text as given <paramref name="expectedText"/>.
        /// Returns <c>false</c> otherwise.
        /// </summary>
        public static bool IsQueryTextMatch(this ExecuteQueryEventArgs<DataSet> args, string expectedText)
        {
            return IsQueryTextMatch(expectedText, args.Query.Text);
        }


        private static bool IsQueryParametersMatch(QueryDataParameters expectedParameters, QueryDataParameters actualParameters)
        {
            var expectedNames = expectedParameters.Select(i => i.Name);
            var actualNames = actualParameters.Select(i => i.Name);

            var expectedValues = expectedParameters.Select(i => i.Value);
            var actualValues = actualParameters.Select(i => i.Value);

            return expectedNames.SequenceEqual(actualNames) && expectedValues.SequenceEqual(actualValues);
        }


        private static bool IsQueryTextMatch(string expected, string actual)
        {
            return SqlHelper.QueriesEqual(ref expected, ref actual);
        }
    }
}
