using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page class for the page which is used for macro resigning and should be available for global administrators only.
    /// </summary>
    public class SystemMacroPage : GlobalAdminPage
    {
        private const string EVENTLOG_SOURCE_REFRESHSECURITYPARAMS = "Macros - Refresh security parameters";


        /// <summary>
        /// Refreshes the security parameters in macros for all the objects of the specified object types.
        /// Signs all the macros with the current user if the old salt is not specified.
        /// </summary>
        /// <param name="objectTypes">Collection of object types that can contain macros</param>
        /// <param name="oldSalt">Old salt </param>
        /// <param name="newSalt">New salt</param>
        /// <param name="refreshAll">If enabled the process skips signature integrity check and re-signs all macros</param>
        /// <param name="useCurrentSalt">Indicates that current salt is used for resigning</param>
        /// <param name="logProcess">Action that provides logging of given message</param>
        [CanDisableLicenseCheck("huFegsIDJu7qhX488nqin9p5Fx2MkSFU2b2Gw3uKQ4QA9bV07ynKHaRttPxvGOIWWZ3dGG4wY0lFwBzEiI7iTQ==")]
        protected void RefreshSecurityParams(IEnumerable<string> objectTypes, string oldSalt, string newSalt, bool refreshAll, bool useCurrentSalt, Action<string> logProcess)
        {
            var oldSaltSpecified = !string.IsNullOrEmpty(oldSalt) && !refreshAll;
            var newSaltSpecified = !string.IsNullOrEmpty(newSalt) && !useCurrentSalt;
            var objectTypeList = objectTypes.ToList();
            var processedObjects = new NameValueCollection(objectTypeList.Count);

            using (new CMSActionContext { LogEvents = false, LogSynchronization = false })
            {
                var processingString = GetString("macros.refreshsecurityparams.processing");

                foreach (var objectType in objectTypeList)
                {
                    var niceObjectType = GetNiceObjectTypeName(objectType);

                    logProcess(string.Format(processingString, niceObjectType));

                    try
                    {
                        var infos = new InfoObjectCollection(objectType)
                        {
                            PageSize = 1000
                        };

                        var csi = infos.TypeInfo.ClassStructureInfo;
                        var orderByIndex = FindOrderByIndex(csi);
                        if (orderByIndex != null)
                        {
                            infos.OrderByColumns = orderByIndex.GetOrderBy();
                        }

                        // Skip object types derived from general data class object type to avoid duplicities
                        if (infos.TypeInfo.OriginalObjectType.Equals(DataClassInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase)
                            && !infos.TypeInfo.ObjectType.Equals(DataClassInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var identityOption = (!oldSaltSpecified) ? MacroIdentityOption.FromUserInfo(CurrentUser) : null;

                        foreach (var info in infos)
                        {
                            try
                            {
                                bool refreshed;
                                if (oldSaltSpecified)
                                {
                                    refreshed = MacroSecurityProcessor.RefreshSecurityParameters(info, oldSalt, newSaltSpecified ? newSalt : ValidationHelper.HashStringSalt, true);
                                }
                                else
                                {
                                    if (refreshAll && newSaltSpecified)
                                    {
                                        // Do not check integrity, but use new salt
                                        refreshed = MacroSecurityProcessor.RefreshSecurityParameters(info, identityOption, true, newSalt);
                                    }
                                    else
                                    {
                                        // Do not check integrity, sign everything with current user
                                        refreshed = MacroSecurityProcessor.RefreshSecurityParameters(info, identityOption, true);
                                    }
                                }

                                if (refreshed)
                                {
                                    var objectName = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(info.Generalized.ObjectDisplayName));
                                    processedObjects.Add(niceObjectType, objectName);
                                }
                            }
                            catch (Exception ex)
                            {
                                string message = $"Signing {niceObjectType} {info.Generalized.ObjectDisplayName} failed: {EventLogProvider.GetExceptionLogMessage(ex)}";

                                using (new CMSActionContext { LogEvents = true })
                                {
                                    EventLogProvider.LogEvent(EventType.ERROR, EVENTLOG_SOURCE_REFRESHSECURITYPARAMS, "ERROR", message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logProcess(ex.Message);

                        using (new CMSActionContext { LogEvents = true })
                        {
                            EventLogProvider.LogException(EVENTLOG_SOURCE_REFRESHSECURITYPARAMS, "ERROR", ex);
                        }
                    }
                }
            }

            LogInformationEvent("PROCESSEDOBJECTS", GetProcessedObjectsForEventLog(processedObjects));
        }


        /// <summary>
        /// Returns nice name for given object type.
        /// </summary>
        private static string GetNiceObjectTypeName(string objectType)
        {
            var objectTypeResourceKey = TypeHelper.GetObjectTypeResourceKey(objectType);

            var niceObjectType = GetString(objectTypeResourceKey);
            if (niceObjectType.Equals(objectTypeResourceKey, StringComparison.OrdinalIgnoreCase))
            {
                if (objectType.StartsWith("bizformitem.bizform.", StringComparison.OrdinalIgnoreCase))
                {
                    var dci = DataClassInfoProvider.GetDataClassInfo(objectType.Substring(PredefinedObjectType.BIZFORM_ITEM_PREFIX.Length));
                    if (dci != null)
                    {
                        niceObjectType = "on-line form " + ResHelper.LocalizeString(dci.ClassDisplayName);
                    }
                }
                else
                {
                    niceObjectType = objectType;
                }
            }
            return niceObjectType;
        }


        /// <summary>
        /// Finds suitable index for order by statement.
        /// </summary>
        private static Index FindOrderByIndex(ClassStructureInfo classStructureInfo)
        {
            var indexes = classStructureInfo.GetTableIndexes();
            if (indexes == null)
            {
                return null;
            }

            // Clustered index has the best performance for paging but when not unique, stable result sets are not guaranteed over individual paging queries
            var clusteredIndex = indexes.GetClusteredIndex();
            if ((clusteredIndex != null) && clusteredIndex.IsUnique)
            {
                return clusteredIndex;
            }

            // Fall back to primary key index and then any index which is better than paging over non-indexed columns
            return indexes.GetPrimaryKeyIndex() ?? indexes.GetIndexes().FirstOrDefault();
        }


        /// <summary>
        /// Writes given information to the event log.
        /// </summary>
        /// <param name="eventCode">Event code</param>
        /// <param name="eventDescription">Additional event information</param>
        protected static void LogInformationEvent(string eventCode, string eventDescription = null)
        {
            EventLogProvider.LogInformation(EVENTLOG_SOURCE_REFRESHSECURITYPARAMS, eventCode, eventDescription);
        }


        /// <summary>
        /// Gets the list of processed objects formatted for use in the event log.
        /// </summary>
        /// <param name="processedObjects">Collection of processed objects to be serialized</param>
        private static string GetProcessedObjectsForEventLog(NameValueCollection processedObjects)
        {
            return processedObjects?.AllKeys.SelectMany(processedObjects.GetValues, (k, v) => $"{k} '{v}'").Join(Environment.NewLine);
        }
    }
}
