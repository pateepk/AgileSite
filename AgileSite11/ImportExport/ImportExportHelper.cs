using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Helper class for import/export process
    /// </summary>
    public static class ImportExportHelper
    {
        #region "Constants"

        /// <summary>
        /// Object translation.
        /// </summary>
        public const string OBJECT_TYPE_TRANSLATION = "objecttranslation";


        /// <summary>
        /// Info type for additional information.
        /// </summary>
        public const string CMS_INFO_TYPE = "cms_info";


        /// <summary>
        /// Maximal length of the exported file name with path.
        /// </summary>
        public const int MAX_FILE_LENGTH = 260;


        /// <summary>
        /// Path where the backup Zip archive will be created.
        /// </summary>
        public const string EXPORT_BACKUP_PATH = "~/App_Data/Backup/";


        /// <summary>
        /// Export extension.
        /// </summary>
        public const string EXPORT_EXTENSION = ".export";


        /// <summary>
        /// Export package extension.
        /// </summary>
        public const string PACKAGE_EXTENSION = ".zip";


        /// <summary>
        /// Data folder.
        /// </summary>
        public const string DATA_FOLDER = "Data";


        /// <summary>
        /// Prefix for BizForm data object type.
        /// </summary>
        public const string BIZFORM_PREFIX = "bizform.";


        /// <summary>
        /// Prefix for custom table data object type.
        /// </summary>
        public const string CUSTOMTABLE_PREFIX = "customtable.";


        /// <summary>
        /// Prefix for forum posts object type.
        /// </summary>
        public const string FORUMPOST_PREFIX = "forumpost.";


        /// <summary>
        /// Prefix for board messages object type.
        /// </summary>
        public const string BOARDMESSAGE_PREFIX = "boardmessage.";


        /// <summary>
        /// Prefix for media files object type.
        /// </summary>
        public const string MEDIAFILE_PREFIX = "mediafile.";


        /// <summary>
        /// BizForm folder.
        /// </summary>
        public const string BIZFORM_FOLDER = "Bizforms";


        /// <summary>
        /// Custom table folder.
        /// </summary>
        public const string CUSTOMTABLE_FOLDER = "Customtables";


        /// <summary>
        /// Forum posts folder.
        /// </summary>
        public const string FORUMPOSTS_FOLDER = "Forumposts";


        /// <summary>
        /// Media files folder.
        /// </summary>
        public const string MEDIAFILES_FOLDER = "Mediafiles";


        /// <summary>
        /// Board messages folder.
        /// </summary>
        public const string BOARDMESSAGES_FOLDER = "Boardmessages";


        /// <summary>
        /// Site macro.
        /// </summary>
        public const string SITE_MACRO = "##SITENAME##";

        #endregion


        #region "Settings keys"

        /// <summary>
        /// Seal the exported modules.
        /// </summary>
        public const string SETTINGS_SEAL_EXPORTED_MODULES = "SealExportedModules";


        /// <summary>
        /// Process documents history.
        /// </summary>
        public const string SETTINGS_DOC_HISTORY = "DocumentHistory";


        /// <summary>
        /// Process document relationships.
        /// </summary>
        public const string SETTINGS_DOC_RELATIONSHIPS = "DocumentRelationships";


        /// <summary>
        /// Document ACLs.
        /// </summary>
        public const string SETTINGS_DOC_ACLS = "DocumentACLs";


        /// <summary>
        /// Process blog comments.
        /// </summary>
        public const string SETTINGS_BLOG_COMMENTS = "BlogComments";


        /// <summary>
        /// Process event attendees.
        /// </summary>
        public const string SETTINGS_EVENT_ATTENDEES = "EventAttendees";


        /// <summary>
        /// Process BizForm data.
        /// </summary>
        public const string SETTINGS_BIZFORM_DATA = "BizFormData";


        /// <summary>
        /// Process custom table data.
        /// </summary>
        public const string SETTINGS_CUSTOMTABLE_DATA = "CustomTableData";


        /// <summary>
        /// Process forum posts.
        /// </summary>
        public const string SETTINGS_FORUM_POSTS = "ForumPosts";


        /// <summary>
        /// Process media files.
        /// </summary>
        public const string SETTINGS_MEDIA_FILES = "MediaFiles";


        /// <summary>
        /// Copy media files (physical files).
        /// </summary>
        public const string SETTINGS_MEDIA_FILES_PHYSICAL = "MediaFilesPhysical";


        /// <summary>
        /// Copy BizForm files (physical files).
        /// </summary>
        public const string SETTINGS_BIZFORM_FILES_PHYSICAL = "BizFormFilesPhysical";


        /// <summary>
        /// Process board messages.
        /// </summary>
        public const string SETTINGS_BOARD_MESSAGES = "BoardMessages";


        /// <summary>
        /// Process export tasks.
        /// </summary>
        public const string SETTINGS_TASKS = "ExportTasks";


        /// <summary>
        /// Run site after import.
        /// </summary>
        public const string SETTINGS_RUN_SITE = "RunSite";


        /// <summary>
        /// Import / export site
        /// </summary>
        public const string SETTINGS_SITE = "ImportSite";


        /// <summary>
        /// Delete site after error during import.
        /// </summary>
        public const string SETTINGS_DELETE_SITE = "DeleteSite";


        /// <summary>
        /// Copy global folders.
        /// </summary>
        public const string SETTINGS_GLOBAL_FOLDERS = "GlobalFolders";


        /// <summary>
        /// Copy global folders.
        /// </summary>
        public const string SETTINGS_ASSEMBLIES = "Assemblies";


        /// <summary>
        /// Copy site folders.
        /// </summary>
        public const string SETTINGS_SITE_FOLDERS = "SiteFolders";


        /// <summary>
        /// Skip translation errors.
        /// </summary>
        public const string SETTINGS_SKIP_OBJECT_ON_TRANSLATION_ERROR = "SkipTranslationErrors";


        /// <summary>
        /// Delete temporary files.
        /// </summary>
        public const string SETTINGS_DELETE_TEMPORARY_FILES = "DeleteTemporaryFiles";


        /// <summary>
        /// Add site bindings in case the object is not imported (updated).
        /// </summary>
        public const string SETTINGS_ADD_SITE_BINDINGS = "AddSiteBindings";


        /// <summary>
        /// Process e-commerce objects.
        /// </summary>
        public const string SETTINGS_PROCESS_ECOMMERCE = "ProcessEcommerce";


        /// <summary>
        /// Process community objects.
        /// </summary>
        public const string SETTINGS_PROCESS_COMMUNITY = "ProcessCommunity";


        /// <summary>
        /// Update site definition if importing to the existing site.
        /// </summary>
        public const string SETTINGS_UPDATE_SITE_DEFINITION = "UpdateSiteDefinition";


        /// <summary>
        /// Copy ASPX templates folder.
        /// </summary>
        public const string SETTINGS_COPY_ASPX_TEMPLATES_FOLDER = "CopyASPXTemplatesFolder";


        /// <summary>
        /// Copy forum custom layouts folder.
        /// </summary>
        public const string SETTINGS_COPY_FORUM_CUSTOM_LAYOUTS_FOLDER = "CopyForumCustomLayoutsFolder";


        /// <summary>
        /// Indicates that web template is exported.
        /// </summary>
        public const string SETTINGS_WEBTEMPLATE_EXPORT = "WebTemplateExport";

        /// <summary>
        /// Indicates if workflow scopes should be imported.
        /// </summary>
        public const string SETTINGS_WORKFLOW_SCOPES = "ImportWorkflowScopes";

        /// <summary>
        /// Indicates if workflow triggers should be imported.
        /// </summary>
        public const string SETTINGS_WORKFLOW_TRIGGERS = "ImportWorkflowTriggers";

        /// <summary>
        /// Indicates if page template scopes should be imported.
        /// </summary>
        public const string SETTINGS_PAGETEMPLATE_SCOPES = "ImportPageTemplateScopes";

        /// <summary>
        /// Indicates if web part/zone/widget variants should be imported with the page template.
        /// </summary>
        public const string SETTINGS_PAGETEMPLATE_VARIANTS = "ImportPageTemplateVariants";

        /// <summary>
        /// Indicates if user personalization should be imported.
        /// </summary>
        public const string SETTINGS_USER_PERSONALIZATIONS = "ImportUserPersonalizations";

        /// <summary>
        /// Indicates if group membership should be imported.
        /// </summary>
        public const string SETTINGS_GROUP_MEMBERSHIP = "ImportGroupMembership";

        /// <summary>
        /// Indicates if user dashboards should be imported.
        /// </summary>
        public const string SETTINGS_USER_DASHBOARDS = "ImportUserDashboards";

        /// <summary>
        /// Indicates if user dashboards should be imported even if user isn't imported
        /// </summary>
        public const string SETTINGS_USER_SITE_DASHBOARDS = "ImportUserSiteDashboards";

        #endregion


        #region "Information keys"

        /// <summary>
        /// Start time of the process.
        /// </summary>
        public const string INFO_START_TIME = "StartTime";


        /// <summary>
        /// Version of the system.
        /// </summary>
        public const string INFO_SYSTEM_VERSION = "SystemVersion";


        /// <summary>
        /// Code name of the exported/imported module.
        /// </summary>
        public const string MODULE_NAME = "ModuleName";


        /// <summary>
        /// Version of the hotfix.
        /// </summary>
        public const string INFO_HOTFIX_VERSION = "HotfixVersion";


        /// <summary>
        /// Machine name.
        /// </summary>
        public const string INFO_MACHINE_NAME = "MachineName";


        /// <summary>
        /// Current user.
        /// </summary>
        public const string INFO_CURRENT_USER = "CurrentUser";


        /// <summary>
        /// Domain name.
        /// </summary>
        public const string INFO_DOMAIN_NAME = "DomainName";


        /// <summary>
        /// Single object.
        /// </summary>
        public const string INFO_SINGLE_OBJECT = "SingleObject";


        /// <summary>
        /// Site included.
        /// </summary>
        public const string INFO_SITE_INCLUDED = "SiteIncluded";


        /// <summary>
        /// Time stamp.
        /// </summary>
        public const string INFO_TIME_STAMP = "TimeStamp";


        /// <summary>
        /// History name.
        /// </summary>
        public const string INFO_HISTORY_NAME = "HistoryName";


        /// <summary>
        /// History date.
        /// </summary>
        public const string INFO_HISTORY_DATE = "HistoryDate";
        
        
        /// <summary>
                                                              /// Webtemplate flag.
                                                              /// </summary>
        public const string INFO_WEBTEMPLATE_FLAG = "Webtemplate";


        /// <summary>
        /// Web application flag.
        /// </summary>
        public const string INFO_SYSTEM_WEBAPP = "WebApplication";

        #endregion


        #region "Variables"

        private static ObjectTypeTreeNode mExportObjectTree;
        private static ObjectTypeTreeNode mImportObjectTree;
        
        // Fictive web site directory for imported physical files (for testing purposes only).
        private static string mImportWebSiteDirectory;

        // Site utilities folder path.
        private static string mSiteUtilsFolderPath;

        // Language suffixes for code behind files
        private static List<string> mLangSuffixes;

        // Name of the App_Code folder
        private static string mAppCodeFolder;

        // List of object types with flag if its global or not. Used by Import/Export, order of the elements matters (objects are in order for import, export order is reverse).
        private static List<ObjectTypeListItem> mObjectTypes;

        private static IEnumerable<string> mObjectTypeCycles;

        // Set of setting key names which will be excluded from synchronization
        private static HashSet<string> mExcludedSettingKeyNames;

        private static readonly object lockObject = new object();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Object tree.
        /// </summary>
        public static ObjectTypeTreeNode ExportObjectTree
        {
            get
            {
                if (mExportObjectTree == null)
                {
                    lock (lockObject)
                    {
                        if (mExportObjectTree == null)
                        {
                            mExportObjectTree = BuildObjectTree(info => info.ImportExportSettings.IsExportable);
                        }
                    }
                }

                return mExportObjectTree;
            }
        }


        /// <summary>
        /// Object tree.
        /// </summary>
        public static ObjectTypeTreeNode ImportObjectTree
        {
            get
            {
                if (mImportObjectTree == null)
                {
                    lock (lockObject)
                    {
                        if (mImportObjectTree == null)
                        {
                            // Include all supported object types in the import tree
                            mImportObjectTree = BuildObjectTree(info => info.ImportExportSettings.SupportsExport);
                        }
                    }
                }

                return mImportObjectTree;
            }
        }


        private static ObjectTypeTreeNode BuildObjectTree(Func<ObjectTypeInfo, bool> condition)
        {
            var tree = ObjectTypeTreeNode.NewObjectTree("##OBJECTS##");
            tree.Active = true;

            // Add documents
            var site = tree.FindNode("##CONTENTMANAGEMENT##", true);
            if (site != null)
            {
                site.AddObjectType("cms.document", null, true);
            }

            // Register object types
            ObjectTypeManager.RegisterTypesToObjectTree(tree, info =>
            {
                // 
                if ((condition == null) || condition(info))
                {
                    return info.ImportExportSettings.ObjectTreeLocations;
                }

                return null;
            });

            return tree;
        }


        /// <summary>
        /// List of object types with flag if its global or not. Used by Import/Export, order of the elements matters (objects are in order for import, export order is reverse).
        /// </summary>
        public static List<ObjectTypeListItem> ObjectTypes
        {
            get
            {
                if (mObjectTypes == null)
                {
                    EnsureObjectTypeOrder();
                }

                return mObjectTypes;
            }
            private set
            {
                mObjectTypes = value;
            }
        }


        /// <summary>
        /// Indicates whether there is a cycle in dependencies among object types supported by import/export.
        /// </summary>
        public static IEnumerable<string> ObjectTypeCycles
        {
            get
            {
                if (mObjectTypeCycles == null)
                {
                    EnsureObjectTypeOrder();
                }

                return mObjectTypeCycles;
            }
            private set
            {
                mObjectTypeCycles = value;
            }
        }


        /// <summary>
        /// Fictive web site directory for imported physical files (for testing purposes only).
        /// </summary>
        public static string ImportWebSiteDirectory
        {
            get
            {
                return mImportWebSiteDirectory ?? (mImportWebSiteDirectory = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSImportWebSiteDirectory"], ""));
            }
            set
            {
                mImportWebSiteDirectory = value;
            }
        }


        /// <summary>
        /// Site utils folder path (default: ~/CMSSiteUtils/)
        /// </summary>
        public static string SiteUtilsFolderPath
        {
            get
            {
                return mSiteUtilsFolderPath ?? (mSiteUtilsFolderPath = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSSiteUtilsFolderPath"], "~/CMSSiteUtils/"));
            }
            set
            {
                mSiteUtilsFolderPath = value;
            }
        }


        /// <summary>
        /// Language suffixes for code behind files
        /// </summary>
        public static List<string> LangSuffixes
        {
            get
            {
                if (mLangSuffixes == null)
                {
                    mLangSuffixes = new List<string>(2);
                    mLangSuffixes.Add(".cs");
                    mLangSuffixes.Add(".vb");
                }

                return mLangSuffixes;
            }
        }


        /// <summary>
        /// Name of the App_Code folder for current instance
        /// </summary>
        public static string AppCodeFolder
        {
            get
            {
                return mAppCodeFolder ?? (mAppCodeFolder = SystemContext.IsWebApplicationProject ? "Old_App_Code" : "App_Code");
            }
        }

        #endregion


        #region "Source folders"

        /// <summary>
        /// Skins folder.
        /// </summary>
        public const string SRC_SKINS_FOLDER = "App_Themes";


        /// <summary>
        /// Web part containers folder.
        /// </summary>
        public const string SRC_CONTAINERS_FOLDER = SRC_SKINS_FOLDER + "\\Containers";


        /// <summary>
        /// Forum custom layouts folder.
        /// </summary>
        public const string SRC_FORUM_LAYOUTS_FOLDER = "CMSModules\\Forums\\Controls\\Layouts\\Custom";


        /// <summary>
        /// Templates folder.
        /// </summary>
        public const string SRC_TEMPLATES_FOLDER = "CMSTemplates";

        #endregion


        #region "Target folders"

        /// <summary>
        /// Templates folder.
        /// </summary>
        public const string FILES_FOLDER = "Files";


        /// <summary>
        /// Global folder.
        /// </summary>
        public const string GLOBAL_FOLDER = "Global";


        /// <summary>
        /// Site folder.
        /// </summary>
        public const string SITE_FOLDER = "Site";


        /// <summary>
        /// E-commerce folder.
        /// </summary>
        public const string ECOMMERCE_FOLDER = "COM";


        /// <summary>
        /// Community folder.
        /// </summary>
        public const string COMMUNITY_FOLDER = "Community";


        /// <summary>
        /// Documents folder.
        /// </summary>
        public const string DOCUMENTS_FOLDER = "Documents";


        /// <summary>
        /// General objects folder.
        /// </summary>
        public const string GENERALOBJECTS_FOLDER = "Objects";

        #endregion


        #region "Methods"

        #region "Excluded settings methods"

        /// <summary>
        /// Returns set of excluded setting key names. Can return null if there are no excluded keys.
        /// </summary>
        public static HashSet<string> GetExcludedSettingKeys()
        {
            return mExcludedSettingKeyNames;
        }


        /// <summary>
        /// Adds given setting key(s) to the excluded keys. Excluded keys are not exported nor imported.
        /// </summary>
        /// <param name="names">Names of the settings key</param>
        public static void AddExcludedSettingKey(params string[] names)
        {
            if (names == null)
            {
                return;
            }

            if (mExcludedSettingKeyNames == null)
            {
                mExcludedSettingKeyNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            }

            foreach (var name in names)
            {
                mExcludedSettingKeyNames.Add(name);
            }
        }


        /// <summary>
        /// Removes given setting key(s) from the excluded keys. Excluded keys are not exported nor imported.
        /// </summary>
        /// <param name="names">Names of the settings key</param>
        public static void RemoveExcludedSettingKey(params string[] names)
        {
            if ((mExcludedSettingKeyNames == null) || (names == null))
            {
                return;
            }

            foreach (var name in names)
            {
                mExcludedSettingKeyNames.Remove(name);
            }
        }


        /// <summary>
        /// Determines whether the given setting key is excluded from import/export.
        /// </summary>
        /// <param name="name">Name of the settings key</param>
        public static bool IsSettingKeyExcluded(string name)
        {
            if (mExcludedSettingKeyNames == null)
            {
                return false;
            }

            return mExcludedSettingKeyNames.Contains(name);
        }

        #endregion


        /// <summary>
        /// Indicates if file is designer code behind file
        /// </summary>
        /// <param name="filePath">File path</param>
        public static bool IsDesignerCodeBehind(string filePath)
        {
            return LangSuffixes.Any(suff => filePath.EndsWithCSafe(".designer" + suff, true));
        }


        /// <summary>
        /// Indicates if file is code file
        /// </summary>
        /// <param name="filePath">File path</param>
        public static bool IsCodeFile(string filePath)
        {
            return LangSuffixes.Any(suff => filePath.EndsWithCSafe(suff, true));
        }


        /// <summary>
        /// Gets the backup folder for a given object
        /// </summary>
        /// <param name="obj">Object to backup or restore</param>
        public static string GetObjectBackupFolder(GeneralizedInfo obj)
        {
            string path = EXPORT_BACKUP_PATH.TrimEnd('/');

            string siteName = obj.ObjectSiteName;
            if (!String.IsNullOrEmpty(siteName))
            {
                path += "/" + siteName;
            }
            else
            {
                path += "/##Global##";
            }

            path += "/" + obj.TypeInfo.ObjectType.Replace(".", "_") + "/" + obj.ObjectGUID;

            return path;
        }


        /// <summary>
        /// Returns Temporary folder physical path.
        /// </summary>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetTemporaryFolder(string webFullPath)
        {
            return DirectoryHelper.CombinePath(TempFileInfoProvider.GetTempFilesRootFolderPath(webFullPath), "ImportExport") + "\\";
        }


        /// <summary>
        /// Returns Site Utils folder physical path.
        /// </summary>
        public static string GetSiteUtilsFolder()
        {
            return Path.EnsureEndBackslash(FileHelper.GetFullFolderPhysicalPath(SiteUtilsFolderPath));
        }


        /// <summary>
        /// Returns Site Utils folder relative path.
        /// </summary>
        public static string GetSiteUtilsFolderRelativePath()
        {
            string filesFolderPath = SiteUtilsFolderPath;

            if (Path.IsPathRooted(filesFolderPath))
            {
                // Rooted path - cannot create relative path
                filesFolderPath = null;
            }
            else
            {
                filesFolderPath = filesFolderPath.StartsWithCSafe("~/") ? filesFolderPath : "~/" + filesFolderPath.TrimStart('/');
                filesFolderPath = filesFolderPath.TrimEnd('/') + "/";
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Returns true if the first given version is lower than the second one.
        /// </summary>
        /// <param name="version1">First version</param>
        /// <param name="hotfixVersion1">First hotfix version</param>
        /// <param name="version2">Second version</param>
        /// <param name="hotfixVersion2">Second hotfix version</param>
        public static bool IsLowerVersion(string version1, string hotfixVersion1, string version2, string hotfixVersion2)
        {
            VersionInfo ver1 = new VersionInfo(version1, hotfixVersion1);
            VersionInfo ver2 = new VersionInfo(version2, hotfixVersion2);

            return (ver1.CompareTo(ver2) < 0);
        }


        /// <summary>
        /// Remove export extensions.
        /// </summary>
        /// <param name="path">Starting path</param>
        public static void RemoveExportExtension(string path)
        {
            foreach (string dir in Directory.GetDirectories(path))
            {
                RemoveExportExtension(dir);
            }

            foreach (string file in Directory.GetFiles(path, EXPORT_EXTENSION))
            {
                string originalFile = file.Substring(0, file.LastIndexOfCSafe('.'));
                try
                {
                    File.Move(file, originalFile);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }

                    throw new Exception(ex.Message + " (" + originalFile + ")", ex);
                }
            }
        }


        /// <summary>
        /// Returns the real file path for the specified path.
        /// </summary>
        /// <param name="filePath">File path</param>
        public static string GetExportFilePath(string filePath)
        {
            if (!filePath.ToLowerCSafe().EndsWithCSafe(EXPORT_EXTENSION))
            {
                filePath += EXPORT_EXTENSION;
            }

            return filePath;
        }


        /// <summary>
        /// Returns the real file path for the specified file.
        /// </summary>
        /// <param name="filePath">File path</param>
        public static string GetRealFilePath(string filePath)
        {
            if (filePath.ToLowerCSafe().EndsWithCSafe(EXPORT_EXTENSION))
            {
                filePath = filePath.Substring(0, filePath.Length - EXPORT_EXTENSION.Length);
            }

            return filePath;
        }


        /// <summary>
        /// Rename exported files.
        /// </summary>
        /// <param name="path">Path where the file should be renamed</param>
        public static void RenameExportedFiles(string path)
        {
            // Go through all directories
            foreach (string dir in Directory.GetDirectories(path))
            {
                RenameExportedFiles(dir);
            }

            // Go through all files
            foreach (string file in Directory.GetFiles(path))
            {
                string exportFile = file + EXPORT_EXTENSION;

                if (File.Exists(exportFile))
                {
                    File.Delete(exportFile);
                }

                File.Move(file, exportFile);
            }
        }


        /// <summary>
        /// Gets the name for the object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="settings">Import or export settings</param>
        public static string GetObjectTypeName(string objectType, AbstractImportExportSettings settings)
        {
            if (objectType.StartsWithCSafe(BIZFORM_PREFIX))
            {
                return settings.GetAPIString("General.Bizform", "Bizform") + " " + objectType.Remove(0, BIZFORM_PREFIX.Length);
            }
            else if (objectType.StartsWithCSafe(CUSTOMTABLE_PREFIX))
            {
                return settings.GetAPIString("General.CustomTable", "Custom table") + " " + objectType.Remove(0, CUSTOMTABLE_PREFIX.Length);
            }
            return settings.GetAPIString(TypeHelper.GetTasksResourceKey(objectType), objectType);
        }


        /// <summary>
        /// Returns URL to export package with given file name.
        /// </summary>
        /// <param name="fileName">Name of export package.</param>
        /// <param name="siteName">Name of site for which the link should be generated.</param>
        public static string GetExportPackageUrl(string fileName, string siteName)
        {
            string path = GetSiteUtilsFolderRelativePath();
            if (path == null)
            {
                return null;
            }

            string fullPath = path + "Export/" + fileName;
            string localUrl = URLHelper.ResolveUrl(path) + "Export/" + fileName;

            // Handle external URL
            if (StorageHelper.IsExternalStorage(fullPath))
            {
                return File.GetFileUrl(fullPath, siteName) ?? localUrl;
            }
            return localUrl;
        }


        /// <summary>
        /// Gets sub folder for specified object type.
        /// </summary>
        /// <param name="settings">Import export settings</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Site objects</param>
        public static string GetObjectTypeSubFolder(AbstractImportExportSettings settings, string objectType, bool siteObjects)
        {
            // Try to get sub folder
            var e = new GetObjectTypeFolderArgs
                {
                    Settings = settings,
                    ObjectType = objectType,
                    SiteObjects = siteObjects
                };

            // Handle the event
            SpecialActionsEvents.GetObjectTypeFolder.StartEvent(e);
            if (e.Folder != null)
            {
                return e.Folder + "\\";
            }


            if (objectType.StartsWithCSafe(BIZFORM_PREFIX))
            {
                return BIZFORM_FOLDER + "\\";
            }

            if (objectType.StartsWithCSafe(CUSTOMTABLE_PREFIX))
            {
                return CUSTOMTABLE_FOLDER + "\\";
            }

            if (objectType.StartsWithCSafe(FORUMPOST_PREFIX))
            {
                return FORUMPOSTS_FOLDER + "\\";
            }

            if (objectType.StartsWithCSafe(MEDIAFILE_PREFIX))
            {
                return MEDIAFILES_FOLDER + "\\";
            }

            if (objectType.StartsWithCSafe(BOARDMESSAGE_PREFIX))
            {
                return BOARDMESSAGES_FOLDER + "\\";
            }

            if (siteObjects)
            {
                string toReturn = null;
                if (IsEcommerceObjectType(objectType))
                {
                    toReturn += ECOMMERCE_FOLDER + "\\";
                }

                // Add site folder
                toReturn += SITE_FOLDER + "\\";

                return toReturn;
            }

            if ((objectType.ToLowerCSafe() == OBJECT_TYPE_TRANSLATION) || (objectType.ToLowerCSafe() == CMS_INFO_TYPE))
            {
                return "";
            }

            if (IsEcommerceObjectType(objectType))
            {
                return ECOMMERCE_FOLDER + "\\";
            }

            if (IsCommunityObjectType(objectType))
            {
                return COMMUNITY_FOLDER + "\\";
            }

            return GENERALOBJECTS_FOLDER + "\\";
        }


        /// <summary>
        /// Returns true if object type is from e-commerce module.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        public static bool IsEcommerceObjectType(string objectType)
        {
            return objectType.ToLowerCSafe().StartsWithCSafe("ecommerce.");
        }


        /// <summary>
        /// Returns true if object type is from community module.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        public static bool IsCommunityObjectType(string objectType)
        {
            return objectType.ToLowerCSafe().StartsWithCSafe("community.");
        }


        /// <summary>
        /// Returns true if object type is general object.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        public static bool IsCMSObjectType(string objectType)
        {
            return objectType.ToLowerCSafe().StartsWithCSafe("cms.");
        }


        /// <summary>
        /// Gets safe object type name.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        public static string GetSafeObjectTypeName(string objectType)
        {
            return objectType.Replace(".", "_").Replace("#", "_");
        }


        /// <summary>
        /// Gets the data for given object type from DataSet
        /// </summary>
        /// <param name="ds">DataSet with source data</param>
        /// <param name="objectType">Object type</param>
        /// <param name="data">DataTable with the specified data for the given object type</param>
        /// <returns>True, if the data table is not empty</returns>
        public static bool GetDataTable(DataSet ds, string objectType, ref DataTable data)
        {
            data = ds.Tables[GetSafeObjectTypeName(objectType)];

            return !DataHelper.DataSourceIsEmpty(data);
        }


        /// <summary>
        /// Gets where condition for specified object type based on documents. Uses the given condition object to build the where condition.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="ds">DataSet with documents</param>
        /// <param name="parentIdColumn">Name of the column with ID value</param>
        /// <param name="sourceColumn">Name of the source column with ID value</param>
        public static WhereCondition GetDocumentsDataWhereCondition(string objectType, DataSet ds, string parentIdColumn, string sourceColumn)
        {
            // Build the list of values
            var values = new HashSet<int>();

            if (ds != null)
            {
                // Go through all document types and prepare the list
                foreach (DataTable table in ds.Tables)
                {
                    var IDs = DataHelper.GetIntegerValues(table, sourceColumn);
                    values.AddRange(IDs);
                }
            }

            // Fill in the values
            return new WhereCondition().WhereIn(parentIdColumn, values.ToList());
        }


        /// <summary>
        /// Validate export file name.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="fileName">File name to validate</param>
        public static string ValidateExportFileName(SiteExportSettings settings, string fileName)
        {
            // Get filename allowed length
            int fileLength = MAX_FILE_LENGTH - settings.TargetPath.Length;
            fileName = fileName.Trim().ToLowerCSafe();

            // Check if not empty
            string result = new Validator().NotEmpty(fileName, settings.GetAPIString("ExportConfiguration.ErrorFileNameEmpty", "You must enter the file name.")).Result;

            // Check if the filename is in correct format
            if (!ValidationHelper.IsFileName(fileName))
            {
                result = settings.GetAPIString("ExportSite.FileWrongFormat", "Please enter valid file name.");
            }

            // File name is not empty
            if ((result != null) && (result == ""))
            {
                // Add extension
                if (Path.GetExtension(fileName) != ".zip")
                {
                    fileName = fileName.TrimEnd('.') + ".zip";
                }

                // Check length of the filename
                if (fileName.Length > fileLength)
                {
                    result = settings.GetAPIString("ExportSite.FileNameToLong", "The file name is too long.");
                }
                // Check if file already exists
                else if (File.Exists(settings.TargetPath + fileName))
                {
                    result = settings.GetAPIString("ExportSite.FileExists", "File with this name already exists.");
                }
            }
            return result;
        }


        /// <summary>
        /// Initialize provided export settings
        /// </summary>
        /// <param name="settings">Export settings to be initialized</param>
        /// <param name="infoObj">Object to be exported, which will be used to initialize export settings</param>
        public static void InitSingleObjectExportSettings(SiteExportSettings settings, GeneralizedInfo infoObj)
        {
            bool siteObject = (infoObj.ObjectSiteID > 0);

            // Set export to single selected object
            settings.DefaultProcessObjectType = ProcessObjectEnum.None;
            settings.LoadDefaultSelection();

            var selectedObjects = new List<string>();

            // Handle related objects
            if (ImportExportEvents.SingleExportSelection.IsBound)
            {
                // Initiate the authentication event
                ImportExportEvents.SingleExportSelection.StartEvent(settings, infoObj, selectedObjects);
            }

            // Add current object
            selectedObjects.Add(infoObj.ObjectCodeName);

            // Select main object
            var ti = infoObj.TypeInfo;

            settings.SetObjectsProcessType(ProcessObjectEnum.Selected, ti.ObjectType, siteObject);
            settings.SetSelectedObjects(selectedObjects, ti.ObjectType, siteObject);

            // Single object export
            settings.SetInfo(INFO_SINGLE_OBJECT, true);

            // Export of site object, include siteinfo
            if (siteObject)
            {
                settings.SiteId = infoObj.ObjectSiteID;
                settings.SetObjectsProcessType(ProcessObjectEnum.All, SiteInfo.OBJECT_TYPE, true);
            }

            // Set some additional settings
            settings.SetSettings(SETTINGS_GLOBAL_FOLDERS, false);
            settings.SetSettings(SETTINGS_SITE_FOLDERS, false);
            settings.SetSettings(SETTINGS_TASKS, false);
            settings.SetSettings(SETTINGS_COPY_ASPX_TEMPLATES_FOLDER, false);
        }


        private static void EnsureObjectTypeOrder()
        {
            var cycles = new List<string>();

            // Try to generate the list automatically
            ObjectTypes = new ObjectTypeSequenceAnalyzer(new ImportObjectTypeFilter())
            {
                Log = (message, indentationLevel, cycle) =>
                {
                    if (cycle)
                    {
                        cycles.Add(message);
                    }
                }
            }
            .GetSequence()
            .ToList();

            ObjectTypeCycles = cycles;
        }


        /// <summary>
        /// Returns formatted message composed from error description and complete exception text.
        /// </summary>
        /// <param name="description">Error description</param>
        /// <param name="ex">Error exception</param>
        internal static string GetFormattedErrorMessage(string description, Exception ex)
        {
            return description + HTMLHelper.HTML_BREAK + HTMLHelper.EnsureHtmlLineEndings(EventLogProvider.GetExceptionLogMessage(ex)) + HTMLHelper.HTML_BREAK;
        }

        #endregion


        #region "Export helper methods"

        /// <summary>
        /// Indicates if file should be post processed
        /// </summary>
        /// <param name="extension">File extension</param>
        public static bool IsPostProcessFile(string extension)
        {
            extension = extension.TrimStart('.');
            switch (extension.ToLowerCSafe())
            {
                case "aspx":
                case "ascx":
                case "master":
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Returns a file name starting with given <paramref name="prefix"/>, followed by the date, time and given <paramref name="extension"/>.
        /// </summary>
        /// <param name="prefix">File name prefix</param>
        /// <param name="extension">File extension (including the period ".")</param>
        public static string GenerateExportFileName(string prefix, string extension = ".zip")
        {
            string fileName = String.Format("{0}_{1}_{2}{3}", prefix, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmm"), extension);

            return ValidationHelper.GetSafeFileName(fileName);
        }

        #endregion
    }
}