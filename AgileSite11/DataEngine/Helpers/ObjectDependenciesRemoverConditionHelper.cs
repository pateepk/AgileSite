using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Helps to create WHERE conditions.
    /// </summary>
    internal class ObjectDependenciesRemoverConditionHelper : AbstractHelper<ObjectDependenciesRemoverConditionHelper>
    {
        /// <summary>
        /// <para>
        /// Creates where condition for all the children of objects targeted by <paramref name="rootsSelector"/>. 
        /// The <paramref name="pathColumnName"/> parameter denotes tree structure column.
        /// The root objects selected by <paramref name="rootsSelector"/> are not returned.
        /// </para>
        /// <para>
        /// Creates where condition with semantics as WHERE column IN LIKE (@var + '/%', @var1 + '/%', ...).
        /// Enables to filter all children of <paramref name="info"/>s within <paramref name="rootsSelector"/> condition by <paramref name="pathColumnName"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Method allows to rewrite WHERE conditions looking like this: WHERE Path LIKE @SubPath1 + '/%' OR Path LIKE @SubPath2 + '/%' OR ... Path LIKE @SubPath2100;
        /// by using INNER JOIN. SubPath parameters are created by SELECT <paramref name="pathColumnName"/> FROM <paramref name="info"/> WHERE <paramref name="rootsSelector"/>.
        /// Path is equal to <paramref name="pathColumnName"/>.
        /// </remarks>
        /// <param name="info">Readonly info object.</param>
        /// <param name="rootsSelector">Selects only chosen <paramref name="info"/> objects.</param>
        /// <param name="pathColumnName">Values from which column of an <paramref name="info"/> are needed.</param>
        /// <example>
        /// Generated condition looks almost like this:
        /// 
        /// WHERE <paramref name="pathColumnName"/> IN (
        ///     (SELECT <paramref name="pathColumnName"/> FROM <paramref name="info"/> WHERE <paramref name="rootsSelector"/>) AS [A]
        ///     INNER JOIN
        ///     (SELECT <paramref name="pathColumnName"/> FROM <paramref name="info"/> WHERE <paramref name="rootsSelector"/>) AS [B]
        ///     ON [A].<paramref name="pathColumnName"/> LIKE [B].<paramref name="pathColumnName"/> + '/%'.
        ///     
        /// A
        /// - B
        /// -- C
        /// 
        /// D
        /// - E
        /// -- F
        /// 
        /// If the <paramref name="rootsSelector"/> selects A and E then B, C, F is returned.
        /// </example>
        internal static WhereCondition GetMultipleSubTrees(BaseInfo info, IWhereCondition rootsSelector, string pathColumnName)
        {
            return HelperObject.GetMultipleSubTreesInternal(info, rootsSelector, pathColumnName);
        }


        protected virtual WhereCondition GetMultipleSubTreesInternal(BaseInfo info, IWhereCondition where, string columnName)
        {
            var pathQuery = info.GetDataQuery(false, settings => settings.Where(where).Columns(columnName), false);
            var pathsToMergeQuery = info.GetDataQuery(false, settings => settings.Column(columnName), false);

            var allSubPathsQuery = new DataQuery().From(
                        new QuerySource(new QuerySourceTable($"({pathQuery.ToString(true)})", "Paths"))
                        .InnerJoin(new QuerySourceTable($"({pathsToMergeQuery.AsSubQuery().ToString(true)})", "SubPaths"), $"[SubPaths].[{columnName}] LIKE [Paths].[{columnName}] + '/%'")
                    ).Column($"[SubPaths].[{columnName}]");

            return new WhereCondition().WhereIn(columnName, allSubPathsQuery);
        }
    }
}
