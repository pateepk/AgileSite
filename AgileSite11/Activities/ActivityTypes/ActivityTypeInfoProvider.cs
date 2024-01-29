using System;

using CMS.DataEngine;

namespace CMS.Activities
{
    /// <summary>
    /// Class providing ActivityTypeInfo management.
    /// </summary>
    public class ActivityTypeInfoProvider : AbstractInfoProvider<ActivityTypeInfo, ActivityTypeInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public ActivityTypeInfoProvider()
            : base(ActivityTypeInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ActivityTypeInfo objects.
        /// </summary>
        public static ObjectQuery<ActivityTypeInfo> GetActivityTypes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns activity type with specified ID.
        /// </summary>
        /// <param name="typeId">Activity type ID</param>        
        public static ActivityTypeInfo GetActivityTypeInfo(int typeId)
        {
            return ProviderObject.GetInfoById(typeId);
        }


        /// <summary>
        /// Returns activity type with specified name.
        /// </summary>
        /// <param name="typeName">Activity type name</param>                
        public static ActivityTypeInfo GetActivityTypeInfo(string typeName)
        {
            return ProviderObject.GetInfoByCodeName(typeName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified activity type.
        /// </summary>
        /// <param name="typeObj">Activity type to be set</param>
        public static void SetActivityTypeInfo(ActivityTypeInfo typeObj)
        {
            ProviderObject.SetInfo(typeObj);
        }


        /// <summary>
        /// Deletes specified activity type.
        /// </summary>
        /// <param name="typeObj">Activity type to be deleted</param>
        public static void DeleteActivityTypeInfo(ActivityTypeInfo typeObj)
        {
            ProviderObject.DeleteInfo(typeObj);
        }


        /// <summary>
        /// Deletes activity type with specified ID.
        /// </summary>
        /// <param name="typeId">Activity type ID</param>
        public static void DeleteActivityTypeInfo(int typeId)
        {
            ActivityTypeInfo typeObj = GetActivityTypeInfo(typeId);
            DeleteActivityTypeInfo(typeObj);
        }


        /// <summary>
        /// Returns display name of particular activity.
        /// </summary>
        /// <param name="typeName">Code name of activity type</param>                
        public static string GetActivityTypeDisplayName(string typeName)
        {
            ActivityTypeInfo ati = ProviderObject.GetInfoByCodeName(typeName);
            if (ati != null)
            {
                return ati.ActivityTypeDisplayName;
            }

            return null;
        }


        /// <summary>
        /// Checks if specified activity type is enabled.
        /// </summary>
        /// <param name="typeName">Code name of activity type</param>                
        public static bool GetActivityTypeEnabled(string typeName)
        {
            ActivityTypeInfo ati = ProviderObject.GetInfoByCodeName(typeName);
            if (ati != null)
            {
                return ati.ActivityTypeEnabled;
            }
            return false;
        }

        #endregion
    }
}