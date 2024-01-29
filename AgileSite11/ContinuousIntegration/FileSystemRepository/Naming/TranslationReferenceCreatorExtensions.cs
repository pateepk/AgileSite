using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// <see cref="TranslationReferenceLoader"/> extensions allowing to create <see cref="ExtendedTranslationReference"/>.
    /// </summary>
    internal static class TranslationReferenceLoaderExtensions
    {
        /// <summary>
        /// Creates translation reference from given base info object.
        /// </summary>
        /// <param name="loader"><see cref="TranslationReferenceLoader"/> instance</param>
        /// <param name="info">Base info object</param>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="loader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="DataEngine.TranslationHelper.UseAdditionalFields"/> of current <see cref="TranslationHelper"/> returns false.</exception>
        internal static ExtendedTranslationReference LoadExtendedFromInfoObject(this TranslationReferenceLoader loader, BaseInfo info)
        {
            if (loader == null)
            {
                throw new ArgumentNullException("loader");
            }

            var translationHelper = (ContinuousIntegrationTranslationHelper)loader.TranslationHelper;

            if (!translationHelper.UseAdditionalFields)
            {
                throw new InvalidOperationException("[ExtendedTranslationReference.LoadExtendedFromDatabase]: Provided TranslationHelper does not use additional fields which are necessary for extended translation reference loading.");
            }

            if (info == null)
            {
                // No translation available
                return null;
            }

            var translationReference = new ExtendedTranslationReference
            {
                ObjectType = info.TypeInfo.ObjectType,
                CodeName = (info.TypeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? info.Generalized.ObjectCodeName : null,
                GUID = (info.TypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? info.Generalized.ObjectGUID : default(Guid),

                // BaseInfo.Site property returns site of parent when current object doesn't have site column. 
                // Register site in translation reference only when current object has site column. Site is always cached. Obtain from provider.
                ExtendedSite = (info.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? loader.LoadExtendedFromInfoObject(info.Site) : null,
                ExtendedParent = loader.LoadExtendedFromDatabase(info.TypeInfo.ParentObjectType, info.GetIntegerValue(info.TypeInfo.ParentIDColumn, 0)),
                ExtendedGroup = loader.LoadExtendedFromDatabase(PredefinedObjectType.GROUP, info.GetIntegerValue(info.TypeInfo.GroupIDColumn, 0)),
                TranslationHelper = translationHelper,
                TypeInfo = info.TypeInfo
            };

            translationReference.AdditionalFields = translationHelper.GetAdditionalFieldsData(info);
            
            loader.LoadExtendedCategory(translationReference);
            loader.LoadExtendedDependencies(translationReference);

            return translationReference;
        }


        /// <summary>
        /// Loads translation reference for requested object. The translation reference is accompanied with data from additional fields
        /// as specified by <see cref="DataEngine.TranslationHelper.GetAdditionalFieldNames"/>.
        /// </summary>
        /// <param name="loader"><see cref="TranslationReferenceLoader"/> instance</param>
        /// <param name="objectType">Type of object</param>
        /// <param name="objectId">Database identifier of object</param>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="loader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="DataEngine.TranslationHelper.UseAdditionalFields"/> of current <see cref="TranslationHelper"/> returns false.</exception>
        internal static ExtendedTranslationReference LoadExtendedFromDatabase(this TranslationReferenceLoader loader, string objectType, int objectId)
        {
            if (loader == null)
            {
                throw new ArgumentNullException("loader");
            }

            var translationHelper = (ContinuousIntegrationTranslationHelper)loader.TranslationHelper;

            if (!translationHelper.UseAdditionalFields)
            {
                throw new InvalidOperationException("[ExtendedTranslationReference.LoadExtendedFromDatabase]: Provided TranslationHelper does not use additional fields which are necessary for extended translation reference loading.");
            }
            
            if (String.IsNullOrEmpty(objectType) || (objectId <= 0))
            {
                // No translation available
                return null;
            }

            var record = translationHelper.GetTranslationRecord(objectType, objectId);
            if (record == null)
            {
                // No translation available
                return null;
            }

            var translationReference = new ExtendedTranslationReference
            {
                ObjectType = ValidationHelper.GetString(record[TranslationHelper.RECORD_OBJECT_TYPE_COLUMN], objectType),
                CodeName = ValidationHelper.GetString(record[TranslationHelper.RECORD_CODE_NAME_COLUMN], null),
                GUID = ValidationHelper.GetGuid(record[TranslationHelper.RECORD_GUID_COLUMN], default(Guid)),
                // Site is always cached. Obtain from provider.
                ExtendedSite = loader.LoadExtendedFromInfoObject(ProviderHelper.GetInfoByName(PredefinedObjectType.SITE, ValidationHelper.GetString(record[TranslationHelper.RECORD_SITE_NAME_COLUMN], null))),
                ExtendedGroup = loader.LoadExtendedFromDatabase(PredefinedObjectType.GROUP, ValidationHelper.GetInteger(record[TranslationHelper.RECORD_GROUP_ID_COLUMN], 0)),
                TranslationHelper = translationHelper
            };

            var additionalFields = record[TranslationHelper.RECORD_ADDITIONAL_FIELDS_COLUMN] as IDictionary<string, object>;
            if (additionalFields != null)
            {
                translationReference.AdditionalFields = additionalFields;
            }

            // Parent
            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            
            loader.LoadExtendedCategory(translationReference);
            loader.LoadExtendedDependencies(translationReference);

            if (typeInfo.IsMainObject)
            {
                // Type of provided object is main type, so it can have no parent (other) type (that would affect object's identification)
                return translationReference;
            }

            var parentId = ValidationHelper.GetInteger(record[TranslationHelper.RECORD_PARENT_ID_COLUMN], 0);
            var parentTypeInfo = ObjectTypeManager.GetTypeInfo(translationReference.ObjectType, true);
            if (!String.IsNullOrEmpty(parentTypeInfo.ParentObjectType) && (parentId > 0))
            {
                translationReference.ExtendedParent = loader.LoadExtendedFromDatabase(parentTypeInfo.ParentObjectType, parentId);
            }

            return translationReference;
        }


        /// <summary>
        /// Loads object's category translation reference.
        /// </summary>
        /// <param name="loader"><see cref="TranslationReferenceLoader"/> instance</param>
        /// <param name="translationReference"><see cref="ExtendedTranslationReference"/> which <see cref="ExtendedTranslationReference.ExtendedCategory"/> will be loaded</param>
        private static void LoadExtendedCategory(this TranslationReferenceLoader loader, ExtendedTranslationReference translationReference)
        {
            var typeInfo = translationReference.TypeInfo;
            var category = typeInfo.CategoryObject;

            if (category == null)
            {
                return;
            }

            translationReference.ExtendedCategory = loader.GetDependentObjectReference(translationReference, category.TypeInfo.ObjectType, typeInfo.CategoryIDColumn);
        }


        /// <summary>
        /// Loads object's additional filter dependencies translation references.
        /// </summary>
        /// <param name="loader"><see cref="TranslationReferenceLoader"/> instance</param>
        /// <param name="translationReference"><see cref="ExtendedTranslationReference"/> which <see cref="ExtendedTranslationReference.ExtendedFilterDependencies"/> will be loaded</param>
        private static void LoadExtendedDependencies(this TranslationReferenceLoader loader, ExtendedTranslationReference translationReference)
        {
            translationReference.ExtendedFilterDependencies = new List<ExtendedTranslationReference>();

            foreach (var dependency in translationReference.TypeInfo.ContinuousIntegrationSettings.FilterDependencies)
            {
                translationReference.ExtendedFilterDependencies.Add(loader.GetDependentObjectReference(translationReference, dependency.DependencyObjectType, dependency.DependencyColumn));
            }
        }


        /// <summary>
        /// Gets translation reference for dependent object specified by parameters.
        /// </summary>
        /// <param name="loader"><see cref="TranslationReferenceLoader"/> instance</param>
        /// <param name="translationReference"><see cref="ExtendedTranslationReference"/> with dependent objects' IDs</param>
        /// <param name="objectType">Object type of loaded object</param>
        /// <param name="columnName">Name of the column holding the loaded object's ID</param>
        private static ExtendedTranslationReference GetDependentObjectReference(this TranslationReferenceLoader loader, ExtendedTranslationReference translationReference, string objectType, string columnName)
        {
            object dependencyIdRaw;
            if (translationReference.AdditionalFields.TryGetValue(columnName, out dependencyIdRaw))
            {
                if (Convert.IsDBNull(dependencyIdRaw))
                {
                    return null;
                }

                var dependencyId = Convert.ToInt32(dependencyIdRaw);
                if (dependencyId > 0)
                {
                    return loader.LoadExtendedFromDatabase(objectType, dependencyId);
                }
            }

            return null;
        }
    }
}
