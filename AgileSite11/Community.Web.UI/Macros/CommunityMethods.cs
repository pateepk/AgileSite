using System;

using CMS;
using CMS.Community.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(CommunityMethods), typeof(TransformationNamespace))]

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Community methods - wrapping methods for macro resolver.
    /// </summary>
    public class CommunityMethods : MacroMethodContainer
    {
        /// <summary>
        /// Gets access text for given access mode
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns group access text for given access mode", 1)]
        [MacroMethodParam(0, "accessMode", typeof (int), "Access mode")]
        public static string GetGroupAccessText(EvaluationContext context, object[] parameters)
        {
            // 0th parameter is CommunityContext, 1st is group access
            int accessMode = ValidationHelper.GetInteger(parameters[1], 0);

            switch (accessMode)
            {
                case 0:
                    return ResHelper.GetString("group.group.accessanybody");

                case 1:
                    return ResHelper.GetString("group.group.accesssitemembers");

                case 3:
                    return ResHelper.GetString("group.group.accessgroupmembers");

                default:
                    return String.Empty;
            }
        }
    }
}
