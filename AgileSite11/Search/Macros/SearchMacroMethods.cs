using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Search;

[assembly: RegisterExtension(typeof(SearchMacroMethods), typeof(SystemNamespace))]

namespace CMS.Search
{
    /// <summary>
    /// Content methods - wrapping methods for macro resolver.
    /// </summary>
    public class SearchMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns DateTime string usable in smart search.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// Value to convert;
        /// Default value
        /// </param>
        [MacroMethod(typeof(string), "Returns DateTime string usable in smart search.", 1)]
        [MacroMethodParam(0, "object", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(DateTime), "Default value.")]
        public static object ToSearchDateTime(EvaluationContext context, params object[] parameters)
        {
            DateTime dtResult = DateTime.MinValue;

            switch (parameters.Length)
            {
                case 1:
                    // Convert to date time
                    dtResult = ValidationHelper.GetDateTime(parameters[0], DateTime.MinValue, context.Culture);
                    break;

                case 2:
                    // Convert to date time
                    dtResult = ValidationHelper.GetDateTime(parameters[0], ValidationHelper.GetDateTime(parameters[1], DateTime.MinValue, context.Culture), context.Culture);
                    break;

                default:
                    throw new NotSupportedException();
            }
            
            return SearchValueConverter.DateToString(dtResult);
        }


        /// <summary>
        /// Returns list of the extensions (separated by comma and space) for which there is a content extractor registered in the system.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">No parameters</param>
        [MacroMethod(typeof(string), "Returns list of the extensions (separated with colons) for which there is a content extractor registered in the system.", 0, IsHidden = true)]
        public static object GetSupportedContentExtractors(EvaluationContext context, params object[] parameters)
        {
            return SearchTextExtractorManager.GetSupportedContentExtractors();
        }
    }
}