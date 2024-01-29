using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using CMS.DataEngine.Serialization;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Loader allowing to instantiate <see cref="TranslationReference"/> from various sources.
    /// </summary>
    public class TranslationReferenceLoader
    {
        private readonly IDictionary<string, XmlSerializer> deserializerCache = new ConcurrentDictionary<string, XmlSerializer>();


        /// <summary>
        /// Gets the current <see cref="TranslationHelper"/> instance.
        /// </summary>
        internal TranslationHelper TranslationHelper
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="translationHelper">Current translation helper</param>
        internal TranslationReferenceLoader(TranslationHelper translationHelper = null)
        {
            TranslationHelper = translationHelper ?? new TranslationHelper();
        }


        /// <summary>
        /// Returns <see cref="TranslationReference"/> instance built from information contained in given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">Element containing all the translation information.</param>
        public TranslationReference LoadFromElement(XmlElement element)
        {
            if (element == null)
            {
                // no element, no reference
                return null;
            }

            XmlSerializer deserializer;
            if (!deserializerCache.TryGetValue(element.Name, out deserializer))
            {
                deserializer = new XmlSerializer(typeof(TranslationReference), new XmlRootAttribute(element.Name));
                deserializerCache[element.Name] = deserializer;
            }

            TranslationReference result;
            using (var reader = new StringReader(element.OuterXml))
            {
                result = (TranslationReference)deserializer.Deserialize(reader);
            }

            // No object type read, no reference
            return String.IsNullOrEmpty(result.ObjectType)
                ? null
                : result;
        }


        /// <summary>
        /// Creates translation reference from given base info object.
        /// </summary>
        /// <param name="info">Base info object</param>
        public TranslationReference LoadFromInfoObject(BaseInfo info)
        {
            if (info == null)
            {
                // No translation available
                return null;
            }

            var typeInfo = info.TypeInfo;
            var siteInfo = (info.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? LoadFromInfoObject(info.Site) : null;

            // Check site ID
            if ((typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && !typeInfo.SupportsGlobalObjects && (siteInfo == null))
            {
                throw new InvalidOperationException("Missing site ID value for object " + info.Generalized.ObjectDisplayName + ". Object type does not support global objects.");
            }

            var translationReference = new TranslationReference
            {
                ObjectType = typeInfo.ObjectType,

                // BaseInfo.Site property returns site of parent when current object doesn't have site column. 
                // Register site in translation reference only when current object has site column. Site is always cached. Obtain from provider.
                Site = siteInfo,
                Group = LoadFromDatabase(PredefinedObjectType.GROUP, info.GetIntegerValue(info.TypeInfo.GroupIDColumn, 0)),
            };

            // Load code name
            if (typeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                translationReference.CodeName = info.Generalized.ObjectCodeName;
                if (String.IsNullOrEmpty(translationReference.CodeName))
                {
                    throw new InvalidOperationException("Missing code name value for object " + info.Generalized.ObjectDisplayName);
                }
            }

            // Load GUID
            if (typeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                translationReference.GUID = info.Generalized.ObjectGUID;
                if (translationReference.GUID == Guid.Empty)
                {
                    throw new InvalidOperationException("Missing GUID value for object " + info.Generalized.ObjectDisplayName);
                }
            }

            // Parent
            if (!typeInfo.IsMainObject)
            {
                // Type of provided object is main type, so it can have no parent (other) type (that would affect object's identification)
                translationReference.Parent = LoadFromDatabase(typeInfo.ParentObjectType, info.GetIntegerValue(typeInfo.ParentIDColumn, 0));
            }

            return translationReference;
        }


        /// <summary>
        /// Creates translation reference for requested object.
        /// </summary>
        /// <param name="objectType">Type of object</param>
        /// <param name="objectId">Database identifier of object</param>
        public TranslationReference LoadFromDatabase(string objectType, int objectId)
        {
            if (String.IsNullOrEmpty(objectType) || (objectId <= 0))
            {
                // No translation available
                return null;
            }

            var record = TranslationHelper.GetTranslationRecord(objectType, objectId);
            if (record == null)
            {
                // No translation available
                return null;
            }

            var siteInfo = ProviderHelper.GetInfoByName(PredefinedObjectType.SITE, ValidationHelper.GetString(record[TranslationHelper.RECORD_SITE_NAME_COLUMN], null));
            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);

            // Check site ID
            if ((typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && !typeInfo.SupportsGlobalObjects && (siteInfo == null))
            {
                throw new InvalidOperationException("Missing site ID value for object " + objectType + " (" + objectId + "). Object type does not support global objects.");
            }

            var translationReference = new TranslationReference
            {
                ObjectType = ValidationHelper.GetString(record[TranslationHelper.RECORD_OBJECT_TYPE_COLUMN], objectType),
            
                // Site is always cached. Obtain from provider.
                Site = LoadFromInfoObject(siteInfo),
                Group = LoadFromDatabase(PredefinedObjectType.GROUP, ValidationHelper.GetInteger(record[TranslationHelper.RECORD_GROUP_ID_COLUMN], 0))
            };

            // Load code name
            if (typeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                translationReference.CodeName = ValidationHelper.GetString(record[TranslationHelper.RECORD_CODE_NAME_COLUMN], null);
                if (String.IsNullOrEmpty(translationReference.CodeName))
                {
                    throw new InvalidOperationException("Missing code name value for object " + objectType + " (" + objectId + ")");
                }
            }

            // Load GUID
            if (typeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                translationReference.GUID = ValidationHelper.GetGuid(record[TranslationHelper.RECORD_GUID_COLUMN], default(Guid));
                if (translationReference.GUID == Guid.Empty)
                {
                    throw new InvalidOperationException("Missing GUID value for object " + objectType + " (" + objectId + ")");
                }
            }
            
            // Parent
            if (typeInfo.IsMainObject)
            {
                // Type of provided object is main type, so it can have no parent (other) type (that would affect object's identification)
                return translationReference;
            }

            var parentId = ValidationHelper.GetInteger(record[TranslationHelper.RECORD_PARENT_ID_COLUMN], 0);
            var parentTypeInfo = ObjectTypeManager.GetTypeInfo(translationReference.ObjectType, true);

            if (!String.IsNullOrEmpty(parentTypeInfo.ParentObjectType) && (parentId > 0))
            {
                translationReference.Parent = LoadFromDatabase(parentTypeInfo.ParentObjectType, parentId);
            }

            return translationReference;
        }
    }
}
