using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.PortalEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class helper for context's methods.
    /// </summary>
    public class UIContextHelper
    {
        #region "Constants"

        // Fields that are skipped when handling URL query
        private static readonly HashSet<string> SPECIAL_FIELDS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "objectid",
            "parentobjectid",
            "displaytitle",
            "resourcename",
            "elementname",
            "saved",
            "elementguid",
            "action",
            "tabname",
            "parenttabname",
            "tabindex"
        };

        /// <summary>
        /// Default directory for uni grids definitions
        /// </summary>
        public static String GRIDDIRECTORY = "~/App_Data/CMSModules/";


        /// <summary>
        /// Represents key for storing query string
        /// </summary>
        public const String QUERY_STRING_KEY = "QueryStringCollection";

        #endregion


        #region "Element URL methods"

        /// <summary>
        /// Returns element's URL based on elements type
        /// </summary>
        /// <param name="ui">UI element info object</param>
        /// <param name="context">Control's UI context</param>
        public static string GetElementUrl(UIElementInfo ui, UIContext context = null)
        {
            var url = ApplicationUrlHelper.GetElementUrl(ui);
            if (ui == null)
            {
                return url;
            }

            var ri = ResourceInfoProvider.GetResourceInfo(ui.ElementResourceID);
            if (ri == null)
            {
                return url;
            }

            // If element is without content and only groups other UIElements in menu or tree
            if (url == ApplicationUrlHelper.COLLAPSIBLE_EMPTY_PARENT_ELEMENT_URL)
            {
                return url;
            }

            if (ui.ElementType != UIElementTypeEnum.Javascript)
            {
                url = HandleUrlQueryString(url, context, ui);
            }

            return url;
        }


        /// <summary>
        /// Returns element's URL based on element's type
        /// </summary>
        /// <param name="moduleName">Element's module</param>
        /// <param name="elementName">Element's name</param>
        /// <param name="context">Control's UI context</param>
        public static string GetElementUrl(String moduleName, String elementName, UIContext context = null)
        {
            var ui = UIElementInfoProvider.GetUIElementInfo(moduleName, elementName);

            return GetElementUrl(ui, context);
        }


        /// <summary>
        /// Returns element's URL
        /// </summary>
        /// <param name="ui">UI element info.</param>
        /// <param name="displayTitle">Indicates whether append display title to result URL.</param>
        /// <param name="objectId">Indicates whether append object ID to URL.</param>
        /// <param name="additionalQuery">Additional query to append to URL</param>
        public static string GetElementUrl(UIElementInfo ui, bool displayTitle, int objectId = 0, string additionalQuery = null)
        {
            String url = GetElementUrl(ui);

            url = URLHelper.UpdateParameterInUrl(url, "displaytitle", displayTitle.ToString());

            if (objectId != 0)
            {
                url = URLHelper.UpdateParameterInUrl(url, "objectId", objectId.ToString());
            }

            if (!String.IsNullOrEmpty(additionalQuery))
            {
                url = URLHelper.AppendQuery(url, additionalQuery);
            }

            return url;
        }


        /// <summary>
        /// Returns element's URL
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        /// <param name="displayTitle">Indicates whether append display title to result URL.</param>
        /// <param name="objectId">Indicates whether append object ID to URL.</param>
        /// <param name="additionalQuery">Additional query to append to URL</param>
        public static string GetElementUrl(String moduleName, String elementName, bool displayTitle, int objectId = 0, string additionalQuery = null)
        {
            return GetElementUrl(UIElementInfoProvider.GetUIElementInfo(moduleName, elementName), displayTitle, objectId, additionalQuery);
        }


        /// <summary>
        /// Manages common properties of element's URL
        /// </summary>
        /// <param name="url">Element's URL</param>
        /// <param name="context">Context to use</param>
        /// <param name="elem">Target UI element</param>
        private static string HandleUrlQueryString(String url, UIContext context, UIElementInfo elem)
        {
            if (String.IsNullOrEmpty(url) || context == null || PortalContext.ViewMode != ViewModeEnum.UI)
            {
                return url;
            }

            var specialQueryStringKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Display title -- Don't append for element with URL.
            if ((!context.DisplayTitle) && (elem.ElementType != UIElementTypeEnum.Url))
            {
                specialQueryStringKeys.Add("displaytitle", "false");
            }

            if (context["SiteID"] != null)
            {
                string siteIDMacro = ValidationHelper.GetString(context["SiteID"], String.Empty);
                string siteIDMacroResolved = MacroResolver.Resolve(siteIDMacro);

                // To avoid possibly incorrect ID value (such as XSS attack)
                int siteID = ValidationHelper.GetInteger(siteIDMacroResolved, 0);
                specialQueryStringKeys.Add("siteid", siteID.ToString());
            }

            int parentId = ValidationHelper.GetInteger(context["ParentObjectID"], 0);
            if (parentId != 0)
            {
                specialQueryStringKeys.Add("parentobjectid", parentId.ToString());
            }

            // Append dialog=1 for root dialog. IsInDialog=1 for all other nested dialogs. Ensures correct dialog header.
            if (context.IsRootDialog)
            {
                specialQueryStringKeys.Add("dialog", "1");
            }
            else if (context.IsDialog)
            {
                specialQueryStringKeys.Add("isindialog", "1");
            }

            int rootUIelementId = context.RootElementID;
            if (rootUIelementId > 0)
            {
                specialQueryStringKeys.Add("rootelementid", rootUIelementId.ToString());
            }

            // Return handler function name
            string returnHandler = ValidationHelper.GetString(context["returnhandler"], String.Empty);
            if (!String.IsNullOrEmpty(returnHandler))
            {
                specialQueryStringKeys.Add("returnhandler", returnHandler);

                // Specify return column type (id, codename...)
                string returnType = ValidationHelper.GetString(context["returntype"], String.Empty);
                if (!String.IsNullOrEmpty(returnType))
                {
                    specialQueryStringKeys.Add("returntype", returnType);
                }
            }

            int objectID = ValidationHelper.GetInteger(context["objectID"], 0);
            if (objectID != 0)
            {
                // Add specific ID for object. f.e. we want UItabs to dispatch 'objectID' also as 'reportID'
                String objectParameterID = ValidationHelper.GetString(context["objectParameterID"], String.Empty);
                if ((objectParameterID != String.Empty) && !specialQueryStringKeys.ContainsKey(objectParameterID))
                {
                    specialQueryStringKeys.Add(objectParameterID, objectID.ToString());
                }
            }

            // Append rest of parameters (those that are not already in query)
            var queryStringValues = context[QUERY_STRING_KEY] as NameValueCollection;
            if (queryStringValues != null)
            {
                foreach (string queryStringKey in queryStringValues)
                {
                    // Only process new fields that are not special. Item might be null when no parameter name is specified.
                    if (!String.IsNullOrEmpty(queryStringKey) && !SPECIAL_FIELDS.Contains(queryStringKey) && !specialQueryStringKeys.ContainsKey(queryStringKey))
                    {
                        String value = queryStringValues[queryStringKey];
                        if (!String.IsNullOrEmpty(value))
                        {
                            specialQueryStringKeys.Add(HttpUtility.UrlEncode(queryStringKey), HttpUtility.UrlEncode(value));
                        }
                    }
                }
            }

            // Append collected keys to URL
            foreach (var specialQueryStringKey in specialQueryStringKeys)
            {
                url = URLHelper.UpdateParameterInUrl(url, specialQueryStringKey.Key, specialQueryStringKey.Value);
            }

            return url;
        }

        #endregion


        #region "Element dialog methods"

        /// <summary>
        /// Appends hash to URL if context contains dialog parameter
        /// </summary>
        /// <param name="context">UI context</param>
        /// <param name="url">URL to hash is added</param>
        public static string AppendDialogHash(UIContext context, String url)
        {
            bool isDialog = ValidationHelper.GetBoolean(context["dialog"], false);
            if (isDialog)
            {
                url = ApplicationUrlHelper.AppendDialogHash(url);
            }
            return url;
        }

        #endregion


        #region "Object methods"

        /// <summary>
        /// Gets object type of (created)edited object (if any)
        /// </summary>
        /// <param name="context">Control's UI context</param>
        public static string GetObjectType(UIContext context)
        {
            String val = ValidationHelper.GetString(context["objecttype"], String.Empty);

            // If object type not set, try find from parent
            if ((val == String.Empty) && (context.UIElement != null))
            {
                val = GetElementObjectType(UIElementInfoProvider.GetUIElementInfo(context.UIElement.ElementParentID));

                // Store found parent's object type
                if (val != String.Empty)
                {
                    context["objecttype"] = val;
                }
            }

            return val;
        }


        /// <summary>
        /// Get object type of element. If not found try to get object type from parent elements.
        /// </summary>
        /// <param name="par">UI element</param>
        private static string GetElementObjectType(UIElementInfo par)
        {
            String val = String.Empty;

            while ((par != null) && (val == String.Empty))
            {
                UIContextData data = new UIContextData();
                data.LoadData(par.ElementProperties);
                val = ValidationHelper.GetString(data["objecttype"], String.Empty);

                par = UIElementInfoProvider.GetUIElementInfo(par.ElementParentID);
            }

            return val;
        }

        #endregion


        #region "General methods"

        /// <summary>
        /// Returns control's UI context. If no parent control contains own UI context, use main page's (stored in request stock helper).
        /// </summary>
        /// <param name="ctrl">Control to start with</param>
        public static UIContext GetUIContext(Control ctrl = null)
        {
            while (ctrl != null)
            {
                // If control contains UI context return it
                var context = ctrl as IUIContextManager;
                if (context != null)
                {
                    return context.UIContext;
                }

                ctrl = ctrl.Parent;
            }

            // If no context found in control collection, try to load one from request stock helper
            return UIContext.Current;
        }


        /// <summary>
        /// Returns object's breadcrumbs text.
        /// </summary>
        /// <param name="context">Current UI context</param>
        /// <param name="obj">Current object</param>
        public static String GetObjectBreadcrumbsText(UIContext context, BaseInfo obj)
        {
            String itemName = String.Empty;
            String value = ValidationHelper.GetString(context["BreadcrumbsItemText"], String.Empty);
            UIElementInfo ui = context.UIElement;

            // First get value from element's property
            if (!String.IsNullOrEmpty(value))
            {
                itemName = context.ContextResolver.ResolveMacros(value);
            }

            // Try to get from object. If no object set, use element's caption
            if (String.IsNullOrEmpty(itemName))
            {
                // Use object representation
                if ((obj != null) && (obj.Generalized.ObjectID > 0))
                {
                    itemName = obj.ToMacroString();
                }

                // If there is no representation, use UI element caption
                if (string.IsNullOrEmpty(itemName))
                {
                    itemName = UIElementInfoProvider.GetElementCaption(ui);
                }
            }

            return itemName;
        }


        /// <summary>
        /// Gets title text based on 'titletext' property. If this property is not found, use element's caption (not localized).
        /// </summary>
        /// <param name="context">Control's UI context</param>
        public static String GetTitleText(UIContext context)
        {
            String titleText = ValidationHelper.GetString(context["titleText"], String.Empty);
            if ((titleText == String.Empty) && (context.UIElement != null))
            {
                titleText = UIElementInfoProvider.GetElementCaption(context.UIElement, false);
            }

            return titleText;
        }


        /// <summary>
        /// Returns element feature. If feature is empty, tries to find it from parents.
        /// </summary>
        /// <param name="ui">UI element to check</param>
        public static String FindElementFeature(UIElementInfo ui)
        {
            if (ui != null)
            {
                // Get parents from ID path
                var elements = ui.GetParentElements();

                while (ui != null)
                {
                    if (!String.IsNullOrEmpty(ui.ElementFeature))
                    {
                        return ui.ElementFeature;
                    }

                    ui = elements[ui.ElementParentID] as UIElementInfo;
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Checks for element's feature availability for UI.
        /// </summary>
        /// <param name="ui">Element to check</param>
        /// <param name="checkParents">If true, and element does not contain feature, tries to look at parents.</param>
        public static bool CheckFeatureAvailableInUI(UIElementInfo ui, bool checkParents = false)
        {
            if (ui != null)
            {
                // Try to get from current element
                String feature = ui.ElementFeature;

                // If not found, and check parents checked - look at parents.
                if (String.IsNullOrEmpty(feature) && checkParents)
                {
                    feature = FindElementFeature(ui);
                }

                // Check found feature
                if (!String.IsNullOrEmpty(feature))
                {
                    return LicenseHelper.IsFeatureAvailableInUI(feature.ToEnum<FeatureEnum>());
                }
            }

            return true;
        }


        /// <summary>
        /// Checks the UI element availability. This check includes evaluation of the element's macro
        /// condition, license check of the element's feature and check if the element's resource (module)
        /// is available.
        /// </summary>
        /// <param name="uiElement">UI Element to check</param>
        public static bool CheckElementAvailabilityInUI(UIElementInfo uiElement)
        {
            return ResourceInfoProvider.IsResourceAvailable(uiElement.ElementResourceID) &&
                CheckElementVisibilityCondition(uiElement) &&
                CheckFeatureAvailableInUI(uiElement);
        }


        /// <summary>
        /// Checks visibility condition of the UI element given.
        /// </summary>
        /// <param name="ui">UI element whose visibility condition to evaluate.</param>
        /// <returns>Returns true if UI element's visibility condition evaluates to true or no condition is specified, false otherwise.</returns>
        public static bool CheckElementVisibilityCondition(UIElementInfo ui)
        {
            return CheckVisibilityCondition(ui.ElementVisibilityCondition);
        }


        /// <summary>
        /// Checks visibility condition of the <paramref name="helpTopic"/> given.
        /// </summary>
        /// <param name="helpTopic">Help topic whose visibility condition to evaluate.</param>
        /// <returns>Returns true if help topic's visibility condition evaluates to true or no condition is specified, false otherwise.</returns>
        public static bool CheckHelpTopicVisiblityCondition(HelpTopicInfo helpTopic)
        {
            return CheckVisibilityCondition(helpTopic.HelpTopicVisibilityCondition);
        }


        /// <summary>
        /// Evaluates <paramref name="visibilityCondition"/> and returns a value indicating whether the condition was met. Returns true
        /// if no visibility condition is specified.
        /// </summary>
        private static bool CheckVisibilityCondition(string visibilityCondition)
        {
            // Check whether condition is defined
            if (MacroProcessor.ContainsMacro(visibilityCondition))
            {
                return ValidationHelper.GetBoolean(MacroResolver.Resolve(visibilityCondition), false);
            }

            return true;
        }


        /// <summary>
        /// Indicates if UI element contains template with IsLayout checked.
        /// </summary>
        /// <param name="ui">UI element to check</param>
        /// <param name="defaultValue">Return default value, if element is not found or does not contain template</param>
        public static bool ElementIsLayout(UIElementInfo ui, bool defaultValue = false)
        {
            if (ui != null)
            {
                if (ui.ElementType == UIElementTypeEnum.PageTemplate)
                {
                    PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(ui.ElementPageTemplateID);
                    if (pti != null)
                    {
                        return pti.PageTemplateIsLayout;
                    }
                }
                else if (ui.ElementType == UIElementTypeEnum.Url)
                {
                    UIContextData data = new UIContextData();
                    data.LoadData(ui.ElementProperties);

                    if (data["isLayout"] != null)
                    {
                        return data["isLayout"].ToBoolean(defaultValue);
                    }
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Gets UI element breadcrumbs suffix.
        /// </summary>
        /// <param name="ui">UI element</param>
        public static string GetElementBreadcrumbsSuffix(UIElementInfo ui)
        {
            string suffix = null;

            if (ui != null)
            {
                // Get breadcrumbs suffix
                UIContextData data = new UIContextData();
                data.LoadData(ui.ElementProperties);
                suffix = ResHelper.GetString(ValidationHelper.GetString(data["breadcrumbsSuffix"], null));
            }

            return suffix;
        }


        /// <summary>
        /// Checks if given element is parent of selected element (based by ID path)
        /// </summary>
        /// <param name="uiElem">Parent element</param>
        /// <param name="context">Control's UI context</param>
        public static UIElementInfo CheckSelectedElement(UIElementInfo uiElem, UIContext context)
        {
            int selectedItemID = ValidationHelper.GetInteger(context["SelectedItemID"], 0);
            if (selectedItemID != 0)
            {
                // Test whether objects are same
                if (uiElem.ElementID == selectedItemID)
                {
                    return uiElem;
                }

                // Check for child element
                UIElementInfo ui = UIElementInfoProvider.GetUIElementInfo(selectedItemID);
                if (ui != null)
                {
                    if (ui.ElementIDPath.Contains(uiElem.ElementIDPath))
                    {
                        return ui;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Appends URL suffix to element's URL. Replace objectID for parentobjectID if object types are different (parent-child)
        /// </summary>
        /// <param name="url">URL to correct</param>
        /// <param name="ui">Child UI element</param>
        /// <param name="context">Control's UI context</param>
        public static String CorrectChildParentRelations(String url, UIElementInfo ui, UIContext context)
        {
            // Append object ID
            int objectID = ValidationHelper.GetInteger(context["ObjectID"], 0);
            if (objectID != 0)
            {
                url = URLHelper.UpdateParameterInUrl(url, "objectid", objectID.ToString());
            }

            UIContextData uid = new UIContextData();
            uid.LoadData(ui.ElementProperties);

            // For parent -> child situation, replace objectid for parentobjectid if objecttypes are different
            String childObjectType = ValidationHelper.GetString(uid["ObjectType"], String.Empty);
            if ((childObjectType != String.Empty) && (childObjectType != GetObjectType(context)))
            {
                // If element target URL already contains parentobjectid, do not add again
                if (url.ToLowerCSafe().Contains("&parentobjectid=") ||
                    url.ToLowerCSafe().Contains("?parentobjectid="))
                {
                    // Remove object ID from URL, wrong object ID
                    url = URLHelper.RemoveParameterFromUrl(url, "objectid");

                    // Replace parentobjectid with current objectID (if not match)
                    if (objectID != 0)
                    {
                        url = URLHelper.UpdateParameterInUrl(url, "parentobjectid", objectID.ToString());
                    }
                }
                else
                {
                    url = url.Replace("&objectid=", "&parentobjectid=");
                    url = url.Replace("?objectid=", "?parentobjectid=");
                }
            }

            return url;
        }

        #endregion


        /// <summary>
        /// Returns URL to Application description for given element.
        /// If provided element doesn't have an application, empty string is returned.
        /// </summary>
        /// <param name="uiElement">UI element info object</param>
        public static String GetApplicationDescriptionUrl(UIElementInfo uiElement)
        {
            if ((uiElement == null) || (uiElement.Application == null))
            {
                return String.Empty;
            }

            UIContextData uiContextData = new UIContextData();
            uiContextData.LoadData(uiElement.ElementProperties);

            // Try find description from properties, if not found empty string is returned.
            string link = ValidationHelper.GetString(uiContextData["descriptionlink"], String.Empty);
            if (!String.IsNullOrEmpty(link))
            {
                return DocumentationHelper.GetDocumentationTopicUrl(link);
            }

            return String.Empty;
        }
    }
}