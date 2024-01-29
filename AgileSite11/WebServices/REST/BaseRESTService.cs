using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Collections;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Web;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.WebServices
{
    /// <summary>
    /// Base REST service to provide basic access and management of CMS data
    /// </summary>
    [RESTErrorHandler]
    public class BaseRESTService
    {
        internal const string CULTURE_KEY = "CMSRESTCulture";
        internal const string USER_KEY = "RESTCurrentUser";
        private const string SITE_KEY = "RESTCurrentSite";
        private const string SITE_NAME_KEY = "RESTCurrentSiteName";

        private const string TEXT_CONTENT_TYPE = "text/xml";
        private const string APPLICATION_CONTENT_TYPE = "application/json";
        private const string APPLICATION_ATOM_XML_CONTENT_TYPE = "application/atom+xml";
        private const string APPLICATION_RSS_XML_CONTENT_TYPE = "application/rss+xml";

        internal const string DOMAIN_QUERY_KEY = "domain";
        private const string WHERE_QUERY_KEY = "where";
        private const string ORDERBY_QUERY_KEY = "orderby";
        private const string COLUMNS_QUERY_KEY = "columns";


        #region "Variables"

        private CurrentUserInfo mCurrentUser;
        private SiteInfo mCurrentSite;
        private string mCurrentUserName;
        private string mCurrentSiteName;
        private static Uri mCurrentBaseUri;
        private Encoding mDefaultEncoding;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets current base URI of the REST service
        /// </summary>
        protected static Uri CurrentBaseUri
        {
            get
            {
                if (mCurrentBaseUri == null)
                {
                    // We need to correct the domain, since OperationContext gives not a good domain
                    string currentDomain = GetQueryParam(DOMAIN_QUERY_KEY);
                    string baseUrl = OperationContext.Current.Channel.LocalAddress.Uri.OriginalString;
                    string origDomain = URLHelper.GetDomain(baseUrl).Replace(".", "\\.").Replace("+", "\\+").Replace("*", "\\*").Replace("?", "\\?");
                    string result = Regex.Replace(baseUrl, origDomain, currentDomain, CMSRegex.IgnoreCase);

                    mCurrentBaseUri = new Uri(result);
                }

                return mCurrentBaseUri;
            }
        }


        /// <summary>
        /// Gets current site name (retrieved from domain accessed).
        /// </summary>
        protected string CurrentSiteName
        {
            get
            {
                return mCurrentSiteName ?? (mCurrentSiteName = GetCurrentSiteName());
            }
        }


        /// <summary>
        /// Gets current site object received from the domain.
        /// </summary>
        protected SiteInfo CurrentSite
        {
            get
            {
                return mCurrentSite ?? (mCurrentSite = GetCurrentSite());
            }
        }


        /// <summary>
        /// Gets current user name received from auth cookie.
        /// </summary>
        protected string CurrentUserName
        {
            get
            {
                return mCurrentUserName ?? (mCurrentUserName = GetCurrentUserName());
            }
        }


        /// <summary>
        /// Gets current user object received from auth cookie.
        /// </summary>
        protected CurrentUserInfo CurrentUser
        {
            get
            {
                return mCurrentUser ?? (mCurrentUser = GetCurrentUser());
            }
        }


        /// <summary>
        /// If true, global administrators will be able to work with sensitive columns (such as UserPassword) via REST service. Other users than global administrators cannot work with these columns regardless this setting value.
        /// </summary>
        public bool AllowedSensitiveColumns
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSRESTAllowSensitiveFields");
            }
        }


        /// <summary>
        /// Gets the list of allowed object types separated by semicolon. Empty string means all object types are allowed.
        /// </summary>
        public string AllowedObjectTypes
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(CurrentSiteName + ".CMSRESTAllowedObjectTypes").Replace(" ", "");
            }
        }


        /// <summary>
        /// Returns default Encoding from settings.
        /// </summary>
        public Encoding DefaultEncoding
        {
            get
            {
                return mDefaultEncoding ?? (mDefaultEncoding = GetDefaultEncoding(CurrentSiteName));
            }
        }

        #endregion


        #region "Objects methods"

        #region "Single object retrieval"

        /// <summary>
        /// Returns object of given type with specified ID.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        protected GeneralizedInfo GetObjectByIDInternal(string objectType, string id)
        {
            return GetObjectByIDInternal(objectType, id, false);
        }


        /// <summary>
        /// Returns object of given type with specified ID.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="binary">If true, binary data are loaded</param>
        protected GeneralizedInfo GetObjectByIDInternal(string objectType, string id, bool binary)
        {
            GeneralizedInfo result = null;

            if (ValidationHelper.IsInteger(id))
            {
                // Get info by ID
                GeneralizedInfo obj = ModuleManager.GetObject(objectType);
                if (obj != null)
                {
                    result = obj.GetObject(ValidationHelper.GetInteger(id, 0));

                    if ((result != null) && binary)
                    {
                        // Ensure binary data
                        result.EnsureBinaryData(true);
                    }
                }
            }
            else
            {
                // Object by code name from current site
                result = GetObjectByNameInternal(objectType, CurrentSiteName, id, binary);
            }

            return result;
        }


        /// <summary>
        /// Returns object of given type with specified name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site (if nothing is specified, global objects are returned)</param>
        /// <param name="name">Code name of the object</param>
        protected GeneralizedInfo GetObjectByNameInternal(string objectType, string siteName, string name)
        {
            return GetObjectByNameInternal(objectType, siteName, name, false);
        }


        /// <summary>
        /// Returns object of given type with specified name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site (if nothing is specified, global objects are returned)</param>
        /// <param name="name">Code name of the object</param>
        /// <param name="binary">If true, binary data are loaded</param>
        protected GeneralizedInfo GetObjectByNameInternal(string objectType, string siteName, string name, bool binary)
        {
            GeneralizedInfo obj = ModuleManager.GetObject(objectType);
            if (obj != null)
            {
                // Try to parse GUID
                var guid = ValidationHelper.GetGuid(name, Guid.Empty);
                var isGuid = (guid != Guid.Empty);

                WhereCondition where = new WhereCondition();

                var ti = obj.TypeInfo;

                // Add site where condition
                if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    var siteWhere = ti.GetSiteWhereCondition(siteName, false);

                    where.Where(siteWhere);
                }

                GeneralizedInfo result = null;

                if (isGuid)
                {
                    // Get info by GUID
                    if (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        where.WhereEquals(ti.GUIDColumn, guid);

                        result = obj.GetObject(where);
                    }
                }
                else if (obj.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // Get info by code name
                    where.WhereEquals(obj.CodeNameColumn, name);

                    result = obj.GetObject(where);
                }

                if ((result != null) && binary)
                {
                    // Ensure binary data
                    result.EnsureBinaryData(true);
                }

                return result;
            }
            return null;
        }

        #endregion


        #region "Multiple objects retrieval"

        /// <summary>
        /// Returns list of objects of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object(s)</param>
        /// <param name="settings">Export settings</param>
        protected DataSet GetObjects(string objectType, TraverseObjectSettings settings)
        {
            GeneralizedInfo obj = ModuleManager.GetObject(objectType);
            if (obj != null)
            {
                int totalRecords;

                // Ensure correct ORDER BY (default by object display name)
                string orderBy = settings.OrderBy;
                if (!string.IsNullOrEmpty(orderBy) && orderBy.Equals("##default##", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = obj.DisplayNameColumn;
                }

                // If no order by was defined, set the default orderby
                if (string.IsNullOrEmpty(orderBy))
                {
                    orderBy = obj.TypeInfo.DefaultOrderBy;
                }

                var q = obj.GetDataQuery(
                    true,
                    s => s
                        .TopN(settings.TopN)
                        .Where(settings.WhereCondition)
                        .OrderBy(orderBy)
                        .Columns(settings.Columns),
                    true
                );

                q.IncludeBinaryData = settings.Binary;

                q.Offset = settings.Offset;
                q.MaxRecords = settings.MaxRecords;

                // If there is no WHERE condition or ORDER BY or COLUMNS parameter, do not use TransactionScope
                var useTransaction = (!settings.WhereCondition.WhereIsEmpty && !string.IsNullOrEmpty(orderBy) && !string.IsNullOrEmpty(settings.Columns));

                DataSet ds;

                // Use TransactionScope for security reasons when settings.Columns, settings.Where or orderby is defined
                using (useTransaction ? new CMSTransactionScope() : null)
                {
                    ds = q.Result;
                    totalRecords = q.TotalRecords;
                }

                EnsureCorrectFormat(ds, objectType);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    EnsureCorrectFormat(ds.Tables[0], objectType);
                    ds.Tables.Add(GetTotalRecordsTable(totalRecords));
                }

                return ds;
            }

            return null;
        }

        #endregion


        #region "Modifying methods"

        /// <summary>
        /// Returns response for modifying methods - ID and GUID of the modified object.
        /// </summary>
        /// <param name="info">Info object which was created/updated/deleted</param>
        /// <param name="settings">Export settings</param>
        protected Stream GetResponse(GeneralizedInfo info, TraverseObjectSettings settings)
        {
            var response = WebOperationContext.Current.OutgoingResponse;

            if (info != null)
            {
                string rootName = ObjectHelper.GetSerializationTableName(info);
                string result;

                var ti = info.TypeInfo;

                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        response.ContentType = APPLICATION_CONTENT_TYPE;

                        // Prepare source object for JSON convert
                        DataContainer jsonObj = new DataContainer();
                        if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            jsonObj[ti.IDColumn] = info.ObjectID;
                        }

                        if (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            jsonObj[ti.GUIDColumn] = info.ObjectGUID;
                        }

                        // Convert to JSON
                        result = jsonObj.ToJSON(rootName, false);

                        // Wrap to JSONP callback
                        result = WrapJSONP(result, settings);
                        break;

                    default:
                        response.ContentType = TEXT_CONTENT_TYPE;
                        StringBuilder resultBuilder = new StringBuilder();

                        using (XmlWriter writer = XmlWriter.Create(resultBuilder, new XmlWriterSettings() { OmitXmlDeclaration = true, Encoding = settings.Encoding }))
                        {
                            // <root>
                            writer.WriteStartElement(rootName);

                            if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                            {
                                // <IDColumn>
                                writer.WriteStartElement(ti.IDColumn);
                                // {ObjectID}
                                writer.WriteValue(info.ObjectID);
                                // </IDColumn>
                                writer.WriteEndElement();
                            }

                            if (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                            {
                                // <GUIDColumn>
                                writer.WriteStartElement(ti.GUIDColumn);
                                // {ObjectGUID}
                                writer.WriteValue(XmlConvert.ToString(info.ObjectGUID));
                                // </GUIDColumn>
                                writer.WriteEndElement();
                            }

                            // </root>
                            writer.WriteEndElement();
                        }

                        // XML -> String
                        result = resultBuilder.ToString();

                        break;
                }

                // Set encoding
                response.ContentType += "; charset=" + settings.Encoding.WebName;

                MemoryStream ms = new MemoryStream(settings.Encoding.GetBytes(result));
                ms.Position = 0;
                return ms;
            }

            response.SetStatusAsNotFound();
            return null;
        }


        /// <summary>
        /// Deletes given objects and returns it as a result.
        /// </summary>
        /// <param name="info">Info object to update/create</param>
        /// <param name="objectType">Type of the object. Used for security check in case of non-existing object.</param>
        /// <param name="stream">Data of the object</param>
        /// <param name="isCreate">If true new object will be created</param>
        /// <param name="siteName">Name of the site to which the object will be assigned (if null, original setting from stream data is preserved)</param>
        protected Stream SetObjectInternal(GeneralizedInfo info, string objectType, Stream stream, bool isCreate, string siteName)
        {
            if (WebOperationContext.Current != null)
            {
                // Check security to prevent redundant processing of unauthorized requests
                if (!IsAuthorizedForObject(info, (isCreate ? PermissionsEnum.Create : PermissionsEnum.Modify), objectType))
                {
                    return ReturnForbiddenStatus();
                }

                int siteId = SiteInfoProvider.GetSiteID(siteName);
                if (!String.IsNullOrEmpty(siteName) && (siteId == 0))
                {
                    return ReturnSiteNotFoundStatus();
                }

                if ((stream != null) && (info != null))
                {
                    int oldId = info.ObjectID;

                    TraverseObjectSettings settings = GetExportSettings("data");

                    if (!settings.Translate)
                    {
#pragma warning disable BH1014 // Do not use System.IO
                        string data = new StreamReader(stream).ReadToEnd();
#pragma warning restore BH1014 // Do not use System.IO

                        List<string> excludedColumns = null;
                        if (!AllowedSensitiveColumns || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                        {
                            excludedColumns = info.TypeInfo.SensitiveColumns;
                        }

                        switch (settings.Format)
                        {
                            case ExportFormatEnum.JSON:
                                HierarchyHelper.LoadObjectFromJSON(info, data, false, true, excludedColumns);
                                break;

                            default:
                                string cultureName = SettingsHelper.AppSettings[CULTURE_KEY] ?? "en-US";
                                HierarchyHelper.LoadObjectFromXML(OperationTypeEnum.Export, info, data, false, true, cultureName, excludedColumns);
                                break;
                        }

                        bool createSiteBinding = false;

                        // Set the site if required
                        if (!string.IsNullOrEmpty(siteName))
                        {
                            if (info.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                            {
                                info.SetValue(info.TypeInfo.SiteIDColumn, siteId);
                            }
                            else if (!string.IsNullOrEmpty(info.TypeInfo.SiteBinding) && (info.TypeInfo.SiteBinding != ObjectTypeInfo.VALUE_UNKNOWN))
                            {
                                createSiteBinding = true;
                            }
                        }

                        // Force create/update
                        info.ObjectID = isCreate ? 0 : oldId;

                        // Check security again - the changes might have set the object to an illegal state (e.g. unauthorized change of privilege level in user object)
                        if (!IsAuthorizedForObject(info, (isCreate ? PermissionsEnum.Create : PermissionsEnum.Modify), info.TypeInfo.ObjectType))
                        {
                            return ReturnForbiddenStatus();
                        }

                        // Save changes to DB
                        info.SubmitChanges(true);

                        if (isCreate && createSiteBinding)
                        {
                            // Create binding to site if required
                            GeneralizedInfo binding = ModuleManager.GetObject(info.TypeInfo.SiteBinding);
                            if (binding != null)
                            {
                                binding.ObjectSiteID = SiteInfoProvider.GetSiteID(siteName);
                                binding.ObjectParentID = info.ObjectID;
                                binding.SetObject();
                            }
                        }

                        // Generate response
                        UriTemplateMatch match = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
                        Uri itemUrl = new Uri(match.BaseUri.OriginalString + "/" + info.ObjectID);
                        WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(itemUrl);

                        return GetResponse(info, settings);
                    }
                }

                ReturnNotFoundStatus();
            }

            return null;
        }


        /// <summary>
        /// Deletes given objects and returns it as a result.
        /// </summary>
        /// <param name="info">GeneralizedInfo to delete</param>
        /// <param name="objectType">Type of the object. Used for security check in case of non-existing object.</param>
        protected Stream DeleteObjectInternal(GeneralizedInfo info, string objectType)
        {
            // Check security
            if (!IsAuthorizedForObject(info, PermissionsEnum.Delete, objectType))
            {
                return ReturnForbiddenStatus();
            }

            if (info != null)
            {
                Stream stream = GetResponse(info, GetExportSettings(null));
                info.DeleteObject();
                return stream;
            }

            return null;
        }

        #endregion


        #endregion


        #region "Helper methods"

        #region "ODATA Methods"

        /// <summary>
        /// Serializes given DataSet to ODATA.
        /// </summary>
        /// <param name="ds">DataSet to serialize</param>
        /// <param name="settings">Export settings</param>
        /// <param name="dataRootName">Root name</param>
        /// <param name="title">Title of the feed</param>
        /// <param name="description">Description of the feed</param>
        /// <param name="feedUri">Feed URI</param>
        /// <param name="itemUriPattern">Feed URI pattern (with macros, will be resolved with object data)</param>
        /// <param name="modifiedDateColumn">Column with modified data</param>
        /// <param name="publishDateColumn">Column with publish date</param>
        /// <param name="nameColumn">Name column</param>
        /// <param name="idColumn">ID column</param>
        protected static SyndicationFeed ToODATA(DataSet ds, TraverseObjectSettings settings, string dataRootName, string title, string description, Uri feedUri, string itemUriPattern, string modifiedDateColumn, string publishDateColumn, string nameColumn, string idColumn)
        {
            if (ds != null)
            {
                SyndicationFeed feed = new SyndicationFeed(title, description, feedUri);
                feed.Description = new TextSyndicationContent(description);
                List<SyndicationItem> items = new List<SyndicationItem>();

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataTable table in ds.Tables)
                    {
                        foreach (DataRow dr in table.Rows)
                        {
                            DateTime publishDate = DateTimeHelper.ZERO_TIME;
                            DateTime modifiedDate = DateTime.Now;
                            if (table.Columns.Contains(publishDateColumn))
                            {
                                publishDate = ValidationHelper.GetDateTime(dr[publishDateColumn], DateTimeHelper.ZERO_TIME);
                            }
                            if (table.Columns.Contains(modifiedDateColumn))
                            {
                                modifiedDate = ValidationHelper.GetDateTime(dr[modifiedDateColumn], DateTimeHelper.ZERO_TIME);
                            }
                            Uri itemUri = GetItemUri(itemUriPattern, dr, settings);
                            DataRowContainer drContainer = new DataRowContainer(dr);
                            XmlReader xml = new XmlTextReader(new MemoryStream(settings.Encoding.GetBytes(drContainer.ToXML(dataRootName, true))));
                            SyndicationContent content = SyndicationContent.CreateXmlContent(xml);


                            string name = "Unknown name";
                            if (table.Columns.Contains(nameColumn))
                            {
                                name = dr[nameColumn].ToString();
                            }

                            string id = "0";
                            if (table.Columns.Contains(idColumn))
                            {
                                id = dr[idColumn].ToString();
                            }

                            SyndicationItem item = new SyndicationItem(name, content, itemUri, id, modifiedDate);
                            if (publishDate != DateTimeHelper.ZERO_TIME)
                            {
                                item.PublishDate = publishDate;
                            }
                            items.Add(item);
                        }
                    }
                }

                feed.Items = items;
                return feed;
            }

            return null;
        }


        /// <summary>
        /// Returns SyndicationFeed of object dataset.
        /// </summary>
        /// <param name="ds">DataSet with objects data</param>
        /// <param name="objectType">Object type of the data inside</param>
        /// <param name="settings">Export settings</param>
        protected virtual SyndicationFeed GetObjectDataSetFeed(DataSet ds, string objectType, TraverseObjectSettings settings)
        {
            GeneralizedInfo info = ModuleManager.GetObject(objectType);
            if (info != null)
            {
                var ti = info.TypeInfo;

                return ToODATA(ds, settings, objectType, objectType, "List of " + objectType + " objects.", GetObjectUri(objectType, settings), "rest/" + objectType + "/{#" + ti.IDColumn.ToLowerInvariant() + "#}", ti.TimeStampColumn, ti.TimeStampColumn, info.CodeNameColumn, ti.IDColumn);
            }
            return null;
        }


        /// <summary>
        /// Returns Item Uri according to pattern.
        /// </summary>
        /// <param name="pattern">Pattern with macros</param>
        /// <param name="dr">DataRow with object data</param>
        /// <param name="settings">Export settings</param>
        protected static Uri GetItemUri(string pattern, DataRow dr, TraverseObjectSettings settings)
        {
            if ((dr != null) && !string.IsNullOrEmpty(pattern))
            {
                string result = pattern + "?format=" + settings.Format.ToString().ToLowerInvariant();

                foreach (DataColumn col in dr.Table.Columns)
                {
                    result = result.Replace("{#" + col.ColumnName.ToLowerInvariant() + "#}", ValidationHelper.GetString(dr[col.ColumnName], ""));
                }

                return new Uri(CurrentBaseUri, result);
            }
            return null;
        }


        /// <summary>
        /// Returns Uri of given object type.
        /// </summary>
        /// <param name="info">GeneralizedInfo</param>
        /// <param name="settings">Export settings</param>
        protected static Uri GetObjectUri(GeneralizedInfo info, TraverseObjectSettings settings)
        {
            if (info != null)
            {
                var relativeUrl = String.Format("rest/{0}/{1}?format={2}", info.TypeInfo.ObjectType.ToLowerInvariant(), info.ObjectID, settings.Format.ToString().ToLowerInvariant());

                return new Uri(CurrentBaseUri, relativeUrl);
            }
            return null;
        }


        /// <summary>
        /// Returns Uri of given object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="settings">Export settings</param>
        protected static Uri GetObjectUri(string objectType, TraverseObjectSettings settings)
        {
            if (objectType != null)
            {
                var relativeUrl = String.Format("rest/{0}?format={1}", objectType.ToLowerInvariant(), settings.Format.ToString().ToLowerInvariant());

                return new Uri(CurrentBaseUri, relativeUrl);
            }
            return null;
        }

        #endregion


        /// <summary>
        /// Returns default encoding according to settings in Site Manager.
        /// </summary>
        /// <param name="siteName">Name of the site from where to take the settings</param>
        public static Encoding GetDefaultEncoding(string siteName)
        {
            Encoding defaultEnc = null;
            string name = SettingsKeyInfoProvider.GetValue(siteName + ".CMSRESTDefaultEncoding");

            try
            {
                defaultEnc = Encoding.GetEncoding(name);
            }
            catch
            {
            }

            return defaultEnc ?? Encoding.UTF8;
        }


        /// <summary>
        /// Returns requested encoding or default encoding if none was specified.
        /// </summary>
        /// <param name="siteName">Name of the site from where to take the settings</param>
        public static Encoding GetRequestedEncoding(string siteName)
        {
            Encoding encoding = null;

            string accCharset = WebOperationContext.Current.IncomingRequest.Headers["Accept-Charset"];
            if (!string.IsNullOrEmpty(accCharset))
            {
                string[] charsets = accCharset.Split(',');
                foreach (string charset in charsets)
                {
                    try
                    {
                        string[] parts = charset.Split(';');
                        encoding = Encoding.GetEncoding(parts[0]);
                        break;
                    }
                    catch
                    {
                    }
                }
            }

            return encoding ?? GetDefaultEncoding(siteName);
        }


        /// <summary>
        /// Wraps JSON result to a JSONP callback method if specified.
        /// </summary>
        /// <param name="originalJSON">Original JSON code</param>
        /// <param name="settings">Settings</param>
        protected static string WrapJSONP(string originalJSON, TraverseObjectSettings settings)
        {
            if (!string.IsNullOrEmpty(settings?.JSONCallback))
            {
                return settings.JSONCallback + "(" + originalJSON + ")";
            }
            return originalJSON;
        }


        /// <summary>
        /// Returns current site name.
        /// </summary>
        public static string GetCurrentSiteName()
        {
            if (!string.IsNullOrEmpty(SiteContext.CurrentSiteName))
            {
                return SiteContext.CurrentSiteName;
            }

            string siteName = (string)RequestStockHelper.GetItem(SITE_NAME_KEY);
            if (string.IsNullOrEmpty(siteName))
            {
                SiteInfo si = SiteInfoProvider.GetRunningSiteInfo(GetQueryParam(DOMAIN_QUERY_KEY), null);
                if (si != null)
                {
                    siteName = si.SiteName;
                    RequestStockHelper.Add(SITE_NAME_KEY, siteName);
                }
            }
            return siteName;
        }


        /// <summary>
        /// Returns current site info according to domain in query string.
        /// </summary>
        public static SiteInfo GetCurrentSite()
        {
            if (SiteContext.CurrentSite != null)
            {
                return SiteContext.CurrentSite;
            }

            SiteInfo site = (SiteInfo)RequestStockHelper.GetItem(SITE_KEY);
            if (site == null)
            {
                string siteName = GetCurrentSiteName();
                if (!string.IsNullOrEmpty(siteName))
                {
                    site = new SiteInfo(SiteInfoProvider.GetSiteInfo(siteName), true);
                    RequestStockHelper.Add(SITE_KEY, site);
                }
            }
            return site;
        }


        /// <summary>
        /// Returns current user.
        /// </summary>
        public static string GetCurrentUserName()
        {
            return RESTSecurityInvoker.GetUserName();
        }


        /// <summary>
        /// Returns current user.
        /// </summary>
        public static CurrentUserInfo GetCurrentUser()
        {
            var user = (CurrentUserInfo)RequestStockHelper.GetItem(USER_KEY);
            if (user == null)
            {
                var userName = GetCurrentUserName();

                if (!String.IsNullOrEmpty(userName) && !RESTSecurityInvoker.IsHashAuthenticated())
                {
                    user = new CurrentUserInfo(UserInfoProvider.GetUserInfo(userName), true);
                    RequestStockHelper.Add(USER_KEY, user);
                }
            }

            return user;
        }


        /// <summary>
        /// Returns query string parameter.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        protected static string GetQueryParam(string name)
        {
            if ((WebOperationContext.Current != null) && (WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null))
            {
                NameValueCollection query = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
                if (query != null)
                {
                    return query[name];
                }
            }
            return null;
        }


        /// <summary>
        /// Creates ExportObjectSettings object from query string parameters.
        /// </summary>
        /// <param name="rootName">Name of the root</param>
        protected virtual TraverseObjectSettings GetExportSettings(string rootName)
        {
            TraverseObjectSettings settings = new TraverseObjectSettings();

            if (WebOperationContext.Current != null)
            {
                NameValueCollection query = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

                SetFormat(settings, query);

                settings.JSONCallback = ValidationHelper.GetString(query["jsonp"], "");
                if (!ValidationHelper.IsCodeName(settings.JSONCallback))
                {
                    settings.JSONCallback = "";
                }

                settings.Binary = ValidationHelper.GetBoolean(query["binary"], false);
                settings.EnsureBinaryData = ValidationHelper.GetBoolean(query["binary"], false);
                settings.IncludeMetadata = ValidationHelper.GetBoolean(query["metadata"], false);
                settings.IncludeObjectData = ValidationHelper.GetBoolean(query["objectdata"], true);
                settings.IncludeChildren = ValidationHelper.GetBoolean(query["children"], false);
                settings.IncludeBindings = ValidationHelper.GetBoolean(query["bindings"], false);
                settings.IncludeOtherBindings = ValidationHelper.GetBoolean(query["otherbindings"], false);
                settings.IncludeMetafiles = ValidationHelper.GetBoolean(query["metafiles"], false);
                settings.IncludeRelationships = ValidationHelper.GetBoolean(query["relationships"], false);
                settings.IncludeCategories = ValidationHelper.GetBoolean(query["categories"], false);
                settings.IncludeTranslations = ValidationHelper.GetBoolean(query["translations"], false);
                settings.MaxRelativeLevel = ValidationHelper.GetInteger(query["maxrelativelevel"], -1);
                settings.CreateHierarchy = ValidationHelper.GetBoolean(query["hierarchy"], false);
                settings.RootName = rootName;

                settings.LocalizeToLanguage = ValidationHelper.GetString(query["localize"], null);

                settings.WhereCondition.Where(query[WHERE_QUERY_KEY]);
                settings.OrderBy = query[ORDERBY_QUERY_KEY];
                settings.Columns = query[COLUMNS_QUERY_KEY];

                settings.TopN = ValidationHelper.GetInteger(query["topn"], 0);
                settings.Offset = ValidationHelper.GetInteger(query["offset"], 0);
                settings.MaxRecords = ValidationHelper.GetInteger(query["maxrecords"], 0);
            }

            // Get the encoding from Accept-Charset header
            settings.Encoding = GetRequestedEncoding(CurrentSiteName);

            return settings;
        }


        /// <summary>
        /// Sets the format according to content-type and querystring parameter (higher priority).
        /// </summary>
        /// <param name="settings">Settings object</param>
        /// <param name="query">Query string values collection</param>
        protected static void SetFormat(TraverseObjectSettings settings, NameValueCollection query)
        {
            // Try to set format according to content type
            if (WebOperationContext.Current != null)
            {
                string contentType = ValidationHelper.GetString(WebOperationContext.Current.IncomingRequest.ContentType, "").ToLowerInvariant();
                string[] parts = contentType.Split(';');
                switch (parts[0])
                {
                    case APPLICATION_CONTENT_TYPE:
                        settings.Format = ExportFormatEnum.JSON;
                        break;

                    case APPLICATION_ATOM_XML_CONTENT_TYPE:
                        settings.Format = ExportFormatEnum.ATOM10;
                        break;

                    case APPLICATION_RSS_XML_CONTENT_TYPE:
                        settings.Format = ExportFormatEnum.RSS20;
                        break;

                    default:
                        settings.Format = ExportFormatEnum.XML;
                        break;
                }
            }

            // If format forced by parameter, set it according to that
            string format = ValidationHelper.GetString(query["format"], "xml").ToLowerInvariant();
            switch (format)
            {
                case "json":
                    settings.Format = ExportFormatEnum.JSON;
                    break;

                case "atom10":
                    settings.Format = ExportFormatEnum.ATOM10;
                    break;

                case "rss20":
                    settings.Format = ExportFormatEnum.RSS20;
                    break;
            }
        }


        /// <summary>
        /// Returns proper stream from given object.
        /// </summary>
        /// <param name="obj">Info object or TreeNode to get into the stream</param>
        /// <param name="objectType">Original object type</param>
        /// <param name="settings">Export settings</param>
        protected virtual Stream GetStream(object obj, string objectType, TraverseObjectSettings settings)
        {
            var response = WebOperationContext.Current.OutgoingResponse;

            if (obj != null)
            {
                DataSet ds = obj as DataSet;
                if (ds != null)
                {
                    // Keep only columns which were requested (for security checks some has to be added when retrieving data)
                    if (!string.IsNullOrEmpty(settings.Columns))
                    {
                        int i = 0;
                        foreach (DataTable table in ds.Tables)
                        {
                            // For objects, only main object matches the columns parameter
                            if ((i == 0) || (objectType.ToLowerInvariant() == PredefinedObjectType.DOCUMENT))
                            {
                                // Get the list of columns to remove
                                string allowedCols = "," + settings.Columns.ToLowerInvariant().Replace(" ", "") + ",";
                                List<string> columnsToRemove = new List<string>();
                                foreach (DataColumn col in table.Columns)
                                {
                                    if (!allowedCols.Contains("," + col.ColumnName.ToLowerInvariant() + ","))
                                    {
                                        columnsToRemove.Add(col.ColumnName);
                                    }
                                }

                                // Remove unwanted columns
                                foreach (string col in columnsToRemove)
                                {
                                    table.Columns.Remove(col);
                                }

                                i++;
                            }
                        }
                    }

                    // Remove sensitive columns
                    if (!AllowedSensitiveColumns || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                    {
                        var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
                        if (typeInfo?.SensitiveColumns != null)
                        {
                            foreach (var colName in typeInfo.SensitiveColumns)
                            {
                                foreach (DataTable table in ds.Tables)
                                {
                                    if (table.Columns.Contains(colName))
                                    {
                                        table.Columns.Remove(colName);
                                    }
                                }
                            }
                        }
                    }

                    if ((settings.Format == ExportFormatEnum.ATOM10) || (settings.Format == ExportFormatEnum.RSS20))
                    {
                        SyndicationFeed feed = GetObjectDataSetFeed(ds, objectType, settings);

                        StringBuilder sb = new StringBuilder();
                        XmlWriterSettings xmlsettings = new XmlWriterSettings();
                        xmlsettings.OmitXmlDeclaration = true;
                        xmlsettings.Indent = true;
                        xmlsettings.CheckCharacters = false;

                        XmlWriter xmlWriter = XmlWriter.Create(sb, xmlsettings);

                        switch (settings.Format)
                        {
                            case ExportFormatEnum.ATOM10:
                                response.ContentType = APPLICATION_ATOM_XML_CONTENT_TYPE;
                                feed.SaveAsAtom10(xmlWriter);
                                break;

                            case ExportFormatEnum.RSS20:
                                response.ContentType = APPLICATION_RSS_XML_CONTENT_TYPE;
                                feed.SaveAsRss20(xmlWriter);
                                break;
                        }

                        // Set encoding
                        response.ContentType += "; charset=" + settings.Encoding.WebName;
                        xmlWriter.Flush();

                        MemoryStream ms = new MemoryStream(settings.Encoding.GetBytes(LocalizeResult(settings, sb.ToString())));
                        ms.Position = 0;
                        return ms;
                    }

                    switch (settings.Format)
                    {
                        case ExportFormatEnum.JSON:
                            response.ContentType = "application/json; charset=" + settings.Encoding.WebName;
                            MemoryStream ms = new MemoryStream(settings.Encoding.GetBytes(LocalizeResult(settings, WrapJSONP(ds.ToJSON(true), settings))));
                            ms.Position = 0;
                            return ms;

                        default:
                            response.ContentType = "text/xml; charset=" + settings.Encoding.WebName;

                            StringBuilder sb = new StringBuilder();
                            XmlWriterSettings xmlSettings = new XmlWriterSettings();
                            xmlSettings.CheckCharacters = false;
                            xmlSettings.OmitXmlDeclaration = true;
                            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

                            ds.WriteXml(writer);
                            writer.Flush();

                            MemoryStream dsStream = new MemoryStream(settings.Encoding.GetBytes(LocalizeResult(settings, sb.ToString())));
                            dsStream.Position = 0;
                            return dsStream;
                    }
                }
                else
                {
                    switch (settings.Format)
                    {
                        case ExportFormatEnum.JSON:
                            response.ContentType = APPLICATION_CONTENT_TYPE;
                            break;

                        case ExportFormatEnum.ATOM10:
                            response.ContentType = "application/atom+xml;type=entry";
                            break;

                        case ExportFormatEnum.RSS20:
                            response.ContentType = APPLICATION_RSS_XML_CONTENT_TYPE;
                            break;

                        default:
                            response.ContentType = TEXT_CONTENT_TYPE;
                            break;
                    }

                    string result;

                    if (obj is GeneralizedInfo)
                    {
                        var info = ((GeneralizedInfo)obj).Clone();

                        EnsureSensitiveColumns(info);

                        settings.ExportItemURI = GetObjectUri(info, settings);
                        result = HierarchyHelper.ExportObject(settings, info);
                    }
                    else if (obj is BaseInfo)
                    {
                        var info = ((BaseInfo)obj).Generalized.Clone();

                        EnsureSensitiveColumns(info);

                        settings.ExportItemURI = GetObjectUri(info, settings);
                        result = HierarchyHelper.ExportObject(settings, info);
                    }
                    else if (obj is IDataContainer)
                    {
                        // General data container
                        IDataContainer hierarchicalObj = (IDataContainer)obj;
                        switch (settings.Format)
                        {
                            case ExportFormatEnum.JSON:
                                result = hierarchicalObj.ToJSON("data", settings.Binary);
                                break;

                            default:
                                result = hierarchicalObj.ToXML("data", settings.Binary);
                                break;
                        }
                    }
                    else if ((obj is IEnumerable) && !(obj is string))
                    {
                        // General collection
                        IEnumerable enumerable = (IEnumerable)obj;
                        switch (settings.Format)
                        {
                            case ExportFormatEnum.JSON:
                                result = enumerable.ToJSON("collection", settings.Binary);
                                break;

                            default:
                                result = enumerable.ToXML("collection", settings.Binary);
                                break;
                        }
                    }
                    else
                    {
                        // General object
                        switch (settings.Format)
                        {
                            case ExportFormatEnum.JSON:
                                result = "{ \"type\": \"" + obj.GetType().Name + "\", \"value\": \"" + obj + "\" }";
                                break;

                            default:
                                result = "<data><type>" + obj.GetType().Name + "</type><value>" + obj + "</value></data>";
                                break;
                        }
                    }

                    // Ensure JSONP support
                    switch (settings.Format)
                    {
                        case ExportFormatEnum.JSON:
                            result = WrapJSONP(result, settings);
                            break;
                    }

                    // Set encoding
                    response.ContentType += "; charset=" + settings.Encoding.WebName;

                    MemoryStream ms = new MemoryStream(settings.Encoding.GetBytes(LocalizeResult(settings, result)));
                    ms.Position = 0;
                    return ms;
                }
            }

            response.SetStatusAsNotFound();
            return null;
        }


        /// <summary>
        /// Clears sensitive column values if needed.
        /// </summary>
        /// <param name="info">Info to be modified</param>
        protected void EnsureSensitiveColumns(BaseInfo info)
        {
            if (!AllowedSensitiveColumns || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                if (info.TypeInfo.SensitiveColumns != null)
                {
                    foreach (var col in info.TypeInfo.SensitiveColumns)
                    {
                        // Remove the sensitive columns
                        info.SetValue(col, null);
                    }
                }
            }
        }


        /// <summary>
        /// Localizes the result (calls LocalizeString on it).
        /// </summary>
        /// <param name="settings">Settings to use</param>
        /// <param name="result">Result to localize</param>
        private static string LocalizeResult(TraverseObjectSettings settings, string result)
        {
            if (!string.IsNullOrEmpty(settings.LocalizeToLanguage))
            {
                result = ResHelper.LocalizeString(result, settings.LocalizeToLanguage, true);
            }
            return result;
        }


        /// <summary>
        /// Returns table containing one column and one row with total records number.
        /// </summary>
        /// <param name="totalRecords">Total records number</param>
        protected DataTable GetTotalRecordsTable(int totalRecords)
        {
            DataTable recordsTable = new DataTable("TotalRecords");
            recordsTable.Columns.Add("TotalRecords");

            DataRow row = recordsTable.NewRow();
            row[0] = totalRecords;
            recordsTable.Rows.Add(row);

            return recordsTable;
        }


        /// <summary>
        /// Ensures correct serialization format of given DataSet.
        /// </summary>
        /// <param name="ds">DataSet to modify</param>
        /// <param name="objectType">ObjectType of the data in the DataSet</param>
        protected void EnsureCorrectFormat(DataSet ds, string objectType)
        {
            if (ds != null)
            {
                string name = objectType.Replace(".", "_").Replace("#", "_");
                ds.DataSetName = TypeHelper.GetPlural(name);
            }
        }


        /// <summary>
        /// Ensures correct serialization format of given DataTable.
        /// </summary>
        /// <param name="dt">DataTable to modify</param>
        /// <param name="objectType">ObjectType of the data in the DataTable</param>
        protected void EnsureCorrectFormat(DataTable dt, string objectType)
        {
            dt.TableName = ObjectHelper.GetSerializationTableName(ModuleManager.GetObject(objectType));
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Sets the response status to Forbidden.
        /// </summary>
        protected Stream ReturnForbiddenStatus()
        {
            var context = WebOperationContext.Current;
            if (context != null)
            {
                context.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
            }
            return null;
        }


        /// <summary>
        /// Sets the response status to Not found.
        /// </summary>
        /// <param name="description">Description for the Not found status</param>
        protected Stream ReturnNotFoundStatus(string description = null)
        {
            var context = WebOperationContext.Current;
            context?.OutgoingResponse.SetStatusAsNotFound(description);
            return null;
        }


        /// <summary>
        /// Sets the response status to Not found with a comment that the site does not exist.
        /// </summary>
        protected Stream ReturnSiteNotFoundStatus()
        {
            return ReturnNotFoundStatus("Site does not exist.");
        }


        /// <summary>
        /// Returns true if current user is granted with specified permission to process operation on the given objecttype.
        /// </summary>
        /// <param name="objectType">Object type of an object</param>
        /// <param name="objectSite">Site the object belongs to</param>
        /// <param name="permission">Permission type</param>
        protected bool IsAuthorizedForObject(string objectType, string objectSite, PermissionsEnum permission)
        {
            BaseInfo obj = ModuleManager.GetObject(objectType);
            if (obj == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(objectSite))
            {
                obj.Generalized.ObjectSiteID = SiteInfoProvider.GetSiteID(objectSite);
            }

            return IsAuthorizedForObject(obj, permission, objectType);
        }


        /// <summary>
        /// Returns true if current user is granted with specified permission to process operation on the given objecttype.
        /// </summary>
        /// <param name="obj">Object to be checked</param>
        /// <param name="permission">Permission type</param>
        /// <param name="objectType">Type of object.</param>
        protected bool IsAuthorizedForObject(BaseInfo obj, PermissionsEnum permission, string objectType)
        {
            // Check the allowed object types
            if (!IsObjectTypeAllowed(objectType))
            {
                return false;
            }

            if (!CheckQueryStringParameters())
            {
                return false;
            }

            // Object does not exists - check if user can edit objects of this type in general
            if (obj == null)
            {
                return IsAuthorizedForObject(objectType, null, permission);
            }

            string siteName = (string.IsNullOrEmpty(obj.Generalized.ObjectSiteName) ? CurrentSiteName : obj.Generalized.ObjectSiteName);

            // If there is no site, there is no permission to check
            if (string.IsNullOrEmpty(siteName))
            {
                return false;
            }

            // Check the user's permissions
            return RESTSecurityInvoker.IsHashAuthenticated() || (CurrentUser != null) && CurrentUser.IsAuthorizedPerObject(permission, obj, siteName);
        }


        /// <summary>
        /// Returns true if given object type is allowed to be accessible by the service. 
        /// </summary>
        /// <param name="objectType">Object type to check.</param>
        protected bool IsObjectTypeAllowed(string objectType)
        {
            // Check the allowed object types
            var allowedTypes = ";" + AllowedObjectTypes.ToLowerInvariant() + ";";

            return (allowedTypes == ";;") || allowedTypes.Contains(";" + objectType.ToLowerInvariant() + ";");
        }


        /// <summary>
        /// Checks WHERE, ORDER BY and COLUMNS query string parameters so non-admin users cannot use malicious code (= SQL injection protection).
        /// </summary>
        protected bool CheckQueryStringParameters()
        {
            string where = GetQueryParam(WHERE_QUERY_KEY);
            if (!CheckSqlQuery(where, SqlSecurityHelper.WhereRegex, QueryScopeEnum.Where))
            {
                return false;
            }

            string orderby = GetQueryParam(ORDERBY_QUERY_KEY);
            if (!CheckSqlQuery(orderby, SqlSecurityHelper.OrderByRegex, QueryScopeEnum.OrderBy))
            {
                return false;
            }

            string columns = GetQueryParam(COLUMNS_QUERY_KEY);
            if (!CheckSqlQuery(columns, SqlSecurityHelper.ColumnsRegex, QueryScopeEnum.Columns))
            {
                return false;
            }

            return true;
        }


        private bool CheckSqlQuery(string query, Regex regEx, QueryScopeEnum scope)
        {
            return String.IsNullOrEmpty(query) || regEx.IsMatch(query) && SqlSecurityHelper.CheckQuery(query, scope);
        }

        #endregion


        #region "URLRewriting methods"

        /// <summary>
        /// Handles the rewriting for REST service.
        /// </summary>
        /// <param name="relativePath">Relative URL</param>
        /// <param name="queryString">Query string</param>
        /// <param name="domain">Domain of the request</param>
        /// <param name="httpMethod">HTTP method of the request</param>
        /// <returns>Two dimensional array, first parameter is rewritten URL, the second is querystring</returns>
        public static string[] RewriteRESTUrl(string relativePath, string queryString, string domain, string httpMethod)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            string pathToLower = relativePath;
            bool isTranslation = false;

            // Transfer translate to query string (for easier handling the translations in the service, saves the need to create more URI templates)
            if (pathToLower.StartsWith("/rest/translate/", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = "/rest/" + relativePath.Substring("/rest/translate/".Length);
                queryString = (string.IsNullOrEmpty(queryString) ? "" : queryString + "&") + "translate=1";
                pathToLower = relativePath.ToLowerInvariant();
                isTranslation = true;
            }
            else if (pathToLower.EndsWith("/rest/translate", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = "/rest/";
                queryString = (string.IsNullOrEmpty(queryString) ? "" : queryString + "&") + "translate=1";
                pathToLower = relativePath;
                isTranslation = true;
            }

            if (pathToLower.StartsWith("/rest/content/", StringComparison.OrdinalIgnoreCase))
            {
                // Handle documents differently, change alias path in the url to query parameter

                // Cut the "/rest/content/" prefix
                string docUrl = relativePath.Substring("/rest/content/".Length);
                string site = "";

                int index;

                if (docUrl.StartsWith("currentsite/", StringComparison.OrdinalIgnoreCase))
                {
                    docUrl = docUrl.Substring(12);
                    site = "currentsite";
                }
                else if (docUrl.StartsWith("site/", StringComparison.OrdinalIgnoreCase))
                {
                    docUrl = docUrl.Substring(5);
                    index = docUrl.IndexOf('/');
                    if (index > -1)
                    {
                        site = docUrl.Substring(0, index);
                        docUrl = docUrl.Substring(index + 1);
                    }
                    else
                    {
                        site = "##ALL##";
                    }
                }

                // Get the culture
                string culture;
                index = docUrl.IndexOf('/');
                if (index > -1)
                {
                    culture = docUrl.Substring(0, index);
                    docUrl = docUrl.Substring(index + 1);
                }
                else
                {
                    culture = "en-us";
                }

                // Get the document command (all / childrenof / document / publish / etc.)
                string command;
                index = docUrl.IndexOf('/');
                if (index > -1)
                {
                    command = docUrl.Substring(0, index);
                    docUrl = docUrl.Substring(index + 1);
                }
                else
                {
                    command = docUrl;
                    docUrl = "";
                }

                string newUrl = "~/DocumentRESTService.svc/content/" + command;

                StringBuilder restQuery = new StringBuilder("domain=");
                restQuery.Append(domain);
                restQuery.Append("&siteName=");
                restQuery.Append(site);
                restQuery.Append("&aliaspath=");
                restQuery.Append("/");
                restQuery.Append(HttpUtility.UrlEncode(docUrl));
                restQuery.Append("&cultureCode=");
                restQuery.Append(culture);

                if (!String.IsNullOrEmpty(queryString))
                {
                    restQuery.Append("&");
                    restQuery.Append(queryString);
                }

                return new[] { newUrl, restQuery.ToString() };
            }
            
            // Standard GeneralizedInfos
            int len = Math.Min(6, relativePath.Length);
            string serviceName;

            if (isTranslation)
            {
                serviceName = httpMethod == "POST" ? "DocumentRESTService.svc" : "ObjectTranslationRESTService.svc";
            }
            else
            {
                serviceName = "RESTService.svc";
            }

            return new[] { "~/" + serviceName + "/" + relativePath.Substring(len), "domain=" + domain + (string.IsNullOrEmpty(queryString) ? "" : "&" + queryString) };
        }

        #endregion
    }
}