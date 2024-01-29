using System;
using System.Collections;
using System.Linq;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.PortalEngine;

[assembly: RegisterExtension(typeof(TransformationMacroMethods), typeof(BaseInfo))]
[assembly: RegisterExtension(typeof(TransformationMacroMethods), typeof(IEnumerable))]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Content methods - wrapping methods for macro resolver.
    /// </summary>
    internal class TransformationMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Applies transformation to list of items.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Collection of items as the first parameter, item transformation name as the second. Optionally, Content-before and content-after transformations may be supplied as 3. and 4. parameters.</param>
        [MacroMethod(typeof (string), "Applies transformation to list of items. Only Text/XML/HTML transformations are supported.", 2)]
        [MacroMethodParam(0, "collection", typeof (IEnumerable), "Collection of items.")]
        [MacroMethodParam(1, "transformationName", typeof (string), "Item transformation name.")]
        [MacroMethodParam(2, "contentBeforeTransformationName", typeof (string), "Name of the 'content before' transformation or 'content before' itself.")]
        [MacroMethodParam(3, "contentAfterTransformationName", typeof (string), "Name of the 'content after' transformation or 'content after' itself.")]
        public static object ApplyTransformation(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                bool isIEnumerable = (parameters[0] is IEnumerable);
                bool isBaseInfo = (parameters[0] is BaseInfo);

                if ((isIEnumerable || isBaseInfo) && !(parameters[0] is string))
                {
                    // Prepare the child resolver
                    StringBuilder sb = new StringBuilder();

                    // Apply before-transformation if any
                    if (parameters.Length > 2)
                    {
                        // Get transformation code
                        string contentBefore = GetStringParam(parameters[2], context.Culture);
                        contentBefore = GetTransformationCode(contentBefore);

                        // Resolve the macros
                        contentBefore = context.Resolver.ResolveMacros(contentBefore);
                        sb.Append(contentBefore);
                    }


                    // Apply rows-transformation
                    string content = GetStringParam(parameters[1], context.Culture);
                    content = GetTransformationCode(content);

                    // Create child resolver to use for resolving the items so we can add data without changing the original resolver
                    MacroResolver bodyResolver = context.Resolver.CreateChild();

                    // Apply the transformation to all items of IEnumerable
                    if (isIEnumerable)
                    {
                        IEnumerable en = (IEnumerable) parameters[0];

                        // Get count if data is collection
                        if (en is ICollection)
                        {
                            bodyResolver.SetNamedSourceData("DataItemCount", ((ICollection) en).Count);
                        }
                        else
                        {
                            // Count items if not a collection
                            int count = en.Cast<object>().Count();

                            bodyResolver.SetNamedSourceData("DataItemCount", count);
                        }

                        int index = 0;
                        foreach (object res in en)
                        {
                            bodyResolver.SetAnonymousSourceData(res);
                            bodyResolver.SetNamedSourceData("DataItemIndex", index);
                            bodyResolver.SetNamedSourceData("DisplayIndex", index);

                            // Resolve the macros
                            string item = bodyResolver.ResolveMacros(content);
                            sb.Append(item);

                            index++;
                        }
                    }
                    // Apply the transformation to BaseInfo
                    else
                    {
                        bodyResolver.SetAnonymousSourceData(parameters[0]);
                        string item = bodyResolver.ResolveMacros(content);
                        sb.Append(item);
                    }

                    // Apply after-transformation if any
                    if (parameters.Length > 3)
                    {
                        // Get transformation code
                        string contentAfter = GetStringParam(parameters[3], context.Culture);
                        contentAfter = GetTransformationCode(contentAfter);

                        // Resolve the macros
                        contentAfter = context.Resolver.ResolveMacros(contentAfter);
                        sb.Append(contentAfter);
                    }

                    // Return result if any
                    if (sb.Length > 0)
                    {
                        return sb.ToString();
                    }
                }

                return null;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Applies ad-hoc text transformation to list of items.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Collection of items as the first parameter, transformation text as the second</param>
        [MacroMethod(typeof (string), "Applies ad-hoc text transformation to list of items.", 2)]
        [MacroMethodParam(0, "collection", typeof (IEnumerable), "Collection of items.")]
        [MacroMethodParam(1, "transformationText", typeof (string), "Text of the transformation.")]
        public static object Transform(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    bool isIEnumerable = (parameters[0] is IEnumerable);
                    bool isBaseInfo = (parameters[0] is BaseInfo);

                    if ((isIEnumerable || isBaseInfo) && !(parameters[0] is string))
                    {
                        // Prepare the child resolver
                        StringBuilder sb = new StringBuilder();
                        string content = GetStringParam(parameters[1], context.Culture);

                        // Apply the transformation to all items
                        if (isIEnumerable)
                        {
                            IEnumerable en = (IEnumerable) parameters[0];

                            // Get count if data is collection
                            if (en is ICollection)
                            {
                                context.Resolver.SetNamedSourceData("DataItemCount", ((ICollection) en).Count);
                            }
                            else
                            {
                                // Count items if not a collection
                                int count = en.Cast<object>().Count();
                                context.Resolver.SetNamedSourceData("DataItemCount", count);
                            }

                            int index = 0;
                            foreach (object res in en)
                            {
                                context.Resolver.SetNamedSourceData("DataItemIndex", index);
                                context.Resolver.SetNamedSourceData("DisplayIndex", index);
                                context.Resolver.SetAnonymousSourceData(res);

                                // Resolve the macros
                                string item = context.Resolver.ResolveMacros(content);
                                sb.Append(item);
                                
                                index++;
                            }
                        }
                            // Apply the transformation to BaseInfo
                        else
                        {
                            context.Resolver.SetAnonymousSourceData(parameters[0]);
                            string item = context.Resolver.ResolveMacros(content);
                            sb.Append(item);
                        }

                        return sb.ToString();
                    }

                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns code of the specified transformation. If such transformation is not found, input string is returned without any change.
        /// </summary>
        /// <param name="transformation">Transformation name or transformation code</param>
        private static string GetTransformationCode(string transformation)
        {
            // Try to load transformation code
            TransformationInfo ti = TransformationInfoProvider.GetTransformation(transformation);
            if (ti != null)
            {
                switch (ti.TransformationType)
                {
                    case TransformationTypeEnum.Text:
                    case TransformationTypeEnum.Html:

                        // Return transformation code
                        return ti.TransformationCode;

                    default:

                        // Not supported transformation type
                        throw new NotSupportedException("[MacroMethods.ApplyTransformation]: Only text/html transformation is supported.");
                }
            }

            // Return input text
            return transformation;
        }
    }
}