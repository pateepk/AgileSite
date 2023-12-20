using System;

using CMS.DataEngine;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Class providing TagGroupInfo management.
    /// </summary>
    public class TagGroupInfoProvider : AbstractInfoProvider<TagGroupInfo, TagGroupInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public TagGroupInfoProvider()
            : base(TagGroupInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the TagGroupInfo objects.
        /// </summary>
        public static ObjectQuery<TagGroupInfo> GetTagGroups()
        {
            return ProviderObject.GetObjectQuery();
        }

        
        /// <summary>
        /// Returns the TagGroupInfo structure for the specified tag group.
        /// </summary>
        /// <param name="tagGroupId">TagGroup id</param>
        public static TagGroupInfo GetTagGroupInfo(int tagGroupId)
        {
            return ProviderObject.GetInfoById(tagGroupId);
        }


        /// <summary>
        /// Returns the TagGroupInfo structure for the specified tag group.
        /// </summary>
        /// <param name="tagGroupName">TagGroup name</param>
        /// <param name="siteId">Site ID</param>
        public static TagGroupInfo GetTagGroupInfo(string tagGroupName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(tagGroupName, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified tagGroup.
        /// </summary>
        /// <param name="tagGroup">Tag group to set</param>
        public static void SetTagGroupInfo(TagGroupInfo tagGroup)
        {
            ProviderObject.SetInfo(tagGroup);
        }


        /// <summary>
        /// Deletes specified tag group.
        /// </summary>
        /// <param name="infoObj">Tag group to delete</param>
        public static void DeleteTagGroupInfo(TagGroupInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified tag group.
        /// </summary>
        /// <param name="tagGroupId">TagGroup id</param>
        public static void DeleteTagGroupInfo(int tagGroupId)
        {
            TagGroupInfo infoObj = GetTagGroupInfo(tagGroupId);
            DeleteTagGroupInfo(infoObj);
        }


        /// <summary>
        /// Returns the TagGroupInfo structure for the specified tag group.
        /// </summary>
        /// <param name="tagGroupGuid">TagGroup GUID</param>
        /// <param name="siteId">Site ID</param>
        public static TagGroupInfo GetTagGroupInfo(Guid tagGroupGuid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(tagGroupGuid, siteId);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all the TagGroupInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<TagGroupInfo> GetTagGroups(int siteId)
        {
            return GetTagGroups().OnSite(siteId);
        }

        #endregion
    }
}