using CMS.DataEngine;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Optimized <see cref="WhereCondition"/> provider for documents which uses CTE instead of dynamically generated queries.
    /// </summary>
    internal sealed class DocumentObjectDependenciesConditionProvider : ObjectDependenciesConditionProvider
    {
        public override WhereCondition GetWhereCondition(BaseInfo info, IWhereCondition objectWhereCondition, string pathColumnName)
        {
            var documentNode = ModuleManager.GetReadOnlyObject(DocumentNodeDataInfo.OBJECT_TYPE);

            var pathQuery = new DocumentQuery().Where(objectWhereCondition).Columns(new QueryColumn(pathColumnName).As("FilteredNodeAliasPath"), new QueryColumn(documentNode.TypeInfo.SiteIDColumn));
            pathQuery.Properties.EnsureExtraColumns = false;

            var pathsToMergeQuery = documentNode.Generalized.GetDataQuery(false, 
                settings => settings.Columns(new QueryColumn(pathColumnName).As("AllNodeAliasPath"), new QueryColumn(documentNode.TypeInfo.SiteIDColumn)), false);

            var selectQuery = new DataQuery().From(new QuerySource(
                            new QuerySourceTable("FilteredPaths", "F"))
                        .InnerJoin(
                            new QuerySourceTable("AllPaths", "A"), "A.NodeSiteID = F.NodeSiteID AND A.AllNodeAliasPath LIKE F.FilteredNodeAliasPath + '/%'"));
            selectQuery.Column("AllNodeAliasPath");

            var query = new DataQuery
            {
                ReturnsSingleColumn = true,
                CustomQueryText = $@"
WITH FilteredPaths (FilteredNodeAliasPath, {documentNode.TypeInfo.SiteIDColumn})
AS
(
    {pathQuery.ToString(true)}
),
AllPaths (AllNodeAliasPath, {documentNode.TypeInfo.SiteIDColumn})
AS
(
    {pathsToMergeQuery.ToString(true)}
)

{selectQuery.ToString(true)}
"
            };

            return new WhereCondition().WhereIn(pathColumnName, query.GetListResult<string>());
        }
    }
}
