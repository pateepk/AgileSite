using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base import / export control
    /// </summary>
    public abstract class ImportExportControl : CMSUserControl
    {
        /// <summary>
        /// Import / export settings
        /// </summary>
        private AbstractImportExportSettings mSettings = null;


        #region "Public properties"

        /// <summary>
        /// Additional settings.
        /// </summary>
        public AbstractImportExportSettings Settings
        {
            get
            {
                return mSettings;
            }
            set
            {
                mSettings = value;
            }
        }


        /// <summary>
        /// Version check.
        /// </summary>
        public int VersionCheck
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["VersionCheck"], -1);
            }
            set
            {
                ViewState["VersionCheck"] = value;
            }
        }


        /// <summary>
        /// Hotfix version check.
        /// </summary>
        public int HotfixVersionCheck
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["HotfixVersionCheck"], -1);
            }
            set
            {
                ViewState["HotfixVersionCheck"] = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets current settings.
        /// </summary>
        public virtual void SaveSettings()
        {
        }


        /// <summary>
        /// Reload data.
        /// </summary>
        public virtual void ReloadData()
        {
        }


        /// <summary>
        /// Checks the license for selected objects.
        /// </summary>
        public static string CheckLicenses(SiteImportSettings settings)
        {
            string domain = string.IsNullOrEmpty(settings.SiteDomain) ? RequestContext.CurrentDomain.ToLowerCSafe() : URLHelper.RemovePort(settings.SiteDomain).ToLowerCSafe();

            // Remove application path
            int slashIndex = domain.IndexOfCSafe("/");
            if (slashIndex > -1)
            {
                domain = domain.Substring(0, slashIndex);
            }

            // Get the list of all objects that may be imported (global and site)
            var objectsToImport = settings.GetSelectedObjectTypes(true).Union(settings.GetSelectedObjectTypes(false));

            using (var context = new CMSActionContext())
            {
                // Disable redirect when license check performed on base info object fails
                context.AllowLicenseRedirect = false;

                foreach (var objectType in objectsToImport)
                {
                    // Check that object type is being imported
                    var globalObjects = settings.GetSelectedObjects(objectType, false) ?? new List<string>();
                    var siteObjects = settings.GetSelectedObjects(objectType, true) ?? new List<string>();
                    int count = globalObjects.Count + siteObjects.Count;

                    if (count > 0)
                    {
                        // Get read only info object for given object type
                        var readOnlyObject = ModuleManager.GetReadOnlyObject(objectType, false);
                        if (readOnlyObject == null)
                        {
                            return String.Format(ResHelper.GetString("import.objecttypenotregistered"), objectType);
                        }

                        using (new CMSActionContext { LogLicenseWarnings = false })
                        {
                            bool importAllowed = true;
                            string exceptionMessage = null;

                            //Check insert operation
                            try
                            {
                                readOnlyObject.Generalized.CheckLicense(ObjectActionEnum.Insert, domain);
                            }
                            catch (LicenseException ex)
                            {
                                importAllowed = false;
                                exceptionMessage = ex.Message;
                            }

                            // Inserting new objects is not allowed. Check that we are really inserting new objects and not overriding existing. 
                            if (!importAllowed && !String.IsNullOrEmpty(readOnlyObject.Generalized.CodeNameColumn))
                            {
                                // Get count of existing objects
                                var q = new ObjectQuery(objectType, false);

                                var importObjectsCodeNames = globalObjects.Union(siteObjects).ToList();
                                q.WhereIn(readOnlyObject.Generalized.CodeNameColumn, importObjectsCodeNames);

                                q.Column(new AggregatedColumn(AggregationType.Count, null).As("Count"));
                                var existingCount = q.GetScalarResult<int>();

                                // No new object is being inserted. Check edit operation
                                if (existingCount == count)
                                {
                                    importAllowed = true;

                                    try
                                    {
                                        readOnlyObject.Generalized.CheckLicense(ObjectActionEnum.Edit, domain);
                                    }
                                    catch (LicenseException ex)
                                    {
                                        importAllowed = false;
                                        exceptionMessage = ex.Message;
                                    }
                                }
                            }

                            // Check if object type can be imported and return message if not 
                            if (!importAllowed)
                            {
                                return String.Format(ResHelper.GetString("import.checklicensefailed"), readOnlyObject.TypeInfo.GetNiceObjectTypeName()) + " " + exceptionMessage;
                            }
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the limit of objects for given license and feature.
        /// </summary>
        /// <param name="license">License</param>
        /// <param name="feature">Feature</param>
        public static int GetLimit(LicenseKeyInfo license, FeatureEnum feature)
        {
            if (license == null)
            {
                return 0;
            }

            // If feature not available, no objects allowed
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(license, feature))
            {
                return 0;
            }

            // Get version limit
            int limit = LicenseKeyInfoProvider.VersionLimitations(license, feature);
            if (limit == 0)
            {
                return int.MaxValue;
            }

            return limit;
        }


        /// <summary>
        /// Checks the version of the controls (without hotfix version).
        /// </summary>
        public bool CheckVersion()
        {
            if (VersionCheck == -1)
            {
                SiteImportSettings importSettings = (SiteImportSettings)Settings;
                if ((importSettings != null) && importSettings.TemporaryFilesCreated)
                {
                    VersionCheck = importSettings.IsLowerVersion(CMSVersion.MainVersion) ? 1 : 0;
                }
                else
                {
                    return false;
                }
            }
            return (VersionCheck > 0);
        }


        /// <summary>
        /// Checks the hotfix version of the controls.
        /// </summary>
        public bool CheckHotfixVersion()
        {
            if (HotfixVersionCheck == -1)
            {
                SiteImportSettings importSettings = (SiteImportSettings)Settings;
                if ((importSettings != null) && importSettings.TemporaryFilesCreated)
                {
                    HotfixVersionCheck = (ValidationHelper.GetInteger(importSettings.HotfixVersion, 0) < ValidationHelper.GetInteger(CMSVersion.HotfixVersion, 0)) ? 1 : 0;
                }
                else
                {
                    return false;
                }
            }
            return (HotfixVersionCheck > 0);
        }

        #endregion
    }
}