using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides source for the document query helper.
    /// </summary>
    internal class DocumentQuerySourceHelper : AbstractHelper<DocumentQuerySourceHelper>
    {
        #region "Cache methods"

        /// <summary>
        /// Gets the base data source table
        /// </summary>
        /// <param name="settings">Settings</param>
        public static QuerySourceTable GetBaseSourceTable(DocumentQuerySourceSettings settings)
        {
            return HelperObject.GetBaseSourceTableInternal(settings);
        }


        /// <summary>
        /// Gets base source for the query.
        /// </summary>
        /// <param name="settings">Settings</param>
        public static QuerySource GetBaseSource(DocumentQuerySourceSettings settings)
        {
            return HelperObject.GetBaseSourceInternal(settings);
        }


        /// <summary>
        /// Joins coupled data to the query source
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="document">Empty document instance for query type reference</param>
        /// <param name="settings">Settings</param>
        public static QuerySource JoinCoupledData(QuerySource source, TreeNode document, DocumentQuerySourceSettings settings)
        {
            if (!document.IsCoupled)
            {
                return source;
            }

            return HelperObject.JoinCoupledDataInternal(source, document, settings);
        }


        /// <summary>
        /// Joins SKU data to the query source
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="settings">Settings</param>
        public static QuerySource JoinSKUData(QuerySource source, DocumentQuerySourceSettings settings)
        {
            // No need to include SKU data if module not loaded
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
            {
                return source;
            }

            return HelperObject.JoinSKUDataInternal(source, settings);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets the base data source table
        /// </summary>
        /// <param name="settings">Settings</param>
        protected virtual QuerySourceTable GetBaseSourceTableInternal(DocumentQuerySourceSettings settings)
        {
            var hints = GetBaseDataSourceHints(settings);

            var table = new QuerySourceTable(SystemViewNames.View_CMS_Tree_Joined, DocumentQueryAliases.BASE_VIEW, hints.ToArray());

            return table;
        }


        /// <summary>
        /// Gets base source for the query.
        /// </summary>
        protected virtual QuerySource GetBaseSourceInternal(DocumentQuerySourceSettings settings)
        {
            var table = GetBaseSourceTableInternal(settings);

            return new QuerySource(table);
        }


        /// <summary>
        /// Joins coupled data to the query source
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="document">Empty document instance for query type reference</param>
        /// <param name="settings">Settings</param>
        protected virtual QuerySource JoinCoupledDataInternal(QuerySource source, TreeNode document, DocumentQuerySourceSettings settings)
        {
            var hints = GetCoupledDataSourceHints(settings);

            return JoinCoupledDataSource(source, document, hints);
        }


        /// <summary>
        /// Joins SKU data to the query source
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="settings">Settings</param>
        protected virtual QuerySource JoinSKUDataInternal(QuerySource source, DocumentQuerySourceSettings settings)
        {
            var hints = GetSKUDataSourceHints(settings);

            return JoinSKUDataSource(source, hints);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets query hints for base source.
        /// </summary>
        protected static List<string> GetBaseDataSourceHints(DocumentQuerySourceSettings settings)
        {
            // Use NOLOCK hint to reduce number of deadlocks.
            var hints = new List<string>();

            if (settings.UseNoLockHint())
            {
                hints.Add(SqlHints.NOLOCK);
            }

            if (settings.UseNoExpandHint())
            {
                hints.Add(SqlHints.NOEXPAND);
            }

            return hints;
        }


        /// <summary>
        /// Gets query hints for SKU source.
        /// </summary>
        /// <param name="settings">Settings</param>
        protected static List<string> GetSKUDataSourceHints(DocumentQuerySourceSettings settings)
        {
            return settings.UseNoLockHint() ? new List<string> { SqlHints.NOLOCK } : null;
        }


        /// <summary>
        /// Gets query hints for coupled data source.
        /// </summary>
        /// <param name="settings">Settings</param>
        protected static List<string> GetCoupledDataSourceHints(DocumentQuerySourceSettings settings)
        {
            return settings.UseNoLockHint() ? new List<string> { SqlHints.NOLOCK } : null;
        }
        

        /// <summary>
        /// Gets source table for SKU data
        /// </summary>
        /// <param name="hints">List of hint to be used for the SKU source</param>
        private static QuerySourceTable GetSKUSourceTable(IEnumerable<string> hints)
        {
            // Get the SKU class
            var classInfo = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.SKU);
            if (classInfo == null)
            {
                return null;
            }

            return new QuerySourceTable(classInfo.ClassTableName, DocumentQueryAliases.SKU_VIEW, hints.ToArray());
        }


        /// <summary>
        /// Gets source table for coupled data
        /// </summary>
        /// <param name="hints">List of hint to be used for the coupled data source</param>
        /// <param name="document">Empty document instance for query type reference</param>
        private static QuerySourceTable GetCoupledDataSourceTable(IEnumerable<string> hints, TreeNode document)
        {
            var tableName = document.DataClassInfo.ClassTableName;
            return new QuerySourceTable(tableName, DocumentQueryAliases.COUPLED_VIEW, hints.ToArray());
        }


        /// <summary>
        /// Joins SKU data to the query data source.
        /// </summary>
        /// <param name="source">Original data source</param>
        /// <param name="hints">Query hints to be applied to the data join</param>
        protected static QuerySource JoinSKUDataSource(QuerySource source, IEnumerable<string> hints)
        {
            var table = GetSKUSourceTable(hints);
            if (table == null)
            {
                return source;
            }

            return source.LeftJoin(table, DocumentQueryAliases.BASE_VIEW + ".NodeSKUID", "SKUID");
        }


        /// <summary>
        /// Joins coupled data to the query data source.
        /// </summary>
        /// <param name="source">Original data source</param>
        /// <param name="document">Empty document instance for query type reference</param>
        /// <param name="hints">Query hints to be applied to the data join</param>
        protected static QuerySource JoinCoupledDataSource(QuerySource source, TreeNode document, IEnumerable<string> hints)
        {
            var table = GetCoupledDataSourceTable(hints, document);
            if (table == null)
            {
                return source;
            }

            var className = document.DataClassInfo.ClassName;
            var classWhere = GetClassWhereCondition(className);
            var idColumn = document.CoupledData.TypeInfo.IDColumn;

            return source.Join(table, DocumentQueryAliases.BASE_VIEW + ".DocumentForeignKeyValue", idColumn, classWhere);
        }


        /// <summary>
        /// Returns where condition for a specific class
        /// </summary>
        /// <param name="className">Class name</param>
        internal static WhereCondition GetClassWhereCondition(string className)
        {
            return new WhereCondition(DocumentQueryAliases.BASE_VIEW + ".ClassName", QueryOperator.Equals, className.AsLiteral());
        }

        #endregion
    }
}
