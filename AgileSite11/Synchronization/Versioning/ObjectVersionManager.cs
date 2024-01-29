using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing object versions management.
    /// </summary>
    public class ObjectVersionManager : AbstractBaseProvider<ObjectVersionManager>
    {
        #region "Object versioning constants"

        /// <summary>
        /// Object versioning restore mode determining that objects will be deleted permanently.
        /// </summary>
        public const string RESTORE_NONE = "NONE";

        /// <summary>
        /// Object versioning restore mode determining that versioned objects will be deleted to recycle bin.
        /// </summary>
        public const string RESTORE_VERSIONEDOBJECTS = "VERSIONEDOBJECTS";

        /// <summary>
        /// Object versioning restore mode determining that all objects supported in staging will be deleted to recycle bin.
        /// </summary>
        public const string RESTORE_ALL = "ALL";


        /// <summary>
        /// Storage key to determine whether a version of particular was created or not
        /// </summary>
        internal const string CREATE_VERSION_STORAGE = "SyncCreateVersion";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets latest version history for given object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        public static ObjectVersionHistoryInfo GetLatestVersion(string objectType, int objectId)
        {
            return ProviderObject.GetLatestVersionInternal(objectType, objectId);
        }


        /// <summary>
        /// Deletes older object versions.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="siteName">Object site name</param>
        public static void DeleteOlderVersions(string objectType, int objectId, string siteName)
        {
            // Get the versions to delete
            int minorHistoryLength = GetMinorVersionHistoryLength(siteName);
            int majorHistoryLength = GetMajorVersionHistoryLength(siteName);

            ProviderObject.DeleteOlderVersionsInternal(objectType, objectId, majorHistoryLength, minorHistoryLength);
        }


        /// <summary>
        /// Destroys complete object history.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        public static void DestroyObjectHistory(string objectType, int objectId)
        {
            ObjectVersionHistoryInfoProvider.DeleteVersionHistories("VersionObjectType='" + SqlHelper.GetSafeQueryString(objectType, false) + "' AND VersionObjectID=" + objectId);
        }


        /// <summary>
        /// Gets object version histories.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        public static DataSet GetObjectHistory(string objectType, int objectId, string where = null, string orderBy = null, int topN = -1, string columns = null)
        {
            return ProviderObject.GetObjectHistoryInternal(objectType, objectId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Destroys object version.
        /// </summary>
        /// <param name="versionId">Version ID</param>
        public static void DestroyObjectVersion(int versionId)
        {
            ObjectVersionHistoryInfoProvider.DeleteVersionHistoryInfo(versionId);
        }


        /// <summary>
        /// Rollbacks object to specified version.
        /// </summary>
        /// <param name="versionId">Version ID</param>
        /// <param name="processChilds">Indicates if child data should be roll-backed</param>
        /// <param name="createNewVersion">If true, new version after rollback is created</param>
        public static int RollbackVersion(int versionId, bool processChilds = false, bool createNewVersion = true)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning);
            }

            using (CMSActionContext context = new CMSActionContext())
            {
                context.AllowAsyncActions = false;
                context.LogEvents = false;

                var currentUser = CMSActionContext.CurrentUser;
                if (currentUser == null)
                {
                    throw new Exception("[ObjectVersionManager.RollbackVersion]: Missing context user.");
                }

                int newVersionId = 0;

                // Process within transaction
                using (var tr = new CMSTransactionScope())
                {
                    // Get the requested version history object
                    ObjectVersionHistoryInfo rollbackVersion = ObjectVersionHistoryInfoProvider.GetVersionHistoryInfo(versionId);

                    if (rollbackVersion == null)
                    {
                        throw new Exception("[ObjectVersionManager.RollbackVersion]: The version to be roll backed doesn't exists.");
                    }

                    rollbackVersion = FixVersionXML(rollbackVersion);

                    // Process rollback version to DB
                    object processedObj = ProcessVersioningSynchronizationTask(rollbackVersion.VersionObjectType, rollbackVersion.VersionXML, rollbackVersion.VersionBinaryDataXML, processChilds, rollbackVersion.VersionObjectSiteID);
                    
                    if (createNewVersion && (processedObj is BaseInfo))
                    {
                        BaseInfo infoObj = processedObj as BaseInfo;

                        // Create new version history
                        ObjectVersionHistoryInfo newVersion = CreateVersion(infoObj, currentUser.UserID, true);
                        if (newVersion != null)
                        {
                            newVersionId = newVersion.VersionID;

                            // Update version number
                            newVersion.VersionNumber = rollbackVersion.VersionNumber;
                            ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(newVersion);

                            // Log event
                            string objType = ResHelper.GetString("Objecttype." + infoObj.TypeInfo.ObjectType.Replace(".", "_"));
                            EventLogProvider.LogEvent(EventType.INFORMATION, objType, "ROLLBACKOBJECT", string.Format(ResHelper.GetAPIString("objectversioning.rollbacklog", "{0} '{1}' has been successfully rolled back."),
                                objType, SqlHelper.GetSafeQueryString(infoObj.Generalized.ObjectDisplayName, false)), RequestContext.RawURL, currentUser.UserID, currentUser.UserName, 0, null,
                                RequestContext.UserHostAddress, infoObj.Generalized.ObjectSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
                        }
                    }

                    tr.Commit();
                }
                return newVersionId;
            }
        }


        /// <summary>
        /// Restores object to specified site ID.
        /// </summary>
        /// <param name="versionId">Version ID</param>
        /// <param name="siteId">Site ID to which object should be restored</param>
        public static GeneralizedInfo RestoreObject(int versionId, int siteId)
        {
            // Get version history
            ObjectVersionHistoryInfo versionHistory = ObjectVersionHistoryInfoProvider.GetVersionHistoryInfo(versionId);
            if (versionHistory == null)
            {
                throw new Exception("[ObjectVersionManager.GetVersion]: History version data not found.");
            }

            versionHistory.VersionSiteBindingIDs = null;

            // For site object version site bindings are null and site ID is supplied to RestoreObjectVersion 
            if (versionHistory.VersionObjectSiteID <= 0)
            {
                // Set site IDs to which object will be restored
                if (siteId > 0)
                {
                    versionHistory.VersionSiteBindingIDs = siteId.ToString();
                }

                // Set site ID to 0 to ensure that only site bindings will be processed
                siteId = 0;
            }

            return RestoreObjectVersion(versionHistory, true, siteId);
        }


        /// <summary>
        /// Restores object, optionally with its children.
        /// </summary>
        /// <param name="versionId">Version ID</param>
        /// <param name="procesChilds">Indicates if also child object should be restored</param>
        public static GeneralizedInfo RestoreObject(int versionId, bool procesChilds)
        {
            // Get version history
            ObjectVersionHistoryInfo versionHistory = ObjectVersionHistoryInfoProvider.GetVersionHistoryInfo(versionId);
            if (versionHistory == null)
            {
                throw new Exception("[ObjectVersionManager.GetVersion]: History version data not found.");
            }

            return RestoreObjectVersion(versionHistory, procesChilds, 0);
        }


        /// <summary>
        /// Creates object version.
        /// </summary>
        /// <param name="infoObj">IInfo object instance</param>
        /// <param name="userId">User that caused creation of object version</param>
        /// <param name="forceVersion">Indicates if new version should be created instead of storing to existing one if possible</param>
        public static ObjectVersionHistoryInfo CreateVersion(GeneralizedInfo infoObj, int userId, bool forceVersion = false)
        {
            if (CMSActionContext.CurrentUser == null)
            {
                throw new Exception("[ObjectVersionManager.CreateVersion]: Missing context user.");
            }

            string oldVersionNumber = null;
            ObjectVersionHistoryInfo versionHistory = null;
            ObjectVersionHistoryInfo latestVersionHistory = GetLatestVersion(infoObj.TypeInfo.ObjectType, infoObj.ObjectID);
            bool versionCreated = false;

            // Check if previous version exists
            if (latestVersionHistory != null)
            {
                if (!forceVersion)
                {
                    // If object not checked out, check if conditions to promote to major version are met
                    if (!infoObj.IsCheckedOut)
                    {
                        int promoteToMajorInterval = GetPromoteToMajorInterval(infoObj.ObjectSiteName);
                        if ((promoteToMajorInterval > 0) && (promoteToMajorInterval <= (DateTime.Now - latestVersionHistory.VersionModifiedWhen).TotalHours))
                        {
                            MakeVersionMajor(latestVersionHistory);
                        }
                    }

                    int useLastVersionInterval = GetUseLastVersionInterval(infoObj.ObjectSiteName);

                    // Object is checked out or modification time interval is set and is lower then last object modification, save the changes into the latest version
                    if (infoObj.IsCheckedOut || (!IsVersionMajor(latestVersionHistory) && (useLastVersionInterval > 0) && (useLastVersionInterval > (DateTime.Now - latestVersionHistory.VersionModifiedWhen).TotalMinutes)))
                    {
                        versionHistory = latestVersionHistory;
                    }
                }

                oldVersionNumber = latestVersionHistory.VersionNumber;
            }

            if (versionHistory == null)
            {
                versionHistory = new ObjectVersionHistoryInfo();
                versionHistory.VersionNumber = GetNewVersionNumber(oldVersionNumber, false);
                versionCreated = true;
            }

            try
            {
                // Fill object version with data
                versionHistory.VersionObjectID = infoObj.ObjectID;
                versionHistory.VersionObjectType = infoObj.TypeInfo.ObjectType;
                versionHistory.VersionModifiedByUserID = userId;
                versionHistory.VersionModifiedWhen = DateTime.Now;
                UpdateVersionHistoryData(versionHistory, infoObj);

                ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);

                // Delete older versions to keep history length in case new version was created
                if (versionCreated)
                {
                    DeleteOlderVersions(infoObj.TypeInfo.ObjectType, infoObj.ObjectID, infoObj.ObjectSiteName);
                }

                return versionHistory;
            }
            catch (Exception ex)
            {
                string objType = ResHelper.GetString("Objecttype." + infoObj.TypeInfo.ObjectType.Replace(".", "_"));
                EventLogProvider.LogException(objType, "CreateVersion", ex);

                throw;
            }
        }


        /// <summary>
        /// Creates object version for deleted object.
        /// </summary>
        /// <param name="infoObj">IInfo object instance</param>
        /// <param name="getOriginalData">Indicates if original object data should be obtained from database</param>
        public static ObjectVersionHistoryInfo EnsureVersion(GeneralizedInfo infoObj, bool getOriginalData = false)
        {
            if (CMSActionContext.CurrentUser == null)
            {
                throw new Exception("[ObjectVersionManager.EnsureVersion]: Missing context user.");
            }

            ObjectVersionHistoryInfo versionHistory = GetLatestVersion(infoObj.TypeInfo.ObjectType, infoObj.ObjectID);
            if (versionHistory != null)
            {
                return versionHistory;
            }

            if (getOriginalData)
            {
                infoObj = infoObj.GetExisting();
                if ((infoObj == null) || !SynchronizationHelper.CheckCreateVersion(infoObj, TaskTypeEnum.UpdateObject))
                {
                    return null;
                }
            }

            versionHistory = CreateVersion(infoObj, CMSActionContext.CurrentUser.UserID, true);
            if (versionHistory != null)
            {
                // If there is no queued worker for version creation already, version is major
                bool major = !RequestStockHelper.Contains(CREATE_VERSION_STORAGE, infoObj.GetObjectKey());

                versionHistory.VersionNumber = GetNewVersionNumber(null, major);

                ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);
            }

            return versionHistory;
        }


        /// <summary>
        /// Ensures object version.
        /// </summary>
        /// <param name="infoObj">IInfo object instance</param>
        public static ObjectVersionHistoryInfo EnsureDeletedVersion(GeneralizedInfo infoObj)
        {
            if (CMSActionContext.CurrentUser == null)
            {
                throw new Exception("[ObjectVersionManager.EnsureVersion]: Missing context user.");
            }

            ObjectVersionHistoryInfo versionHistory = GetLatestVersion(infoObj.TypeInfo.ObjectType, infoObj.ObjectID);

            // Object don't have version or is versioned and versioning is turned off
            if ((versionHistory == null) || (!AllowObjectVersioning(infoObj) && infoObj.TypeInfo.SupportsVersioning))
            {
                versionHistory = CreateVersion(infoObj, CMSActionContext.CurrentUser.UserID, true);
            }
            else
            {
                // Ensure actual data for recycle bin versions of versioned objects when versioning is turned on
                UpdateVersionHistoryData(versionHistory, infoObj);
            }

            if (versionHistory != null)
            {
                // For objects with site binding include site IDs to the version
                GeneralizedInfo siteBinding = infoObj.TypeInfo.SiteBindingObject;
                if (siteBinding != null)
                {
                    var bindingTypeInfo = siteBinding.TypeInfo;

                    var ds = siteBinding.GetData(null, bindingTypeInfo.ParentIDColumn + "=" + infoObj.ObjectID, null, 0, bindingTypeInfo.SiteIDColumn, false);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        IList<string> siteIds = DataHelper.GetStringValues(ds.Tables[0], bindingTypeInfo.SiteIDColumn);
                        versionHistory.VersionSiteBindingIDs = siteIds.Join(";");
                    }
                }

                // Set deleted properties
                versionHistory.VersionDeletedByUserID = CMSActionContext.CurrentUser.UserID;
                versionHistory.VersionDeletedWhen = DateTime.Now;

                // Update in database
                ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);
            }

            return versionHistory;
        }


        /// <summary>
        /// Determines whether the file with the specified extension (case insensitive) is versioned on site specified by name.
        /// </summary>
        /// <param name="objectType">Object type to check versioned extensions for</param>
        /// <param name="extension">File extension to check.</param>
        /// <param name="siteName">Site name on which is media file located</param>
        public static bool IsObjectExtensionVersioned(string objectType, string extension, string siteName)
        {
            extension = extension.Trim().TrimStart('.');

            // Get the versioned extensions from the settings
            string globalExtensions = GetObjectVersionedExtensions(objectType, siteName).Trim();

            // No extensions specified - no extensions versioned by default
            if (String.IsNullOrEmpty(globalExtensions))
            {
                return false;
            }
            else
            {
                globalExtensions = ";" + globalExtensions + ";";
                return (globalExtensions.IndexOfCSafe(";" + extension + ";", true) >= 0);
            }
        }


        /// <summary>
        /// Returns allowed extensions list from settings.
        /// </summary>
        /// <param name="objectType">Object type to check versioned extensions for</param>
        /// <param name="siteName">Name of the site</param>
        public static string GetObjectVersionedExtensions(string objectType, string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSVersioningExtensions" + objectType.Replace(".", ""));
        }


        /// <summary>
        /// Indicates if the object versioning is allowed for specified object under particular site.
        /// </summary>
        /// <param name="infoObj">IInfo object instance</param>
        public static bool AllowObjectVersioning(GeneralizedInfo infoObj)
        {
            return infoObj.SupportsVersioning && infoObj.VersioningEnabled;
        }


        /// <summary>
        /// Indicates if the object under specified site can be restored from recycle bin.
        /// </summary>
        /// <param name="infoObj">IInfo object instance</param>
        public static bool AllowObjectRestore(GeneralizedInfo infoObj)
        {
            // Create version only for allowed objects
            if (infoObj.AllowRestore)
            {
                string restoreType = SettingsKeyInfoProvider.GetValue(infoObj.ObjectSiteName + ".CMSRestoreObjects");
                switch (restoreType)
                {
                    // Do not create version for recycle bin
                    case RESTORE_NONE:
                        return false;

                    // Create recycle bin version for versioned objects only
                    case RESTORE_VERSIONEDOBJECTS:
                        return AllowObjectVersioning(infoObj);

                    // Create recycle bin version for all objects
                    case RESTORE_ALL:
                        return CMSActionContext.CurrentCreateVersion;
                }
            }
            return false;
        }


        /// <summary>
        /// Indicates if object has at least one version.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <returns>True if specified object has at least one version,False otherwise</returns>
        public static bool ObjectHasVersions(string objectType, int objectId)
        {
            DataSet dsVersion = GetObjectHistory(objectType, objectId, null, null, 1, "VersionID");
            return !DataHelper.DataSourceIsEmpty(dsVersion);
        }


        /// <summary>
        /// Gets new version number according to previous version number.
        /// </summary>
        /// <param name="oldVersionNumber">Old version number</param>
        /// <param name="isMajorVersion">If true, the version number is considered major</param>
        /// <returns>New version number</returns>
        public static string GetNewVersionNumber(string oldVersionNumber, bool isMajorVersion)
        {
            return ProviderObject.GetNewVersionNumberInternal(oldVersionNumber, isMajorVersion);
        }


        /// <summary>
        /// Gets object version history length.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int GetMinorVersionHistoryLength(string siteName)
        {
            return ValidationHelper.GetInteger(SettingsKeyInfoProvider.GetValue(siteName + ".CMSObjectVersionHistoryLength"), 50);
        }


        /// <summary>
        /// Gets object major version history length.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int GetMajorVersionHistoryLength(string siteName)
        {
            return ValidationHelper.GetInteger(SettingsKeyInfoProvider.GetValue(siteName + ".CMSObjectVersionHistoryMajorVersionsLength"), 25);
        }


        /// <summary>
        /// Gets time interval for which last version will be used to store version data
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int GetUseLastVersionInterval(string siteName)
        {
            return ValidationHelper.GetInteger(SettingsKeyInfoProvider.GetValue(siteName + ".CMSObjectVersionHistoryUseLastVersionInterval"), 5);
        }


        /// <summary>
        /// Gets time interval between 2 modifications of object for which last version will be promoted to major version
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int GetPromoteToMajorInterval(string siteName)
        {
            return ValidationHelper.GetInteger(SettingsKeyInfoProvider.GetValue(siteName + ".CMSObjectVersionHistoryPromoteToMajorTimeInterval"), 12);
        }


        /// <summary>
        /// Make major version from specified version
        /// </summary>
        /// <param name="version">Version to be changed to major</param>
        /// <returns>Returns true if new version is created</returns>
        public static bool MakeVersionMajor(ObjectVersionHistoryInfo version)
        {
            if ((version != null) && !IsVersionMajor(version))
            {
                // Make version major
                version.VersionNumber = GetNewVersionNumber(version.VersionNumber, true);
                ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                // Delete older versions
                DeleteOlderVersions(version.VersionObjectType, version.VersionObjectID, ProviderHelper.GetCodeName(PredefinedObjectType.SITE, version.VersionObjectSiteID));

                return true;
            }

            return false;
        }


        /// <summary>
        /// Indicates if specified version is major version
        /// </summary>
        /// <param name="version">Version to be checked</param>
        public static bool IsVersionMajor(ObjectVersionHistoryInfo version)
        {
            if (version != null)
            {
                string[] verNumbers = version.VersionNumber.Split('.');
                return ((verNumbers.Length >= 2) && (verNumbers[1] == "0"));
            }

            return false;
        }


        /// <summary>
        /// Indicates if versions tab should be displayed for specified object type
        /// </summary>
        /// <param name="infoObj">Object to display versions tab for</param>
        public static bool DisplayVersionsTab(GeneralizedInfo infoObj)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                if (!LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning))
                {
                    return false;
                }
            }

            return ((infoObj.SupportsVersioning && AllowObjectVersioning(infoObj)) || ObjectHasVersions(infoObj.TypeInfo.ObjectType, infoObj.ObjectID));
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets latest version history for given object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        protected virtual ObjectVersionHistoryInfo GetLatestVersionInternal(string objectType, int objectId)
        {
            string where = GetWhereCondition(objectType, objectId);

            // Look for the permissions in cache
            DataSet dsVersion = (DataSet)RequestStockHelper.GetItem("ObjectVersionHistory_" + objectType + "_" + objectId);
            if (dsVersion == null)
            {
                dsVersion = ObjectVersionHistoryInfoProvider.GetVersionHistories().Where(where).OrderByDescending("VersionID").TopN(1);
            }

            if (!DataHelper.DataSourceIsEmpty(dsVersion))
            {
                return new ObjectVersionHistoryInfo(dsVersion.Tables[0].Rows[0]);
            }
            return null;
        }


        /// <summary>
        /// Deletes older object versions.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="minorVersionHistoryLength">Minor versions history length to ensure</param>
        /// <param name="majorVersionHistoryLength">Major versions history length to ensure</param>
        protected virtual void DeleteOlderVersionsInternal(string objectType, int objectId, int majorVersionHistoryLength, int minorVersionHistoryLength)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ObjectID", objectId);
            parameters.Add("@ObjectType", objectType);

            if ((majorVersionHistoryLength > 0) || (minorVersionHistoryLength > 0))
            {
                // Get major version history
                DataSet objVersionHistory = GetObjectHistory(objectType, objectId, "VersionNumber LIKE N'%.0'", null, -1, "COUNT(VersionNumber)");
                int historyLength = ValidationHelper.GetInteger(objVersionHistory.Tables[0].Rows[0][0], 0);

                if (!DataHelper.DataSourceIsEmpty(objVersionHistory))
                {
                    // Delete major version history
                    if ((majorVersionHistoryLength > 0) && (historyLength > majorVersionHistoryLength))
                    {
                        ConnectionHelper.ExecuteQuery("CMS.ObjectVersionHistory.deleteoldermajorversions", parameters, null, null, majorVersionHistoryLength);
                    }
                }

                // Get minor version history
                objVersionHistory = GetObjectHistory(objectType, objectId, "VersionNumber NOT LIKE N'%.0'", null, -1, "COUNT(VersionNumber)");
                historyLength = ValidationHelper.GetInteger(objVersionHistory.Tables[0].Rows[0][0], 0);

                if (!DataHelper.DataSourceIsEmpty(objVersionHistory))
                {
                    // Delete minor version history
                    if ((minorVersionHistoryLength > 0) && (historyLength > minorVersionHistoryLength))
                    {
                        ConnectionHelper.ExecuteQuery("CMS.ObjectVersionHistory.deleteolderminorversions", parameters, null, null, minorVersionHistoryLength);
                    }
                }
            }
        }


        /// <summary>
        /// Gets object version histories.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        protected virtual DataSet GetObjectHistoryInternal(string objectType, int objectId, string where, string orderBy, int topN, string columns)
        {
            // Prepare where condition
            string objWhere = GetWhereCondition(objectType, objectId);
            where = SqlHelper.AddWhereCondition(where, objWhere);

            return ObjectVersionHistoryInfoProvider.GetVersionHistories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }


        /// <summary>
        /// Changes object ID for the previous object versions during object restore.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="originalVersionId">Original object version ID</param>
        /// <param name="newVersionId">New object version ID</param>
        protected virtual void ChangePreviousVersionsObjectIDInternal(string objectType, int originalVersionId, int newVersionId)
        {
            // Prepare the parameters for the object ID change
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@OriginalObjectID", originalVersionId);
            parameters.Add("@NewObjectID", newVersionId);
            parameters.Add("@ObjectType", objectType);

            // Change object ID for previous versions
            ConnectionHelper.ExecuteQuery("cms.objectversionhistory.changeobjectid", parameters);
        }


        /// <summary>
        /// Gets new version number according to previous version number.
        /// </summary>
        /// <param name="oldVersionNumber">Old version number</param>
        /// <param name="isMajorVersion">If true, the version number is considered major</param>
        /// <returns>New version number</returns>
        protected virtual string GetNewVersionNumberInternal(string oldVersionNumber, bool isMajorVersion)
        {
            string currentVersionNumber = oldVersionNumber;

            if (string.IsNullOrEmpty(currentVersionNumber))
            {
                // First version number, assign the version 1
                if (isMajorVersion)
                {
                    return "1.0";
                }
                else
                {
                    return "0.1";
                }
            }
            else
            {
                // Number already exists, get new version number
                try
                {
                    string[] numbers = currentVersionNumber.Split('.');
                    // Get current version numbers
                    int majorVersion = ValidationHelper.GetInteger(numbers[0], 0);
                    int minorVersion = ValidationHelper.GetInteger(numbers[1], 0);

                    // Increase version number
                    if (isMajorVersion)
                    {
                        majorVersion += 1;
                        minorVersion = 0;
                    }
                    else
                    {
                        minorVersion += 1;
                    }
                    return majorVersion + "." + minorVersion;
                }
                catch
                {
                    return oldVersionNumber;
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Process versioning synchronization task of object with given parameters.
        /// </summary>
        /// <param name="objectType">Type of object associated with task</param>
        /// <param name="data">Data of object associated with this task</param>
        /// <param name="binaryData">Binary data of object associated with this task</param>
        /// <param name="processChilds">Whether to process childs of object associated with this task</param>
        /// <param name="siteId">Site ID of object associated with this task</param>
        /// <returns>Resulting <see cref="ICMSObject"/>.></returns>
        private static object ProcessVersioningSynchronizationTask(string objectType, string data, string binaryData, bool processChilds, int siteId)
        {
            var siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);
            var userName = CMSActionContext.CurrentUser?.UserName;
            var taskGroups = SynchronizationActionContext.CurrentTaskGroups.Select(g => g.TaskGroupCodeName);

            return ModuleCommands.SynchronizationProcessTask(OperationTypeEnum.Versioning, TaskTypeEnum.UpdateObject, objectType, data, binaryData, processChilds, siteName, userName, taskGroups);
        }


        /// <summary>
        /// Corrects object ID in the version's object data. If the object was restored from recycle bin before and has new ID, this method ensures that VersionXML will contain current VersionObjectID.
        /// </summary>
        /// <param name="versionHistory">Object version history</param>
        private static ObjectVersionHistoryInfo FixVersionXML(ObjectVersionHistoryInfo versionHistory)
        {
            var settings = new SynchronizationObjectSettings
            {
                Operation = OperationTypeEnum.Versioning
            };

            DataSet dsObject = DataHelper.GetDataSetFromXml(versionHistory.VersionXML);
            BaseInfo infoObj = ModuleManager.GetObject(dsObject.Tables[0].Rows[0], versionHistory.VersionObjectType);

            if ((infoObj != null) && (infoObj.GetIntegerValue(infoObj.TypeInfo.IDColumn, 0) != versionHistory.VersionObjectID))
            {
                infoObj.SetValue(infoObj.TypeInfo.IDColumn, versionHistory.VersionObjectID);
                versionHistory.VersionXML = SynchronizationHelper.GetObjectXml(settings, infoObj, TaskTypeEnum.UpdateObject);
            }

            return versionHistory;
        }


        /// <summary>
        /// Gets object where condition used in queries for object history.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <returns>Object where condition</returns>
        private static string GetWhereCondition(string objectType, int objectId)
        {
            objectType = ValidationHelper.GetString(objectType, String.Empty);
            string where = "VersionObjectType = N'" + SqlHelper.EscapeQuotes(objectType) + "'";
            where = SqlHelper.AddWhereCondition(where, "VersionObjectID = " + objectId);

            return where;
        }


        /// <summary>
        /// Restores object specified by its history, optionally it restores also object children.
        /// </summary>
        /// <param name="versionHistory">Object version history info object</param>
        /// <param name="processChilds">Indicates if also object children should be restored</param>
        /// <param name="siteId">ID of the site to which object should be restored</param>
        /// <returns>Restored object IInfo object</returns>
        private static GeneralizedInfo RestoreObjectVersion(ObjectVersionHistoryInfo versionHistory, bool processChilds, int siteId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning);
            }

            if (versionHistory != null)
            {
                using (new CMSActionContext { EnableLogContext = false })
                {
                    object processedObj;

                    // Version data are updated explicitly, disable creating version here
                    using (new CMSActionContext { CreateVersion = false })
                    {
                        var useSiteId = ((versionHistory.VersionObjectSiteID > 0) && (siteId > 0)) ? siteId : versionHistory.VersionObjectSiteID;
                        
                        processedObj = ProcessVersioningSynchronizationTask(versionHistory.VersionObjectType, versionHistory.VersionXML, versionHistory.VersionBinaryDataXML, processChilds, useSiteId);
                    }

                    if (processedObj is BaseInfo)
                    {
                        BaseInfo infoObj = (BaseInfo)processedObj;

                        // Update site bindings
                        var ti = infoObj.TypeInfo;

                        if ((infoObj.Generalized.ObjectSiteID <= 0) && (ti.SiteBindingObject != null))
                        {
                            // Ensure object site bindings
                            if (!String.IsNullOrEmpty(versionHistory.VersionSiteBindingIDs))
                            {
                                string[] siteIds = versionHistory.VersionSiteBindingIDs.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string sId in siteIds)
                                {
                                    int targetSiteId = ValidationHelper.GetInteger(sId, 0);

                                    string targetSiteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, targetSiteId);
                                    if (!string.IsNullOrEmpty(targetSiteName))
                                    {
                                        // Use clon not to modify cached TypeInfo.SiteBindingObject
                                        var bindingObj = ti.SiteBindingObject.Generalized.Clone();

                                        // Create site binding if the target site is set
                                        var bindingTypeInfo = bindingObj.TypeInfo;

                                        bindingObj.SetValue(bindingTypeInfo.SiteIDColumn, targetSiteId);
                                        bindingObj.SetValue(bindingTypeInfo.ParentIDColumn, infoObj.Generalized.ObjectID);

                                        // Update / insert the site binding
                                        bindingObj.Generalized.SetObject();
                                    }
                                }
                            }
                        }


                        // Remove recycle bin version
                        if (AllowObjectVersioning(infoObj))
                        {
                            // Remove rec. bin flags
                            versionHistory.VersionDeletedByUserID = 0;
                            versionHistory.SetValue("VersionDeletedWhen", null);

                            // Ensure actual data 
                            UpdateVersionHistoryData(versionHistory, infoObj);
                            ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);
                        }
                        else
                        {
                            DestroyObjectVersion(versionHistory.VersionID);
                        }

                        // Change object ID for previous versions
                        ProviderObject.ChangePreviousVersionsObjectIDInternal(ti.ObjectType, versionHistory.VersionObjectID, infoObj.Generalized.ObjectID);

                        return infoObj;
                    }

                    throw new Exception("[ObjectVersionManager.RestoreObjectVersion]: Processed object is not BaseInfo.");
                }
            }
            return null;
        }


        /// <summary>
        /// Ensure object actual data in version history
        /// </summary>
        /// <param name="versionHistory">Version history object to update</param>
        /// <param name="infoObj">Object with actual data</param>
        private static void UpdateVersionHistoryData(ObjectVersionHistoryInfo versionHistory, BaseInfo infoObj)
        {
            // Ensure actual data 
            var settings = new SynchronizationObjectSettings();

            settings.Operation = OperationTypeEnum.Versioning;

            versionHistory.VersionXML = SynchronizationHelper.GetObjectXml(settings, infoObj, TaskTypeEnum.UpdateObject);
            versionHistory.VersionBinaryDataXML = SynchronizationHelper.GetObjectBinaryXml(OperationTypeEnum.Versioning, infoObj.TypeInfo.ObjectType, infoObj.Generalized.ObjectID, TaskTypeEnum.UpdateObject);
            versionHistory.VersionObjectSiteID = infoObj.Generalized.ObjectSiteID;
            versionHistory.VersionObjectDisplayName = infoObj.Generalized.ObjectDisplayName;
        }

        #endregion


        #region "Locking methods"

        /// <summary>
        /// Checks out the object to particular user
        /// </summary>
        /// <param name="infoObj">Object to check out</param>
        /// <param name="user">User who is checking out the object</param>
        public void CheckOut(BaseInfo infoObj, IUserInfo user)
        {
            using (var transaction = new CMSTransactionScope())
            {
                infoObj.Generalized.UpdateFromDB(false);

                var settings = infoObj.ObjectSettings;

                if (infoObj.Generalized.IsCheckedOut)
                {
                    // Already checked out
                    throw new ObjectVersioningException(settings, ComponentEvents.CHECKOUT);
                }

                // Mark as checked out
                settings.ObjectCheckedOutByUserID = user.UserID;
                settings.ObjectCheckedOutWhen = DateTime.Now;

                if (infoObj.Generalized.SupportsVersioning && infoObj.Generalized.VersioningEnabled)
                {
                    // Ensure the version in version history (needs to be here to ensure the version to which we can roll back in case of undo checkout)
                    EnsureVersion(infoObj);

                    // Create new version which will be modified during the time when the object is checked out
                    var versionInfo = CreateVersion(infoObj, user.UserID, true);
                    if (versionInfo != null)
                    {
                        // Set the version number to the checked out version ID
                        settings.ObjectCheckedOutVersionHistoryID = versionInfo.VersionID;
                    }
                }

                settings.Update();
                transaction.Commit();
            }
        }


        /// <summary>
        /// Cancels the object checkout.
        /// </summary>
        /// <param name="infoObj">Object to check out</param>
        public void UndoCheckOut(BaseInfo infoObj)
        {
            using (var transaction = new CMSTransactionScope())
            {
                GeneralizedInfo genInfo = infoObj;

                // Make sure we have latest check-out info available
                genInfo.UpdateFromDB(false);

                var settings = infoObj.ObjectSettings;

                if (!genInfo.IsCheckedOut || !UserCanPerformUndoCheckout(settings, CMSActionContext.CurrentUser))
                {
                    throw new ObjectVersioningException(settings, ComponentEvents.UNDO_CHECKOUT);
                }

                // Store the history ID to rollback to
                int oldVersionHistoryId = settings.ObjectCheckedOutVersionHistoryID;

                // Mark as checked in
                settings.ObjectCheckedOutByUserID = 0;
                settings.ObjectCheckedOutWhen = DateTimeHelper.ZERO_TIME;
                settings.ObjectCheckedOutVersionHistoryID = 0;
                settings.Update();

                if (infoObj.TypeInfo.SupportsVersioning && infoObj.Generalized.VersioningEnabled)
                {
                    // Delete the checkout version
                    DestroyObjectVersion(oldVersionHistoryId);

                    // Rollback to previous version
                    var latestVersion = GetLatestVersion(infoObj.TypeInfo.ObjectType, infoObj.Generalized.ObjectID);
                    if (latestVersion != null)
                    {
                        RollbackVersion(latestVersion.VersionID, false, false);
                    }
                }

                transaction.Commit();
            }

            // Make sure the object data is properly updated from rollback version
            if (infoObj.TypeInfo.SupportsVersioning)
            {
                infoObj.Generalized.UpdateFromDB(false);
            }
        }


        private static bool UserCanPerformUndoCheckout(ObjectSettingsInfo settings, IUserInfo currentUser)
        {
            return settings.ObjectCheckedOutByUserID == currentUser.UserID || currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin);
        }


        /// <summary>
        /// Checks in the object.
        /// </summary>
        /// <param name="infoObj">Object to check in</param>
        /// <param name="versionNumber">Version number, optional. Is assigned automatically when not specified.</param>
        /// <param name="comment">Version comment, optional</param>
        /// <exception cref="ObjectVersioningException">Throws an exception if it is not possible to check in the object.</exception>
        public void CheckIn(BaseInfo infoObj, string versionNumber = null, string comment = null)
        {
            using (var transaction = new CMSTransactionScope())
            {
                GeneralizedInfo genInfo = infoObj;
                genInfo.UpdateFromDB(false);

                var settings = infoObj.ObjectSettings;

                if (!genInfo.IsCheckedOut || settings.ObjectCheckedOutByUserID != CMSActionContext.CurrentUser.UserID)
                {
                    throw new ObjectVersioningException(settings, ComponentEvents.CHECKIN);
                }

                // Mark as checked in
                settings.ObjectCheckedOutByUserID = 0;
                settings.ObjectCheckedOutWhen = DateTimeHelper.ZERO_TIME;
                settings.ObjectCheckedOutVersionHistoryID = 0;

                if (infoObj.TypeInfo.SupportsVersioning && infoObj.Generalized.VersioningEnabled)
                {
                    if (!String.IsNullOrEmpty(versionNumber) || (comment != null))
                    {
                        // Ensure the version in version history
                        var versionInfo = EnsureVersion(genInfo);
                        if (versionInfo != null)
                        {
                            // Update the version number and comment
                            if (!String.IsNullOrEmpty(versionNumber))
                            {
                                versionInfo.VersionNumber = versionNumber;
                            }
                            if (comment != null)
                            {
                                versionInfo.VersionComment = comment;
                            }

                            versionInfo.Update();
                        }
                    }
                }

                settings.Update();
                transaction.Commit();
            }
        }

        #endregion
    }
}