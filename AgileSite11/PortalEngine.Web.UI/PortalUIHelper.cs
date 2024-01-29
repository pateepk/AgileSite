using System;
using System.Data;
using System.Web;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Class for UI helper methods
    /// </summary>
    public class PortalUIHelper
    {
        /// <summary>
        /// Key that enables to find whether the splitview mode is enabled.
        /// </summary>
        private const int SPLITVIEW_ENABLED_KEY = 0;


        /// <summary>
        /// Key that enables to find the compare culture of the splitview mode.
        /// </summary>
        private const int SPLITVIEW_CULTURE_KEY = 1;


        /// <summary>
        /// Key that enables to find whether the splitview mode should be displayed horizontally or vertically.
        /// </summary>
        private const int SPLITVIEW_MODE_KEY = 2;


        /// <summary>
        /// Key that enables to find whether scrollbars should be synchronized in the splitview mode.
        /// </summary>
        private const int SPLITVIEW_SCROLL_KEY = 3;


        /// <summary>
        /// Returns the link icon to the management of the module UI element
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        public static string GetResourceUIElementLink(string moduleName, string elementName)
        {
            var elem = UIElementInfoProvider.GetUIElementInfo(moduleName, elementName);
            if (elem != null)
            {
                return GetResourceUIElementsLink(elem.ElementResourceID, elem.ElementID);
            }

            return null;
        }


        /// <summary>
        /// Returns the link icon to the management of the module UI elements
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="elementId">UI Element ID</param>
        public static string GetResourceUIElementsLink(int resourceId, int elementId = 0)
        {
            string url = UIContextHelper.GetElementUrl("CMS", "EditModule", false, resourceId);

            url = URLHelper.AddParameterToUrl(url, "tabName", "Modules.UserInterface");

            if (elementId > 0)
            {
                url = URLHelper.AddParameterToUrl(url, "elementId", elementId.ToString());
            }

            return GetEditLink(url, GetUIElementInfoText(elementId));
        }


        private static string GetUIElementInfoText(int elementId)
        {
            var element = UIElementInfoProvider.GetUIElementInfo(elementId);
            if (element == null)
            {
                return null;
            }

            return String.Format("{0}, {1}", ResourceInfoProvider.GetResourceInfo(element.ElementResourceID).ResourceName, element.ElementName);
        }


        /// <summary>
        /// Returns the link icon to the management of the module UI elements
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="info">Extra information text</param>
        public static string GetClassFieldsLink(int classId, string info = null)
        {
            string url = UIContextHelper.GetElementUrl("CMS", "EditClass", false, classId);
            url = URLHelper.AddParameterToUrl(url, "tabName", "Fields");

            return GetEditLink(url, info);
        }


        /// <summary>
        /// Returns the link icon to the management of the module UI elements
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="info">Extra information text</param>
        public static string GetClassAlternativeFormsLink(int classId, string info = null)
        {
            string url = UIContextHelper.GetElementUrl("CMS", "EditClass", false, classId);
            url = URLHelper.AddParameterToUrl(url, "tabName", "AlternativeForms");

            return GetEditLink(url, info);
        }


        /// <summary>
        /// Returns the link icon to the management of the module UI elements
        /// </summary>
        /// <param name="url">URL of the target page</param>
        /// <param name="info">Extra information text</param>
        public static string GetEditLink(string url, string info = null)
        {
            string text = CoreServices.Localization.GetString("General.Edit");
            if (info != null)
            {
                text += String.Concat(" (", info, ")");
            }

            return String.Format("<a target=\"_blank\" href=\"{0}\">{1}</a>", url, UIHelper.GetAccessibleIconTag("icon-edit", text, FontIconSizeEnum.Standard));
        }


        /// <summary>
        /// Ensures the bootstrap CSS class for specified body CSS class string if it is allowed for view mode.
        /// </summary>
        /// <param name="bodyClass">The body class.</param>
        /// <param name="viewMode">The view mode.</param>
        /// <param name="page">The current page.</param>
        public static void EnsureBootstrapBodyClass(ref String bodyClass, ViewModeEnum viewMode, Page page)
        {
            // Skip bootstrap class addition for live sites
            if ((viewMode == ViewModeEnum.UI) || (page is IAdminPage))
            {
                bodyClass = String.Format("{0} cms-bootstrap", bodyClass);
            }
        }


        /// <summary>
        /// User culture code for culture version comparison.
        /// </summary>
        public static string SplitModeCultureCode
        {
            get
            {
                string siteName = SiteContext.CurrentSiteName;
                string prefCulture = LocalizationContext.PreferredCultureCode;

                // Get split view cookie
                string values = (string)ContextHelper.GetItem("CMSSplitMode", true, false, true);
                values = HttpUtility.UrlDecode(values);

                string culture = DataHelper.GetPartialValue(values, "|", SPLITVIEW_CULTURE_KEY, "#");
                if (culture == "#")
                {
                    // Get first different culture for default
                    DataSet siteCulturesDS = CultureSiteInfoProvider.GetSiteCultures(siteName);

                    foreach (DataRow dr in siteCulturesDS.Tables[0].Rows)
                    {
                        string cultureCode = ValidationHelper.GetString(dr["CultureCode"], "");
                        if (cultureCode != prefCulture)
                        {
                            culture = cultureCode;
                            SetSplitModeValues(culture, SPLITVIEW_CULTURE_KEY);
                            break;
                        }
                    }
                }
                else
                {
                    // If culture isn't allowed on current site, set preferred culture to cookies
                    if (!CultureSiteInfoProvider.IsCultureAllowed(culture, siteName))
                    {
                        culture = prefCulture;
                        SetSplitModeValues(culture, SPLITVIEW_CULTURE_KEY);
                    }
                }

                return culture;
            }
            set
            {
                SetSplitModeValues(value, SPLITVIEW_CULTURE_KEY);
            }
        }


        /// <summary>
        /// Split mode view (vertical/horizontal).
        /// </summary>
        public static SplitModeEnum SplitMode
        {
            get
            {
                // Get split view cookie
                SplitModeEnum mode = SplitModeEnum.Vertical;
                string values = (string)ContextHelper.GetItem("CMSSplitMode", true, false, true);
                if (values != null)
                {
                    values = HttpUtility.UrlDecode(values);
                    // Get split view mode
                    string modeStr = DataHelper.GetPartialValue(values, "|", SPLITVIEW_MODE_KEY, null);
                    if (!String.IsNullOrEmpty(modeStr))
                    {
                        if (Enum.IsDefined(typeof(SplitModeEnum), modeStr))
                        {
                            mode = (SplitModeEnum)Enum.Parse(typeof(SplitModeEnum), modeStr);
                        }
                    }
                }

                return mode;
            }
            set
            {
                SetSplitModeValues(value.ToString(), SPLITVIEW_MODE_KEY);
            }
        }


        /// <summary>
        /// Indicates if scrollbars should be synchronized in split mode.
        /// </summary>
        public static bool SplitModeSyncScrollbars
        {
            get
            {
                // Get split view cookie
                string values = (string)ContextHelper.GetItem("CMSSplitMode", true, false, true);
                if (values != null)
                {
                    values = HttpUtility.UrlDecode(values);
                    // Get sync scrollbar flag
                    return ValidationHelper.GetBoolean(DataHelper.GetPartialValue(values, "|", SPLITVIEW_SCROLL_KEY, "1"), false);
                }

                return false;
            }
            set
            {
                SetSplitModeValues(value.ToString(), SPLITVIEW_SCROLL_KEY);
            }
        }


        /// <summary>
        /// Indicates if split mode should be displayed.
        /// </summary>
        public static bool DisplaySplitMode
        {
            get
            {
                var currentSite = SiteContext.CurrentSite;

                // Compare feature is not supported for content only sites. 
                if ((currentSite == null) || currentSite.SiteIsContentOnly)
                {
                    return false;
                }

                // Allow split mode only for sites with more than two cultures
                var siteCultures = CultureSiteInfoProvider.GetSiteCultures(currentSite.SiteName);
                if (siteCultures.Items.Count >= 2)
                {
                    bool displayMode = false;
                    string values = (string)ContextHelper.GetItem("CMSSplitMode", true, false, true);
                    if (values != null)
                    {
                        values = HttpUtility.UrlDecode(values);
                        // Get split view mode
                        string displayModeStr = DataHelper.GetPartialValue(values, "|", SPLITVIEW_ENABLED_KEY, null);
                        displayMode = ValidationHelper.GetBoolean(displayModeStr, false);
                    }

                    return displayMode;
                }

                return false;
            }
        }


        /// <summary>
        /// Sets values of split mode.
        /// </summary>
        private static void SetSplitModeValues(string newValue, int index)
        {
            // 0 - display split mode
            // 1 - split view culture
            // 2 - split view mode
            // 3 - synchronize scrollbars


            // Validate the culture code
            if (index == 1)
            {
                string siteName = SiteContext.CurrentSiteName;
                if (!String.IsNullOrEmpty(siteName))
                {
                    newValue = CultureSiteInfoProvider.CheckCultureCode(newValue, siteName);
                }
            }

            // Get split view cookie
            string values = (string)ContextHelper.GetItem("CMSSplitMode", true, false, true);
            values = HttpUtility.UrlDecode(values);

            // Get default values, check integrity
            if ((values == null) || (values.Split(new[] { "|" }, StringSplitOptions.None).Length != 4))
            {
                values = "0|" + LocalizationContext.PreferredCultureCode + "|" + SplitModeEnum.Vertical + "|1";
            }

            // Set new value
            values = DataHelper.SetPartialValue(values, newValue, "|", index);
            ContextHelper.Add("CMSSplitMode", values, true, false, true, DateTime.Now.AddYears(1), false);
        }


        /// <summary>
        /// Get control resolver for macro resolving in web parts and controls
        /// </summary>
        /// <param name="page">Page instance</param>
        /// <param name="context">UIContext</param>
        public static MacroResolver GetControlResolver(Page page, UIContext context = null)
        {
            var resolver = DocumentContext.CurrentResolver.CreateChild();

            if (page != null)
            {
                resolver.SetNamedSourceData("Page", page);
            }

            if (context != null)
            {
                resolver.SetNamedSourceData("UIContext", context);

                resolver.SetNamedSourceDataCallback("EditedObject", x => context.EditedObject, false);
                resolver.SetNamedSourceDataCallback("EditedObjectParent", x => context.EditedObjectParent, false);
            }

            return resolver;
        }
    }
}
