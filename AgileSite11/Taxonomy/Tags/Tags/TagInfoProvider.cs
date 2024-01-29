using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.Taxonomy
{
    using TypedDataSet = InfoDataSet<TagInfo>;

    /// <summary>
    /// Class providing TagInfo management.
    /// </summary>
    public class TagInfoProvider : AbstractInfoProvider<TagInfo, TagInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the TagInfo objects.
        /// </summary>
        public static ObjectQuery<TagInfo> GetTags()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the TagInfo structure for the specified tag.
        /// </summary>
        /// <param name="tagId">Tag id</param>
        public static TagInfo GetTagInfo(int tagId)
        {
            return ProviderObject.GetInfoById(tagId);
        }


        /// <summary>
        /// Returns all the tags filtered by where condition and sorted by orderBy expression.
        /// </summary>
        /// <param name="where">The WHERE condition to use</param>
        /// <param name="orderBy">The ORDER BY expression to use to sort the result</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to be selected</param>
        public static ObjectQuery<TagInfo> GetTags(string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetTags().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }


        /// <summary>
        /// Returns all the tags for specified document filtered by where condition.
        /// </summary>
        /// <param name="documentId">ID of the document the tags of which should be returned</param>
        /// <param name="where">The WHERE condition to use</param>
        /// <param name="orderBy">The ORDER BY expression to use to sort the result</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to be selected</param>
        public static ObjectQuery<TagInfo> GetTags(int documentId, string where = null, string orderBy = null, int topN = 0, string columns = null)
        {
            // Get where condition for document
            where = SqlHelper.AddWhereCondition(where, string.Format("TagID IN (SELECT TagID FROM CMS_DocumentTag WHERE DocumentID = {0})", documentId));

            return GetTags(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns tags set according specified criteria.
        /// </summary>
        /// <param name="tagGroupName">Code name of the tag group</param>
        /// <param name="siteId">Tag group site ID</param>
        /// <param name="where">Where condition to filter the data</param>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="topN">Number of records to return</param>        
        public static TypedDataSet GetTags(string tagGroupName, int siteId, string where = null, string orderBy = null, int topN = 0)
        {
            return ProviderObject.GetTagsInternal(tagGroupName, siteId, where, orderBy, topN);
        }


        /// <summary>
        /// Sets (updates or inserts) specified tag.
        /// </summary>
        /// <param name="tag">Tag to set</param>
        public static void SetTagInfo(TagInfo tag)
        {
            ProviderObject.SetInfo(tag);
        }


        /// <summary>
        /// Deletes specified tag.
        /// </summary>
        /// <param name="tag">Tag object</param>
        public static void DeleteTagInfo(TagInfo tag)
        {
            ProviderObject.DeleteInfo(tag);
        }


        /// <summary>
        /// Deletes specified tag.
        /// </summary>
        /// <param name="tagId">Tag id</param>
        public static void DeleteTagInfo(int tagId)
        {
            TagInfo infoObj = GetTagInfo(tagId);
            DeleteTagInfo(infoObj);
        }
        
        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        ///  Deletes tags which are used by no documents.
        /// </summary>
        public static void DeleteNotUsedTags()
        {
            ProviderObject.DeleteNotUsedTagsInternal();
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns all the tags filtered by specified tag group and document properties (path, culture code, ...) which can be entered as where condition and sorted by orderBy expression.
        /// </summary>
        /// <param name="tagGroupName">Tag group name to use</param>
        /// <param name="siteId">Tag group site ID</param>
        /// <param name="where">The WHERE condition to use</param>
        /// <param name="orderBy">The ORDER BY expression to use to sort the result</param>
        /// <param name="topN">Number of records to return</param>
        protected virtual TypedDataSet GetTagsInternal(string tagGroupName, int siteId, string where, string orderBy, int topN)
        {
            // Prepare the where condition
            WhereCondition condition = new WhereCondition(where);
            if (!string.IsNullOrEmpty(tagGroupName) && (siteId > 0))
            {
                condition.WhereEquals("TagGroupName", tagGroupName).WhereEquals("TagGroupSiteID", siteId);
            }

            var parameters = condition.Parameters;
            parameters.EnsureDataSet<TagInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("cms.tag.selectDocumentsTags", parameters, condition.WhereCondition, orderBy, topN).As<TagInfo>();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(TagInfo info)
        {
            CheckObject(info);

            info.Generalized.EnsureCodeName();

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        ///  Deletes tags which are used by no documents.
        /// </summary>
        protected virtual void DeleteNotUsedTagsInternal()
        {
            BulkDelete(new WhereCondition().WhereLessOrEquals("TagCount", 0));
        }

        #endregion
    }
}