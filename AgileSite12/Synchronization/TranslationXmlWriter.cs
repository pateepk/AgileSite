using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Synchronization
{
    /// <summary>
    /// Writes registered translations in XML format
    /// </summary>
    internal class TranslationXmlWriter
    {
        /// <summary>
        /// Recorded translations to be included to the output
        /// </summary>
        private Dictionary<string, ISet<int>> Translations
        {
            get;
            set;
        }


        /// <summary>
        /// Translation helper instance
        /// </summary>
        private TranslationHelper TranslationHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Output writer
        /// </summary>
        private StreamWriter Writer
        {
            get;
            set;
        }


        /// <summary>
        /// Creates new instance of <see cref="TranslationXmlWriter"/>.
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="translationHelper">Translation helper</param>
        public TranslationXmlWriter(StreamWriter writer, TranslationHelper translationHelper)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (translationHelper == null)
            {
                throw new ArgumentNullException("translationHelper");
            }

            Writer = writer;

            TranslationHelper = translationHelper;
            Translations = new Dictionary<string, ISet<int>>(StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Writes the recorded translations to the output
        /// </summary>
        public void WriteTranslations()
        {
            RegisterCollectedTranslations();

            if (!DataHelper.DataSourceIsEmpty(TranslationHelper.TranslationTable))
            {
                Writer.WriteLine(TranslationHelper.GetTranslationsXml(false));
            }
        }


        /// <summary>
        /// Registers required translations for the given object
        /// </summary>
        /// <param name="info">Info object</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/></exception>
        public void RegisterObjectTranslations(GeneralizedInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            var ti = info.TypeInfo;

            // Site translation
            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterTranslation(info, ti.SiteIDColumn, PredefinedObjectType.SITE);
            }

            // Parent object translation
            if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterTranslation(info, ti.ParentIDColumn, info.ParentObjectType);
            }

            // Dependency translation
            if (ti.ObjectDependencies != null)
            {
                foreach (var dep in ti.ObjectDependencies)
                {
                    string dependencyType = info.GetDependencyObjectType(dep);

                    if (!string.IsNullOrEmpty(dependencyType))
                    {
                        RegisterTranslation(info, dep.DependencyColumn, dependencyType);
                    }
                }
            }

            if (ColumnsTranslationEvents.RegisterRecords.IsBound)
            {
                ColumnsTranslationEvents.RegisterRecords.StartEvent(TranslationHelper, ti.ObjectType, info);
            }
        }


        /// <summary>
        /// Adds given ID into the translation table to correct list according to objectType
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        private void AddTranslation(string objectType, int id)
        {
            ISet<int> ids;

            // Ensure list for the given object type
            if (!Translations.TryGetValue(objectType, out ids))
            {
                ids = new HashSet<int>();
                Translations.Add(objectType, ids);
            }

            ids.Add(id);
        }


        /// <summary>
        /// Registers requested translations to the translation helper of settings. Ensures the translation helper within settings if necessary.
        /// Clears the collected translations once they are registered.
        /// </summary>
        private void RegisterCollectedTranslations()
        {
            if (Translations.Count == 0)
            {
                return;
            }

            foreach (string objectType in Translations.Keys)
            {
                var ids = Translations[objectType].ToList();
                string siteName = (objectType == "cms.site" ? null : TranslationHelper.AUTO_SITENAME);

                TranslationHelper.RegisterRecords(ids, objectType, siteName);
            }

            Translations.Clear();
        }


        /// <summary>
        /// Handles FK ID (can be used to fill translation helper for example).
        /// </summary>
        /// <param name="obj">Object (TreeNode / InfoObject) to export</param>
        /// <param name="columnName">Column name of the dependency</param>
        /// <param name="objectType">Object type of the dependency</param>
        private void RegisterTranslation(GeneralizedInfo obj, string columnName, string objectType)
        {
            var id = ValidationHelper.GetInteger(obj.GetValue(columnName), 0);
            if (id > 0)
            {
                AddTranslation(objectType, id);
            }
        }
    }
}
