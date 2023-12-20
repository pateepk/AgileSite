using System;
using System.Collections.Concurrent;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Form info helper for portal engine purposes
    /// </summary>
    public static class PortalFormHelper
    {
        #region "Variables"

        // Cached Before/After FormInfos for web parts.
        private static readonly ConcurrentDictionary<string, FormInfo> mPositionInfosWebParts = new ConcurrentDictionary<string, FormInfo>(StringComparer.OrdinalIgnoreCase);

        // Cached partial FormInfos for widgets.
        private static readonly ConcurrentDictionary<string, FormInfo> mWidgetPartialFormInfos = new ConcurrentDictionary<string, FormInfo>(StringComparer.OrdinalIgnoreCase);

        // Cached web part FormInfo objects.
        private static readonly CMSStatic<ProviderDictionary<string, FormInfo>> mWebPartFormInfos = new CMSStatic<ProviderDictionary<string, FormInfo>>(() => new ProviderDictionary<string, FormInfo>("WebPartFormInfo", null, StringComparer.InvariantCultureIgnoreCase, true));

        // Cached widget FormInfo objects.
        private static readonly CMSStatic<ProviderDictionary<string, FormInfo>> mWidgetFormInfos = new CMSStatic<ProviderDictionary<string, FormInfo>>(() => new ProviderDictionary<string, FormInfo>("WidgetFormInfo", null, StringComparer.InvariantCultureIgnoreCase, true));

        // Cached Before/After FormInfo for UI page templates' properties.
        private static readonly ConcurrentDictionary<UIElementPropertiesPosition, FormInfo> mUiTemplateDefaultProperties = new ConcurrentDictionary<UIElementPropertiesPosition, FormInfo>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Cached web part FormInfo objects.
        /// </summary>
        private static ProviderDictionary<string, FormInfo> WebPartFormInfos
        {
            get
            {
                return mWebPartFormInfos;
            }
        }


        /// <summary>
        /// Cached widget FormInfo objects.
        /// </summary>
        private static ProviderDictionary<string, FormInfo> WidgetFormInfos
        {
            get
            {
                return mWidgetFormInfos;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears all cached objects.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        internal static void Clear(bool logTasks)
        {
            ClearWebPartFormInfos(logTasks);
            ClearWidgetFormInfos(logTasks);
        }


        /// <summary>
        /// Clears cached web part FormInfos.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearWebPartFormInfos(bool logTasks)
        {
            WebPartFormInfos?.Clear(logTasks);
        }


        /// <summary>
        /// Clears cached web part FormInfos.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearWidgetFormInfos(bool logTasks)
        {
            WidgetFormInfos?.Clear(logTasks);
        }


        /// <summary>
        /// Returns cached and merged FormInfo for web parts.
        /// </summary>
        /// <param name="webPartName">Name of the web part. Use suffix FormHelper.CORE to set different name for form info which doesn't contain before/after definitions</param>
        /// <param name="webPartProperties">XML schema for web part FormInfo</param>
        /// <param name="beforeDefinition">FormInfo for Before definition of the web part</param>
        /// <param name="afterDefinition">FormInfo for After definition of the web part</param>
        /// <param name="clone">When returned FormInfo is used only for reading then cloning can be set to FALSE</param>
        /// <param name="defaultValues">XML definitions with overridden system default values</param> 
        /// <returns>Returns FormInfo object</returns>
        public static FormInfo GetWebPartFormInfo(string webPartName, string webPartProperties, FormInfo beforeDefinition, FormInfo afterDefinition, bool clone, String defaultValues = "")
        {
            if (String.IsNullOrEmpty(webPartName))
            {
                return null;
            }

            // Check if web part FormInfo is already stored in cache
            var fi = WebPartFormInfos[webPartName];
            if (fi != null)
            {
                return clone ? fi.Clone() : fi;
            }

            // FormInfo is not in cache = combine it with before/after definitions
            fi = new FormInfo(webPartProperties);

            // Combine with global schema, overwrite existing fields
            if (beforeDefinition != null)
            {
                FormInfo beforeFI = beforeDefinition.Clone();
                beforeFI.CombineWithForm(fi, true, null, true);
                fi = beforeFI;
            }

            // Combine with global schema, don't overwrite existing fields
            if (afterDefinition != null)
            {
                fi.CombineWithForm(afterDefinition, false, null, true);
            }

            // Merge default values with xml
            if (!String.IsNullOrEmpty(defaultValues))
            {
                String str = FormHelper.MergeFormDefinitions(fi.GetXmlDefinition(), defaultValues);
                fi = new FormInfo(str);
            }

            WebPartFormInfos[webPartName] = fi;

            if (clone)
            {
                return fi.Clone();
            }
            else
            {
                return fi;
            }
        }


        /// <summary>
        /// Returns cached and merged FormInfo for widgets.
        /// </summary>
        /// <param name="widgetName">Name of the widget</param>
        /// <param name="zoneType">Type of the zone where widget is placed</param>
        /// <param name="widgetPropertiesFormDefinition">Widget properties form definition</param>
        /// <param name="clone">When returned FormInfo is used only for reading then cloning can be set to FALSE</param>
        /// <param name="defaultValuesFormDefinition">Form definition with overridden system default values</param> 
        /// <returns>Returns FormInfo object</returns>
        public static FormInfo GetWidgetFormInfo(string widgetName, WidgetZoneTypeEnum zoneType, string widgetPropertiesFormDefinition, bool clone, string defaultValuesFormDefinition = "")
        {
            if (String.IsNullOrEmpty(widgetName))
            {
                return null;
            }

            string name = String.Format("{0}_{1}", widgetName, zoneType);

            // Check if web part FormInfo is already stored in cache
            var widgetProperties = WidgetFormInfos[name];
            if (widgetProperties != null)
            {
                return clone ? widgetProperties.Clone() : widgetProperties;
            }

            // FormInfo is not in cache = combine it with before/after definitions
            widgetProperties = new FormInfo(widgetPropertiesFormDefinition);

            // Combine with global schema, overwrite existing fields
            if ((zoneType == WidgetZoneTypeEnum.User) || (zoneType == WidgetZoneTypeEnum.Dashboard) || (zoneType == WidgetZoneTypeEnum.Group))
            {
                FormInfo zoneProperties = GetWidgetPartialFormInfoFromXml("Simple.xml");
                zoneProperties.CombineWithForm(widgetProperties, true, null, true, false);
                widgetProperties = zoneProperties;
            }
            else if (zoneType == WidgetZoneTypeEnum.Editor)
            {
                var before = GetWidgetPartialFormInfoFromXml("Before.xml");
                before.CombineWithForm(widgetProperties, overwriteExisting: false, includeCategories: true);

                var zoneProperties = GetWidgetPartialFormInfoFromXml("Edit.xml");
                before.CombineWithForm(zoneProperties, overwriteExisting: false, includeCategories: true);

                var after = GetWidgetPartialFormInfoFromXml("After.xml");
                before.CombineWithForm(after, overwriteExisting: false, includeCategories: true);

                if (!String.IsNullOrEmpty(defaultValuesFormDefinition))
                {
                    var fullFormDefinition = FormHelper.MergeFormDefinitions(before.GetXmlDefinition(), defaultValuesFormDefinition);
                    widgetProperties = new FormInfo(fullFormDefinition);
                }
                else
                {
                    widgetProperties = before;
                }
            }

            WidgetFormInfos[name] = widgetProperties;

            if (clone)
            {
                return widgetProperties.Clone();
            }

            return widgetProperties;
        }


        private static FormInfo GetWidgetPartialFormInfoFromXml(string fileName)
        {
            var result = mWidgetPartialFormInfos.GetOrAdd(fileName, key =>
            {
                var fi = new FormInfo(LoadProperties("Widget", key));
                return fi.Clone();
            });

            return result.Clone();
        }


        /// <summary>
        /// Gets web part properties.
        /// </summary>
        /// <param name="type">Type of the web part</param>
        /// <param name="position">Position of the properties</param>
        public static string GetWebPartProperties(WebPartTypeEnum type, PropertiesPosition position)
        {
            var resourceName = "After.xml";
            if (position == PropertiesPosition.Before)
            {
                resourceName = "Before.xml";
            }

            switch (type)
            {
                case WebPartTypeEnum.DataSource:
                    resourceName = "DataSource." + resourceName;
                    break;

                case WebPartTypeEnum.Filter:
                    resourceName = "Filter." + resourceName;
                    break;

                case WebPartTypeEnum.Placeholder:
                    resourceName = "Placeholder." + resourceName;
                    break;

                case WebPartTypeEnum.Invisible:
                    resourceName = "Invisible." + resourceName;
                    break;

                case WebPartTypeEnum.Basic:
                    resourceName = "Basic." + resourceName;
                    break;

                case WebPartTypeEnum.UI:
                    resourceName = "UI." + resourceName;
                    break;
            }

            return LoadProperties("WebPart", resourceName);
        }


        /// <summary>
        /// Loads the properties from resource.
        /// </summary>
        /// <param name="resourceNamespace">Resource namespace.</param>
        /// <param name="resourceName">Resource name.</param>
        public static string LoadProperties(string resourceNamespace, string resourceName)
        {
            var loader = new ConfigurationPropertiesLoader();
            return loader.Load(resourceNamespace, resourceName);
        }


        /// <summary>
        /// Returns FormInfo for Before/After section of a web part
        /// </summary>
        /// <param name="type">Type of the web part</param>
        /// <param name="position">Content position</param>
        /// <returns>Returns FormInfo for the specified web part. Returns NULL if nothing found.</returns>
        public static FormInfo GetPositionFormInfo(WebPartTypeEnum type, PropertiesPosition position)
        {
            string name = Enum.GetName(typeof(WebPartTypeEnum), type) + "_" + Enum.GetName(typeof(PropertiesPosition), position);

            var result = mPositionInfosWebParts.GetOrAdd(name, key =>
            {
                FormInfo fi = null;

                var definition = GetWebPartProperties(type, position);
                if (!String.IsNullOrEmpty(definition))
                {
                    fi = new FormInfo(definition);
                }
                return fi;
            });

            return result;
        }


        /// <summary>
        /// Returns general FormInfo pro UI templates (stored in XML file)
        /// </summary>
        /// <param name="position">Requested form info position</param>
        public static FormInfo GetUIElementDefaultPropertiesForm(UIElementPropertiesPosition position)
        {
            var result = mUiTemplateDefaultProperties.GetOrAdd(position, key =>
            {
                FormInfo fi = null;
                var definition = String.Empty;

                switch (key)
                {
                    case UIElementPropertiesPosition.Before:
                        definition = LoadProperties("UIElement", "Before.xml");
                        break;

                    case UIElementPropertiesPosition.After:
                        definition = LoadProperties("UIElement", "After.xml");
                        break;

                    case UIElementPropertiesPosition.Both:
                        var before = LoadProperties("UIElement", "Before.xml");
                        var after = LoadProperties("UIElement", "After.xml");

                        definition = FormHelper.MergeFormDefinitions(before, after);
                        break;
                }

                if (!String.IsNullOrEmpty(definition))
                {
                    fi = new FormInfo(definition);
                }

                return fi;
            });

            return result;
        }


        /// <summary>
        /// Gets the default properties for the given web part
        /// </summary>
        /// <param name="wpi">Web part</param>
        public static DataRow GetDefaultWebPartProperties(WebPartInfo wpi)
        {
            // Parent web part
            WebPartInfo parentWpi = null;

            if (wpi.WebPartParentID > 0)
            {
                parentWpi = WebPartInfoProvider.GetWebPartInfo(wpi.WebPartParentID);
            }

            // Get the form definition
            string wpProperties = wpi.WebPartProperties;

            // Use merged web part properties if parent web part exists
            if (parentWpi != null)
            {
                wpProperties = FormHelper.MergeFormDefinitions(parentWpi.WebPartProperties, wpi.WebPartProperties);
            }

            var webPartType = (WebPartTypeEnum)wpi.WebPartType;

            FormInfo beforeFI = GetPositionFormInfo(webPartType, PropertiesPosition.Before);
            FormInfo afterFI = GetPositionFormInfo(webPartType, PropertiesPosition.After);

            // Get merged web part FormInfo 
            FormInfo fi = GetWebPartFormInfo(wpi.WebPartName, wpProperties, beforeFI, afterFI, true, wpi.WebPartDefaultValues);

            // Get data row with required columns
            DataRow dr = fi.GetDataRow();

            // Load default properties values                    
            fi.LoadDefaultValues(dr);

            return dr;
        }

        #endregion
    }
}
