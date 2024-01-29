using System;

using CMS;
using CMS.DocumentEngine;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(TreeNodeCollectionMethods), typeof(TreeNodeCollection))]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// TreeNodeCollection methods - wrapping methods for macro resolver.
    /// </summary>
    public class TreeNodeCollectionMethods : MacroMethodContainer
    {
        /// <summary>
        /// Class names on collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(TreeNodeCollection), "Class names on page collection.", 1)]
        [MacroMethodParam(0, "collection", typeof (TreeNodeCollection), "Collection to filter.")]
        [MacroMethodParam(1, "classNames", typeof (string), "List of class names separated by semicolon.")]
        public static object ClassNames(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported

                    if (parameters[0] is TreeNodeCollection)
                    {
                        string classNames = GetStringParam(parameters[1], context.Culture);

                        TreeNodeCollection col = (TreeNodeCollection) ((TreeNodeCollection) parameters[0]).CloneCollection();
                        col.ClassNames = classNames;

                        return col;
                    }

                    return null;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}