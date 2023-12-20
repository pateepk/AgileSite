using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing WidgetInfo management.
    /// </summary>
    public class WidgetInfoProvider : AbstractInfoProvider<WidgetInfo, WidgetInfoProvider>
    {
        /// <summary>
        /// Returns the WidgetInfo structure for the specified widget id.
        /// </summary>
        /// <param name="widgetId">Widget id</param>
        public static WidgetInfo GetWidgetInfo(int widgetId)
        {
            return ProviderObject.GetInfoById(widgetId);
        }


        /// <summary>
        /// Returns the WidgetInfo structure for the specified widget name.
        /// </summary>
        /// <param name="widgetName">Widget name (code name)</param>
        public static WidgetInfo GetWidgetInfo(string widgetName)
        {
            return ProviderObject.GetInfoByCodeName(widgetName);
        }


        /// <summary>
        /// Returns the WidgetInfo structure for the specified widget GUID.
        /// </summary>
        /// <param name="widgetGuid">Widget Guid</param>
        /// <returns>Widget info object</returns>
        public static WidgetInfo GetWidgetInfo(Guid widgetGuid)
        {
            return ProviderObject.GetInfoByGuid(widgetGuid);
        }


        /// <summary>
        /// Returns all widgets.
        /// </summary>
        public static ObjectQuery<WidgetInfo> GetWidgets()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified widget.
        /// </summary>
        /// <param name="widget">Widget to set</param>
        public static void SetWidgetInfo(WidgetInfo widget)
        {
            ProviderObject.SetInfo(widget);
        }


        /// <summary>
        /// Deletes specified widget with dependencies.
        /// </summary>
        /// <param name="infoObj">Widget object</param>
        public static void DeleteWidgetInfo(WidgetInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified widget with dependencies.
        /// </summary>
        /// <param name="widgetId">Widget ID</param>
        public static void DeleteWidgetInfo(int widgetId)
        {
            WidgetInfo infoObj = GetWidgetInfo(widgetId);
            DeleteWidgetInfo(infoObj);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WidgetInfo info)
        {
            CheckObject(info);

            int oldCategoryID = 0;

            // Update or insert object to database
            if (info.WidgetID > 0)
            {
                // Get old category ID, have to load from DB
                var existing = GetObjectQuery().WhereEquals("WidgetID", info.WidgetID).TopN(1).FirstOrDefault();
                if (existing != null)
                {
                    oldCategoryID = existing.WidgetCategoryID;
                }
            }

            // Clear widget public fields 'cache'
            info.WidgetPublicFileds = null;

            base.SetInfo(info);

            // Update widget category widget children count
            WidgetCategoryInfoProvider.UpdateCategoryWidgetChildCount(oldCategoryID, info.WidgetCategoryID);

            // Clear cached definitions
            PortalFormHelper.ClearWidgetFormInfos(true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WidgetInfo info)
        {
            if (info != null)
            {
                // Get widget category ID
                int categoryID = info.WidgetCategoryID;

                // Delete object from database
                base.DeleteInfo(info);

                // Update widget category widget children count
                WidgetCategoryInfoProvider.UpdateCategoryWidgetChildCount(0, categoryID);
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public WidgetInfoProvider()
            : base(WidgetInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }
    }
}