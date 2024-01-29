using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Class providing <see cref="SearchTaskAzureInfo"/> management.
    /// </summary>
    public class SearchTaskAzureInfoProvider : AbstractInfoProvider<SearchTaskAzureInfo, SearchTaskAzureInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="SearchTaskAzureInfoProvider"/>.
        /// </summary>
        public SearchTaskAzureInfoProvider()
            : base(SearchTaskAzureInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="SearchTaskAzureInfo"/> objects.
        /// </summary>
        public static ObjectQuery<SearchTaskAzureInfo> GetSearchTaskAzureInfos()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="SearchTaskAzureInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="SearchTaskAzureInfo"/> ID.</param>
        public static SearchTaskAzureInfo GetSearchTaskAzureInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="SearchTaskAzureInfo"/> with specified name.
        /// </summary>
        /// <param name="name"><see cref="SearchTaskAzureInfo"/> name.</param>
        public static SearchTaskAzureInfo GetSearchTaskAzureInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="SearchTaskAzureInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="SearchTaskAzureInfo"/> to be set.</param>
        public static void SetSearchTaskAzureInfo(SearchTaskAzureInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="SearchTaskAzureInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="SearchTaskAzureInfo"/> to be deleted.</param>
        public static void DeleteSearchTaskAzureInfo(SearchTaskAzureInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="SearchTaskAzureInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="SearchTaskAzureInfo"/> ID.</param>
        public static void DeleteSearchTaskAzureInfo(int id)
        {
            SearchTaskAzureInfo infoObj = GetSearchTaskAzureInfo(id);
            DeleteSearchTaskAzureInfo(infoObj);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="SearchTaskAzureInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="SearchTaskAzureInfo"/> to be set.</param>
        protected override void SetInfo(SearchTaskAzureInfo infoObj)
        {
            if (infoObj != null && infoObj.SearchTaskAzureCreated == DateTimeHelper.ZERO_TIME)
            {
                infoObj.SearchTaskAzurePriority = (infoObj.SearchTaskAzureType == SearchTaskTypeEnum.Rebuild) ? 1 : 0;
                infoObj.SetValue(nameof(infoObj.SearchTaskAzureCreated), DateTime.Now);
            }

            base.SetInfo(infoObj);
        }
    }
}