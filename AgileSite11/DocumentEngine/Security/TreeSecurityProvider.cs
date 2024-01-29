using System;
using System.Collections;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Tree provider with an additional security functionality to filter the selection results by the user permissions.
    /// </summary>
    public static class TreeSecurityProvider
    {
        #region "Constants"

        /// <summary>
        /// Storage key used for grouping user's authorization per node.
        /// </summary>
        private const string IS_AUTHORIZED_PER_NODE_STORAGE_KEY = "IsAuthorizedPerNode";

        #endregion

        #region "Public methods"

        /// <summary>
        /// Returns true if user is granted with specified permission for particular node.
        /// </summary>
        /// <param name="nodeData">Node data row</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="userInfo">UserInfo instance</param> 
        /// <param name="culture">Document culture code</param>
        public static AuthorizationResultEnum IsAuthorizedPerNodeData(object nodeData, NodePermissionsEnum permission, UserInfo userInfo, string culture)
        {
            // If no document given, allowed automatically
            if (DataHelper.DataSourceIsEmpty(nodeData))
            {
                return AuthorizationResultEnum.Allowed;
            }

            IDataContainer data;
            if (nodeData is DataRow)
            {
                // Data row
                data = new DataRowContainer((DataRow)nodeData);
            }
            else if (nodeData is IDataContainer)
            {
                // Node row
                data = (IDataContainer)nodeData;
            }
            else
            {
                throw new Exception("[TreeSecurityProvider.IsAuthorizedPerNodeData]: Data type '" + nodeData.GetType().FullName + "' is not supported.");
            }

            // Check user info
            if (userInfo == null)
            {
                throw new Exception("[TreeSecurityProvider.IsAuthorizedPerNodeData]: No user given.");
            }

            // Get from the request storage for duplicate results
            string resultKey = GetAuthorizationResultKey(userInfo, ValidationHelper.GetInteger(data.GetValue("NodeID"), 0), permission, culture);
            if (RequestStockHelper.Contains(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false))
            {
                return (AuthorizationResultEnum)RequestStockHelper.GetItem(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false);
            }

            AuthorizationResultEnum result;
            TreeProvider tree = null;

            // Global administrator is allowed automatically
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                result = AuthorizationResultEnum.Allowed;
            }
            else
            {
                tree = new TreeProvider(userInfo);
                QueryDataParameters parameters;
                string permissionName = GetPermissionName(permission);

                // Set global result
                AuthorizationResultEnum globalResult = (permission == NodePermissionsEnum.Create) ? AuthorizationResultEnum.Insignificant : AuthorizationResultEnum.Denied;

                // Check site
                SiteInfo si = SiteInfoProvider.GetSiteInfo(ValidationHelper.GetInteger(data.GetValue("NodeSiteID"), 0));
                if (si == null)
                {
                    throw new Exception("[TreeSecurityProvider.IsAuthorizedPerNodeData]: Node site not found.");
                }

                // Check user's allowed cultures
                switch (permission)
                {
                    case NodePermissionsEnum.Modify:
                    case NodePermissionsEnum.Create:
                    case NodePermissionsEnum.Delete:
                    case NodePermissionsEnum.Destroy:
                    case NodePermissionsEnum.ModifyPermissions:
                        // Document culture is defined
                        if (!string.IsNullOrEmpty(culture) && userInfo.UserHasAllowedCultures && !userInfo.IsCultureAllowed(culture, si.SiteName))
                        {
                            return AuthorizationResultEnum.Denied;
                        }
                        break;
                }

                // Get global permissions result (cms.content permissions)
                if (UserInfoProvider.IsAuthorizedPerResource("cms.content", permissionName, si.SiteName, userInfo))
                {
                    globalResult = AuthorizationResultEnum.Allowed;
                }

                // If there is no permission for the 'cms.content', check the document-type permissions 
                if ((globalResult != AuthorizationResultEnum.Allowed) && (permission != NodePermissionsEnum.Create))
                {
                    parameters = new QueryDataParameters();

                    // Get global permissions result (document-type permissions)
                    parameters.Add("@UserID", userInfo.UserID);
                    parameters.Add("@NodeID", data.GetValue("NodeID"));
                    parameters.Add("@PermissionName", permissionName);

                    // Get user generic roles
                    string genRoles = "'" + RoleName.EVERYONE + "'";
                    genRoles += ", '" + (userInfo.IsPublic() ? RoleName.NOTAUTHENTICATED : RoleName.AUTHENTICATED) + "'";

                    parameters.Add("@GenericRoles", genRoles);

                    parameters.Add("@SiteID", si.SiteID);
                    parameters.Add("@Date", DateTime.Now);

                    DataSet dsResult = ConnectionHelper.ExecuteQuery("cms.user.isauthorizedpernode", parameters);
                    if (!DataHelper.DataSourceIsEmpty(dsResult) && (dsResult.Tables[0].Rows.Count > 0))
                    {
                        globalResult = AuthorizationResultEnum.Allowed;
                    }
                    else
                    {
                        globalResult = AuthorizationResultEnum.Denied;
                    }
                }


                // Check if document level permissions are available
                string domain = RequestContext.CurrentDomain;
                if (!string.IsNullOrEmpty(domain) && !LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.DocumentLevelPermissions))
                {
                    result = globalResult;
                }
                else
                {
                    // Get local permissions result
                    int aclId = ValidationHelper.GetInteger(data.GetValue("NodeACLID"), 0);
                    DataSet dsAclItems = AclItemInfoProvider.GetAclItems(aclId, si.SiteID, userInfo);

                    if (!DataHelper.DataSourceIsEmpty(dsAclItems))
                    {
                        int allowed = 0;
                        int denied = 0;

                        // Combine the permissions
                        foreach (DataRow drAclItem in dsAclItems.Tables[0].Rows)
                        {
                            allowed = allowed | Convert.ToInt32(drAclItem["Allowed"]);
                            denied = denied | Convert.ToInt32(drAclItem["Denied"]);
                        }

                        // ACL is DENIED
                        if ((denied >> Convert.ToInt32(permission)) % 2 == 1)
                        {
                            result = AuthorizationResultEnum.Denied;
                        }
                        // ACL is ALLOWED or globalresult is ALLOWED
                        else if (((allowed >> Convert.ToInt32(permission)) % 2 == 1) || (globalResult == AuthorizationResultEnum.Allowed))
                        {
                            result = AuthorizationResultEnum.Allowed;
                        }
                        // No ACL is set and globalresult is INSIGNIFICANT
                        else if (globalResult == AuthorizationResultEnum.Insignificant)
                        {
                            result = AuthorizationResultEnum.Insignificant;
                        }
                        // No ACL is set
                        else
                        {
                            result = AuthorizationResultEnum.Denied;
                        }
                    }
                    else
                    {
                        result = globalResult;
                    }
                }
            }

            // Run the custom event handler
            if (DocumentEvents.AuthorizeDocument.IsBound)
            {
                TreeNode node = TreeProvider.GetDocument(data, tree);
                DocumentEvents.AuthorizeDocument.StartEvent(userInfo, node, GetPermissionName(permission), ref result);
            }

            // Save the result and return
            RequestStockHelper.AddToStorage(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, result, false);
            return result;
        }


        /// <summary>
        /// Invalidates permission authorization results per node for given user.
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="nodeId">Node identifier</param>
        /// <param name="cultureCode">Document culture code</param>
        public static void InvalidateTreeNodeAuthorizationResults(UserInfo userInfo, int nodeId, string cultureCode)
        {
            foreach (string permissionName in Enum.GetNames(typeof(NodePermissionsEnum)))
            {
                NodePermissionsEnum permission = (NodePermissionsEnum)Enum.Parse(typeof(NodePermissionsEnum), permissionName);
                string resultKey = GetAuthorizationResultKey(userInfo, nodeId, permission, cultureCode);
                if (RequestStockHelper.Contains(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false))
                {
                    RequestStockHelper.Remove(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false);
                }
            }
        }


        /// <summary>
        /// Returns true if user is granted with specified permission for particular node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission name</param>
        /// <param name="userInfo">UserInfo instance</param> 
        /// <param name="culture">Document culture to be checked</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        public static AuthorizationResultEnum IsAuthorizedPerNode(TreeNode node, NodePermissionsEnum permission, UserInfo userInfo, bool checkUserAllowedCultures = true, string culture = null)
        {
            // If no document given, allowed automatically
            if (node == null)
            {
                return AuthorizationResultEnum.Allowed;
            }

            if (checkUserAllowedCultures)
            {
                if (culture == null)
                {
                    culture = ValidationHelper.GetString(node.GetValue("DocumentCulture"), null);
                    if (culture == null)
                    {
                        throw new Exception("[TreeSecurityProvider.IsAuthorizedPerNode]: The page culture has to be specified.");
                    }
                }
            }

            AuthorizationResultEnum result = IsAuthorizedPerNodeData(node, permission, userInfo, culture);

            // Initiate the event
            DocumentEvents.AuthorizeDocument.StartEvent(userInfo, node, GetPermissionName(permission), ref result);

            // Save the result and return
            return result;
        }


        /// <summary>
        /// Returns true if specified user is granted with given permission for the ACL specified by the ACLID value.
        /// </summary>
        /// <param name="aclId">ACLID value</param>
        /// <param name="permission">Required permission</param>
        /// <param name="userInfo">Current user info object</param>
        /// <param name="siteId">Site ID</param>
        /// <remarks>If specified ACLID does not exist, true is returned.</remarks>
        public static AuthorizationResultEnum IsAuthorizedPerACL(int aclId, NodePermissionsEnum permission, UserInfo userInfo, int siteId)
        {
            return IsAuthorizedPerACL(aclId, new[] { permission }, userInfo, siteId)[0];
        }


        /// <summary>
        /// Returns true if specified user is granted with given permissions for the ACL specified by the ACLID value.
        /// </summary>
        /// <param name="aclId">ACLID value</param>
        /// <param name="permissions">Required list of permissions</param>
        /// <param name="userInfo">Current user info object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentNodeId">Parent node ID for 'ExploreTree' permission check</param>
        /// <remarks>If specified ACLID does not exist, true is returned.</remarks>
        public static AuthorizationResultEnum[] IsAuthorizedPerACL(int aclId, NodePermissionsEnum[] permissions, UserInfo userInfo, int siteId, int parentNodeId = 0)
        {
            // Check the number of permissions
            if ((permissions == null) || (permissions.Length < 0))
            {
                throw new Exception("[TreeSecurityProvider.IsAuthorizedPerACL]: Given list of permissions is empty!");
            }

            int permissionsCount = permissions.Length;
            AuthorizationResultEnum[] results = new AuthorizationResultEnum[permissionsCount];

            // Get local permissions result
            DataSet dsAclItems = AclItemInfoProvider.GetAclItems(aclId, siteId, userInfo);

            if (!DataHelper.DataSourceIsEmpty(dsAclItems))
            {
                int allowed = 0;
                int denied = 0;

                foreach (DataRow drAclItem in dsAclItems.Tables[0].Rows)
                {
                    DateTime dt = ValidationHelper.GetDateTime(drAclItem["ValidTo"], DateTimeHelper.ZERO_TIME);
                    if ((dt == DateTimeHelper.ZERO_TIME) || (dt > DateTime.Now))
                    {
                        allowed = allowed | Convert.ToInt32(drAclItem["Allowed"]);
                        denied = denied | Convert.ToInt32(drAclItem["Denied"]);
                    }
                }

                // Get permissions check results
                for (int i = 0; i < permissionsCount; ++i)
                {
                    // Get permission
                    NodePermissionsEnum permission = permissions[i];

                    if ((permission == NodePermissionsEnum.ExploreTree) && (parentNodeId > 0))
                    {
                        // Get the parent node ACL ID
                        var parentNode = DocumentNodeDataInfoProvider.GetDocumentNodes().Columns("NodeACLID").TopN(1).Where("NodeID = " + parentNodeId).FirstOrDefault();
                        int parentAclId = (parentNode != null) ? parentNode.NodeACLID : 0;

                        // If parent ACL ID is different check parent permission for ExploreTree
                        if (aclId != parentAclId)
                        {
                            results[i] = IsAuthorizedPerACL(parentAclId, NodePermissionsEnum.ExploreTree, userInfo, siteId);
                            continue;
                        }
                    }

                    // ACL is DENIED
                    if ((denied >> Convert.ToInt32(permission)) % 2 == 1)
                    {
                        results[i] = AuthorizationResultEnum.Denied;
                    }
                    // ACL is ALLOWED
                    else if ((allowed >> Convert.ToInt32(permission)) % 2 == 1)
                    {
                        results[i] = AuthorizationResultEnum.Allowed;
                    }
                    // No ACL is set
                    else
                    {
                        results[i] = AuthorizationResultEnum.Insignificant;
                    }
                }
            }
            else
            {
                for (int i = 0; i < permissionsCount; ++i)
                {
                    results[i] = AuthorizationResultEnum.Insignificant;
                }
            }

            return results;
        }


        /// <summary>
        /// Filters all tables of the given DataSet and returns only those records for which the user was granted with required permission.
        /// </summary>
        /// <param name="sourceDataSet">Source DataSet</param>
        /// <param name="permission">Required permission</param>
        /// <param name="userInfo">UserInfo instance</param>
        /// <param name="allowOwner">If true, documents the owner of which is the given user are not filtered out even if the user has not the permission</param>
        public static DataSet FilterDataSetByClassPermissions(DataSet sourceDataSet, NodePermissionsEnum permission, UserInfo userInfo, bool allowOwner = false)
        {
            // If source dataset is empty, returns it
            if (DataHelper.DataSourceIsEmpty(sourceDataSet))
            {
                return sourceDataSet;
            }

            // Reserve log item
            DataRow sdr = SecurityDebug.StartSecurityOperation("FilterDataSetByClassPermissions");
            string sourceData = null;
            if (sdr != null)
            {
                sourceData = DataHelper.GetObjectString(sourceDataSet);
            }

            DataSet dsResult = sourceDataSet.Clone();
            int rowsIndex = 0;

            // Get the permission
            string permissionName = GetPermissionName(permission);

            var siteId = sourceDataSet.Tables[0].Rows[0]["NodeSiteID"].ToInteger(0);
            string siteName = SiteInfoProvider.GetSiteName(siteId);

            // For each table in source dataset
            for (int tableIndex = 0; tableIndex < sourceDataSet.Tables.Count; tableIndex++)
            {
                DataTable sourceTable = sourceDataSet.Tables[tableIndex];
                DataTable targetTable = dsResult.Tables[tableIndex];

                // For each row in current table
                while (rowsIndex < sourceTable.Rows.Count)
                {
                    bool allowed = false;
                    DataRow row = sourceTable.Rows[rowsIndex];

                    string className = row["ClassName"].ToString();

                    // Get actual site name
                    siteId = row["NodeSiteID"].ToInteger(0);
                    siteName = SiteInfoProvider.GetSiteName(siteId);

                    int owner = ValidationHelper.GetInteger(row["NodeOwner"], 0);

                    // If user is authorised, sets flag
                    if ((allowOwner && (owner == userInfo.UserID)) || (UserInfoProvider.IsAuthorizedPerClass(className, permissionName, siteName, userInfo) &&
                                                                       UserInfoProvider.IsAuthorizedPerResource("cms.content", permissionName, siteName, userInfo)))
                    {
                        allowed = true;
                    }

                    // Selects all rows with specific ClassName
                    DataRow[] selectedRows = sourceTable.Select("ClassName = '" + className + "'");

                    // If user is authorized, imports all rows with specific Class
                    foreach (DataRow dr in selectedRows)
                    {
                        if (allowed)
                        {
                            targetTable.ImportRow(dr);
                        }
                        sourceTable.Rows.Remove(dr);
                    }

                    ++rowsIndex;
                }
            }

            // Log the security
            if (sdr != null)
            {
                string result = DataHelper.GetObjectString(dsResult);

                SecurityDebug.FinishSecurityOperation(sdr, userInfo.UserName, sourceData, permissionName, result, siteName);
            }

            return dsResult;
        }


        /// <summary>
        /// Filters all tables of the given DataSet and returns only those records for which the user was granted with required permission.
        /// </summary>
        /// <param name="sourceDataSet">Source DataSet</param>
        /// <param name="permission">Required permission</param>
        /// <param name="userInfo">Current user info object</param>
        /// <param name="allowOwner">If true, documents the owner of which is the given user are not filtered out even if the user has not the permission</param>
        /// <param name="deleteRecords">If true, the filtered records are deleted from the DataSet. If false, records with flag columns to identify if user was granted with required permission are returned</param>
        /// <param name="topN">Expected maximum of records to return</param>
        public static DataSet FilterDataSetByPermissions(DataSet sourceDataSet, NodePermissionsEnum permission, UserInfo userInfo, bool allowOwner = false, bool deleteRecords = true, int topN = 0)
        {
            return FilterDataSetByPermissions(sourceDataSet, new[] { permission }, userInfo, allowOwner, deleteRecords, topN);
        }


        /// <summary>
        /// Filters all tables of the given DataSet and returns only those records for which the user was granted with required permissions.
        /// </summary>
        /// <param name="sourceDataSet">Source DataSet</param>
        /// <param name="permissions">List of required permission</param>
        /// <param name="userInfo">Current user info object</param>
        /// <param name="allowOwner">If true, documents the owner of which is the given user are not filtered out even if the user has not the permission</param>
        /// <param name="deleteRecords">If true, the filtered records are deleted from the DataSet. If false, records with flag columns to identify if user was granted with required permissions are returned</param>
        /// <param name="topN">Expected maximum of records to return</param>
        public static DataSet FilterDataSetByPermissions(DataSet sourceDataSet, NodePermissionsEnum[] permissions, UserInfo userInfo, bool allowOwner, bool deleteRecords, int topN = 0)
        {
            return FilterDataSetByPermissions(sourceDataSet, permissions, userInfo, allowOwner, deleteRecords, null, topN);
        }


        /// <summary>
        /// Filters all tables of the given DataSet and returns records with flag columns to identify if user was granted with required permissions.
        /// </summary>
        /// <param name="sourceDataSet">Source DataSet</param>
        /// <param name="permissions">List of required permission</param>
        /// <param name="userInfo">Current user info object</param>
        /// <param name="allowOwner">If true, documents the owner of which is the given user are not filtered out even if the user has not the permission</param>
        /// <param name="cultureCodes">Culture codes separated by ';' to perform permissions check separately for each culture</param>
        /// <param name="topN">Expected maximum of records to return</param>
        public static DataSet FilterDataSetByPermissions(DataSet sourceDataSet, NodePermissionsEnum[] permissions, UserInfo userInfo, bool allowOwner, string cultureCodes, int topN = 0)
        {
            return FilterDataSetByPermissions(sourceDataSet, permissions, userInfo, allowOwner, false, cultureCodes, topN);
        }


        /// <summary>
        /// Gets result of the given permission and culture from DataRow.
        /// </summary>
        /// <param name="dr">DataRow with data</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="cultureCode">Culture code</param>
        public static bool CheckPermission(DataRow dr, NodePermissionsEnum permission, string cultureCode)
        {
            return DataHelper.GetBoolValue(dr, GetPermissionColumnName(permission, cultureCode), true);
        }


        /// <summary>
        /// Returns name of the permission specified by the enumeration value.
        /// </summary>
        /// <param name="permission">Enumeration value of the permission</param>
        public static string GetPermissionName(NodePermissionsEnum permission)
        {
            string permissionName = null;

            switch (permission)
            {
                case NodePermissionsEnum.Read:
                    permissionName = "read";
                    break;

                case NodePermissionsEnum.Modify:
                    permissionName = "modify";
                    break;

                case NodePermissionsEnum.Create:
                    permissionName = "create";
                    break;

                case NodePermissionsEnum.Delete:
                    permissionName = "delete";
                    break;

                case NodePermissionsEnum.Destroy:
                    permissionName = "destroy";
                    break;

                case NodePermissionsEnum.ExploreTree:
                    permissionName = "exploretree";
                    break;

                case NodePermissionsEnum.ModifyPermissions:
                    permissionName = "modifypermissions";
                    break;
            }

            return permissionName;
        }


        /// <summary>
        /// Indicates if given document permission is culture specific.
        /// </summary>
        /// <param name="permission">Permission</param>
        public static bool IsCultureSpecificPermission(NodePermissionsEnum permission)
        {
            // Check user's allowed cultures
            switch (permission)
            {
                case NodePermissionsEnum.Modify:
                case NodePermissionsEnum.Create:
                case NodePermissionsEnum.Delete:
                case NodePermissionsEnum.Destroy:
                case NodePermissionsEnum.ModifyPermissions:
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Gets name of the column for given permission.
        /// </summary>
        /// <param name="permission">Permission</param>
        /// <param name="cultureCode">Culture code of the permisson</param>
        public static string GetPermissionColumnName(NodePermissionsEnum permission, string cultureCode)
        {
            string columnName = "Permission";
            columnName += GetPermissionName(permission);
            // Include culture
            if (IsCultureSpecificPermission(permission) && !string.IsNullOrEmpty(cultureCode))
            {
                columnName += ValidationHelper.GetIdentifier(cultureCode, "").ToUpperCSafe();
            }
            return columnName;
        }


        /// <summary>
        /// Returns 1 if current iDocument should be added to the filtered results, returns 2 if current document should be added but
        /// position should be decreased
        /// </summary>
        /// <param name="settings">Check permission settings</param>
        /// <param name="doc">Current result document</param>
        /// <param name="position">Current document index</param>
        public static int FilterSearchResults(SearchResults settings, ILuceneSearchDocument doc, int position)
        {
            if (doc == null)
            {
                return 0;
            }


            #region "Site name/SiteID"

            // Get site name from iDocument
            string siteName = doc.Get(SearchFieldsConstants.SITE);
            int siteId;

            // Get Site Id
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                siteId = si.SiteID;
            }
            // If site info doesn't exist, current iDocument cannot be in search results
            else
            {
                return 0;
            }

            siteName = siteName.ToLowerCSafe();

            #endregion


            #region "Publish from/to"

            string from = doc.Get("documentpublishfrom");
            string to = doc.Get("documentpublishto");

            DateTime publishedFrom = DateTime.MinValue;
            DateTime publishedTo = DateTime.MaxValue;

            try
            {
                if (!String.IsNullOrEmpty(from))
                {
                    publishedFrom = SearchValueConverter.StringToDate(from);
                }

                if (!String.IsNullOrEmpty(to))
                {
                    publishedTo = SearchValueConverter.StringToDate(to);
                }
            }
            catch
            {
            }

            DateTime now = DateTime.Now;
            if ((now < publishedFrom) || (now > publishedTo))
            {
                return 0;
            }

            #endregion


            #region "Permissions"

            var docParams = settings.DocumentParameters;

            var globalPermissions = docParams.GlobalPermissions;
            if (globalPermissions != null)
            {
                var user = (UserInfo)settings.User;

                // Indicates whether user has a global permission
                bool globalPermission;

                if (globalPermissions[siteName] != null)
                {
                    globalPermission = globalPermissions[siteName].Value;
                }
                else
                {
                    if ((user != null) && (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerResource("cms.content", GetPermissionName(NodePermissionsEnum.Read), siteName, user)))
                    {
                        globalPermissions[siteName] = true;
                    }
                    else
                    {
                        globalPermissions[siteName] = false;
                    }

                    globalPermission = globalPermissions[siteName].Value;
                }

                // Get classname and ACL ID                
                string className = doc.Get("classname");
                int ACLID = ValidationHelper.GetInteger(doc.Get("nodeaclid"), 0);

                // Create class id table if not exists
                var classIDPermissionsSites = docParams.ClassIDPermissions;

                var classIDPermissions = classIDPermissionsSites[siteName];
                if (classIDPermissions == null)
                {
                    classIDPermissions = new SafeDictionary<string, bool>();
                    classIDPermissionsSites[siteName.ToLowerCSafe()] = classIDPermissions;
                }

                // Create ACL id table if not exists
                var ACLIDPermissionsSites = docParams.ACLIDPermissions;

                var ACLIDPermissions = ACLIDPermissionsSites[siteName];
                if (ACLIDPermissions == null)
                {
                    ACLIDPermissions = new SafeDictionary<int, object>();
                    ACLIDPermissionsSites[siteName] = ACLIDPermissions;
                }


                // If there is not a record for specific classname, adds it
                if (!classIDPermissions.ContainsKey(className))
                {
                    classIDPermissions[className] = UserInfoProvider.IsAuthorizedPerClass(className, GetPermissionName(NodePermissionsEnum.Read), siteName, user);
                }

                // If there is not a record for specific ACLID, adds it
                if (!ACLIDPermissions.ContainsKey(ACLID))
                {
                    ACLIDPermissions[ACLID] = IsAuthorizedPerACL(ACLID, NodePermissionsEnum.Read, user, siteId);
                }

                // Check if document is allowed
                if (!IsDocumentAllowed(user, NodePermissionsEnum.Read, null, siteName, classIDPermissions[className], (AuthorizationResultEnum)ACLIDPermissions[ACLID], globalPermission))
                {
                    return 0;
                }
            }

            #endregion


            #region "Cultures"

            var cultures = docParams.Cultures;
            var culture = docParams.Culture;

            if (cultures != null)
            {
                string docCulture = ValidationHelper.GetString(doc.Get(SearchFieldsConstants.CULTURE), String.Empty).ToLowerCSafe();
                int nodeId = ValidationHelper.GetInteger(doc.Get("nodeid"), 0);

                // Document is in current culture
                if (docCulture == culture.ToLowerCSafe())
                {
                    // Remove added documents in non-required version
                    if (cultures.Contains(nodeId))
                    {
                        settings.Results[cultures[nodeId]] = null;
                    }

                    // Add position of current document
                    cultures[nodeId] = position;
                    return 1;
                }

                // Add non-current culture
                if (!cultures.Contains(nodeId))
                {
                    cultures[nodeId] = position;
                    return 1;
                }
                // Document in default culture already exists in results collection
                else
                {
                    return 0;
                }
            }

            #endregion


            return 1;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Filters all tables of the given DataSet and returns only those records for which the user was granted with required permissions.
        /// </summary>
        /// <param name="sourceDataSet">Source DataSet</param>
        /// <param name="permissions">List of required permission</param>
        /// <param name="userInfo">Current user info object</param>
        /// <param name="allowOwner">If true, documents the owner of which is the given user are not filtered out even if the user has not the permission</param>
        /// <param name="deleteRecords">If true, the filtered records are deleted from the DataSet. If false, the filtered records are marked with a flag</param>
        /// <param name="cultureCodes">Culture codes separated by ';' to perform permissions check separately for each culture</param>
        /// <param name="topN">Expected maximum of records to return</param>
        private static DataSet FilterDataSetByPermissions(DataSet sourceDataSet, NodePermissionsEnum[] permissions, UserInfo userInfo, bool allowOwner, bool deleteRecords, string cultureCodes, int topN)
        {
            // If source dataset is empty, returns it
            if (DataHelper.DataSourceIsEmpty(sourceDataSet))
            {
                return sourceDataSet;
            }

            // Check the number of permissions
            if ((permissions == null) || (permissions.Length < 0))
            {
                throw new Exception("[TreeSecurityProvider.FilterDataSetByPermissions]: Given list of permissions is empty!");
            }

            // Reserve log item
            DataRow sdr = SecurityDebug.StartSecurityOperation("FilterDataSetByPermissions");
            string sourceData = null;
            if (sdr != null)
            {
                sourceData = DataHelper.GetObjectString(sourceDataSet);
            }

            // Get permission names
            int permissionsCount = permissions.Length;
            string[] permissionNames = new string[permissionsCount];
            for (int i = 0; i < permissionsCount; ++i)
            {
                permissionNames[i] = GetPermissionName(permissions[i]);
            }

            string siteName = null;

            // Do not filter for global administrator
            if (!userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                // Make sure that the Dataset can be modified
                sourceDataSet = sourceDataSet.AsModifyable();

                siteName = SiteInfoProvider.GetSiteName(sourceDataSet.Tables[0].Rows[0]["NodeSiteID"].ToInteger(0));

                string[] cultureList = (cultureCodes == null) ? null : cultureCodes.Split(new[] { ';' });
                bool[] globalPermissions = new bool[permissionsCount];

                // Check global CMS Content permissions first
                for (int i = 0; i < permissionsCount; ++i)
                {
                    globalPermissions[i] = (UserInfoProvider.IsAuthorizedPerResource("cms.content", permissionNames[i], siteName, userInfo));
                }

                Hashtable classIDPermissions = new Hashtable();
                Hashtable ACLIDPermissions = new Hashtable();

                // For all tables in dataset
                foreach (DataTable dt in sourceDataSet.Tables)
                {
                    ArrayList deleteRows = new ArrayList();

                    // Ensure permissions flag columns
                    if (!deleteRecords)
                    {
                        for (int i = 0; i < permissionsCount; ++i)
                        {
                            NodePermissionsEnum permission = permissions[i];
                            if (IsCultureSpecificPermission(permission) && (cultureList != null))
                            {
                                for (int j = 0; j < cultureList.Length; ++j)
                                {
                                    DataHelper.EnsureColumn(dt, GetPermissionColumnName(permission, cultureList[j]), typeof(bool));
                                }
                            }
                            else
                            {
                                DataHelper.EnsureColumn(dt, GetPermissionColumnName(permission, null), typeof(bool));
                            }
                        }
                    }

                    int recordsCount = 0;

                    // Find rows to filter out
                    foreach (DataRow dr in dt.Rows)
                    {
                        // Maximum of records was reached, filter out records
                        if ((topN <= 0) || !deleteRecords || (recordsCount < topN))
                        {
                            string className = dr["ClassName"].ToString().ToLowerCSafe();
                            int ACLID = ValidationHelper.GetInteger(dr["NodeACLID"], 0);
                            int siteId = ValidationHelper.GetInteger(dr["NodeSiteID"], 0);
                            int owner = ValidationHelper.GetInteger(dr["NodeOwner"], 0);
                            string documentSiteName = DataHelper.GetNotEmpty(SiteInfoProvider.GetSiteName(siteId), siteName);
                            int parentNodeId = ValidationHelper.GetInteger(dr["NodeParentID"], 0);

                            string ACLKey = GetACLKeyName(ACLID, permissions, parentNodeId);

                            // If there is not a record for specific ACLID, adds it
                            if (!ACLIDPermissions.ContainsKey(ACLKey))
                            {
                                ACLIDPermissions[ACLKey] = IsAuthorizedPerACL(ACLID, permissions, userInfo, siteId, parentNodeId);
                            }

                            bool authorized = true;
                            for (int i = 0; i < permissionsCount; ++i)
                            {
                                NodePermissionsEnum permission = permissions[i];
                                string permissionName = permissionNames[i];
                                string key = className + "_" + permissionName;

                                // If there is not a record for specific classname, adds it
                                if (!classIDPermissions.ContainsKey(key))
                                {
                                    classIDPermissions[key] = UserInfoProvider.IsAuthorizedPerClass(className, permissionName, documentSiteName, userInfo);
                                }

                                if (!deleteRecords && IsCultureSpecificPermission(permission) && (cultureList != null))
                                {
                                    for (int j = 0; j < cultureList.Length; ++j)
                                    {
                                        // Check if document is allowed.
                                        bool allowed = (allowOwner && (owner == userInfo.UserID)) || IsDocumentAllowed(userInfo, permission, cultureList[j], documentSiteName, (bool)classIDPermissions[key], ((AuthorizationResultEnum[])ACLIDPermissions[ACLKey])[i], globalPermissions[i]);

                                        // Set appropriate flag
                                        DataHelper.SetDataRowValue(dr, GetPermissionColumnName(permission, cultureList[j]), allowed);
                                    }
                                }
                                else
                                {
                                    // Check if document is allowed.
                                    bool allowed = (allowOwner && (owner == userInfo.UserID)) || IsDocumentAllowed(userInfo, permission, null, documentSiteName, (bool)classIDPermissions[key], ((AuthorizationResultEnum[])ACLIDPermissions[ACLKey])[i], globalPermissions[i]);

                                    if (!allowed && deleteRecords && !deleteRows.Contains(dr))
                                    {
                                        // Mark row for deletion
                                        deleteRows.Add(dr);
                                    }
                                    // Set appropriate flag
                                    else if (!deleteRecords)
                                    {
                                        DataHelper.SetDataRowValue(dr, GetPermissionColumnName(permission, null), allowed);
                                    }
                                    authorized &= allowed;
                                }
                            }

                            if (authorized)
                            {
                                // Increase counter
                                recordsCount++;
                            }
                        }
                        else
                        {
                            // Mark row for deletion
                            deleteRows.Add(dr);
                        }
                    }

                    // Delete the filtered rows
                    foreach (DataRow dr in deleteRows)
                    {
                        dt.Rows.Remove(dr);
                    }
                }

                // Accept DataSet changes
                sourceDataSet.AcceptChanges();
            }

            // Run custom filtering event if set
            if (DocumentEvents.FilterDataSetByPermissions.IsBound)
            {
                for (int i = 0; i < permissionsCount; ++i)
                {
                    // Initiate the event handler
                    DocumentEvents.FilterDataSetByPermissions.StartEvent(userInfo, ref sourceDataSet, permissionNames[i]);
                }
            }

            // Log the security
            if (sdr != null)
            {
                string result = DataHelper.GetObjectString(sourceDataSet);

                SecurityDebug.FinishSecurityOperation(sdr, userInfo.UserName, sourceData, string.Join(", ", permissionNames), result, siteName);
            }

            return sourceDataSet;
        }


        /// <summary>
        /// Gets key name to access ACL permissions in hashtable.
        /// </summary>
        /// <param name="ACLID">ACL ID</param>
        /// <param name="permissions">Permissions array</param>
        /// <param name="parentNodeId">Parent node ID</param>
        /// <returns>If permissions contains 'ExploreTree' then returns key in format 'ACLID|parentNodeId' else 'ACLID'</returns>
        private static string GetACLKeyName(int ACLID, NodePermissionsEnum[] permissions, int parentNodeId)
        {
            foreach (NodePermissionsEnum enumPer in permissions)
            {
                // Explore tree permission is based on parent document settings
                if (enumPer == NodePermissionsEnum.ExploreTree)
                {
                    return ACLID + "|" + parentNodeId;
                }
            }
            return ACLID.ToString();
        }


        /// <summary>
        /// Indicates if user has allowed specified culture.
        /// </summary>
        /// <param name="permission">Permission</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="user">User info</param>
        /// <param name="siteName">Site name</param>
        public static bool HasUserCultureAllowed(NodePermissionsEnum permission, string cultureCode, UserInfo user, string siteName)
        {
            // Check user's allowed cultures
            if (IsCultureSpecificPermission(permission))
            {
                // Document culture is defined
                if (!string.IsNullOrEmpty(cultureCode))
                {
                    if (user.UserHasAllowedCultures)
                    {
                        if (!user.IsCultureAllowed(cultureCode, siteName))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Checks whether document is allowed.
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        /// <param name="isAuthorizedByClassName">Indicates if authorized by class permissions</param>
        /// <param name="isAuthorizedByACL">Indicates ACL permissions authorization result</param>
        /// <param name="globalPermissions">If user has global permissions</param>
        private static bool IsDocumentAllowed(UserInfo userInfo, NodePermissionsEnum permission, string cultureCode, string siteName, bool isAuthorizedByClassName, AuthorizationResultEnum isAuthorizedByACL, bool globalPermissions)
        {
            bool allowed = false;

            if (((globalPermissions || isAuthorizedByClassName) && (isAuthorizedByACL != AuthorizationResultEnum.Denied)) || (isAuthorizedByACL == AuthorizationResultEnum.Allowed))
            {
                // Check user's allowed cultures
                allowed = HasUserCultureAllowed(permission, cultureCode, userInfo, siteName);
            }

            return allowed;
        }


        /// <summary>
        /// Gets the authorization result key for request.
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="nodeId">Node identifier</param>
        /// <param name="permission">Permission type</param>
        /// <param name="cultureCode">Culture code of document</param>
        /// <returns>Key for retrieving authorization result</returns>
        internal static string GetAuthorizationResultKey(UserInfo userInfo, int nodeId, NodePermissionsEnum permission, string cultureCode)
        {
            string resultKey = String.Format("{0}|{1}|{2}", nodeId, permission, userInfo.UserID);
            // Add document culture
            if (!string.IsNullOrEmpty(cultureCode))
            {
                resultKey += "|" + cultureCode;
            }
            return resultKey;
        }

        #endregion
    }
}