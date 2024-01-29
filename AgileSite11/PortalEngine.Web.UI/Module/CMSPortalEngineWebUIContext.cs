using CMS.Base;
using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal controls context
    /// </summary>
    internal class CMSPortalEngineWebUIContext : AbstractContext<CMSPortalEngineWebUIContext>
    {
        /// <summary>
        /// Returns value that indicates whether for requested zone and template type is available clipboard item
        /// </summary>
        /// <param name="zoneType">Zone type</param>
        /// <param name="templateType">Template type</param>
        internal static bool ContainsClipBoardItem(WidgetZoneTypeEnum zoneType, PageTemplateTypeEnum templateType)
        {
            bool contains = false;
            string key = WebPartClipBoardManager.GetKey(zoneType, templateType);

            if (!Current.ContainsColumn(key))
            {
                WebPartClipBoardItem wpcli = SessionHelper.GetValue(key) as WebPartClipBoardItem;
                wpcli = WebPartClipBoardManager.EnsureValidItem(wpcli, zoneType, templateType);

               contains = (wpcli != null);
               Current[key] = contains;
            }
            else
            {
                contains = ValidationHelper.GetBoolean(Current[key], false);
            }
            return contains;
        }
    }
}
