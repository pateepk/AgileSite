using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Modules
{
    using TypedDataSet = InfoDataSet<PermissionNameInfo>;

    /// <summary>
    /// Permission info data container class.
    /// </summary>
    public class PermissionNameInfoProvider : AbstractInfoProvider<PermissionNameInfo, PermissionNameInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public PermissionNameInfoProvider()
            : base(PermissionNameInfo.TYPEINFORESOURCE, new HashtableSettings
                {
                    ID = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all permission names.
        /// </summary>
        public static ObjectQuery<PermissionNameInfo> GetPermissionNames()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static PermissionNameInfo GetPermissionNameInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the permission name info object for the resource.
        /// </summary>
        /// <param name="resourceName">Name of the resource which permission depends to</param>
        /// <param name="permissionName">Name of the permission to retrieve</param>
        /// <param name="siteName">Site which the permission depends to</param>
        public static PermissionNameInfo GetPermissionNameInfoForResource(string resourceName, string permissionName, string siteName)
        {
            return ProviderObject.GetPermissionNameInfoForResourceInternal(resourceName, permissionName, siteName);
        }


        /// <summary>
        /// Returns the permission name info object for the class.
        /// </summary>
        /// <param name="className">Name of the class which permission depends to</param>
        /// <param name="permissionName">Name of the permission to retrieve</param>
        /// <param name="siteName">Site which the permission depends to</param>
        public static PermissionNameInfo GetPermissionNameInfoForClass(string className, string permissionName, string siteName)
        {
            return ProviderObject.GetPermissionNameInfoForClassInternal(className, permissionName, siteName);
        }


        /// <summary>
        /// Returns the PermissionNameInfo object by the given parameters.
        /// </summary>
        /// <param name="permissionName">Permission name to retrieve</param>
        /// <param name="resourceName">Resource name (null when Class permission)</param>
        /// <param name="className">Class name (null when Resource permission)</param>
        public static PermissionNameInfo GetPermissionNameInfo(string permissionName, string resourceName, string className)
        {
            return ProviderObject.GetPermissionNameInfoInternal(permissionName, resourceName, className);
        }


        /// <summary>
        /// Returns the permission name info object.
        /// </summary>
        /// <param name="permissionId">Id of the permission to retrieve</param>
        public static PermissionNameInfo GetPermissionNameInfo(int permissionId)
        {
            return ProviderObject.GetInfoById(permissionId);
        }


        /// <summary>
        /// Sets the specified permission data.
        /// </summary>
        /// <param name="permissionObj">New permission info data</param>
        public static void SetPermissionInfo(PermissionNameInfo permissionObj)
        {
            ProviderObject.SetInfo(permissionObj);
        }


        /// <summary>
        /// Deletes the specified permission.
        /// </summary>
        /// <param name="pni">Permission object</param>
        public static void DeletePermissionInfo(PermissionNameInfo pni)
        {
            ProviderObject.DeleteInfo(pni);
        }


        /// <summary>
        /// Deletes the specified permission.
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        public static void DeletePermissionInfo(int permissionId)
        {
            PermissionNameInfo pni = GetPermissionNameInfo(permissionId);
            DeletePermissionInfo(pni);
        }


        /// <summary>
        /// Gets the DataSet of the resource permissions.
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        public static TypedDataSet GetResourcePermissions(int resourceId)
        {
            return ProviderObject.GetResourcePermissionsInternal(resourceId);
        }


        /// <summary>
        /// Gets the DataSet of the class permissions.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static TypedDataSet GetClassPermissions(int classId)
        {
            return ProviderObject.GetClassPermissionsInternal(classId);
        }


        /// <summary>
        /// Gets the DataSet of the permissions for specific resource and role.
        /// </summary>
        /// <param name="resourceId">ID of the resource the permissions are included in</param>
        /// <param name="roleId">ID of the role which is granted with requested permissions</param>
        public static TypedDataSet GetPermissions(int resourceId, int roleId)
        {
            return ProviderObject.GetPermissionsInternal(resourceId, roleId);
        }


        /// <summary>
        /// Gets the where condition to limit permissions for specific resource and role.
        /// </summary>
        /// <param name="resourceId">ID of the resource the permissions are included in</param>
        /// <param name="roleId">ID of the role which is granted with requested permissions</param>
        public static string GetPermissionsWhereCondition(int resourceId, int roleId)
        {
            string bindingWhere = ProviderObject.GetBindingWhereCondition(RolePermissionInfo.OBJECT_TYPE, roleId);
            string parentWhere = ProviderObject.GetParentWhereCondition(resourceId);

            return SqlHelper.AddWhereCondition(bindingWhere, parentWhere);
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific parent.
        /// </summary>
        /// <param name="objectParentId">Object parent ID to filter by</param>
        public virtual string GetParentWhereCondition(int objectParentId)
        {
            // Build the condition for the parent
            if ((objectParentId > 0) && (TypeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                // ObjectParentColumn = <objectParentID>
                return $"{TypeInfo.ParentIDColumn} = {objectParentId}";
            }

            return "";
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific binding dependencies. Use this method for 3 and more-keys bindings.
        /// </summary>
        /// <param name="bindingType">Binding object type name</param>
        /// <param name="dependencies">Pairs of binding dependencies to filter by (first variable in each pair - dependency object type, second variable in each pair - dependency value)</param>
        public virtual string GetBindingWhereCondition(string bindingType, params object[] dependencies)
        {
            return TypeInfo.GetBindingWhereCondition(bindingType, dependencies);
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific binding dependency. Use this method only for 2-keys bindings.
        /// </summary>
        /// <param name="bindingType">Binding object type name</param>
        /// <param name="bindingDependencyId">Binding dependency ID to filter by</param>
        public string GetBindingWhereCondition(string bindingType, int bindingDependencyId)
        {
            return GetBindingWhereCondition(bindingType, null, bindingDependencyId);
        }


        /// <summary>
        /// Creates the default.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static void CreateDefaultClassPermissions(int classId)
        {
            CreateClassPermission(classId, "Read", "{$permissionnames.read$}", 1, "{$permissiondescription.document.read$}");
            CreateClassPermission(classId, "Modify", "{$permissionnames.modify$}", 2, "{$permissiondescription.document.modify$}");
            CreateClassPermission(classId, "CreateSpecific", "{$permissionnames.create$}", 3, "{$permissiondescription.document.create$}");
            CreateClassPermission(classId, "Create", "{$permissionnames.createanywhere$}", 4, "{$permissiondescription.document.createanywhere$}");
            CreateClassPermission(classId, "Delete", "{$general.delete$}", 5, "{$permissiondescription.document.delete$}");
            CreateClassPermission(classId, "Destroy", "{$permissionnames.destroy$}", 6, "{$permissiondescription.document.destroy$}");
            CreateClassPermission(classId, "ExploreTree", "{$permissionnames.exploretree$}", 7, "{$permissiondescription.document.exploretree$}");
            CreateClassPermission(classId, "ModifyPermissions", "{$permissionnames.modifypermissions$}", 8, "{$permissiondescription.document.modifypermissions$}");
        }


        /// <summary>
        /// Creates the default custom table permissions.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static void CreateDefaultCustomTablePermissions(int classId)
        {
            CreateClassPermission(classId, "Read", "{$permissionnames.read$}", 1, "{$permissiondescription.customtable.read$}");
            CreateClassPermission(classId, "Modify", "{$permissionnames.modify$}", 2, "{$permissiondescription.customtable.modify$}");
            CreateClassPermission(classId, "Create", "{$permissionnames.create$}", 3, "{$permissiondescription.customtable.create$}");
            CreateClassPermission(classId, "Delete", "{$general.delete$}", 4, "{$permissiondescription.customtable.delete$}");
        }


        /// <summary>
        /// Sets default permission for the new class.
        /// </summary>
        /// <param name="classId">Class Id</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="permissionDisplayName">Permission display name</param>
        public static void CreateClassPermission(int classId, string permissionName, string permissionDisplayName)
        {
            CreateClassPermission(classId, permissionName, permissionDisplayName, 0);
        }


        /// <summary>
        /// Sets default permission for the new class.
        /// </summary>
        /// <param name="classId">Class Id</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="permissionDisplayName">Permission display name</param>
        /// <param name="permissionOrder">Permission order</param>
        public static void CreateClassPermission(int classId, string permissionName, string permissionDisplayName, int permissionOrder)
        {
            CreateClassPermission(classId, permissionName, permissionDisplayName, 0, null);
        }


        /// <summary>
        /// Sets default permission for the new class.
        /// </summary>
        /// <param name="classId">Class Id</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="permissionDisplayName">Permission display name</param>
        /// <param name="permissionOrder">Permission order</param>
        /// <param name="permissionDescription">Permission description</param>
        public static void CreateClassPermission(int classId, string permissionName, string permissionDisplayName, int permissionOrder, string permissionDescription)
        {
            // Create new permission
            PermissionNameInfo pi = new PermissionNameInfo();

            pi.PermissionName = permissionName;
            pi.PermissionDisplayName = permissionDisplayName;
            pi.ResourceId = 0; // null
            pi.ClassId = classId;
            pi.PermissionDisplayInMatrix = true;
            if (permissionOrder > 0)
            {
                pi.PermissionOrder = permissionOrder;
            }

            if (!String.IsNullOrEmpty(permissionDescription))
            {
                pi.PermissionDescription = permissionDescription;
            }

            // Save to database
            using (CMSActionContext context = new CMSActionContext())
            {
                // Disable logging into event log
                context.LogEvents = false;

                SetPermissionInfo(pi);
            }
        }


        /// <summary>
        /// Deletes all resource permissions.
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        public static void DeleteResoucePermissions(int resourceId)
        {
            // Delete all permissions
            DataSet permissionsDS = GetResourcePermissions(resourceId);
            if (!DataHelper.DataSourceIsEmpty(permissionsDS))
            {
                foreach (DataRow dr in permissionsDS.Tables[0].Rows)
                {
                    int permissionId = ValidationHelper.GetInteger(dr["PermissionID"], 0);
                    DeletePermissionInfo(permissionId);
                }
            }
        }


        /// <summary>
        /// Deletes all class permissions.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static void DeleteClassPermissions(int classId)
        {
            // Delete all permissions
            DataSet permissionsDS = GetClassPermissions(classId);
            if (!DataHelper.DataSourceIsEmpty(permissionsDS))
            {
                foreach (DataRow dr in permissionsDS.Tables[0].Rows)
                {
                    int permissionId = ValidationHelper.GetInteger(dr["PermissionID"], 0);
                    DeletePermissionInfo(permissionId);
                }
            }
        }


        /// <summary>
        /// Moves specified permission up.
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        public static void MovePermissionUp(int permissionId)
        {
            ProviderObject.MovePermissionUpInternal(permissionId);
        }


        /// <summary>
        /// Moves specified permission down.
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        public static void MovePermissionDown(int permissionId)
        {
            ProviderObject.MovePermissionDownInternal(permissionId);
        }


        /// <summary>
        /// Returns order of the last permission.
        /// </summary>
        /// <param name="classId">ID of the class to get last permission order for</param>
        /// <param name="resourceId">ID of the resource to get last permission order for</param>
        /// <returns>Number representing the last permission order for the class or resource.</returns>
        public static int GetLastPermissionOrder(int classId, int resourceId)
        {
            BaseInfo info = null;
            if (classId > 0)
            {
                info = ModuleManager.GetReadOnlyObject(PermissionNameInfo.OBJECT_TYPE_CLASS);
            }
            else if (resourceId > 0)
            {
                info = ModuleManager.GetReadOnlyObject(PermissionNameInfo.OBJECT_TYPE_RESOURCE);
            }
            if (info != null)
            {
                return info.Generalized.GetLastObjectOrder();
            }

            return 0;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the permission name info object for the resource.
        /// </summary>
        /// <param name="resourceName">Name of the resource which permission depends to</param>
        /// <param name="permissionName">Name of the permission to retrieve</param>
        /// <param name="siteName">Site which the permission depends to</param>
        protected virtual PermissionNameInfo GetPermissionNameInfoForResourceInternal(string resourceName, string permissionName, string siteName)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();

            parameters.Add("@ResourceName", resourceName);
            parameters.Add("@PermissionName", permissionName);
            parameters.Add("@SiteName", siteName);

            // Get the data
            DataSet permissionDataSet = ConnectionHelper.ExecuteQuery("cms.permission.SelectInfoForResource", parameters);

            if (!DataHelper.DataSourceIsEmpty(permissionDataSet))
            {
                return new PermissionNameInfo(permissionDataSet.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns the permission name info object for the class.
        /// </summary>
        /// <param name="className">Name of the class which permission depends to</param>
        /// <param name="permissionName">Name of the permission to retrieve</param>
        /// <param name="siteName">Site which the permission depends to</param>
        protected virtual PermissionNameInfo GetPermissionNameInfoForClassInternal(string className, string permissionName, string siteName)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ClassName", className);
            parameters.Add("@PermissionName", permissionName);
            parameters.Add("@SiteName", siteName);

            // Get the data
            DataSet permissionDataSet = ConnectionHelper.ExecuteQuery("cms.permission.SelectInfoForClass", parameters);

            if (!DataHelper.DataSourceIsEmpty(permissionDataSet))
            {
                return new PermissionNameInfo(permissionDataSet.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns the PermissionNameInfo object by the given parameters.
        /// </summary>
        /// <param name="permissionName">Permission name to retrieve</param>
        /// <param name="resourceName">Resource name (null when Class permission)</param>
        /// <param name="className">Class name (null when Resource permission)</param>
        protected virtual PermissionNameInfo GetPermissionNameInfoInternal(string permissionName, string resourceName, string className)
        {
            // Prepare the WHERE condition
            string where = "PermissionName = '" + SqlHelper.GetSafeQueryString(permissionName, false) + "'";

            // Create WHERE condition
            if (!string.IsNullOrEmpty(className))
            {
                DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(className);
                if (ci != null)
                {
                    where += " AND (ClassID = " + ci.ClassID.ToString() + ")";
                }
            }
            else if (!string.IsNullOrEmpty(resourceName))
            {
                ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(resourceName);
                if (ri != null)
                {
                    where += " AND (ResourceID = " + ri.ResourceID.ToString() + ")";
                }
            }

            // Get and process the result
            DataSet ds = GetPermissionNames().Where(where).BinaryData(true);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new PermissionNameInfo(ds.Tables[0].Rows[0]);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(PermissionNameInfo info)
        {
            if (info != null)
            {
                if (((info.ResourceId > 0) && (info.ClassId > 0)) || ((info.ResourceId <= 0) && (info.ClassId <= 0)))
                {
                    throw new Exception("[PermissionNameInfoProvider.SetPermissionInfo]: Permission '" + info.PermissionName + "' must be assigned to either resource or class.");
                }

                // Ensure permission order for class permissions
                EnsureClassPermissionOrder(info);

                // Save to the database
                base.SetInfo(info);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(PermissionNameInfo info)
        {
            // Clear permission list in resource info
            ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(info.ResourceId);
            if ((ri != null) && (ri.PermissionNames != null))
            {
                ri.PermissionNames.Remove(info.PermissionName);
            }

            base.DeleteInfo(info);
        }


        /// <summary>
        /// Gets the DataSet of the resource permissions.
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        protected virtual TypedDataSet GetResourcePermissionsInternal(int resourceId)
        {
            return GetObjectQuery().WhereEquals("ResourceID", resourceId).TypedResult;
        }


        /// <summary>
        /// Gets the DataSet of the class permissions.
        /// </summary>
        /// <param name="classId">Class ID</param>
        protected virtual TypedDataSet GetClassPermissionsInternal(int classId)
        {
            return GetObjectQuery().WhereEquals("ClassID", classId).TypedResult;
        }


        /// <summary>
        /// Gets the DataSet of the permissions for specific resource and role.
        /// </summary>
        /// <param name="resourceId">ID of the resource the permissions are included in</param>
        /// <param name="roleId">ID of the role which is granted with requested permissions</param>
        protected virtual TypedDataSet GetPermissionsInternal(int resourceId, int roleId)
        {
            string where = GetPermissionsWhereCondition(resourceId, roleId);
            return GetPermissionNames().Where(where).OrderBy("PermissionOrder").TypedResult;
        }


        /// <summary>
        /// Moves specified permission up.
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        protected virtual void MovePermissionUpInternal(int permissionId)
        {
            if (permissionId > 0)
            {
                var infoObj = GetInfoById(permissionId);
                if (infoObj != null)
                {
                    infoObj.Generalized.MoveObjectUp();
                }
            }
        }


        /// <summary>
        /// Moves specified permission down.
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        protected virtual void MovePermissionDownInternal(int permissionId)
        {
            if (permissionId > 0)
            {
                var infoObj = GetInfoById(permissionId);
                if (infoObj != null)
                {
                    infoObj.Generalized.MoveObjectDown();
                }
            }
        }


        /// <summary>
        /// Method to ensure correct class default perrmissions order
        /// </summary>
        /// <param name="permission">Permission which order should be ensured</param>
        protected virtual void EnsureClassPermissionOrder(PermissionNameInfo permission)
        {
            if ((permission.ClassId > 0) && (permission.PermissionOrder == 0))
            {
                int permissionOrder = 0;
                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(permission.ClassId);
                if (dci != null)
                {
                    switch (dci.TypeInfo.ObjectType)
                    {
                        // Ensure correct order for document type permissions
                        case PredefinedObjectType.DOCUMENTTYPE:
                            switch (permission.PermissionName.ToLowerCSafe())
                            {
                                case "read":
                                    permissionOrder = 1;
                                    break;

                                case "modify":
                                    permissionOrder = 2;
                                    break;

                                case "createspecific":
                                    permissionOrder = 3;
                                    break;

                                case "create":
                                    permissionOrder = 4;
                                    break;

                                case "delete":
                                    permissionOrder = 5;
                                    break;

                                case "destroy":
                                    permissionOrder = 6;
                                    break;

                                case "exploretree":
                                    permissionOrder = 7;
                                    break;

                                case "modifypermissions":
                                    permissionOrder = 8;
                                    break;
                            }
                            break;

                        // Ensure correct order for custom table permissions
                        case PredefinedObjectType.CUSTOMTABLECLASS:
                            switch (permission.PermissionName.ToLowerCSafe())
                            {
                                case "read":
                                    permissionOrder = 1;
                                    break;

                                case "modify":
                                    permissionOrder = 2;
                                    break;

                                case "create":
                                    permissionOrder = 3;
                                    break;

                                case "delete":
                                    permissionOrder = 4;
                                    break;
                            }
                            break;
                    }

                    if (permissionOrder > 0)
                    {
                        permission.PermissionOrder = permissionOrder;
                    }
                }
            }
        }

        #endregion
    }
}