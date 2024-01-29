using System;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Base;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(ContentMethods), typeof(TreeNode))]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Content methods - wrapping methods for macro resolver.
    /// </summary>
    public class ContentMethods : MacroMethodContainer
    {
        /// <summary>
        /// Casts node to specified page type node with available fields.
        /// </summary>
        /// <param name="settings">Evaluation context with child resolver</param>
        /// <param name="parameters">Page to cast as the first parameter, page type class name as the second</param>
        [MacroMethod(typeof(TreeNode), "Casts page to specified page type providing page type specific fields.", 2)]
        [MacroMethodParam(0, "document", typeof(string), "Page to cast.")]
        [MacroMethodParam(1, "className", typeof(string), "Class name of page type.")]
        public static object As(MacroSettings settings, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported

                    if ((parameters[0] is TreeNode) && (parameters[1] is string))
                    {
                        string className = parameters[1] as string;
                        TreeNode node = (TreeNode)parameters[0];
                        DataClassInfo dci = null;

                        // Get different data class
                        if ((node != null) && !node.ClassName.EqualsCSafe(className, true))
                        {
                            dci = DataClassInfoProvider.GetDataClassInfo(className);
                        }

                        // Get specific document of different class
                        if (dci != null)
                        {
                            return TreeNode.New<TreeNode>(dci.ClassName, node, node.TreeProvider);
                        }
                        else
                        {
                            return node;
                        }
                    }

                    return parameters[0];

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if page is in one/all of the selected categories.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if page is in selected categories.", 2)]
        [MacroMethodParam(0, "document", typeof(TreeNode), "Page to check.")]
        [MacroMethodParam(1, "categories", typeof(string), "Category names separated with a semicolon.")]
        [MacroMethodParam(2, "allCategories", typeof(bool), "If true, page must be in all of the specified categories.")]
        public static object IsInCategories(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return ContentTransformationFunctions.IsInCategories(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return ContentTransformationFunctions.IsInCategories(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if page is translated to one/all of the selected cultures.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if page is translated to one of selected cultures.", 2)]
        [MacroMethodParam(0, "document", typeof(TreeNode), "Page to check.")]
        [MacroMethodParam(1, "cultures", typeof(string), "Culture codes separated with a semicolon.")]
        [MacroMethodParam(2, "publishedOnly", typeof(bool), "Indicates whether only published translations should be considered.")]
        public static object IsTranslatedTo(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return ContentTransformationFunctions.IsTranslatedTo(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return ContentTransformationFunctions.IsTranslatedTo(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }

        
        /// <summary>
        /// Returns true if page has any/all of the specified tags.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if page has specified tags.", 2)]
        [MacroMethodParam(0, "document", typeof(TreeNode), "Page to check.")]
        [MacroMethodParam(1, "tags", typeof(string), "List of tags separated with a semicolon.")]
        [MacroMethodParam(2, "allTags", typeof(bool), "If true, page must have all specified tags. If false, one of specified tags is sufficient.")]
        public static object HasTags(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return ContentTransformationFunctions.HasTags(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return ContentTransformationFunctions.HasTags(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if page is in specified relationship with selected document.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if page has left or right relationship with specified page.", 4)]
        [MacroMethodParam(0, "document", typeof(TreeNode), "Page to check.")]
        [MacroMethodParam(1, "side", typeof(string), "Side of checked page in the relationship.")]
        [MacroMethodParam(2, "relationship", typeof(string), "Relationship name.")]
        [MacroMethodParam(3, "relatedDocument", typeof(string), "Related page.")]
        [MacroMethodParam(4, "relatedDocumentSite", typeof(string), "Related page site name.")]
        public static object IsInRelationship(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 5:
                    return ContentTransformationFunctions.IsInRelationship(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetString(parameters[2], null), ValidationHelper.GetString(parameters[3], null), ValidationHelper.GetString(parameters[4], null));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}