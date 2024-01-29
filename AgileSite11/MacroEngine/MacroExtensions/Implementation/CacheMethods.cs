using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide basic caching methods in the MacroEngine.
    /// </summary>
    internal class CacheMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns CMSCacheDependency object created from given string.
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(CMSCacheDependency), "Returns CMSCacheDependency object created from given string.", 1)]
        [MacroMethodParam(0, "dependencies", typeof(string), "Cache dependency strings.", IsParams = true)]
        public static object GetCacheDependency(params object[] parameters)
        {
            bool includeDefaultDependencies = false;
            var keys = new List<string>();
            
            foreach (object param in parameters)
            {
                if (param is bool)
                {
                    includeDefaultDependencies = (bool)param;
                }
                else
                {
                    keys.Add(ValidationHelper.GetString(param, ""));
                }
            }

            // Build the dependency and set the properties
            var dependency = (keys.Count > 0) ? CacheHelper.GetCacheDependency(keys) : CacheHelper.GetCacheDependency((string[])null, null);

            dependency.IncludeDefaultDependencies = includeDefaultDependencies;

            return dependency;
        }


        /// <summary>
        /// Caches the result of the given expression. The expression is evaluated only when not found in cache.
        /// </summary>
        /// <param name="evalContext">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Evaluates the given expression and puts it to the cache. The expression is evaluated only when not found in cache.", 1)]
        [MacroMethodParam(0, "expression", typeof(object), "Expression to be evaluated and cached.", AsExpression = true)]
        [MacroMethodParam(1, "cacheMinutes", typeof(int), "Cache minutes")]
        [MacroMethodParam(2, "condition", typeof(bool), "Cache condition")]
        [MacroMethodParam(3, "cacheItemName", typeof(string), "Cache item name.")]
        [MacroMethodParam(4, "cacheItemNameParts", typeof(string), "Cache item name parts.")]
        [MacroMethodParam(5, "cacheDependency", typeof(CMSCacheDependency), "Cache dependency object (use GetCacheDependency method to get the object).")]
        public static object Cache(EvaluationContext evalContext, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                // Get the expression
                var expr = parameters[0] as MacroExpression;
                if (expr != null)
                {
                    var resolver = evalContext.Resolver;

                    // Prepare the caching parameters
                    int cacheMinutes = GetParamValue(parameters, 1, 10);
                    bool condition = GetParamValue(parameters, 2, true);

                    string customCacheItemName = GetParamValue<string>(parameters, 3, null);

                    // Get item name parts
                    CMSCacheDependency dependency = null;

                    var itemNameParts = new List<string>();

                    for (int i = 4; i < parameters.Length; i++)
                    {
                        dependency = parameters[i] as CMSCacheDependency;

                        if (dependency == null)
                        {
                            itemNameParts.Add(GetStringParam(parameters[i], evalContext.Culture));
                        }
                    }

                    // Use default cache item name if original not provided
                    if ((itemNameParts.Count == 0) && string.IsNullOrEmpty(customCacheItemName))
                    {
                        customCacheItemName = String.Format("{0}{1}{2}", CacheHelper.BaseCacheKey, CacheHelper.SEPARATOR, expr);
                    }

                    // Additional cache key prevents evil user from poisoning system cache
                    customCacheItemName = String.Format("{0}{1}{2}", CacheHelper.MACRO_KEY, CacheHelper.SEPARATOR, customCacheItemName);

                    object res = CacheHelper.Cache(cs =>
                        {
                            // Set clone of the context with empty cache dependencies
                            var origSettings = resolver.Settings;
                            var settings = resolver.Settings.Clone();

                            if (settings.CacheDependencies != null)
                            {
                                settings.CacheDependencies.Clear();
                            }
                            resolver.Settings = settings;

                            // Deal with the dependencies (track them in inner expression only when user dependency is not defined or has IncludeDefaultDependencies flag)
                            if ((dependency == null) || dependency.IncludeDefaultDependencies)
                            {
                                settings.TrackCacheDependencies = true;
                            }
                            else
                            {
                                settings.TrackCacheDependencies = false;
                            }


                            // Evaluate the expression
                            bool securityPassed;
                            var result = GetLazyParamValue(expr, evalContext, out securityPassed);

                            if (!securityPassed)
                            {
                                MacroDebug.LogSecurityCheckFailure(evalContext.OriginalExpression, evalContext.UserName, evalContext.IdentityName, evalContext.User?.UserName);
                                cs.BoolCondition = false;
                                return null;
                            }

                            resolver.Settings = origSettings;

                            // Add default dependencies
                            if (settings.TrackCacheDependencies)
                            {
                                if (dependency == null)
                                {
                                    // No user defined dependency, create new dependency with automatic ones
                                    dependency = CacheHelper.GetCacheDependency(settings.FileCacheDependencies, settings.CacheDependencies);
                                }
                                else
                                {
                                    // Union of custom dependencies and automatic ones
                                    var cacheDependencies = new List<string>();

                                    if (settings.CacheDependencies != null)
                                    {
                                        cacheDependencies.AddRange(settings.CacheDependencies);
                                    }
                                    if (dependency.CacheKeys != null)
                                    {
                                        cacheDependencies.AddRange(dependency.CacheKeys);
                                    }

                                    dependency = CacheHelper.GetCacheDependency(settings.FileCacheDependencies, cacheDependencies);
                                }
                            }

                            // Put the data to cache
                            cs.CacheDependency = dependency;

                            return result;
                        },
                        new CacheSettings(cacheMinutes)
                            {
                                BoolCondition = condition, 
                                CustomCacheItemName = customCacheItemName,
                                CacheItemNameParts = itemNameParts
                            }
                    );
                    
                    return res;
                }
            }

            throw new NotSupportedException();
        }
    }
}