using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide debug methods in the MacroEngine.
    /// </summary>
    internal class DebugMethods : MacroMethodContainer
    {
        /// <summary>
        /// Registers the methods
        /// </summary>
        protected override void RegisterMethods()
        {
            base.RegisterMethods();

            if (SystemContext.DevelopmentMode)
            {
                // Register the DebugExpression method
                var debugExpr = new MacroMethod("DebugExpression", DebugExpression)
                {
                    Comment = "Wraps the macro expression to the debug container to examine its internal values.",
                    MinimumParameters = 1
                };

                debugExpr.AddParameter("object", typeof(object), "Object to be examined.", false, true);
                debugExpr.AddParameter("onlyPublic", typeof(bool), "If true, only public members are exposed. Default is false.");

                RegisterMethod(debugExpr);

                // Register the DebugExpression method
                var debugContext = new MacroMethod("DebugContext", DebugContext)
                {
                    Comment = "Wraps the macro context to the debug container to examine its internal values.",
                    MinimumParameters = 0
                };

                debugContext.AddParameter("onlyPublic", typeof(bool), "If true, only public members are exposed. Default is false.");

                RegisterMethod(debugContext);

            }
        }


        /// <summary>
        /// Wraps the macro context to the debug container to examine its internal values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        public static object DebugContext(EvaluationContext context, params object[] parameters)
        {
            // Only global admin has the ability to use reflection debug
            if (!CheckGlobalAdmin(context))
            {
                return null;
            }

            // Only public members
            var onlyPublic = GetOptionalParam(parameters, 0, false);

            // Wrap into a debug container
            var container = DebugContainer.Wrap(context, onlyPublic);

            return container;
        }


        /// <summary>
        /// Wraps the given expression to the debug container to examine its internal values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        public static object DebugExpression(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Only global admin has the ability to use reflection debug
                if (!CheckGlobalAdmin(context))
                {
                    return null;
                }

                var obj = parameters[0];

                // Only public members
                var onlyPublic = GetOptionalParam(parameters, 1, false);

                // Wrap into a debug container
                var container = DebugContainer.Wrap(obj, onlyPublic);

                return container;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Wraps the given object to the debug container to examine its internal values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Wraps the given object to the debug container to examine its internal values.", 1)]
        [MacroMethodParam(0, "object", typeof(object), "Object to be examined.")]
        [MacroMethodParam(1, "onlyPublic", typeof(bool), "If true, only public members are exposed. Default is false.")]
        public static object Debug(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Only global admin has the ability to use reflection debug
                if (!CheckGlobalAdmin(context))
                {
                    return null;
                }

                var obj = parameters[0];

                // Only public members
                var onlyPublic = GetOptionalParam(parameters, 1, false);

                // Wrap into a debug container
                var container = DebugContainer.Wrap(obj, onlyPublic);

                return container;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Wraps the given object to the debug container to examine its internal values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Wraps the given object to the debug container to examine its internal values.", 1)]
        [MacroMethodParam(0, "object", typeof(object), "Object to be examined.")]
        public static object CacheItem(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        if (!CheckGlobalAdmin(context))
                        {
                            return null;
                        }

                        var key = GetStringParam(parameters[0], null);

                        return CacheHelper.GetItem(key);
                    }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}