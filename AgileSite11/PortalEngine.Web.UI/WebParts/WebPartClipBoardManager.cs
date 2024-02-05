using CMS.Helpers;


namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part clipboard manager ensures basic operation for clipboard items
    /// </summary>
    internal class WebPartClipBoardManager
    {
        #region "Variables"

        static bool useSingleStorage = true;

        #endregion


        /// <summary>
        /// Returns true if clipboard contain any item for specified type and template scope
        /// </summary>
        /// <param name="zoneType">Zone type</param>
        /// <param name="templateType">Template type</param>
        public static bool ContainsClipBoardItem(WidgetZoneTypeEnum zoneType, PageTemplateTypeEnum templateType)
        {
            return CMSPortalEngineWebUIContext.ContainsClipBoardItem(zoneType, templateType);
        }


        /// <summary>
        /// Sets the clipboard item
        /// </summary>
        /// <param name="item">Clipboard item</param>
        public static void SetClipBoardItem(WebPartClipBoardItem item)
        {
            SessionHelper.SetValue(GetKey(item.ZoneType, item.TemplateScope), item, true);
        }


        /// <summary>
        /// Gets clipboard item
        /// </summary>
        /// <param name="zoneInstance">Zone instance</param>
        /// <param name="pti">Template instance</param>
        public static WebPartClipBoardItem GetClipBoardItem(WebPartZoneInstance zoneInstance, PageTemplateInstance pti)
        {

            WidgetZoneTypeEnum zoneType = zoneInstance.WidgetZoneType;
            PageTemplateTypeEnum templateType = PageTemplateTypeEnum.Unknown;
            if (pti.ParentPageTemplate != null)
            {
                templateType = pti.ParentPageTemplate.PageTemplateType;
            }

            string key = GetKey(zoneType, templateType);

            WebPartClipBoardItem wpcli = SessionHelper.GetValue(key) as WebPartClipBoardItem;
            wpcli = EnsureValidItem(wpcli, zoneType, templateType);
            return wpcli;
        }


        /// <summary>
        /// Gets the key for specified type
        /// </summary>
        /// <param name="zoneType">Zone type</param>
        /// <param name="templateScope">Template scope</param>
        /// <param name="usePrefix">Indicates whether unique prefix should be used</param>
        internal static string GetKey(WidgetZoneTypeEnum zoneType, PageTemplateTypeEnum templateScope, bool usePrefix = true)
        {
            string prefix = "CMSWebPartClipBoard_";
            if (!usePrefix)
            {
                return zoneType.ToStringRepresentation() + "_" + GetTemplateScopeType(templateScope).ToStringRepresentation();
            }
            else if (useSingleStorage)
            {
                return prefix;
            }

            return prefix + zoneType.ToStringRepresentation() + "_" + GetTemplateScopeType(templateScope).ToStringRepresentation();
        }


        /// <summary>
        /// Returns valid item for specific parameters, otherwise return null
        /// </summary>
        /// <param name="item">Clipboard item</param>
        /// <param name="zoneType">Zone type</param>
        /// <param name="templateType">Template type</param>
        internal static WebPartClipBoardItem EnsureValidItem(WebPartClipBoardItem item, WidgetZoneTypeEnum zoneType, PageTemplateTypeEnum templateType)
        {
            if (item != null)
            {
                if (!item.IsValid)
                {
                    string key = GetKey(zoneType, templateType);
                    SessionHelper.Remove(key);
                    return null;
                }
                else if ((item.ZoneType != zoneType) || (item.TemplateScope != GetTemplateScopeType(templateType)))
                {
                    return null;
                }
            }

            return item;
        }


        /// <summary>
        /// Ensures correct template scope type
        /// </summary>
        /// <param name="templateType">Original template type</param>
        public static PageTemplateTypeEnum GetTemplateScopeType(PageTemplateTypeEnum templateType)
        {
            switch (templateType)
            {
                case PageTemplateTypeEnum.UI:
                    return templateType;

                default:
                    return PageTemplateTypeEnum.Unknown;
            }
        }
    }

}
