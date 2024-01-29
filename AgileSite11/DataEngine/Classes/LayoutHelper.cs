using System;
using System.Collections.Generic;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Helper for layout objects
    /// </summary>
    public static class LayoutHelper
    {
        #region "Variables"

        /// <summary>
        /// List of default form layout namespaces
        /// </summary>
        private static List<string> mDefaultNamespaces = new List<string> { "CMS.FormEngine.Web.UI" };


        private static string[] mDefaultDirectives;

        #endregion


        #region "Properties"

        /// <summary>
        /// List of default form layout namespaces
        /// </summary>
        public static List<string> DefaultNamespaces
        {
            get
            {
                return mDefaultNamespaces;
            }
        }


        /// <summary>
        /// Returns list of default form layout directives (ToLower).
        /// </summary>
        public static string[] DefaultDirectives
        {
            get
            {
                if (mDefaultDirectives == null)
                {
                    mDefaultDirectives = StringExtensions.ToLowerCSafe(GetLayoutDirectives()).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return mDefaultDirectives;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the layout type enumeration for the given string value.
        /// </summary>
        /// <param name="type">String type</param>
        /// <param name="defaultValue">Default value - optional; HTML is default value by default</param>
        public static LayoutTypeEnum GetLayoutTypeEnum(string type, LayoutTypeEnum defaultValue = LayoutTypeEnum.Html)
        {
            LayoutTypeEnum result = defaultValue;

            if (type != null)
            {
                switch (type.ToLowerCSafe())
                {
                    case "ascx":
                        result = LayoutTypeEnum.Ascx;
                        break;

                    case "html":
                        result = LayoutTypeEnum.Html;
                        break;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the layout type code for the given enum value.
        /// </summary>
        /// <param name="type">String type</param>
        public static string GetLayoutTypeCode(LayoutTypeEnum type)
        {
            string result = "html";

            switch (type)
            {
                case LayoutTypeEnum.Ascx:
                    result = "ascx";
                    break;

                case LayoutTypeEnum.Html:
                    result = "html";
                    break;
            }

            return result;
        }


        /// <summary>
        /// Adds the layout directives to the transformation.
        /// </summary>
        /// <param name="code">Code of the layout</param>
        /// <param name="type">Type of the layout</param>
        public static string AddLayoutDirectives(string code, LayoutTypeEnum type)
        {
            if (type == LayoutTypeEnum.Ascx)
            {
                return AddLayoutDirectives(code);
            }

            // Do not add directives to non-ascx code.
            return code;
        }


        /// <summary>
        /// Adds the layout directives to the transformation.
        /// </summary>
        /// <param name="code">Code of the layout</param>
        public static string AddLayoutDirectives(string code)
        {
            if ((code != null) && !code.StartsWithCSafe("<%@ Control ", true))
            {
                // Add the directives
                return GetLayoutDirectives() + code;
            }
            else
            {
                // Already contains directives
                return code;
            }
        }


        /// <summary>
        /// Layout directives are set before layout code.
        /// </summary>
        private static string GetLayoutDirectives()
        {
            // Get language code from web.config.
            string lang = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSProgrammingLanguage"], "C#");

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<%@ Control Language=\"{0}\" Inherits=\"CMS.FormEngine.Web.UI.CMSAbstractFormLayout\" %> \n", lang);

            // Add namespaces
            foreach (string ns in mDefaultNamespaces)
            {
                string reg = String.Format("<%@ Register TagPrefix=\"cms\" Namespace=\"{0}\" Assembly=\"{0}\" %>", ns);
                sb.AppendLine(reg);
            }

            return sb.ToString();
        }

        #endregion
    }
}