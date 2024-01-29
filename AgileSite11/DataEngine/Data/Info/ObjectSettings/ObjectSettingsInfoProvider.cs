using System;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class providing ObjectSettingsInfo management.
    /// </summary>
    public class ObjectSettingsInfoProvider : AbstractInfoProvider<ObjectSettingsInfo, ObjectSettingsInfoProvider>, IFullNameInfoProvider
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectSettingsInfoProvider()
            : base(ObjectSettingsInfo.TYPEINFO, new HashtableSettings
                {
                    FullName = true, 
                    UseWeakReferences = true
                })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns all object settings.
        /// </summary>
        public static ObjectQuery<ObjectSettingsInfo> GetObjectSettings()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets the object settings for the specified object, ensures the settings if not exists.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        public static ObjectSettingsInfo GetObjectSettingsInfo(string objectType, int objectId)
        {
            return ProviderObject.GetObjectSettingsInfoInternal(objectType, objectId);
        }


        /// <summary>
        /// Gets the object settings with the specified ID.
        /// </summary>
        /// <param name="settingsId">Object settings ID</param>
        public static ObjectSettingsInfo GetObjectSettingsInfo(int settingsId)
        {
            return ProviderObject.GetInfoById(settingsId);
        }


        /// <summary>
        /// Sets (updates or inserts) the specified object settings.
        /// </summary>
        /// <param name="settingsObj">Object settings to be set</param>
        public static void SetObjectSettingsInfo(ObjectSettingsInfo settingsObj)
        {
            ProviderObject.SetInfo(settingsObj);
        }


        /// <summary>
        /// Deletes the specified object settings.
        /// </summary>
        /// <param name="settingsObj">Object settings to be deleted</param>
        public static void DeleteObjectSettingsInfo(ObjectSettingsInfo settingsObj)
        {
            ProviderObject.DeleteInfo(settingsObj);
        }


        /// <summary>
        /// Deletes the object settings matching the specified where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteObjectSettings(string where)
        {
            ProviderObject.DeleteObjectSettingsInternal(where);
        }


        /// <summary>
        /// Deletes settings for the given object
        /// </summary>
        /// <param name="obj">Object</param>
        public static void DeleteSettingsForObject(BaseInfo obj)
        {
            ProviderObject.DeleteSettingsForObjectInternal(obj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Deletes settings for the given object
        /// </summary>
        /// <param name="obj">Object</param>
        protected virtual void DeleteSettingsForObjectInternal(BaseInfo obj)
        {
            // Delete the object settings
            string objectSettingsWhere = string.Format("ObjectSettingsObjectType = '{0}'", SqlHelper.GetSafeQueryString(obj.TypeInfo.ObjectType, false));

            objectSettingsWhere = SqlHelper.AddWhereCondition(objectSettingsWhere, "ObjectSettingsObjectID = " + obj.Generalized.ObjectID);

            DeleteObjectSettings(objectSettingsWhere);
        }


        /// <summary>
        /// Gets the object settings for the specified object, ensures the settings if not exists.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        protected virtual ObjectSettingsInfo GetObjectSettingsInfoInternal(string objectType, int objectId)
        {
            // Build the full name
            string fullName = String.Format("{0}.{1}", objectType, objectId);

            ObjectSettingsInfo info = GetInfoByFullName(fullName);

            return info;
        }


        /// <summary>
        /// Deletes the object settings matching the specified where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteObjectSettingsInternal(string where)
        {
            BulkDelete(new WhereCondition(where));
        }

        #endregion


        #region "Full name methods"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(ObjectSettingsInfo.OBJECT_TYPE, "ObjectSettingsObjectType;ObjectSettingsObjectID");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            if (String.IsNullOrEmpty(fullName) || !fullName.Contains("."))
            {
                return null;
            }

            string objectType = null;
            string objectId = null;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out objectType, out objectId))
            {
                return new WhereCondition()
                    .WhereEquals("ObjectSettingsObjectType", objectType)
                    .WhereEquals("ObjectSettingsObjectID", ValidationHelper.GetInteger(objectId, 0))
                    .ToString(true);
            }

            return null;
        }

        #endregion
    }
}
