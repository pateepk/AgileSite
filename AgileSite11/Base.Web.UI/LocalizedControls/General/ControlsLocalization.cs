using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Localization support for controls
    /// </summary>
    public static class ControlsLocalization
    {
        /// <summary>
        /// Returns a localized string for a control
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="stringName">String name</param>
        /// <param name="culture">String culture</param>
        public static string GetString(Control ctrl, string stringName, string culture = null)
        {
            stringName = ApplyResourcePrefixes(ctrl, stringName);

            return ResHelper.GetString(stringName, culture);
        }


        /// <summary>
        /// Applies resource prefixes to the given string name
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="stringName">String name</param>
        private static string ApplyResourcePrefixes(Control ctrl, string stringName)
        {
            if (!String.IsNullOrEmpty(stringName))
            {
                // Get the closest resource manager and apply its prefixes
                var manager = (ctrl as IResourcePrefixManager) ?? ControlsHelper.GetParentControl<IResourcePrefixManager>(ctrl);
                if (manager != null)
                {
                    stringName = ApplyResourcePrefixes(stringName, manager.ResourcePrefixes);
                }
            }

            return stringName;
        }


        /// <summary>
        /// Gets the localized text for the control.
        /// </summary>
        /// <param name="ctrl">Control for which the text is retrieved</param>
        /// <param name="resourceString">Resource string to localize</param>
        /// <param name="source">Source for the localization - may be 'database' or 'file'</param>
        /// <param name="text">Text to localize</param>
        public static string GetLocalizedText(Control ctrl, string resourceString, string source, string text)
        {
            // Use ResourceString property
            if (!String.IsNullOrEmpty(resourceString))
            {
                resourceString = ApplyResourcePrefixes(ctrl, resourceString);

                // Set text using ResHelper
                if (String.IsNullOrEmpty(source) || source.EqualsCSafe("database", StringComparison.InvariantCultureIgnoreCase))
                {
                    text = ResHelper.GetString(resourceString);
                }
                else if (source.EqualsCSafe("file", StringComparison.InvariantCultureIgnoreCase))
                {
                    text = ResHelper.GetFileString(resourceString);
                }
            }
            else
            {
                // Get raw text
                if (String.IsNullOrEmpty(source) || source.EqualsCSafe("database", StringComparison.InvariantCultureIgnoreCase))
                {
                    text = ResHelper.LocalizeString(text);
                }
                else if (source.EqualsCSafe("file", StringComparison.InvariantCultureIgnoreCase) && String.IsNullOrEmpty(resourceString))
                {
                    text = ResHelper.LocalizeFileString(text);
                }
            }

            return text;
        }


        /// <summary>
        /// Applies resource prefixes to the given string name
        /// </summary>
        /// <param name="stringName">String name</param>
        /// <param name="prefixes">List of prefixes to apply</param>
        internal static string ApplyResourcePrefixes(string stringName, ICollection<string> prefixes)
        {
            if ((prefixes != null) && (prefixes.Count > 0))
            {
                // Create fallback for all prefixes in control hierarchy e.g. ParentPrefix.ResString|CurrentPrefix.ResString|OriginalPrefix.ResString
                var builder = new StringBuilder();

                foreach (var prefix in prefixes)
                {
                    builder.AppendFormat("{0}.{1}|", prefix, stringName);
                }

                stringName = builder.Append(stringName).ToString();
            }

            return stringName;
        }
        

        /// <summary>
        /// Gets all non-empty prefixes from control hierarchy (via control parent).
        /// </summary>        
        internal static ICollection<string> GetPrefixFromControlHierarchy(Control control)
        {
            List<string> resourcePrefixes = null;

            while (control != null)
            {
                var prefixManager = control as IResourcePrefixManager;
                if (prefixManager != null)
                {
                    var prefix = prefixManager.ResourcePrefix;

                    if (!String.IsNullOrEmpty(prefix))
                    {
                        if (resourcePrefixes == null)
                        {
                            resourcePrefixes = new List<string>();
                        }

                        resourcePrefixes.Add(prefix);
                    }
                }

                control = control.Parent;
            }

            if (resourcePrefixes == null)
            {
                return null;
            }

            return ((IEnumerable<string>)resourcePrefixes).Reverse().Distinct().ToList();
        }
    }
}
