using System;

using CMS;
using CMS.DocumentEngine.Web.UI;
using CMS.MacroEngine;
using CMS.Forums.Web.UI;

[assembly: RegisterExtension(typeof(ForumMethods), typeof(TransformationNamespace))]

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Forum methods - wrapping methods for macro resolver.
    /// </summary>
    public class ForumMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns link to selected post.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns link to selected post.", 2)]
        [MacroMethodParam(0, "postIdPath", typeof (object), "Post ID path.")]
        [MacroMethodParam(1, "forumId", typeof (object), "Forum ID.")]
        [MacroMethodParam(2, "aliasPath", typeof (object), "Alias path.")]
        public static object GetPostURL(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return ForumTransformationFunctions.GetPostURL(parameters[0], parameters[1]);

                case 3:
                    return ForumTransformationFunctions.GetPostURL(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}