using CMS;
using CMS.MacroEngine;
using CMS.Newsletters;

[assembly: RegisterExtension(typeof(NewsletterInfoMethods), typeof(NewsletterInfo))]


namespace CMS.Newsletters
{    
    /// <summary>
    /// Macro methods for class NewsletterInfo
    /// </summary>
    public class NewsletterInfoMethods : MacroMethodContainer
    {
        #region "Methods"

        /// <summary>
        /// Macro method for method GetNiceName
        /// </summary>
        [MacroMethod(typeof(string), "Gets the nice name of the newsletter based on its type", 0)]
        [MacroMethodParam(0, "obj", typeof(NewsletterInfo), "Object instance")]
        public static object GetNiceName(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                NewsletterInfo obj = GetParamValue(parameters, 0, default(NewsletterInfo));

                // If the main object is null, avoid null reference exception and just return null
                if (obj == null)
                {
                    return null;
                }

                // Call the method
                return obj.GetNiceName();
            }

            throw new System.NotSupportedException();
        }

        #endregion
    }
}
