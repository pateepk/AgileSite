using CMS.DataEngine;

namespace CMS.DocumentEngine.PageBuilder
{
    /// <summary>
    /// Class providing <see cref="TempPageBuilderWidgetsInfo"/> management.
    /// </summary>
    public class TempPageBuilderWidgetsInfoProvider : AbstractInfoProvider<TempPageBuilderWidgetsInfo, TempPageBuilderWidgetsInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="TempPageBuilderWidgetsInfoProvider"/>.
        /// </summary>
        public TempPageBuilderWidgetsInfoProvider()
            : base(TempPageBuilderWidgetsInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="TempPageBuilderWidgetsInfo"/> objects.
        /// </summary>
        public static ObjectQuery<TempPageBuilderWidgetsInfo> GetPageBuilderWidgets()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="TempPageBuilderWidgetsInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="TempPageBuilderWidgetsInfo"/> ID.</param>
        public static TempPageBuilderWidgetsInfo GetPageBuilderWidgetsInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="TempPageBuilderWidgetsInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="TempPageBuilderWidgetsInfo"/> to be set.</param>
        public static void SetPageBuilderWidgetsInfo(TempPageBuilderWidgetsInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="TempPageBuilderWidgetsInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="TempPageBuilderWidgetsInfo"/> to be deleted.</param>
        public static void DeletePageBuilderWidgetsInfo(TempPageBuilderWidgetsInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="TempPageBuilderWidgetsInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="TempPageBuilderWidgetsInfo"/> ID.</param>
        public static void DeletePageBuilderWidgetsInfo(int id)
        {
            var infoObj = GetPageBuilderWidgetsInfo(id);
            DeletePageBuilderWidgetsInfo(infoObj);
        }


        /// <summary>
        /// Bulk deletes temporary configurations based on the given condition.
        /// </summary>
        internal static void BulkDeleteData(WhereCondition deleteWhere)
        {
            ProviderObject.BulkDeleteDataInternal(deleteWhere);
        }


        /// <summary>
        /// Bulk deletes temporary configurations based on the given condition.
        /// </summary>
        internal virtual void BulkDeleteDataInternal(WhereCondition deleteWhere)
        {
            ProviderObject.BulkDelete(deleteWhere);
        }
    }
}