using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.FormEngine;
using CMS.Modules;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Helper methods for document types
    /// </summary>
    public class DocumentTypeHelper
    {
        #region "Variables"

        /// <summary>
        /// Table of resolved class names [classNames -> resolved]
        /// </summary>
        private static readonly CMSStatic<SafeDictionary<string, string>> mClassNames = new CMSStatic<SafeDictionary<string, string>>(() => new SafeDictionary<string, string>());

        #endregion


        #region "Properties"

        /// <summary>
        /// Table of resolved class names [classNames -> resolved]
        /// </summary>
        private static SafeDictionary<string, string> ClassNames
        {
            get
            {
                return mClassNames;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears the class names resolving cache.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        internal static void ClearClassNames(bool logTasks)
        {
            ClassNames.Clear();

            // Create webfarm task if needed
            if (logTasks)
            {
                WebFarmHelper.CreateTask(new ClearResolvedClassNamesWebFarmTask());
            }
        }



        /// <summary>
        /// Synchronizes site bindings with the given class.
        /// </summary>
        /// <param name="info">Document type</param>>
        public static void SynchronizeSiteBindingsWithResource(DocumentTypeInfo info)
        {
            if ((info != null) && (info.ClassResourceID != 0))
            {
                ClassSiteInfoProvider.DeleteClassSiteInfos(info.ClassID);

                var siteIDs = ResourceSiteInfoProvider.GetResourceSites()
                    .Column("SiteID")
                    .WhereEquals("ResourceID", info.ClassResourceID)
                    .GetListResult<int>()
                    .ToList();
                siteIDs.ForEach(siteID => ClassSiteInfoProvider.AddClassToSite(info.ClassID, siteID));
            }
        }


        /// <summary>
        /// Resolves the document type classes, if the input value is constant TreeProvider.ALL_CLASSES, returns all class names used on any site, otherwise resolves * as a wildcard in class name to get all matching. Supports the list of classes separated by semicolon.
        /// </summary>
        /// <param name="classNames">Class names value</param>
        /// <param name="siteName">Site name</param>
        public static string GetClassNames(string classNames, string siteName = null)
        {
            // If empty, do not process
            if (String.IsNullOrEmpty(classNames))
            {
                return classNames;
            }

            // If not all and not wildcard, do not process
            bool all = (classNames == DataClassInfoProvider.ALL_CLASSNAMES);
            if (!all && !classNames.Contains("*"))
            {
                return classNames;
            }

            // Try to get from cache, append site name if macro is used because each site can have different page types assigned
            string cacheKey = GetCacheKey(classNames, siteName);
            string result = ClassNames[cacheKey];
            if (result != null)
            {
                return result;
            }

            // Prepare the where condition
            var classes = new List<string>();
            var where = new WhereCondition();

            bool getClasses = true;

            if (!all)
            {
                // Prepare the where condition
                getClasses = false;
                string[] names = classNames.Split(';');

                foreach (string className in names)
                {
                    // skip empty class name
                    if (!String.IsNullOrEmpty(className.Trim()))
                    {
                        if (className.Contains("*"))
                        {
                            // Class name by pattern
                            string pattern = SqlHelper.EscapeLikeText(SqlHelper.EscapeQuotes(className)).Replace("*", "%");

#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                            where.Or().WhereLike("ClassName", pattern);
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.

                            getClasses = true;
                        }
                        else
                        {
                            // Add a specific class
                            classes.Add(className);
                        }
                    }
                }
            }

            // Load class names
            if (getClasses)
            {
                // Add site ID where condition
                int siteId = ProviderHelper.GetId(PredefinedObjectType.SITE, siteName);
                if (siteId > 0)
                {
                    where = new WhereCondition()
                            .WhereEquals("NodeSiteID", siteId)
                            .And()
                            .Where(where);
                }

                var usedClasses = GetClassNames(where);

                classes.AddRange(usedClasses);
            }

            // Save the result to the cache
            result = classes.Join(";");

            ClassNames[cacheKey] = result;

            return result;
        }


        /// <summary>
        /// Gets the class names of documents based on the given where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        public static IEnumerable<string> GetClassNames(IWhereCondition where)
        {
            return
                new DocumentQuery()
                    .Distinct()
                    .Column("ClassName")
                    .Where(where)
                    .GetListResult<string>();
        }


        /// <summary>
        /// Returns the document type classes DataSet.
        /// </summary>
        public static ObjectQuery<DataClassInfo> GetDocumentTypeClasses()
        {
            return DataClassInfoProvider.GetClasses().WithObjectType(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE);
        }


        /// <summary>
        /// Removes fields from page type.
        /// </summary>
        /// <param name="classInfo">Page type to update</param>
        /// <param name="fieldNames">Field names to remove</param>
        public static void RemoveColumnsFromPageType(DataClassInfo classInfo, params string[] fieldNames)
        {
            if ((classInfo == null) || !classInfo.ClassIsDocumentType || (fieldNames.Length == 0))
            {
                return;
            }

            var formInfo = new FormInfo(classInfo.ClassFormDefinition);

            var update = formInfo.RemoveFields(field => fieldNames.Contains(field.Name));
            if (!update)
            {
                // There is no need to update page type, no fields were removed
                return;
            }

            // Update definitions
            classInfo.ClassFormDefinition = formInfo.GetXmlDefinition();
            classInfo.Update();

            // Clear default queries for this page type
            QueryInfoProvider.ClearDefaultQueries(classInfo, true, true);

            // Update alternative forms of form class
            foreach (var fieldName in fieldNames)
            {
                FormHelper.RemoveFieldFromAlternativeForms(classInfo, fieldName, 0);
            }

            FormHelper.UpdateInheritedClasses(classInfo);
        }


        /// <summary>
        /// Returns key to access cached data for given class names and site name.
        /// </summary>
        /// <param name="classNames">Class names</param>
        /// <param name="siteName">Site name</param>
        private static string GetCacheKey(string classNames, string siteName)
        {
            return String.Format("{0}|{1}", classNames.ToLowerCSafe(), (siteName != null) ? siteName.ToLowerCSafe() : String.Empty);
        }

        #endregion
    }
}
