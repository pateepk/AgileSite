using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Path macro methods
    /// </summary>
    internal class PathMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Gets the where condition for separate items in current path
        /// </summary>
        /// <param name="context">Evaluation context</param>
        /// <param name="parameters">Macro parameters</param>
        [MacroMethod(typeof(string), "Returns where condition for path parts for current path.", 2)]
        [MacroMethodParam(0, "pathColumn", typeof(string), "")]
        [MacroMethodParam(1, "path", typeof(string), "")]
        public static object WhereItemInPath(EvaluationContext context, params object[] parameters)
        {
            // Check number of required parameters
            if (parameters.Length < 2)
            {
                return String.Empty;
            }

            // Get parameters
            string pathColumn = Convert.ToString(parameters[0]);
            string path = Convert.ToString(parameters[1]);

            // Get path segments
            var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var completePaths = new List<string>();

            // Get list of complete paths for each level
            string currentSegment = String.Empty;
            foreach (string segment in pathSegments)
            {
                currentSegment += "/" + SqlHelper.EscapeQuotes(segment);
                completePaths.Add(currentSegment);
            }

            return new WhereCondition().WhereIn(pathColumn, completePaths).ToString(true);
        }
    }


    /// <summary>
    /// Web part macro methods. Used in format WebPart.GetValue("webPartId", "propertyName")
    /// </summary>
    internal class WebPartMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns value of the web part property specified by web part id
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns value of the web part property specified by web part id.", 2)]
        [MacroMethodParam(0, "id", typeof(string), "Web part id.")]
        [MacroMethodParam(1, "propertyName", typeof(string), "Property name.")]
        public static object GetValue(EvaluationContext context, params object[] parameters)
        {
            // Properties
            string webPartId;
            string propertyName;

            // Get template instance
            PageTemplateInstance instance = GetValueInternal(context, out webPartId, out propertyName, parameters);

            if (instance != null)
            {
                WebPartInstance part = instance.GetWebPart(webPartId);
                if (part != null)
                {
                    return part.GetValue(propertyName);
                }
            }
            return null;
        }


        /// <summary>
        /// Get the value for the context and specific parameters
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="id">Returns object id</param>
        /// <param name="propertyName">Returns used property name</param>
        /// <param name="parameters">Method parameters</param>
        internal static PageTemplateInstance GetValueInternal(EvaluationContext context, out string id, out string propertyName, params object[] parameters)
        {
            id = null;
            propertyName = null;

            // Process parameters
            switch (parameters.Length)
            {
                case 2:
                    id = Convert.ToString(parameters[0]);
                    propertyName = Convert.ToString(parameters[1]);
                    break;

                default:
                    return null;
            }

            return context.RelatedObject as PageTemplateInstance;
        }
    }


    /// <summary>
    /// Web part zone macro methods. Used in format WebPartZone.GetValue("zoneId", "propertyName")
    /// </summary>
    internal class WebPartZoneMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns value of the web part zone property specified by zone id
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns value of the web part zone property specified by web part zone id.", 2)]
        [MacroMethodParam(0, "id", typeof(string), "Web part id.")]
        [MacroMethodParam(1, "propertyName", typeof(string), "Property name.")]
        public static object GetValue(EvaluationContext context, params object[] parameters)
        {
            // Properties
            string zoneId;
            string propertyName;

            // Get template instance
            PageTemplateInstance instance = WebPartMacroMethods.GetValueInternal(context, out zoneId, out propertyName, parameters);

            if (instance != null)
            {
                WebPartZoneInstance part = instance.GetZone(zoneId);
                if (part != null)
                {
                    return part.GetValue(propertyName);
                }
            }

            return null;
        }
    }
}
