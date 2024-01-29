using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.WebServices
{
    /// <summary>
    /// REST service to access and manage CMS data
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RESTService : BaseRESTService, IRESTService
    {
        #region "Properties"

        /// <summary>
        /// Returns the outgoing response
        /// </summary>
        private static OutgoingWebResponseContext OutgoingResponse
        {
            get
            {
                var context = WebOperationContext.Current;
                return context?.OutgoingResponse;
            }
        }

        #endregion


        #region "Objects methods"

        #region "Single object retrieval"

        /// <summary>
        /// Returns object of given type with specified ID. If ID is not integer, than it's considered object name and 
        /// object from current site with given name is returned.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        public Stream GetObject(string objectType, string id)
        {
            TraverseObjectSettings settings = GetExportSettings("data");
            GeneralizedInfo info = GetObjectByIDInternal(objectType, id, settings.Binary);

            // Check security (we can check it after it has been retrieved from DB, because we don't know whether to check site/global permissions before
            if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
            {
                return ReturnForbiddenStatus();
            }

            string objType = (info == null ? "" : info.TypeInfo.ObjectType);
            return GetStream(info, objType, settings);
        }


        /// <summary>
        /// Returns object of given type with given name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="objectName">Code name of the object</param>
        public Stream GetSiteObject(string objectType, string siteName, string objectName)
        {
            TraverseObjectSettings settings = GetExportSettings("data");
            GeneralizedInfo info = GetObjectByNameInternal(objectType, siteName, objectName, settings.Binary);

            // Check security (we can check it after it has been retrieved from DB, because we don't know whether to check site/global permissions before
            if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
            {
                return ReturnForbiddenStatus();
            }

            // Check whether the provided site name makes sense
            if (!String.IsNullOrEmpty(siteName) && (SiteInfoProvider.GetSiteID(siteName) == 0))
            {
                return ReturnSiteNotFoundStatus();
            }

            string objType = (info == null ? "" : info.TypeInfo.ObjectType);
            return GetStream(info, objType, settings);
        }


        /// <summary>
        /// Returns global object of given type with given name.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        public Stream GetGlobalObject(string objectType, string objectName)
        {
            TraverseObjectSettings settings = GetExportSettings("data");
            GeneralizedInfo info = GetObjectByNameInternal(objectType, null, objectName, settings.Binary);

            // Check security (we can check it after it has been retrieved from DB, because we don't know whether to check site/global permissions before
            if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
            {
                return ReturnForbiddenStatus();
            }

            string objType = (info == null ? "" : info.TypeInfo.ObjectType);
            return GetStream(info, objType, settings);
        }


        /// <summary>
        /// Returns object of given type with given name from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        public Stream GetCurrentSiteObject(string objectType, string objectName)
        {
            TraverseObjectSettings settings = GetExportSettings("data");
            GeneralizedInfo info = GetObjectByNameInternal(objectType, CurrentSiteName, objectName, settings.Binary);

            // Check security (we can check it after it has been retrieved from DB, because we don't know whether to check site/global permissions before
            if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
            {
                return ReturnForbiddenStatus();
            }

            string objType = (info == null ? "" : info.TypeInfo.ObjectType);
            return GetStream(info, objType, settings);
        }

        #endregion


        #region "Multiple objects retrieval"

        /// <summary>
        /// Returns list of objects of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object(s)</param>
        public Stream GetObjects(string objectType)
        {
            return GetSiteObjects(objectType, CurrentSiteName);
        }


        /// <summary>
        /// Returns objects of given type from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        public Stream GetSiteObjects(string objectType, string siteName)
        {
            // Check security
            if (!IsAuthorizedForObject(objectType, siteName, PermissionsEnum.Read))
            {
                return ReturnForbiddenStatus();
            }

            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            if (typeInfo != null)
            {
                TraverseObjectSettings settings = GetExportSettings("data");

                int siteId = SiteInfoProvider.GetSiteID(siteName);
                if (siteId == 0)
                {
                    return ReturnSiteNotFoundStatus();
                }

                settings.WhereCondition.Where(typeInfo.GetSiteWhereCondition(siteId, false));

                return GetStream(GetObjects(objectType, settings), objectType, settings);
            }

            // Object not found
            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Returns global objects of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        public Stream GetGlobalObjects(string objectType)
        {
            // Check security
            if (!IsAuthorizedForObject(objectType, null, PermissionsEnum.Read))
            {
                return ReturnForbiddenStatus();
            }

            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            if (typeInfo != null)
            {
                var settings = GetExportSettings("data");

                if (typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    settings.WhereCondition.WhereNull(typeInfo.SiteIDColumn);
                }

                return GetStream(GetObjects(objectType, settings), objectType, settings);
            }

            // Object not found
            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Returns objects of given type from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        public Stream GetCurrentSiteObjects(string objectType)
        {
            // Check security
            if (!IsAuthorizedForObject(objectType, CurrentSiteName, PermissionsEnum.Read))
            {
                return ReturnForbiddenStatus();
            }

            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            if (typeInfo != null)
            {
                TraverseObjectSettings settings = GetExportSettings("data");

                var siteId = SiteInfoProvider.GetSiteID(CurrentSiteName);

                settings.WhereCondition.Where(typeInfo.GetSiteWhereCondition(siteId, false));

                return GetStream(GetObjects(objectType, settings), objectType, settings);
            }

            // Object not found
            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Returns all objects of given object type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        public Stream GetAllObjects(string objectType)
        {
            // Check security
            if ((CurrentUser == null) || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return ReturnForbiddenStatus();
            }

            if (!IsAuthorizedForObject(objectType, CurrentSiteName, PermissionsEnum.Read))
            {
                return ReturnForbiddenStatus();
            }

            TraverseObjectSettings settings = GetExportSettings("data");
            return GetStream(GetObjects(objectType, settings), objectType, settings);
        }


        /// <summary>
        /// Returns list of child objects of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="childObjectType">Object type of children</param>
        public Stream GetChildren(string objectType, string id, string childObjectType)
        {
            // Get the object site so we can check proper security settings
            GeneralizedInfo parentObj = GetObjectByIDInternal(objectType, id);
            if (parentObj != null)
            {
                // Check security - user must be allowed for parent object and for child object type
                string siteName = parentObj.ObjectSiteName;
                if (!IsAuthorizedForObject(parentObj, PermissionsEnum.Read, objectType) || !IsAuthorizedForObject(childObjectType, siteName, PermissionsEnum.Read))
                {
                    return ReturnForbiddenStatus();
                }

                GeneralizedInfo obj = ModuleManager.GetReadOnlyObject(childObjectType);
                if (obj != null)
                {
                    TraverseObjectSettings settings = GetExportSettings("data");

                    settings.WhereCondition.WhereEquals(obj.TypeInfo.ParentIDColumn, ValidationHelper.GetInteger(id, 0));

                    return GetStream(GetObjects(childObjectType, settings), childObjectType, settings);
                }
            }

            // If object does not exist and user is not allowed for given objectType, return Forbidden to prevent enumeration of existing objects
            if (!IsAuthorizedForObject(objectType, String.Empty, PermissionsEnum.Read))
            {
                return ReturnForbiddenStatus();
            }

            // Object not found
            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Returns list of binding objects of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="bindingObjectType">Object type of bindings</param>
        public Stream GetBindings(string objectType, string id, string bindingObjectType)
        {
            // Get the object site so we can check proper security settings
            GeneralizedInfo parentObj = GetObjectByIDInternal(objectType, id, false);
            if (parentObj != null)
            {
                // Check security
                string siteName = parentObj.ObjectSiteName;
                if (!IsAuthorizedForObject(objectType, siteName, PermissionsEnum.Read) || !IsAuthorizedForObject(bindingObjectType, siteName, PermissionsEnum.Read))
                {
                    return ReturnForbiddenStatus();
                }

                GeneralizedInfo obj = ModuleManager.GetReadOnlyObject(bindingObjectType);
                if (obj != null)
                {
                    TraverseObjectSettings settings = GetExportSettings("data");

                    settings.WhereCondition.WhereEquals(obj.TypeInfo.ParentIDColumn, ValidationHelper.GetInteger(id, 0));

                    return GetStream(GetObjects(bindingObjectType, settings), bindingObjectType, settings);
                }
            }

            if (!IsAuthorizedForObject(objectType, null, PermissionsEnum.Read))
            {
                return ReturnForbiddenStatus();
            }

            // Object not found
            return ReturnNotFoundStatus();
        }

        #endregion


        #region "Modifying methods"

        /// <summary>
        /// Deletes object of given type satisfying given parameters (WHERE/TOPN).
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        public Stream DeleteObjects(string objectType)
        {
            GeneralizedInfo info = ModuleManager.GetObject(objectType);
            if (info != null)
            {
                // Check security
                if ((CurrentUser == null) || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                {
                    return ReturnForbiddenStatus();
                }

                if (!IsAuthorizedForObject(objectType, CurrentSiteName, PermissionsEnum.Delete))
                {
                    return ReturnForbiddenStatus();
                }

                DataSet ds = GetObjects(objectType, GetExportSettings(null));
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Set the data to the object
                        foreach (string col in info.ColumnNames)
                        {
                            if (ds.Tables[0].Columns.Contains(col))
                            {
                                info.SetValue(col, dr[col]);
                            }
                        }

                        // Delete the object
                        info.DeleteObject();
                    }
                }

            }
            return null;
        }


        /// <summary>
        /// Deletes object of given type with specified ID. If ID is not integer, than it's considered object name and 
        /// object from current site with given name is returned.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        public Stream DeleteObject(string objectType, string id)
        {
            GeneralizedInfo info = GetObjectByIDInternal(objectType, id);
            return DeleteObjectInternal(info, objectType);
        }


        /// <summary>
        /// Deletes object of given type with given name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="objectName">Code name of the object</param>
        public Stream DeleteSiteObject(string objectType, string siteName, string objectName)
        {
            GeneralizedInfo info = GetObjectByNameInternal(objectType, siteName, objectName);
            return DeleteObjectInternal(info, objectType);
        }


        /// <summary>
        /// Deletes global object of given type with given name.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        public Stream DeleteGlobalObject(string objectType, string objectName)
        {
            GeneralizedInfo info = GetObjectByNameInternal(objectType, null, objectName);
            return DeleteObjectInternal(info, objectType);
        }


        /// <summary>
        /// Deletes object of given type with given name from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        public Stream DeleteCurrentSiteObject(string objectType, string objectName)
        {
            GeneralizedInfo info = GetObjectByNameInternal(objectType, CurrentSiteName, objectName);
            return DeleteObjectInternal(info, objectType);
        }


        /// <summary>
        /// Updates object of given type with specified ID. If ID is not integer, than it's considered object name and 
        /// object from current site with given name is returned.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="stream">Object data</param>
        public Stream UpdateObject(string objectType, string id, Stream stream)
        {
            GeneralizedInfo info = GetObjectByIDInternal(objectType, id, true);
            return SetObjectInternal(info, objectType, stream, false, null);
        }


        /// <summary>
        /// Updates object of given type with given name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="objectName">Code name of the object</param>
        /// <param name="stream">Object data</param>
        public Stream UpdateSiteObject(string objectType, string siteName, string objectName, Stream stream)
        {
            GeneralizedInfo info = GetObjectByNameInternal(objectType, siteName, objectName, true);

            return SetObjectInternal(info, objectType, stream, false, siteName);
        }


        /// <summary>
        /// Updates global object of given type with given name.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        /// <param name="stream">Object data</param>
        public Stream UpdateGlobalObject(string objectType, string objectName, Stream stream)
        {
            GeneralizedInfo info = GetObjectByNameInternal(objectType, null, objectName, true);
            return SetObjectInternal(info, objectType, stream, false, null);
        }


        /// <summary>
        /// Updates object of given type with given name from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        /// <param name="stream">Object data</param>
        public Stream UpdateCurrentSiteObject(string objectType, string objectName, Stream stream)
        {
            GeneralizedInfo info = GetObjectByNameInternal(objectType, CurrentSiteName, objectName, true);
            return SetObjectInternal(info, objectType, stream, false, null);
        }


        /// <summary>
        /// Creates an object of given type specified by it's parameters.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="stream">Object XML data</param>
        public Stream CreateObject(string objectType, Stream stream)
        {
            GeneralizedInfo info = ModuleManager.GetObject(objectType);
            return SetObjectInternal(info, objectType, stream, true, null);
        }


        /// <summary>
        /// Creates an object of given type specified by it's parameters.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site to which the object will be assigned</param>
        /// <param name="stream">Object XML data</param>
        public Stream CreateSiteObject(string objectType, string siteName, Stream stream)
        {
            GeneralizedInfo info = ModuleManager.GetObject(objectType);
            return SetObjectInternal(info, objectType, stream, true, siteName);
        }


        /// <summary>
        /// Creates an object of given type specified by it's parameters.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="stream">Object XML data</param>
        public Stream CreateCurrentSiteObject(string objectType, Stream stream)
        {
            GeneralizedInfo info = ModuleManager.GetObject(objectType);
            return SetObjectInternal(info, objectType, stream, true, CurrentSiteName);
        }


        /// <summary>
        /// Creates global object of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="stream">Object XML data</param>
        public Stream CreateGlobalObject(string objectType, Stream stream)
        {
            GeneralizedInfo info = ModuleManager.GetObject(objectType);
            return SetObjectInternal(info, objectType, stream, true, null);
        }

        #endregion

        #endregion


        #region "Service methods"

        /// <summary>
        /// Exposes Service Document (available data).
        /// </summary>
        public AtomPub10ServiceDocumentFormatter GetServiceDocument()
        {
            TraverseObjectSettings settings = GetExportSettings(null);

            // Get correct content type according to a format
            string contentType = GetContentType(settings);

            bool allObjTypes = string.IsNullOrEmpty(AllowedObjectTypes);
            string allowedTypes = ";" + AllowedObjectTypes.ToLowerInvariant() + ";";

            var objectCollection = new List<ResourceCollectionInfo>();

            foreach (string objType in ObjectTypeManager.MainObjectTypes)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(objType);

                // Virtual objects do not have DataClassInfo and therefore objects of their type can not be serialized and served via REST (it is not possible to call ObjectHelper.GetSerializationTableName) - e.g. cms.device
                // Objects without ObjectClassName (e.g. cms.general) can not be serialized as well
                if (typeInfo.IsVirtualObject || typeInfo.ObjectClassName == ObjectTypeInfo.VALUE_UNKNOWN)
                {
                    continue;
                }

                // Check security - user must be allowed for the object type, otherwise skip it
                if (!IsAuthorizedForObject(objType, null, PermissionsEnum.Read))
                {
                    continue;
                }

                if (allObjTypes || allowedTypes.Contains(objType.ToLowerInvariant()))
                {
                    var relativeUrl = $"rest/{objType}?format={settings.Format.ToString().ToLowerInvariant()}";

                    var collection = new ResourceCollectionInfo(new TextSyndicationContent(objType), new Uri(CurrentBaseUri, relativeUrl), null, new [] { contentType });

                    objectCollection.Add(collection);
                }
            }

            // Create the list of workspaces which are supported according to settings
            int serviceTypeEnabled = SettingsKeyInfoProvider.GetIntValue(CurrentSiteName + ".CMSRESTServiceTypeEnabled");
            List<Workspace> workspaces = new List<Workspace>();
            // Include the Objects workspace only if the objectCollection is not empty
            if (((serviceTypeEnabled == 0) || (serviceTypeEnabled == 1)) && objectCollection.Any())
            {
                Workspace objectsWorkspace = new Workspace("Objects", objectCollection);
                workspaces.Add(objectsWorkspace);
            }
            if ((serviceTypeEnabled == 0) || (serviceTypeEnabled == 2))
            {
                Workspace documentsWorkspace = new Workspace("Content", new List<ResourceCollectionInfo> { new ResourceCollectionInfo(new TextSyndicationContent("Documents"), new Uri(CurrentBaseUri, "rest/content/all"), null, new [] { contentType }) });
                workspaces.Add(documentsWorkspace);
            }
            ServiceDocument doc = new ServiceDocument(workspaces);

            return (AtomPub10ServiceDocumentFormatter)doc.GetFormatter();
        }


        /// <summary>
        /// Exposes the list of all sites.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public AtomPub10ServiceDocumentFormatter GetSiteList(string objectType)
        {
            TraverseObjectSettings settings = GetExportSettings(null);

            // Get correct content type according to a format
            string contentType = GetContentType(settings);

            List<ResourceCollectionInfo> objectCollection = new List<ResourceCollectionInfo>();
            DataSet ds = SiteInfoProvider.GetSites().Columns("SiteName");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string siteName = ValidationHelper.GetString(dr[0], "");

                    // Check security - user must be allowed for the object type on listed site, otherwise skip the site
                    if (!IsAuthorizedForObject(objectType, siteName, PermissionsEnum.Read))
                    {
                        continue;
                    }

                    var relativeUrl = $"rest/{objectType}/site/{siteName}?format={settings.Format.ToString().ToLowerInvariant()}";

                    var collection = new ResourceCollectionInfo(new TextSyndicationContent(siteName), new Uri(CurrentBaseUri, relativeUrl), null, new [] { contentType });

                    objectCollection.Add(collection);
                }
            }
            Workspace objectsWorkspace = new Workspace("Sites", objectCollection);

            ServiceDocument doc = new ServiceDocument(new List<Workspace> { objectsWorkspace });

            return (AtomPub10ServiceDocumentFormatter)doc.GetFormatter();
        }


        /// <summary>
        /// Returns list of child object types of given object.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        public AtomPub10ServiceDocumentFormatter GetChildObjectTypes(string objectType, string id)
        {
            TraverseObjectSettings settings = GetExportSettings(null);

            // Get correct content type according to a format
            string contentType = GetContentType(settings);

            var objectCollection = new List<ResourceCollectionInfo>();

            var info = GetObjectByIDInternal(objectType, id);
            if (info != null)
            {
                // Check security - user must be allowed for the object, otherwise return forbidden status
                string siteName = info.ObjectSiteName;
                if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
                {
                    ReturnForbiddenStatus();

                    return null;
                }

                foreach (string type in info.TypeInfo.ChildObjectTypes)
                {
                    // Check security - user must be allowed for the child object type, otherwise skip it
                    if (!IsAuthorizedForObject(type, siteName, PermissionsEnum.Read))
                    {
                        continue;
                    }

                    var relativeUrl = $"rest/{objectType}/{id}/children/{type}?format={settings.Format.ToString().ToLowerInvariant()}";

                    var collection = new ResourceCollectionInfo(new TextSyndicationContent(type), new Uri(CurrentBaseUri, relativeUrl), null, new [] { contentType });

                    objectCollection.Add(collection);
                }
            }

            // If object does not exist and user is not allowed for given objectType, return Forbidden to prevent enumeration of existing objects
            if (!IsAuthorizedForObject(objectType, String.Empty, PermissionsEnum.Read))
            {
                ReturnForbiddenStatus();

                return null;
            }

            Workspace objectsWorkspace = new Workspace("Child object types", objectCollection);

            ServiceDocument doc = new ServiceDocument(new List<Workspace> { objectsWorkspace });

            return (AtomPub10ServiceDocumentFormatter)doc.GetFormatter();
        }


        /// <summary>
        /// Returns list of binding object types of given object.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        public AtomPub10ServiceDocumentFormatter GetBindingsObjectTypes(string objectType, string id)
        {
            TraverseObjectSettings settings = GetExportSettings(null);

            // Get correct content type according to a format
            string contentType = GetContentType(settings);

            var objectCollection = new List<ResourceCollectionInfo>();

            var info = GetObjectByIDInternal(objectType, id);
            if (info == null)
            {
                // If object does not exist and user is not allowed for given objectType, return Forbidden to prevent enumeration of existing objects
                if (!IsAuthorizedForObject(objectType, String.Empty, PermissionsEnum.Read))
                {
                    ReturnForbiddenStatus();

                    return null;
                }
            }
            else
            {
                // Check security - user must be allowed for the object, otherwise return forbidden status
                if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
                {
                    ReturnForbiddenStatus();

                    return null;
                }

                foreach (string type in info.TypeInfo.BindingObjectTypes)
                {
                    var relativeUrl = $"rest/{objectType}/{id}/bindings/{type}?format={settings.Format.ToString().ToLowerInvariant()}";

                    var collection = new ResourceCollectionInfo(new TextSyndicationContent(type), new Uri(CurrentBaseUri, relativeUrl), null, new[] { contentType });

                    objectCollection.Add(collection);
                }
            }

            Workspace objectsWorkspace = new Workspace("Binding object types", objectCollection);

            ServiceDocument doc = new ServiceDocument(new List<Workspace> { objectsWorkspace });

            return (AtomPub10ServiceDocumentFormatter)doc.GetFormatter();
        }


        /// <summary>
        /// Returns content type according to format settings.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        private static string GetContentType(TraverseObjectSettings settings)
        {
            string contentType = "text/xml";
            switch (settings.Format)
            {
                case ExportFormatEnum.JSON:
                    contentType = "application/json";
                    break;

                case ExportFormatEnum.ATOM10:
                    contentType = "application/atom+xml";
                    break;

                case ExportFormatEnum.RSS20:
                    contentType = "application/rss+xml";
                    break;
            }
            return contentType;
        }

        #endregion


        #region "Macro methods"

        /// <summary>
        /// Returns the result of the macro expression.
        /// </summary>
        /// <param name="expression">Data macro expression to evaluate</param>
        public Stream GetMacroResult(string expression)
        {
            // Only global admin can use macro expression queries in REST
            if (!RESTSecurityInvoker.IsHashAuthenticated() && ((CurrentUser == null) || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)))
            {
                return ReturnForbiddenStatus();
            }

            var resolver = MacroResolver.GetInstance();

            // Set current user for resolver
            resolver.SetNamedSourceData("CurrentUser", CurrentUser);

            // Set request data for resolver
            resolver.SetNamedSourceData("querystring", ServiceQueryHelper.Instance);
            resolver.SetNamedSourceData("cookies", ServiceCookieHelper.Instance);

            try
            {
                // Set current site in context for macro resolver
                SiteContext.CurrentSite = CurrentSite;

                var result = resolver.ResolveMacroExpression(expression);
                if (result != null)
                {
                    if (result.Match && (result.Result != null))
                    {
                        var info = result.Result as GeneralizedInfo;
                        if (info != null)
                        {
                            var objectType = info.TypeInfo.ObjectType;
                            // Check security
                            if (!IsAuthorizedForObject(info, PermissionsEnum.Read, objectType))
                            {
                                return ReturnForbiddenStatus();
                            }

                            return GetStream(info, objectType, GetExportSettings("data"));
                        }

                        return GetStream(result.Result, null, GetExportSettings("data"));
                    }
                }
            }
            catch
            {
            }

            // No macro match, set status to not found
            OutgoingResponse.SetStatusAsNotFound();
            return null;
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Returns hash string which can be than used for given URL instead of authentication.
        /// </summary>
        /// <param name="url">URL of which to get the hash (starting with '/rest' prefix)</param>
        public Stream GetHashForURL(string url)
        {
            if (CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                string hash = RESTServiceHelper.GetUrlPathAndQueryHash(url);
                if (!string.IsNullOrEmpty(hash))
                {
                    byte[] result = Encoding.ASCII.GetBytes(hash);
                    return new MemoryStream(result);
                }
            }
            return ReturnForbiddenStatus();
        }
        

        /// <summary>
        /// Validates user name and password and sets security token which will protect application against CSRF.  
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="token">Token.</param>
        public Stream SetToken(string userName, string password, string token)
        {
            if (CurrentUserName == userName)
            {
                UserInfo user = AuthenticationHelper.AuthenticateUser(userName, password, CurrentSiteName, false, AuthenticationSourceEnum.ExternalOrAPI);

                if (user != null)
                {
                    SessionHelper.SetValue(SecurityHelper.SESSION_TOKEN_HEADER, token);
                    OutgoingResponse.StatusCode = HttpStatusCode.OK;
                    return null;
                }
            }

            return ReturnForbiddenStatus();
        }

        #endregion
    }
}