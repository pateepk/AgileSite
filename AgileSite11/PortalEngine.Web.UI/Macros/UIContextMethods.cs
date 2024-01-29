using System;

using CMS;
using CMS.MacroEngine;
using CMS.Base;
using CMS.FormEngine.Web.UI;
using CMS.Modules;
using CMS.PortalEngine.Web.UI;

[assembly: RegisterExtension(typeof(UIContextMethods), typeof(UIContext))]

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// UI context methods
    /// </summary>
    internal class UIContextMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns element's URL specified by its module name and element name
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns element URL specified by its module name and element name.", 2)]
        [MacroMethodParam(0, "contextObject", typeof(string), "UIContext object.")]
        [MacroMethodParam(1, "moduleName", typeof(string), "Module name.")]
        [MacroMethodParam(2, "elementName", typeof(object), "Element name.")]
        [MacroMethodParam(3, "displayTitle", typeof(object), "Indicates whether display title.")]
        [MacroMethodParam(4, "objectId", typeof(object), "Object ID to pass.")]
        [MacroMethodParam(5, "additionalQuery", typeof(object), "Additional query appended to URL.")]
        public static object GetElementUrl(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 2)
            {
                string moduleName = parameters[1].ToString(String.Empty);
                string elementName = parameters[2].ToString(String.Empty);
                bool displayTitle = ((parameters.Length > 3) && parameters[3].ToBoolean(false));
                int objectId = (parameters.Length > 4) ? parameters[4].ToInteger(0) : 0;
                String additionalQuery = (parameters.Length > 5) ? parameters[5].ToString("") : "";

                // Create base element's URL
                String url = UIContextHelper.GetElementUrl(moduleName, elementName, displayTitle, objectId, additionalQuery);

                return url;
            }

            return ApplicationUrlHelper.GetElementUrl();
        }


        /// <summary>
        /// Returns element's dialog URL specified by its module name and element name
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns element dialog URL specified by its module name and element name.", 2)]
        [MacroMethodParam(0, "contextObject", typeof(string), "UIContext object.")]
        [MacroMethodParam(1, "moduleName", typeof(string), "Module name.")]
        [MacroMethodParam(2, "elementName", typeof(object), "Element name.")]
        [MacroMethodParam(3, "objectId", typeof(object), "Object ID to pass.")]
        [MacroMethodParam(4, "additionalQuery", typeof(object), "Additional query appended to URL.")]
        public static object GetElementDialogUrl(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 2)
            {
                string moduleName = parameters[1].ToString(String.Empty);
                string elementName = parameters[2].ToString(String.Empty);
                int objectId = (parameters.Length > 3) ? parameters[3].ToInteger(0) : 0;
                string additionalQuery = (parameters.Length > 4) ? parameters[4].ToString(String.Empty) : string.Empty;

                // Create base element's URL
                String url = ApplicationUrlHelper.GetElementDialogUrl(moduleName, elementName, objectId, additionalQuery);

                return url;
            }

            return ApplicationUrlHelper.GetElementUrl();
        }
    }
}
