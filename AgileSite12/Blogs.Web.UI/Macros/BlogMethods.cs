using System;

using CMS;
using CMS.Blogs.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(BlogMethods), typeof(TransformationNamespace))]

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Blog methods - wrapping methods for macro resolver.
    /// </summary>
    public class BlogMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns user name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns user name.", 1)]
        [MacroMethodParam(0, "userId", typeof (object), "User ID.")]
        public static object GetUserName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return BlogTransformationFunctions.GetUserName(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user full name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns user full name.", 1)]
        [MacroMethodParam(0, "userId", typeof (object), "User ID.")]
        public static object GetUserFullName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return BlogTransformationFunctions.GetUserFullName(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns number of comments of given blog.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (int), "Returns number of comments of the given blog.", 2)]
        [MacroMethodParam(0, "postId", typeof (object), "Post page ID.")]
        [MacroMethodParam(1, "postAliasPath", typeof (object), "Post alias path.")]
        public static object GetBlogCommentsCount(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3: // Use this case because of backward compatibility for removed trackback feature
                    return BlogTransformationFunctions.GetBlogCommentsCount(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets a list of links of tags assigned for the specific document pointing to the page with URL specified.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Gets a list of links of tags assigned for the specific page pointing to the page with URL specified.", 3)]
        [MacroMethodParam(0, "documentGroupId", typeof (object), "ID of the group page tags belong to.")]
        [MacroMethodParam(1, "documentTags", typeof (object), "String containing all the tags related to the page.")]
        [MacroMethodParam(2, "documentListPageUrl", typeof (string), "URL of the page displaying other pages of the specified tag.")]
        public static object GetDocumentTags(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return BlogTransformationFunctions.GetDocumentTags(parameters[0], parameters[1], ValidationHelper.GetString(parameters[2], ""));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}