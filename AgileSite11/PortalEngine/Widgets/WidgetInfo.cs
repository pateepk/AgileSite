using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Xml;

using CMS;
using CMS.Helpers;
using CMS.Base;
using CMS.FormEngine;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Core;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WidgetInfo), WidgetInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Widget info class.
    /// </summary>
    public class WidgetInfo : AbstractInfo<WidgetInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.widget";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WidgetInfoProvider), OBJECT_TYPE, "CMS.Widget", "WidgetID", "WidgetLastModified", "WidgetGUID", "WidgetName", "WidgetDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("WidgetCategoryID", WidgetCategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("WidgetWebPartID", WebPartInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("WidgetLayoutID", WebPartLayoutInfo.OBJECT_TYPE)
            },
            DeleteObjectWithAPI = true,
            ModuleName = ModuleName.WIDGETS,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ThumbnailGUIDColumn = "WidgetThumbnailGUID",
            FormDefinitionColumn = "WidgetProperties",
            HasMetaFiles = true,
            EnabledColumn = "WidgetIsEnabled",
            DefaultData = new DefaultDataSettings(),
            SerializationSettings =
            {
                StructuredFields = new List<IStructuredField>
                {
                    new StructuredField("WidgetProperties"),
                    new StructuredField("WidgetDefaultValues")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Constants"

        /// <summary>
        /// Community group page list
        /// </summary>
        public const string GROUPPAGESLIST = "cms.grouppageslist";

        #endregion


        #region "Variables"

        private string mWidgetPublicFields;
        private List<string> mWidgetFields;

        #endregion


        #region "Properties"

        /// <summary>
        /// Widget ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetID
        {
            get
            {
                return GetIntegerValue("WidgetID", 0);
            }
            set
            {
                SetValue("WidgetID", value);
            }
        }


        /// <summary>
        /// Widget name.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetName
        {
            get
            {
                return GetStringValue("WidgetName", String.Empty);
            }
            set
            {
                SetValue("WidgetName", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetDisplayName
        {
            get
            {
                return GetStringValue("WidgetDisplayName", String.Empty);
            }
            set
            {
                SetValue("WidgetDisplayName", value);
            }
        }


        /// <summary>
        /// Widget GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WidgetGUID
        {
            get
            {
                return GetGuidValue("WidgetGUID", Guid.Empty);
            }
            set
            {
                SetValue("WidgetGUID", value);
            }
        }


        /// <summary>
        /// Time of last modification.
        /// </summary>
        [DatabaseField]
        public virtual DateTime WidgetLastModified
        {
            get
            {
                return GetDateTimeValue("WidgetLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WidgetLastModified", value);
            }
        }


        /// <summary>
        /// Widget web part ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetWebPartID
        {
            get
            {
                return GetIntegerValue("WidgetWebPartID", 0);
            }
            set
            {
                SetValue("WidgetWebPartID", value);
            }
        }


        /// <summary>
        /// Widget category ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetCategoryID
        {
            get
            {
                return GetIntegerValue("WidgetCategoryID", 0);
            }
            set
            {
                SetValue("WidgetCategoryID", value);
            }
        }


        /// <summary>
        /// Widget default values.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetDefaultValues
        {
            get
            {
                return GetStringValue("WidgetDefaultValues", String.Empty);
            }
            set
            {
                SetValue("WidgetDefaultValues", value);
            }
        }


        /// <summary>
        /// Widget documentation.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetDocumentation
        {
            get
            {
                return GetStringValue("WidgetDocumentation", String.Empty);
            }
            set
            {
                SetValue("WidgetDocumentation", value);
            }
        }


        /// <summary>
        /// Widget security bit array.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetSecurity
        {
            get
            {
                // Default Authorized roles security = 2
                return GetIntegerValue("WidgetSecurity", 2);
            }
            set
            {
                SetValue("WidgetSecurity", value);
            }
        }


        /// <summary>
        /// Indicates whether the access to widget is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowedFor
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(WidgetSecurity, 1);
            }
            set
            {
                WidgetSecurity = SecurityHelper.SetSecurityAccessEnum(WidgetSecurity, value, 1);
            }
        }


        /// <summary>
        /// Widget description.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetDescription
        {
            get
            {
                return GetStringValue("WidgetDescription", String.Empty);
            }
            set
            {
                SetValue("WidgetDescription", value);
            }
        }


        /// <summary>
        /// Widget layout ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetLayoutID
        {
            get
            {
                return GetIntegerValue("WidgetLayoutID", 0);
            }
            set
            {
                SetValue("WidgetLayoutID", value, 0);
            }
        }


        /// <summary>
        /// Widget properties.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetProperties
        {
            get
            {
                return GetStringValue("WidgetProperties", String.Empty);
            }
            set
            {
                SetValue("WidgetProperties", value);
            }
        }


        /// <summary>
        /// Indicates whether the widget is for group members.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetForGroup
        {
            get
            {
                return GetBooleanValue("WidgetForGroup", false);
            }
            set
            {
                SetValue("WidgetForGroup", value);
            }
        }


        /// <summary>
        /// Indicates whether the widget is for user.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetForUser
        {
            get
            {
                return GetBooleanValue("WidgetForUser", false);
            }
            set
            {
                SetValue("WidgetForUser", value);
            }
        }


        /// <summary>
        /// Indicates whether the widget is for editor.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetForEditor
        {
            get
            {
                return GetBooleanValue("WidgetForEditor", false);
            }
            set
            {
                SetValue("WidgetForEditor", value);
            }
        }


        /// <summary>
        /// Indicates whether the widget can be used as inline.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetForInline
        {
            get
            {
                return GetBooleanValue("WidgetForInline", false);
            }
            set
            {
                SetValue("WidgetForInline", value);
            }
        }


        /// <summary>
        /// Indicates whether the widget can be used in dashboard.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetForDashboard
        {
            get
            {
                return GetBooleanValue("WidgetForDashboard", false);
            }
            set
            {
                SetValue("WidgetForDashboard", value);
            }
        }

        /// <summary>
        /// Enables or disables widget.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetIsEnabled
        {
            get
            {
                return GetBooleanValue("WidgetIsEnabled", true);
            }
            set
            {
                SetValue("WidgetIsEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the widget public fields.
        /// </summary>
        public virtual string WidgetPublicFileds
        {
            get
            {
                if (mWidgetPublicFields == null)
                {
                    string visibleFields;

                    mWidgetFields = GetFields(out visibleFields);
                    mWidgetPublicFields = visibleFields;
                }

                return mWidgetPublicFields;
            }
            set
            {
                if (value == null)
                {
                    mWidgetFields = null;
                }
                mWidgetPublicFields = value;
            }
        }


        /// <summary>
        /// Gets the widget field list.
        /// </summary>
        public List<string> WidgetFields
        {
            get
            {
                if (mWidgetFields == null)
                {
                    string visibleFields;

                    mWidgetFields = GetFields(out visibleFields);
                    mWidgetPublicFields = visibleFields;
                }

                return mWidgetFields;
            }
        }


        /// <summary>
        /// Indicates whether the widget properties dialog should be displayed when inserting a widget to the page.
        /// </summary>
        [DatabaseField]
        public virtual bool WidgetSkipInsertProperties
        {
            get
            {
                return GetBooleanValue("WidgetSkipInsertProperties", false);
            }
            set
            {
                SetValue("WidgetSkipInsertProperties", value);
            }
        }


        /// <summary>
        /// Widget thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WidgetThumbnailGUID
        {
            get
            {
                return GetGuidValue("WidgetThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("WidgetThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Widget icon class defining the widget thumbnail.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetIconClass
        {
            get
            {
                return GetStringValue("WidgetIconClass", null);
            }
            set
            {
                SetValue("WidgetIconClass", value, string.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WidgetInfoProvider.DeleteWidgetInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {

            // Do not store empty IconClass, use NULL value
            if (string.IsNullOrEmpty(WidgetIconClass))
            {
                WidgetIconClass = null;
            }

            WidgetInfoProvider.SetWidgetInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns widget properties which are visible for users.
        /// </summary>
        /// <returns>Field names separated by semicolon</returns>
        private List<string> GetFields(out string visibleFields)
        {
            visibleFields = String.Empty;

            WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(WidgetWebPartID);
            if (wpi == null)
            {
                return null;
            }

            string properties = FormHelper.MergeFormDefinitions(wpi.WebPartProperties, WidgetProperties);

            // Parse XML for both form definitions
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(properties);

            List<string> allfields = new List<string>();

            if (xmlDoc.DocumentElement != null)
            {
                XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("/form/field");
                foreach (XmlNode node in nodes)
                {
                    XmlAttribute attr = node.Attributes["visible"];
                    string column = node.Attributes["column"].Value;

                    allfields.Add(column.ToLowerCSafe());

                    if ((attr != null) && (ValidationHelper.GetBoolean(attr.Value, false)))
                    {
                        visibleFields += column + ";";
                    }
                }
            }

            return allfields;
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Set category
            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                WidgetCategoryID = ValidationHelper.GetInteger(p["cms.widget" + ".categoryid"], 0);
            }
            Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WidgetInfo object.
        /// </summary>
        public WidgetInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WidgetInfo object from the given DataRow.
        /// </summary>
        public WidgetInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            // Set default security to authorized roles = 2
            WidgetSecurity = 2;

            // Other default values
            WidgetForGroup = false;
            WidgetForEditor = false;
            WidgetForUser = false;
            WidgetForInline = false;
            WidgetForDashboard = false;
            WidgetIsEnabled = true;
        }

        #endregion


        #region "Macro methods"

        /// <summary>
        /// Gets the list of object types that may use the widget
        /// </summary>
        [MacroMethod]
        public IEnumerable<string> GetUsageObjectTypes()
        {
            var objectTypes = new List<string>
            {
                PageTemplateInfo.OBJECT_TYPE,
                PredefinedObjectType.DOCUMENT,
                GROUPPAGESLIST,
                PredefinedObjectType.VERSIONHISTORY,
                PredefinedObjectType.PERSONALIZATION,
                PredefinedObjectType.DASHBOARD
            };

            if (ModuleEntryManager.IsModuleLoaded(ModuleName.MVTEST))
            {
                objectTypes.Add(PredefinedObjectType.DOCUMENTMVTVARIANT);
            }

            if (ModuleEntryManager.IsModuleLoaded(ModuleName.CONTENTPERSONALIZATION))
            {
                objectTypes.Add(PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT);
            }

            return objectTypes;
        }


        /// <summary>
        /// Gets the objects using the widget as a query with result columns ObjectType, ObjectID, Source, ItemID and ItemObjectType.
        /// </summary>
        [MacroMethod]
        public IDataQuery GetUsages()
        {
            var q = new MultiObjectQuery();

            var name = WidgetName;

            // Find all page templates where current widget is used
            AddTypeToUsageQuery(q, PageTemplateInfo.OBJECT_TYPE, PageTemplateInfo.TYPEINFO.IDColumn, GetWidgetSearchCondition("PageTemplateWebParts", name), "pagetemplate");

            // Find all page templates where current widget is used
            AddTypeToUsageQuery(q, PredefinedObjectType.DOCUMENT, "DocumentID", GetWidgetSearchCondition("DocumentWebParts", name), "editorwidget");

            // Find all page templates where current widget is used
            AddTypeToUsageQuery(q, GROUPPAGESLIST, "DocumentID", GetWidgetSearchCondition("DocumentGroupWebParts", name), "groupwidget", "NodeGroupID", PredefinedObjectType.GROUP);

            // Find all last version histories where current widget is used
            var editorWidgetVersionCondition =
                new WhereCondition()
                    .WhereContains("NodeXML", String.Format("type=\"{0}\"", name))
                    .WhereIn("VersionHistoryID", new IDQuery(PredefinedObjectType.DOCUMENT, "DocumentCheckedOutVersionHistoryID"));

            AddTypeToUsageQuery(q, PredefinedObjectType.VERSIONHISTORY, "VersionHistoryID", editorWidgetVersionCondition, "editorwidgetversion");

            // Find all dashboards where current widget is used
            AddTypeToUsageQuery(q, PredefinedObjectType.PERSONALIZATION, "PersonalizationID", GetWidgetSearchCondition("PersonalizationWebParts", name), "userwidget", "PersonalizationUserID", UserInfo.OBJECT_TYPE);

            // Find all dashboards where current widget is used
            AddTypeToUsageQuery(q, PredefinedObjectType.DASHBOARD, "PersonalizationID", GetWidgetSearchCondition("PersonalizationWebParts", name), "dashboard", "PersonalizationUserID", UserInfo.OBJECT_TYPE);

            // Check if MVT feature is loaded
            if (ModuleEntryManager.IsModuleLoaded(ModuleName.MVTEST))
            {
                // Find widget in MVT variants
                AddTypeToUsageQuery(q, PredefinedObjectType.DOCUMENTMVTVARIANT, "MVTVariantID", GetWidgetSearchCondition("MVTVariantWebParts", name, ""), "mvtvariant");
            }

            // Check if Content personalization feature is loaded
            if (ModuleEntryManager.IsModuleLoaded(ModuleName.CONTENTPERSONALIZATION))
            {
                // Find widget in Content personalization variants
                AddTypeToUsageQuery(q, PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT, "VariantID", GetWidgetSearchCondition("VariantWebParts", name, ""), "cpvariant");
            }

            // Order by source type by default to keep usages from various types grouped
            q.DefaultOrderByType = true;

            // Data is generally inconsistent, so any global condition should be use on the results
            q.UseGlobalWhereOnResult = true;

            return q;
        }

        #endregion


        #region "Private helper methods"

        /// <summary>
        /// Creates where condition to find current widget in form definition (usable for page template, MVT variants and CP variants)
        /// </summary>
        /// <param name="columnName">Widget definition column name</param>
        /// <param name="name">Widget name</param>
        /// <param name="webPartXmlLocation">Location where web part nodes can be found in XML</param>
        private WhereCondition GetWidgetSearchCondition(string columnName, string name, string webPartXmlLocation = "/page/webpartzone/")
        {
            var containsSearchPattern = String.Format("type=\"{0}\"", name);
            var xmlSearchPattern = String.Format(webPartXmlLocation + "webpart[lower-case(@type) = \"{0}\"][@iswidget = \"true\"]", name.ToLowerCSafe());

            return
                new WhereCondition()
                    // Pre-filter by simpler LIKE to maximize performance as CAST to XML is expensive
                    .WhereContains(columnName, containsSearchPattern)
                    // Filter by XML query to avoid false positives (collision with web parts)
                    .WhereEquals(
                        columnName
                            .AsColumn()
                            .Cast(FieldDataType.Xml)
                            .XmlExists(xmlSearchPattern),
                        true.AsLiteral()
                    );
        }


        /// <summary>
        /// Adds type to query to get all usages of current widget
        /// </summary>
        /// <param name="query">Multi object query</param>
        /// <param name="objectType">Object type to add to search in</param>
        /// <param name="idColumn">ID column name of the object</param>
        /// <param name="condition">Condition how to find the current widget</param>
        /// <param name="source">Text information about the source</param>
        /// <param name="itemId">ID of the additional object</param>
        /// <param name="itemObjectType">Object type of the additional object</param>
        private void AddTypeToUsageQuery(MultiObjectQuery query, string objectType, string idColumn, IWhereCondition condition, string source, string itemId = null, string itemObjectType = null)
        {
            source = CoreServices.Localization.GetString(String.Format("widgets.usage.{0}", source));

            query.Type(objectType, t => t
                                    .Columns
                                    (
                                        objectType.AsValue(true).AsColumn("ObjectType"),
                                        new QueryColumn(idColumn).As("ObjectID"),
                                        source.AsValue(true).AsColumn("Source"),
                                        (itemId != null) ? new QueryColumn(itemId).As("ItemID") : null,
                                        (itemObjectType != null) ? itemObjectType.AsValue(true).AsColumn("ItemObjectType") : null
                                    )
                                    .Where(condition)
                );
        }

        #endregion
    }
}