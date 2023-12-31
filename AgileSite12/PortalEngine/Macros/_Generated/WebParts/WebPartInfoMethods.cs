﻿using CMS;
using CMS.MacroEngine;
using CMS.PortalEngine;

[assembly: RegisterExtension(typeof(WebPartInfoMethods), typeof(WebPartInfo))]


namespace CMS.PortalEngine
{    
    /// <summary>
    /// Macro methods for class WebPartInfo
    /// </summary>
    public class WebPartInfoMethods : MacroMethodContainer
    {
        #region "Methods"

        /// <summary>
        /// Macro method for method GetUsageObjectTypes
        /// </summary>
        [MacroMethod(typeof(System.Collections.Generic.IEnumerable<System.String>), "Gets the list of object types that may use the web part", 0)]
        [MacroMethodParam(0, "obj", typeof(WebPartInfo), "Object instance")]
        public static object GetUsageObjectTypes(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                WebPartInfo obj = GetParamValue(parameters, 0, default(WebPartInfo));

                // If the main object is null, avoid null reference exception and just return null
                if (obj == null)
                {
                    return null;
                }

                // Call the method
                return obj.GetUsageObjectTypes();
            }

            throw new System.NotSupportedException();
        }


        /// <summary>
        /// Macro method for method GetUsages
        /// </summary>
        [MacroMethod(typeof(CMS.DataEngine.IDataQuery), "Gets the objects using the web part as a query with result columns ObjectType, ObjectID.", 0)]
        [MacroMethodParam(0, "obj", typeof(WebPartInfo), "Object instance")]
        public static object GetUsages(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                WebPartInfo obj = GetParamValue(parameters, 0, default(WebPartInfo));

                // If the main object is null, avoid null reference exception and just return null
                if (obj == null)
                {
                    return null;
                }

                // Call the method
                return obj.GetUsages();
            }

            throw new System.NotSupportedException();
        }

        #endregion
    }
}
