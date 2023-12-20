using System;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Methods for cloning of info objects
    /// </summary>
    internal class InfoCloneHelper
    {
        /// <summary>
        /// Inserts the object as a new object to the DB with inner data and structure (according to given settings) cloned from the original.
        /// </summary>
        /// <param name="obj">Object to clone</param>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <returns>Returns the newly created clone</returns>
        public static BaseInfo InsertAsClone(BaseInfo obj, CloneSettings settings, CloneResult result)
        {
            GeneralizedInfo genObj = obj.Generalized;
            GeneralizedInfo genCloneObj;

            BaseInfo cloneObj;
            ObjectTypeInfo typeInfo = obj.TypeInfo;

            // Disable event logging for objects (one cloning task will be logged at the end).
            using (new CMSActionContext { LogEvents = false })
            {
                using (new InfoCloneActionContext().UseSynchronizationTempQueue())
                {
                    // Ensure binary data before cloning so cloned version has the data
                    obj.EnsureBinaryData(true);

                    // Set the clone base if not set
                    if (settings.CloneBase == null)
                    {
                        settings.CloneBase = obj;
                    }

                    cloneObj = obj.CloneObject();
                    genCloneObj = cloneObj.Generalized;

                    if (result == null)
                    {
                        result = new CloneResult();
                    }

                    ClearCloneFields(cloneObj);

                    // Do not remember old identifiers values
                    genCloneObj.ResetChanges();

                    // Set ParentID if specified
                    if (settings.ParentID > 0)
                    {
                        if (typeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            cloneObj.SetValue(typeInfo.ParentIDColumn, settings.ParentID);
                        }
                    }

                    // Set the target Site
                    if (!settings.KeepOriginalSiteID)
                    {
                        if (typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            if ((settings.CloneToSiteID > 0) || typeInfo.SupportsGlobalObjects)
                            {
                                if (settings.CloneToSiteID <= 0)
                                {
                                    cloneObj.SetValue(typeInfo.SiteIDColumn, null);
                                }
                                else
                                {
                                    cloneObj.SetValue(typeInfo.SiteIDColumn, settings.CloneToSiteID);
                                }
                            }
                        }
                    }

                    SetCloneDisplayName(settings, cloneObj);

                    // Hard set external columns - will change with code name change
                    obj.CopyExternalColumns(cloneObj);

                    SetCloneCodeName(settings, cloneObj);

                    // Delocalize all text fields except for DisplayName column
                    if (!settings.KeepFieldsTranslated)
                    {
                        LocalizeStringsInClone(obj, cloneObj);
                    }

                    TranslateDependencies(settings, cloneObj);

                    // Fire OnBeforeInsertClone event if defined
                    if (settings.BeforeCloneInsertCallback != null)
                    {
                        settings.BeforeCloneInsertCallback(settings, cloneObj);
                    }

                    // Inserts the object to DB (can be overridden by each InfoClass) to do further actions.
                    cloneObj.InsertAsCloneInternal(settings, result, obj);

                    // Add translation from original object ID to cloned object ID (can be added after InsertAsCloneInternal, because this method calls Insert on clone object and therefore has new ID)
                    settings.Translations.AddIDTranslation(typeInfo.ObjectType, genObj.ObjectID, genCloneObj.ObjectID, genCloneObj.ObjectGroupID);

                    // Delete codename/display name from settings (child objects and others should generate default unique codenames).
                    string originalCodeName = settings.CodeName;
                    string originalDisplayName = settings.DisplayName;
                    settings.CodeName = null;
                    settings.DisplayName = null;

                    // Fire OnAfterInsertClone event if defined
                    if (settings.AfterCloneInsertCallback != null)
                    {
                        settings.AfterCloneInsertCallback(settings, cloneObj);
                    }

                    CloneChildren(obj, settings, result, cloneObj);

                    // Do not process other objects if object is binding
                    if (!typeInfo.IsBinding)
                    {
                        CloneBindings(obj, settings, result, cloneObj);

                        AssignCloneToSite(settings, cloneObj);

                        CloneOtherBindings(obj, settings, result, cloneObj);

                        CloneMetafiles(obj, settings, result, cloneObj);

                        TranslateSiteSpecificDependencies(obj, settings, cloneObj);
                    }

                    settings.CodeName = originalCodeName;
                    settings.DisplayName = originalDisplayName;

                    // Do the post-processing
                    cloneObj.InsertAsClonePostprocessing(settings, result, obj);

                    // Raise final user custom handler
                    if (settings.AfterCloneStructureInsertCallback != null)
                    {
                        settings.AfterCloneStructureInsertCallback(settings, cloneObj);
                    }
                }
            }


            // Log cloning to event log
            if (genCloneObj.LogEvents)
            {
                string message = string.Format("Object '{0}' has been cloned as '{1}'.",
                                               (string.IsNullOrEmpty(genObj.ObjectDisplayName) ? genObj.ObjectCodeName : genObj.ObjectDisplayName),
                                               (string.IsNullOrEmpty(genCloneObj.ObjectDisplayName) ? genCloneObj.ObjectCodeName : genCloneObj.ObjectDisplayName));

                string objectSource = genObj.TypeInfo.GetNiceObjectTypeName();

                CoreServices.EventLog.LogEvent("I", objectSource, "CLONEOBJ", message);
            }

            return cloneObj;
        }


        /// <summary>
        /// Tries to translate dependencies (only those dependencies which has a record in settings.Translations table - those are the objects cloned during the whole cloning process)
        /// This is needed when some dependency is cloned before this object is cloned
        /// </summary>
        private static void TranslateDependencies(CloneSettings settings, BaseInfo cloneObj)
        {
            ObjectTypeInfo typeInfo = cloneObj.TypeInfo;

            if (typeInfo.ObjectDependencies == null)
            {
                return;
            }

            foreach (var dep in typeInfo.ObjectDependencies)
            {
                // Try to translate the column (if the dependency is SiteObject)
                ObjectTypeInfo depTypeInfo = ObjectTypeManager.GetTypeInfo(dep.DependencyObjectType);
                if ((depTypeInfo == null) || (depTypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                {
                    continue;
                }

                // Original ID of the dependency
                int oldId = ValidationHelper.GetInteger(cloneObj.GetValue(dep.DependencyColumn), 0);
                int newId = settings.Translations.GetNewID(dep.DependencyObjectType, oldId, depTypeInfo.CodeNameColumn, TranslationHelper.AUTO_SITEID, depTypeInfo.SiteIDColumn, depTypeInfo.ParentIDColumn, depTypeInfo.GroupIDColumn);
                if (newId > 0)
                {
                    // Set translated ID if translation found, otherwise keep it as it was
                    cloneObj.SetValue(dep.DependencyColumn, newId);
                }
            }
        }


        /// <summary>
        /// Generates unique display name for a cloned object
        /// </summary>
        private static void SetCloneDisplayName(CloneSettings settings, BaseInfo cloneObj)
        {
            ObjectTypeInfo typeInfo = cloneObj.TypeInfo;

            if (typeInfo.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var name = !String.IsNullOrEmpty(settings.DisplayName) ? settings.DisplayName : cloneObj.Generalized.GetUniqueDisplayName(false);

                cloneObj.SetValue(typeInfo.DisplayNameColumn, name);
            }
        }


        /// <summary>
        /// Generates unique code name for a cloned object
        /// </summary>
        private static void SetCloneCodeName(CloneSettings settings, BaseInfo cloneObj)
        {
            ObjectTypeInfo typeInfo = cloneObj.TypeInfo;

            if (typeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (!String.IsNullOrEmpty(settings.CodeName))
                {
                    cloneObj.SetValue(typeInfo.CodeNameColumn, settings.CodeName);
                    if (!cloneObj.CheckUniqueCodeName())
                    {
                        throw new Exception("Code name has to be unique.");
                    }
                }
                else
                {
                    cloneObj.SetValue(typeInfo.CodeNameColumn, cloneObj.Generalized.GetUniqueCodeName());
                }
            }
        }


        /// <summary>
        /// Localizes resource strings in the object
        /// </summary>
        private static void LocalizeStringsInClone(BaseInfo obj, BaseInfo cloneObj)
        {
            var displayNameColumn = obj.TypeInfo.DisplayNameColumn;

            foreach (string col in cloneObj.ColumnNames)
            {
                if (col.Equals(displayNameColumn, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string content = ValidationHelper.GetString(cloneObj.GetValue(col), "").Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.StartsWith("{$", StringComparison.Ordinal))
                    {
                        cloneObj.SetValue(col, CoreServices.Localization.LocalizeString(content));
                    }
                }
            }
        }


        /// <summary>
        /// Clones object children
        /// </summary>
        private static void CloneChildren(BaseInfo obj, CloneSettings settings, CloneResult result, BaseInfo cloneObj)
        {
            // Child objects (clone only when MaxRelativeLevel is not 0, if it's less than zero, than all levels are cloned, if it's above zero, than at least direct descendants should be cloned)
            if (settings.IncludeChildren && (settings.MaxRelativeLevel != 0) && (obj.Children != null))
            {
                int originalMaxRelativeLevel = settings.MaxRelativeLevel;
                int originalParentId = settings.ParentID;

                // Decrease MaxRelativeLevel by one when going one level deep
                settings.MaxRelativeLevel = settings.MaxRelativeLevel - 1;

                // Set parent ID to new cloned object
                settings.ParentID = cloneObj.Generalized.ObjectID;

                // Disable staging task logging (for performance reasons). Main task will be logged at the end of cloning
                using (CMSActionContext childContext = new CMSActionContext())
                {
                    childContext.AllowLicenseRedirect = false;
                    childContext.LogLicenseWarnings = false;

                    foreach (var childCollection in obj.Children)
                    {
                        if (settings.ExcludedChildTypes.Contains(childCollection.ObjectType))
                        {
                            continue;
                        }

                        try
                        {
                            foreach (BaseInfo child in childCollection)
                            {
                                // Clone the child objects
                                InsertAsClone(child, settings, result);
                            }
                        }
                        catch (LicenseException)
                        {
                            // Skip. Child object type is not allowed under current license.
                        }
                    }
                }

                // Restore the original MaxRelativeLevel when children processing is done
                settings.MaxRelativeLevel = originalMaxRelativeLevel;
                settings.ParentID = originalParentId;
            }
        }


        /// <summary>
        /// Tries to translate site object dependencies (only when cloning to other site)
        /// </summary>
        private static void TranslateSiteSpecificDependencies(BaseInfo obj, CloneSettings settings, BaseInfo cloneObj)
        {
            ObjectTypeInfo typeInfo = cloneObj.TypeInfo;

            var objectSiteId = obj.Generalized.ObjectSiteID;

            if ((objectSiteId > 0) && (settings.CloneToSiteID > 0) && (settings.CloneToSiteID != objectSiteId) && (typeInfo.ObjectDependencies != null))
            {
                foreach (ObjectDependency dep in typeInfo.ObjectDependencies)
                {
                    string col = dep.DependencyColumn;
                    string objType = dep.DependencyObjectType;

                    var req = dep.DependencyType;

                    // Try to translate the column (if the dependency is SiteObject)
                    var depTypeInfo = ObjectTypeManager.GetTypeInfo(objType);

                    if ((depTypeInfo != null) && (depTypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                    {
                        // Original ID of the dependency
                        int oldId = ValidationHelper.GetInteger(cloneObj.GetValue(col), 0);

                        var depObj = ModuleManager.GetObject(objType);
                        if (depObj != null)
                        {
                            // Get the dependency object and check if it's not global object
                            depObj = depObj.GetObject(oldId);
                            if (depObj != null)
                            {
                                var genDepObj = depObj.Generalized;
                                if (genDepObj.ObjectSiteID > 0)
                                {
                                    // Get the object ID for the equivalent CodeName on the other site
                                    var parameters = new GetIDParameters
                                    {
                                        CodeName = genDepObj.ObjectCodeName,
                                        CodeNameColumn = depTypeInfo.CodeNameColumn,
                                        SiteId = settings.CloneToSiteID,
                                        SiteIdColumn = depTypeInfo.SiteIDColumn,
                                        ParentId = genDepObj.ObjectParentID,
                                        ParentIdColumn = depTypeInfo.ParentIDColumn,
                                        GroupId = genDepObj.ObjectGroupID,
                                        GroupIdColumn = depTypeInfo.GroupIDColumn
                                    };

                                    int newId = TranslationHelper.GetIDFromDB(parameters, depTypeInfo.ObjectType);
                                    if (newId == 0)
                                    {
                                        // Translation was not found
                                        if (req == ObjectDependencyEnum.NotRequired)
                                        {
                                            // Column not required, delete the value
                                            cloneObj.SetValue(col, null);
                                        }
                                        else
                                        {
                                            // Required without specified default value => cannot be cloned to other site
                                            throw new Exception("[BaseInfo.InsertAsClone]: Cannot clone object to other site because one of his dependencies does not have proper site equivalent.");
                                        }
                                    }
                                    else
                                    {
                                        // Set translated ID
                                        cloneObj.SetValue(col, newId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Clears the fields that should be populated with a new value in the cloned object
        /// </summary>
        private static void ClearCloneFields(BaseInfo cloneObj)
        {
            ObjectTypeInfo typeInfo = cloneObj.TypeInfo;

            // Delete the ID column
            if (typeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                cloneObj.SetValue(typeInfo.IDColumn, null);
            }

            // Delete GUID column
            if (typeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                cloneObj.SetValue(typeInfo.GUIDColumn, null);
            }

            // Delete TimeStamp column
            if (typeInfo.TimeStampColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                cloneObj.SetValue(typeInfo.TimeStampColumn, DateTime.Now);
            }
        }


        /// <summary>
        /// Creates site binding for the cloned object
        /// </summary>
        private static void AssignCloneToSite(CloneSettings settings, BaseInfo cloneObj)
        {
            ObjectTypeInfo typeInfo = cloneObj.TypeInfo;

            if (!String.IsNullOrEmpty(typeInfo.SiteBinding) && (settings.AssignToSiteID > 0))
            {
                var siteBinding = ModuleManager.GetObject(typeInfo.SiteBinding);
                var genSiteBinding = siteBinding.Generalized;

                genSiteBinding.ObjectParentID = cloneObj.Generalized.ObjectID;
                genSiteBinding.ObjectSiteID = settings.AssignToSiteID;

                if (genSiteBinding.GetExisting() == null)
                {
                    siteBinding.Insert();
                }
            }
        }


        /// <summary>
        /// Clones the object metafiles
        /// </summary>
        private static void CloneMetafiles(BaseInfo obj, CloneSettings settings, CloneResult result, BaseInfo cloneObj)
        {
            if (settings.IncludeMetafiles && (obj.MetaFiles != null))
            {
                // Set parent ID to new cloned object
                int originalParentId = settings.ParentID;
                settings.ParentID = cloneObj.Generalized.ObjectID;

                foreach (BaseInfo metafile in obj.MetaFiles)
                {
                    // Clone metafiles
                    InsertAsClone(metafile, settings, result);
                }

                settings.ParentID = originalParentId;
            }
        }


        /// <summary>
        /// Clones object other bindings
        /// </summary>
        private static void CloneOtherBindings(BaseInfo obj, CloneSettings settings, CloneResult result, BaseInfo cloneObj)
        {
            if (settings.IncludeOtherBindings && (obj.OtherBindings != null))
            {
                int cloneId = cloneObj.Generalized.ObjectID;

                // Set parent ID to 0, we need to change different column
                int originalParentId = settings.ParentID;
                settings.ParentID = 0;

                using (CMSActionContext childContext = new CMSActionContext())
                {
                    childContext.AllowLicenseRedirect = false;
                    childContext.LogLicenseWarnings = false;

                    foreach (var bindingCollection in obj.OtherBindings)
                    {
                        if (bindingCollection.TypeInfo.ObjectDependencies != null)
                        {
                            string foreignKeyColumn = ValidationHelper.GetString(bindingCollection.TypeInfo.ObjectDependencies.First().DependencyColumn, "");

                            bool allowed = !settings.ExcludedOtherBindingTypes.Contains(bindingCollection.ObjectType);
                            if (allowed)
                            {
                                // Include only bindings which are not site bindings (these are cloned separately afterwards)
                                if (AllowCloneToOtherSite(obj, settings, bindingCollection.ObjectType, true))
                                {
                                    try
                                    {
                                        foreach (BaseInfo binding in bindingCollection)
                                        {
                                            // Change the foreign key to newly cloned object
                                            BaseInfo bindingClone = binding.CloneObject();

                                            bindingClone.SetValue(foreignKeyColumn, cloneId);

                                            InsertAsClone(bindingClone, settings, result);
                                        }
                                    }
                                    catch (LicenseException)
                                    {
                                        // Skip. Binding object type is not allowed under current license.
                                    }
                                }
                            }
                        }
                    }
                }

                settings.ParentID = originalParentId;
            }
        }


        /// <summary>
        /// Clones object bindings
        /// </summary>
        public static void CloneBindings(BaseInfo obj, CloneSettings settings, CloneResult result, BaseInfo cloneObj)
        {
            if ((settings.IncludeBindings || settings.IncludeSiteBindings) && (obj.Bindings != null))
            {
                // Set parent ID to new cloned object
                int originalParentId = settings.ParentID;
                settings.ParentID = cloneObj.Generalized.ObjectID;

                // Disable staging task logging (for performance reasons). Main task will be logged at the end of cloning
                using (CMSActionContext childContext = new CMSActionContext())
                {
                    childContext.AllowLicenseRedirect = false;
                    childContext.LogLicenseWarnings = false;

                    foreach (var bindingCollection in obj.Bindings)
                    {
                        bool allowed = !settings.ExcludedBindingTypes.Contains(bindingCollection.ObjectType);
                        if (allowed)
                        {
                            if ((settings.IncludeBindings && !bindingCollection.TypeInfo.IsSiteBinding) ||
                                (settings.IncludeSiteBindings && bindingCollection.TypeInfo.IsSiteBinding))
                            {
                                // Include only bindings which are not site bindings (these are cloned separately afterwards)
                                if (AllowCloneToOtherSite(obj, settings, bindingCollection.ObjectType, false))
                                {
                                    try
                                    {
                                        // Clone the binding objects
                                        foreach (BaseInfo binding in bindingCollection)
                                        {
                                            InsertAsClone(binding, settings, result);
                                        }
                                    }
                                    catch (LicenseException)
                                    {
                                        // Skip. Binding object type is not allowed under current license.
                                    }
                                }
                            }
                        }
                    }
                }

                settings.ParentID = originalParentId;
            }
        }


        /// <summary>
        /// Determines if given collection of binding can be cloned (returns false for bindings of site objects when cloning to other site is requested).
        /// </summary>
        /// <param name="obj">Cloned object</param>
        /// <param name="settings">Clone settings</param>
        /// <param name="bindingObjectType">Object type of the binding object type</param>
        /// <param name="isOtherBinding">Determines whether it's other binding or binding</param>
        private static bool AllowCloneToOtherSite(BaseInfo obj, CloneSettings settings, string bindingObjectType, bool isOtherBinding)
        {
            var objectSiteId = obj.Generalized.ObjectSiteID;

            // For site object do not clone bindings to other site objects 
            if ((objectSiteId > 0) && (settings.CloneToSiteID > 0) && (settings.CloneToSiteID != objectSiteId))
            {
                var bindingInfo = ModuleManager.GetObject(bindingObjectType);
                if (bindingInfo != null)
                {
                    var bindingTypeInfo = bindingInfo.TypeInfo;

                    var bindingDependencies = bindingTypeInfo.ObjectDependencies;
                    if (bindingDependencies != null)
                    {
                        var firstDependency = bindingDependencies.FirstOrDefault();
                        if (firstDependency != null)
                        {
                            string bindingObjType = isOtherBinding ? bindingTypeInfo.ParentObjectType : firstDependency.DependencyObjectType;

                            var bindingObj = ModuleManager.GetObject(bindingObjType);
                            if (((bindingObj != null) && (bindingObj.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)) || !bindingTypeInfo.SupportsCloneToOtherSite)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
