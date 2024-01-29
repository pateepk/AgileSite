using System;
using System.Linq;
using System.Text;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// SQL generator base
    /// </summary>
    public abstract class AbstractSqlGenerator : ISqlGenerator
    {
        #region "Variables"

        /// <summary>
        /// Excluded columns of the document which are not included into the queries.
        /// </summary>
        protected static string mExcludeColumns = ";msrepl_tran_version;rowguid;";


        /// <summary>
        /// Gets or sets whether With (EXPAND) should be generated in views.
        /// </summary>
        private static bool? mGenerateWithExpand;


        private static readonly SafeDictionary<SqlOperationTypeEnum, string> mGeneralQueries = new SafeDictionary<SqlOperationTypeEnum, string>().With(d =>
        {
            d[SqlOperationTypeEnum.GeneralSelect] = SqlHelper.GENERAL_SELECT;
            d[SqlOperationTypeEnum.GeneralInsert] = SqlHelper.GENERAL_INSERT;
            d[SqlOperationTypeEnum.GeneralUpdate] = SqlHelper.GENERAL_UPDATE;
            d[SqlOperationTypeEnum.GeneralUpsert] = SqlHelper.GENERAL_UPSERT;
            d[SqlOperationTypeEnum.GeneralDelete] = SqlHelper.GENERAL_DELETE;
        });

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the excluded columns of the document which are not included into the queries.
        /// </summary>
        public static string ExcludeColumns
        {
            get
            {
                return mExcludeColumns;
            }
            set
            {
                mExcludeColumns = value;
            }
        }


        /// <summary>
        /// Gets or sets whether With (EXPAND) should be generated in views.
        /// </summary>
        public static bool GenerateWithExpand
        {
            get
            {
                if (mGenerateWithExpand == null)
                {
                    mGenerateWithExpand = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSGenerateWithExpand"], true);
                }

                return mGenerateWithExpand.Value;
            }
            set
            {
                mGenerateWithExpand = value;
            }
        }

        #endregion


        #region "View methods"

        /// <summary>
        /// Generates the Tree_Joined indexed view query
        /// </summary>
        /// <param name="indexes">Returns extra code for the initialization of the view</param>
        protected static string GetTreeJoinedView(out string indexes)
        {
            var indexed = TableManager.USE_INDEXED_VIEWS;
            var schemaPrefix = indexed ? SqlHelper.GetSafeOwner(SqlHelper.GetDBSchemaOrDefault()) + "." : null;
            var lockHint = indexed ? null : "WITH (NOLOCK) ";

            // Build the query
            var result = new StringBuilder();

            result.Append("SELECT ");
            result.Append("C.ClassName, C.ClassDisplayName, ");
            result.Append(GetColumnList("CMS.Tree", ExcludeColumns, true, "T"));
            result.Append(", ");
            result.Append(GetColumnList("CMS.Document", ExcludeColumns, true, "D"));
            result.Append("\n");
            result.Append("FROM ", schemaPrefix, "CMS_Tree T ", lockHint);
            result.Append("INNER JOIN ", schemaPrefix, "CMS_Document D ", lockHint, "ON T.NodeOriginalNodeID = D.DocumentNodeID ");
            result.Append("INNER JOIN ", schemaPrefix, "CMS_Class C ", lockHint, "ON T.NodeClassID = C.ClassID");

            if (indexed)
            {
                // Build the indexes scripts
                var extra = new StringBuilder();

                extra.Append(@"
CREATE UNIQUE CLUSTERED INDEX [IX_", SystemViewNames.View_CMS_Tree_Joined, "_NodeSiteID_DocumentCulture_NodeID] ON [", SystemViewNames.View_CMS_Tree_Joined, @"] 
(
    [NodeSiteID] ASC,
    [DocumentCulture] ASC,
    [NodeID] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
"
                    );

                extra.Append("\n");

                extra.Append(@"
CREATE NONCLUSTERED INDEX [IX_", SystemViewNames.View_CMS_Tree_Joined, "_ClassName_NodeSiteID_DocumentForeignKeyValue_DocumentCulture] ON [", SystemViewNames.View_CMS_Tree_Joined, @"] 
(
    [ClassName] ASC,
    [NodeSiteID] ASC,
    [DocumentForeignKeyValue] ASC,
    [DocumentCulture] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
"
                    );

                indexes = extra.ToString();
            }
            else
            {
                indexes = null;
            }

            return result.ToString();
        }


        /// <summary>
        /// Generates the code for the Community_Member view
        /// </summary>
        protected static string GetCommunityMemberView()
        {
            StringBuilder result = new StringBuilder();

            result.Append("SELECT ");
            result.Append(GetColumnList("CMS.User", ExcludeColumns, true));
            result.Append(", ");
            result.Append(GetColumnList("CMS.UserSettings", ExcludeColumns, true));
            result.Append(", ");
            result.Append(GetColumnList("Community.GroupMember", ExcludeColumns, true));
            result.Append(", CMS_UserSite.SiteID, CMS_Avatar.AvatarID,CMS_Avatar.AvatarGUID, CMS_Avatar.AvatarName ");
            result.Append("FROM CMS_User ");
            result.Append("LEFT JOIN CMS_UserSettings ON CMS_UserSettings.UserSettingsUserID = CMS_User.UserID ");
            result.Append("LEFT JOIN CMS_UserSite ON CMS_UserSite.UserID = CMS_User.UserID ");
            result.Append("LEFT JOIN Community_GroupMember ON Community_GroupMember.MemberUserID = CMS_User.UserID ");
            result.Append("LEFT JOIN CMS_Avatar ON CMS_Avatar.AvatarID = CMS_UserSettings.UserAvatarID ");

            return result.ToString();
        }


        /// <summary>
        /// Generates the code for the User view
        /// </summary>
        protected static string GetUserView()
        {
            StringBuilder result = new StringBuilder();

            result.Append("SELECT ");
            result.Append(GetColumnList("CMS.User", ExcludeColumns, true));
            result.Append(", ");
            result.Append(GetColumnList("CMS.UserSettings", ExcludeColumns, true));
            result.Append(", CMS_Avatar.AvatarID, CMS_Avatar.AvatarFileName, CMS_Avatar.AvatarGUID ");
            result.Append("FROM CMS_User ");
            result.Append("LEFT OUTER JOIN CMS_UserSettings ON CMS_User.UserID = CMS_UserSettings.UserSettingsUserID ");
            result.Append("LEFT OUTER JOIN CMS_Avatar ON CMS_UserSettings.UserAvatarID = CMS_Avatar.AvatarID ");

            return result.ToString();
        }


        /// <summary>
        /// Generates the code for the View_Community_Friend_Friends view.
        /// </summary>
        protected static string GetCommunityFriendFriendsView()
        {
            return GetCommunityFriendsBasedView("FriendUserID");
        }


        /// <summary>
        /// Generates the code for the View_Community_Friend_RequestedFriends view.
        /// </summary>
        protected static string GetCommunityFriendRequestedFriendsView()
        {
            return GetCommunityFriendsBasedView("FriendRequestedUserID");
        }


        /// <summary>
        /// Generates the code used in View_Community_Friend_* views.
        /// </summary>
        /// <param name="keyColumn">Column which to use as key for joining Community_Friend table with CMS_User table.</param>
        private static string GetCommunityFriendsBasedView(string keyColumn)
        {
            StringBuilder result = new StringBuilder();

            result.Append("SELECT ");
            result.Append(GetColumnList("Community.Friend", ExcludeColumns, true));
            result.Append(", ");
            result.Append(GetColumnList("CMS.User", ExcludeColumns, true));
            result.Append(", ");
            result.Append(GetColumnList("CMS.UserSettings", ExcludeColumns, true));
            result.Append(", CMS_Avatar.AvatarID, CMS_Avatar.AvatarFileName, CMS_Avatar.AvatarGUID ");
            result.AppendFormat("FROM Community_Friend INNER JOIN CMS_User ON Community_Friend.{0} = CMS_User.UserID ", keyColumn);
            result.Append("LEFT OUTER JOIN CMS_UserSettings ON CMS_User.UserID = CMS_UserSettings.UserSettingsUserID ");
            result.Append("LEFT OUTER JOIN CMS_Avatar ON CMS_UserSettings.UserAvatarID = CMS_Avatar.AvatarID ");

            return result.ToString();
        } 


        /// <summary>
        /// View name.
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="indexes">Returns extra code for the initialization of the view</param>
        public virtual string GetSystemViewSqlQuery(string viewName, out string indexes)
        {
            indexes = null;

            switch (viewName)
            {
                case SystemViewNames.View_CMS_Tree_Joined:
                    // Document view
                    return GetTreeJoinedView(out indexes);

                case SystemViewNames.View_CMS_User:
                    // Users view
                    return GetUserView();

                case SystemViewNames.View_Community_Member:
                    // Community members view
                    return GetCommunityMemberView();

                case SystemViewNames.View_Community_Friend_Friends:
                    // Community friends view
                    return GetCommunityFriendFriendsView();

                case SystemViewNames.View_Community_Friend_RequestedFriends:
                    // Community requested friends view
                    return GetCommunityFriendRequestedFriendsView();
            }

            return null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Generates the given type of query for table specified by its className.
        /// </summary>
        /// <param name="className">Class name of the document data</param>
        /// <param name="queryType">Query type</param>
        /// <param name="siteName">CodeName of the site</param>
        public virtual string GetSqlQuery(string className, SqlOperationTypeEnum queryType, string siteName)
        {
            // Get general query regardless of class name
            var generalQuery = GetGeneralQuery(queryType);
            if (generalQuery != null)
            {
                return generalQuery;
            }

            return GenerateQuery(className, queryType);
        }


        /// <summary>
        /// Gets a general query of the given type
        /// </summary>
        /// <param name="queryType">Query type</param>
        private static string GetGeneralQuery(SqlOperationTypeEnum queryType)
        {
            var generalQuery = mGeneralQueries[queryType];

            return generalQuery;
        }


        /// <summary>
        /// Generates the given type of query for table specified by its className.
        /// </summary>
        /// <param name="className">Class name of the document data</param>
        /// <param name="queryType">Query type</param>
        private string GenerateQuery(string className, SqlOperationTypeEnum queryType)
        {
            // Get the class information
            var csi = ClassStructureInfo.GetClassInfo(className);
            var dci = DataClassInfoProvider.GetDataClassInfo(className);

            if ((csi == null) || (dci == null))
            {
                return "";
            }

            var tableName = dci.ClassTableName;
            var sb = new StringBuilder();

            switch (queryType)
            {
                // SELECT query
                case SqlOperationTypeEnum.SelectQuery:
                    {
                        // Get primary key column name
                        string pkName = csi.IDColumn;

                        sb.Append("SELECT ##COLUMNS## FROM ", tableName, " WHERE ", GetPKWhereCondition(pkName));
                    }
                    break;

                // SELECT ALL query
                case SqlOperationTypeEnum.SelectAll:
                    sb.Append(SqlHelper.GetSelectQuery(tableName));
                    break;

                // INSERT query
                case SqlOperationTypeEnum.InsertQuery:
                case SqlOperationTypeEnum.InsertWithIdentity:
                    {
                        // Get primary key column name
                        string pkName = csi.IDColumn;
                        bool multiplePK = (pkName != null) && (pkName.Contains(";"));

                        // Allow insert the identity column
                        bool insertIdentity = (queryType == SqlOperationTypeEnum.InsertWithIdentity);
                        if (insertIdentity)
                        {
                            sb.Append("SET IDENTITY_INSERT ", tableName, " ON; ");
                        }

                        // Insert query
                        sb.Append("INSERT INTO ", tableName, " (");

                        // List the columns
                        bool colFound = false;
                        foreach (string column in csi.ColumnNames)
                        {
                            if (!IsColumnExcluded(ExcludeColumns, column) && (insertIdentity || multiplePK || !column.EqualsCSafe(pkName, true)))
                            {
                                sb.Append("[", column, "], ");
                                colFound = true;
                            }
                        }

                        // Check if some column was generated
                        if (!colFound)
                        {
                            throw new Exception("Unable to generate insert query for class '" + csi.ClassName + "', the data table must contain at least one data column to be able to insert new records.");
                        }

                        sb.Length = sb.Length - 2;
                        sb.Append(" ) VALUES ( ");

                        // List the values
                        foreach (string column in csi.ColumnNames)
                        {
                            if (!IsColumnExcluded(ExcludeColumns, column) && (insertIdentity || multiplePK || !column.EqualsCSafe(pkName, true)))
                            {
                                sb.Append("@", column, ", ");
                            }
                        }
                        sb.Length = sb.Length - 2;
                        sb.Append(");");

                        if (insertIdentity)
                        {
                            // Disable the identity insert
                            sb.Append(" SET IDENTITY_INSERT ", tableName, " OFF;");
                        }
                        else if (!multiplePK)
                        {
                            // Select the identity of the new record
                            sb.Append(" SELECT SCOPE_IDENTITY() AS [", pkName, "] ");
                        }
                    }
                    break;

                // UPDATE query
                case SqlOperationTypeEnum.UpdateQuery:
                    {
                        // Get primary key column name
                        string pkName = csi.IDColumn;
                        bool multiplePK = (pkName != null) && (pkName.Contains(";"));

                        string pkNames = ";" + pkName.ToLowerInvariant() + ";";
                        sb.Append("UPDATE ", tableName, " SET ");

                        // List the columns
                        bool update = false;
                        foreach (string column in csi.ColumnNames)
                        {
                            if (!IsColumnExcluded(ExcludeColumns, column) && !IsColumnExcluded(pkNames, column))
                            {
                                update = true;
                                sb.Append("[", column, "] = @", column, ", ");
                            }
                        }

                        if (update)
                        {
                            // Update only the items with matching primary key(s)
                            sb.Length = sb.Length - 2;
                            sb.Append(" WHERE ");
                            if (multiplePK)
                            {
                                sb.Append(GetPKWhereCondition(pkName));
                            }
                            else
                            {
                                sb.Append("[", pkName, "] = @", pkName);
                            }
                        }
                        else
                        {
                            sb.Length = 0;
                            sb.Append("SELECT NULL");
                        }
                    }
                    break;

                // DELETE query
                case SqlOperationTypeEnum.DeleteQuery:
                    {
                        // Get primary key column name
                        string pkName = csi.IDColumn;

                        sb.Append("DELETE FROM ", tableName, " WHERE ", GetPKWhereCondition(pkName));
                    }
                    break;

                // DELETE ALL query
                case SqlOperationTypeEnum.DeleteAll:
                    {
                        sb.Append("DELETE FROM ", tableName, " WHERE ##WHERE##");
                    }
                    break;

                // UPDATE ALL query
                case SqlOperationTypeEnum.UpdateAll:
                    {
                        sb.Append("UPDATE ", tableName, " SET ##VALUES## WHERE ##WHERE##");
                    }
                    break;
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns the where condition for given primary key (single or list separated by semicolon).
        /// </summary>
        /// <param name="pkName">Primary key name (list)</param>
        public virtual string GetPKWhereCondition(string pkName)
        {
            StringBuilder sb = new StringBuilder();

            bool multiplePK = (pkName != null) && (pkName.Contains(";"));
            if (multiplePK)
            {
                string[] pkNames = pkName.Split(';');
                foreach (string pk in pkNames)
                {
                    if (pk != "")
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(" AND ");
                        }
                        sb.Append("[", pk, "] = @", pk);
                    }
                }
            }
            else
            {
                sb.Append("[", pkName, "] = @ID");
            }

            return sb.ToString();
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Returns true if the column is excluded.
        /// </summary>
        /// <param name="excludedColumns">List of excluded columns (starting and ending with semicolon ";")</param>
        /// <param name="column">Column to check</param>
        public static bool IsColumnExcluded(string excludedColumns, string column)
        {
            return excludedColumns.IndexOf(";" + column + ";", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }


        /// <summary>
        /// Generates the column list for the SQL query.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="excludedColumns">List of the excluded columns</param>
        /// <param name="fullName">Generate full name, including the table name?</param>
        /// <param name="tableName">Table name</param>
        public static string GetColumnList(string className, string excludedColumns, bool fullName, string tableName = null)
        {
            // Get the class
            var dci = DataClassInfoProvider.GetDataClassInfo(className);
            if (dci == null)
            {
                return "";
            }

            var classTableName = dci.ClassTableName;
            var tm = new TableManager(dci.ClassConnectionString);

            // Get XMLSchema directly from database because data in ClassStructureInfo may be not updated yet
            var xmlSchema = tm.GetXmlSchema(classTableName);
            var csi = new ClassStructureInfo(className, xmlSchema, classTableName);

            if (fullName && (tableName == null))
            {
                tableName = classTableName;
            }

            excludedColumns = excludedColumns.ToLowerInvariant();

            // Build the list
            var columns = csi.ColumnNames.Where(column => !IsColumnExcluded(excludedColumns, column)).Select(column =>
             {
                 // Border columns with [ ]
                 if (!column.StartsWith("[", StringComparison.Ordinal))
                 {
                     column = "[" + column + "]";
                 }

                 if (fullName)
                 {
                     return tableName + "." + column;
                 }

                 return column;
             });

            return String.Join(", ", columns);
        }

        #endregion
    }
}