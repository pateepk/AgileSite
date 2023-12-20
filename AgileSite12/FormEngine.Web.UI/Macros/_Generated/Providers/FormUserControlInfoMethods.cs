using CMS;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(FormUserControlInfoMethods), typeof(FormUserControlInfo))]


namespace CMS.FormEngine.Web.UI
{    
    /// <summary>
    /// Macro methods for class FormUserControlInfo
    /// </summary>
    public class FormUserControlInfoMethods : MacroMethodContainer
    {
        #region "Methods"

        /// <summary>
        /// Macro method for method GetUsageObjectTypes
        /// </summary>
        [MacroMethod(typeof(System.Collections.Generic.IEnumerable<System.String>), "Gets the list of object types that may use the form control", 0)]
        [MacroMethodParam(0, "obj", typeof(FormUserControlInfo), "Object instance")]
        public static object GetUsageObjectTypes(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                FormUserControlInfo obj = GetParamValue(parameters, 0, default(FormUserControlInfo));

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
        [MacroMethod(typeof(CMS.DataEngine.IDataQuery), "Gets the objects using the form control as a query with result columns ObjectType, ObjectID.", 0)]
        [MacroMethodParam(0, "obj", typeof(FormUserControlInfo), "Object instance")]
        public static object GetUsages(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                FormUserControlInfo obj = GetParamValue(parameters, 0, default(FormUserControlInfo));

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