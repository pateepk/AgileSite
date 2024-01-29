using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro methods.
    /// </summary>
    public class MacroMethods : CoreMethods
    {
        #region "Variables"

        /// <summary>
        /// Table of available methods.
        /// </summary>
        protected static SafeDictionary<string, MacroMethod> mMethods = null;

        /// <summary>
        /// List of methods which are available for any type
        /// </summary>
        protected static List<MacroMethod> mAnyTypeMethods = null;

        /// <summary>
        /// Index of methods available for specified type.
        /// </summary>
        protected static SafeDictionary<Type, List<MacroMethod>> mMethodsByType = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the hashtable with all methods.
        /// </summary>
        public static SafeDictionary<string, MacroMethod> Methods
        {
            get
            {
                return mMethods;
            }
        }

        #endregion


        #region "Management methods"

        #region "General management methods"

        /// <summary>
        /// Gets the specific method delegate.
        /// </summary>
        /// <param name="name">Method name</param>
        public static MacroMethod GetMethod(string name)
        {
            if (mMethods != null)
            {
                if (mMethods.Contains(name.ToLowerCSafe()))
                {
                    return mMethods[name.ToLowerCSafe()];
                }
            }

            return null;
        }


        /// <summary>
        /// Executes the given method with parameters.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="resolver">Resolver object</param>
        /// <param name="parameters">Method parameters</param>
        public static object ExecuteMethod(string name, MacroResolver resolver, params object[] parameters)
        {
            MacroMethod retval = GetMethod(name);
            if (retval.MethodResolver != null)
            {
                // Call with resolver object
                Func<MacroResolver, object[], object> method = retval.MethodResolver;
                return method(resolver, parameters);
            }
            else if (retval.Method != null)
            {
                // Call without resolver object
                Func<object[], object> method = retval.Method;
                return method(parameters);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Executes the given method with parameters.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="parameters">Method parameters</param>
        public static object ExecuteMethod(string name, params object[] parameters)
        {
            return ExecuteMethod(name, null, parameters);
        }


        /// <summary>
        /// Returns true if methods are registred in the hashtable of all methods.
        /// </summary>
        public static bool AreMethodsRegistered()
        {
            // Methods are registered if the hashtable is not empty and "+" method is registered.
            return (mMethods != null) && (mMethods["+"] != null);
        }


        /// <summary>
        /// Returns the list of macro methods suitable for specified object.
        /// </summary>
        /// <param name="obj">Object for which the list should be returned</param>
        public static List<MacroMethod> GetMethodsForObject(object obj)
        {
            return GetMethodsForObject(obj, true);
        }


        /// <summary>
        /// Returns the list of macro methods suitable for specified object.
        /// </summary>
        /// <param name="obj">Object for which the list should be returned</param>
        /// <param name="includeAnyTypeMethods">If true, methods for any type are included as well</param>
        public static List<MacroMethod> GetMethodsForObject(object obj, bool includeAnyTypeMethods)
        {
            return GetMethodsForObject(obj, includeAnyTypeMethods, false);
        }


        /// <summary>
        /// Returns the list of macro methods suitable for specified object.
        /// </summary>
        /// <param name="obj">Object for which the list should be returned</param>
        /// <param name="includeAnyTypeMethods">If true, methods for any type are included as well</param>
        /// <param name="exactType">If true only methods with exactly the same type are returned (= no inherited members)</param>
        public static List<MacroMethod> GetMethodsForObject(object obj, bool includeAnyTypeMethods, bool exactType)
        {
            // Any type methods
            if (obj == null)
            {
                if (mAnyTypeMethods == null)
                {
                    mAnyTypeMethods = new List<MacroMethod>();
                    if (Methods != null)
                    {
                        foreach (MacroMethod method in Methods.Values)
                        {
                            if (method.AllowedTypes == null)
                            {
                                mAnyTypeMethods.Add(method);
                            }
                        }
                    }
                }

                return mAnyTypeMethods;
            }

            if (mMethodsByType == null)
            {
                mMethodsByType = new SafeDictionary<Type, List<MacroMethod>>();
            }

            // Methods with defined types
            List<MacroMethod> retval = mMethodsByType[obj.GetType()];
            if (retval == null)
            {
                retval = new List<MacroMethod>();
                if (Methods != null)
                {
                    foreach (MacroMethod method in Methods.Values)
                    {
                        if (method.AllowedTypes == null)
                        {
                            if (includeAnyTypeMethods)
                            {
                                retval.Add(method);
                            }
                        }
                        else
                        {
                            bool add = method.AllowedTypes.Any(allowedType => (!exactType && allowedType.IsInstanceOfType(obj)) || (allowedType == obj.GetType()));
                            if (add)
                            {
                                retval.Add(method);
                            }
                        }
                    }
                }
                mMethodsByType[obj.GetType()] = retval;
            }
            return retval;
        }

        #endregion


        #region "Public registration methods"

        /// <summary>
        /// Registers the given method within the method table.
        /// </summary>
        /// <param name="method">Macro method to register</param>
        public static void RegisterMethod(MacroMethod method)
        {
            if (mMethods == null)
            {
                mMethods = new SafeDictionary<string, MacroMethod>();
            }

            mMethods[method.Name.ToLowerCSafe()] = method;

            // Clear the hashtables
            mAnyTypeMethods = null;
            mMethodsByType = null;
        }

        #endregion


        #region "Internal registration methods"

        /// <summary>
        /// Registers the given method within the method table.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        /// <param name="type">Return type of the method</param>
        /// <param name="comment">Comment for the method</param>
        /// <param name="minimumParameters">Minimal number of parameters needed to call the method</param>
        /// <param name="parameterDefinition">Parameter definition in format {{name, type, comment}, {name, type, comment}}</param>
        /// <param name="specialParameters">A list of special parameters needed to be supplied by resolver</param>
        /// <param name="allowedTypes">List of types for which the method is applicable (set to null for all types to be allowed)</param>
        protected static void RegisterMethodInternal(string name, Func<EvaluationContext, object[], object> method, Type type, string comment, int minimumParameters, object[,] parameterDefinition, string[] specialParameters, List<Type> allowedTypes)
        {
            // Create the object
            MacroMethod methodToReg = new MacroMethod(name, method)
            {
                Type = type,
                Comment = comment,
                MinimumParameters = minimumParameters,
                SpecialParameters = specialParameters,
                AllowedTypes = allowedTypes,
            };

            AddParametersAndRegister(parameterDefinition, methodToReg);
        }


        /// <summary>
        /// Registers the given method within the method table.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        /// <param name="type">Return type of the method</param>
        /// <param name="comment">Comment for the method</param>
        /// <param name="minimumParameters">Minimal number of parameters needed to call the method</param>
        /// <param name="parameterDefinition">Parameter definition in format {{name, type, comment}, {name, type, comment}}</param>
        /// <param name="specialParameters">A list of special parameters needed to be supplied by resolver</param>
        /// <param name="allowedTypes">List of types for which the method is applicable (set to null for all types to be allowed)</param>
        protected static void RegisterMethodInternal(string name, Func<MacroResolver, object[], object> method, Type type, string comment, int minimumParameters, object[,] parameterDefinition, string[] specialParameters, List<Type> allowedTypes)
        {
            // Create the object
            MacroMethod methodToReg = new MacroMethod(name, method)
            {
                Type = type,
                Comment = comment,
                MinimumParameters = minimumParameters,
                SpecialParameters = specialParameters,
                AllowedTypes = allowedTypes,
            };

            AddParametersAndRegister(parameterDefinition, methodToReg);
        }


        /// <summary>
        /// Registers the given method within the method table.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        /// <param name="type">Return type of the method</param>
        /// <param name="comment">Comment for the method</param>
        /// <param name="minimumParameters">Minimal number of parameters needed to call the method</param>
        /// <param name="parameterDefinition">Parameter definition in format {{name, type, comment}, {name, type, comment}}</param>
        /// <param name="specialParameters">A list of special parameters needed to be supplied by resolver</param>
        /// <param name="allowedTypes">List of types for which the method is applicable (set to null for all types to be allowed)</param>
        protected static void RegisterMethodInternal(string name, Func<object[], object> method, Type type, string comment, int minimumParameters, object[,] parameterDefinition, string[] specialParameters, List<Type> allowedTypes)
        {
            // Create the object
            MacroMethod methodToReg = new MacroMethod(name, method)
            {
                Type = type,
                Comment = comment,
                MinimumParameters = minimumParameters,
                SpecialParameters = specialParameters,
                AllowedTypes = allowedTypes,
            };

            AddParametersAndRegister(parameterDefinition, methodToReg);
        }


        /// <summary>
        /// Adds the parameters from definition and registres the method to the system.
        /// </summary>
        /// <param name="parameterDefinition">Parameter definition</param>
        /// <param name="methodToReg">Method to register</param>
        private static void AddParametersAndRegister(object[,] parameterDefinition, MacroMethod methodToReg)
        {
            // Add the parameters
            if (parameterDefinition != null)
            {
                for (int i = 0; i <= parameterDefinition.GetUpperBound(0); i++)
                {
                    string paramName = parameterDefinition[i, 0].ToString();
                    string paramComment = parameterDefinition[i, 2].ToString();
                    Type paramType = (Type)parameterDefinition[i, 1];
                    bool isParams = false;
                    if (parameterDefinition.GetUpperBound(1) == 3)
                    {
                        isParams = (bool)parameterDefinition[i, 3];
                    }

                    methodToReg.AddParameter(paramName, paramType, paramComment, isParams);
                }
            }

            // Register the method
            RegisterMethod(methodToReg);
        }

        #endregion

        #endregion
    }
}