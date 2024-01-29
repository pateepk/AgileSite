using System;
using System.Collections.Generic;
using System.Reflection;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Container for the property registration
    /// </summary>
    public class RegisteredProperties<TParent>
    {
        #region "Variables"

        /// <summary>
        /// Collection of the properties registered by the lambda function.
        /// </summary>
        protected StringSafeDictionary<PropertySettings<TParent>> mPropertyFunctions = null;


        /// <summary>
        /// List of registered properties
        /// </summary>
        protected List<string> mRegisteredProperties = null;


        /// <summary>
        /// If true, the properties of this type were already registered
        /// </summary>
        protected bool mPropertiesRegistered = false;


        /// <summary>
        /// Registration callback handler
        /// </summary>
        public delegate void RegistrationCallbackHandler();

        /// <summary>
        /// Registration callback method
        /// </summary>
        protected RegistrationCallbackHandler mRegistrationCallback = null;


        /// <summary>
        /// Flags to identify the properties to register
        /// </summary>
        private const BindingFlags PROPERTY_FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        // SyncRoot for mRegisteredProperties collection
        private readonly object regPropSyncRoot = new object();

        #endregion


        #region "Sync methods"

        /// <summary>
        /// Adds the property name to the collection of registered properties (Ensures locking for thread safety)
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void AddProperty(string propertyName)
        {
            if (mRegisteredProperties != null)
            {
                lock (regPropSyncRoot)
                {
                    mRegisteredProperties.Add(propertyName);
                }
            }
        }


        /// <summary>
        /// Removes the property name to the collection of registered properties (Ensures locking for thread safety)
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Returns true if property name was successfully removed, otherwise false</returns>
        protected bool RemoveProperty(string propertyName)
        {
            if (mRegisteredProperties != null)
            {
                lock (regPropSyncRoot)
                {
                    return mRegisteredProperties.Remove(propertyName);
                }
            }

            return false;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static RegisteredProperties()
        {
            TypeManager.RegisterGenericType(typeof(RegisteredProperties<TParent>));
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="registrationCallback">Callback method called when the request for registering of the properties is made</param>
        public RegisteredProperties(RegistrationCallbackHandler registrationCallback)
        {
            mRegistrationCallback = registrationCallback;
        }


        /// <summary>
        /// Gets list of registered properties.
        /// </summary>
        public List<string> GetRegisteredProperties()
        {
            if (!mPropertiesRegistered)
            {
                lock (this)
                {
                    if (!mPropertiesRegistered)
                    {
                        mRegisteredProperties = new List<string>();
                        mPropertyFunctions = new StringSafeDictionary<PropertySettings<TParent>>();

                        // Register the properties
                        mRegistrationCallback?.Invoke();

                        mPropertiesRegistered = true;
                    }
                }
            }

            return mRegisteredProperties;
        }


        /// <summary>
        /// Gets the type of the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public Type GetPropertyType(string propertyName)
        {
            // Ensure the properties
            GetRegisteredProperties();

            Type result = null;

            // Get the property registration
            var property = mPropertyFunctions[propertyName];
            if (property != null)
            {
                result = property.PropertyType;
            }

            return result;
        }


        /// <summary>
        /// Gets the empty object of the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        internal object GetPropertyEmptyObject(string propertyName)
        {
            // Ensure the properties
            GetRegisteredProperties();

            object result = null;

            // Get the property registration
            var property = mPropertyFunctions[propertyName];
            if (property != null)
            {
                result = property.EmptyObject;
            }

            return result;
        }


        /// <summary>
        /// Returns true if given property is already registered.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public bool Contains(string propertyName)
        {
            return mPropertyFunctions.ContainsKey(propertyName);
        }


        /// <summary>
        /// Evaluates the particular property
        /// </summary>
        /// <param name="obj">Calling object</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returning the property value</param>
        public bool Evaluate(TParent obj, string columnName, out object value)
        {
            // Ensure the properties
            GetRegisteredProperties();

            bool result = false;
            value = null;

            // Get the property binding
            var property = mPropertyFunctions[columnName];
            if (property != null)
            {
                // Evaluate the value
                value = property.Evaluate(obj);
                result = true;
            }

            return result;
        }


        /// <summary>
        /// Evaluates the particular property
        /// </summary>
        /// <param name="obj">Calling object</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returning the property value</param>
        public bool Set(TParent obj, string columnName, object value)
        {
            // Ensure the properties
            GetRegisteredProperties();

            bool result = false;

            // Get the property binding
            var property = mPropertyFunctions[columnName];
            if (property != null)
            {
                // Set the value
                property.Set(obj, value);
                result = true;
            }

            return result;
        }


        /// <summary>
        /// Returns true if given property has a setter
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool HasSetter(string columnName)
        {
            // Get the property binding
            var property = mPropertyFunctions[columnName];

            return (property != null) && property.HasSetter();
        }


        /// <summary>
        /// Registers the given parametrized property to the object.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="parameter">Parameter for the lambda expression</param>
        /// <param name="lambdaExpr">Lambda expression for the getter of the property (object, parameter) => return value</param>
        /// <param name="setLambdaExpr">Lambda expression for the setter of the property (object, parameter, value) => set</param>
        public PropertySettings<TParent> Add<PropertyType>(string propertyName, object parameter, Func<TParent, object, object> lambdaExpr, Action<TParent, object, object> setLambdaExpr) where PropertyType : new()
        {
            // Register the property
            var prop = new PropertySettings<TParent>(propertyName, lambdaExpr, setLambdaExpr, parameter, null);
            if (typeof(PropertyType) != typeof(object))
            {
                prop.PropertyType = typeof(PropertyType);
            }

            if (!mPropertyFunctions.ContainsKey(propertyName))
            {
                // Register the property
                AddProperty(propertyName);
            }

            mPropertyFunctions[propertyName] = prop;

            return prop;
        }


        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="lambdaExpr">Lambda expression for the getter of the property (object) => return value</param>
        /// <param name="setLambdaExpr">Lambda expression for the setter of the property (object, value) => set</param>
        public PropertySettings<TParent> Add<TProperty>(string propertyName, Func<TParent, object> lambdaExpr, Action<TParent, object> setLambdaExpr)
        {
            if (lambdaExpr == null)
            {
                // Remove the property upon request (func == null)
                mPropertyFunctions.Remove(propertyName);
                RemoveProperty(propertyName);

                return null;
            }

            // Register the property
            var prop = new PropertySettings<TParent>(propertyName, lambdaExpr, setLambdaExpr, null);
            if (typeof(TProperty) != typeof(object))
            {
                prop.PropertyType = typeof(TProperty);
            }

            if (!mPropertyFunctions.ContainsKey(propertyName))
            {
                AddProperty(propertyName);
            }

            mPropertyFunctions[propertyName] = prop;

            return prop;
        }


        /// <summary>
        /// Hides the given property from the property list
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <exception cref="System.Exception">Thrown when property is not registered and therefore cannot be hidden.</exception>
        public void Hide(string propertyName)
        {
            if (!RemoveProperty(propertyName))
            {
                throw new Exception("[RegisteredProperties.Hide]: Property '" + propertyName + "' is not registered and therefore cannot be hidden.");
            }
        }


        /// <summary>
        /// Collects the marked properties from the given object
        /// </summary>
        /// <param name="type">Type for which collect the properties</param>
        public void CollectProperties(Type type)
        {
            bool registerAll = type.GetCustomAttributes(typeof(RegisterAllPropertiesAttribute), true).Length > 0;

            // Search properties by their registration flag
            var props = type.GetProperties(PROPERTY_FLAGS);

            foreach (var p in props)
            {
                // Register all only from declaring type
                if (registerAll && (p.DeclaringType == type))
                {
                    // Check if excluded
                    bool excluded = p.GetCustomAttributes(typeof(NotRegisterPropertyAttribute), true).Length > 0;
                    if (!excluded)
                    {
                        // If column, do not register as property
                        bool column = p.GetCustomAttributes(typeof(RegisterColumnAttribute), true).Length > 0;
                        if (!column)
                        {
                            RegisterProperty(p, null, false);
                        }
                    }
                }

                // Register property with explicit flag RegisterProperty
                var attributes = p.GetCustomAttributes(typeof(RegisterPropertyAttribute), true);

                foreach (AbstractPropertyAttribute attribute in attributes)
                {
                    var hidden = attribute.Hidden;

                    RegisterProperty(p, attribute.Name, hidden);
                }
            }

            // Collect registered extensions
            if (typeof(IExtensible).IsAssignableFrom(typeof(TParent)))
            {
                var extensionProps = Extension<object>.GetPropertiesForType(type);
                if (extensionProps != null)
                {
                    foreach (var prop in extensionProps.TypedValues)
                    {
                        if (!String.IsNullOrEmpty(prop.Name))
                        {
                            var p = prop;

                            Add<object>(p.Name, o => ((IExtensible)o).Property<object>(p.Name).Value, null);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Collects the marked properties from the given object
        /// </summary>
        /// <param name="type">Type for which collect the properties</param>
        public void CollectColumns(Type type)
        {
            // Search properties by their registration flag
            var members = type.GetMembers(PROPERTY_FLAGS);

            foreach (var member in members)
            {
                // Register property with explicit flag RegisterProperty
                var attributes = member.GetCustomAttributes(typeof(RegisterColumnAttribute), true);

                foreach (AbstractPropertyAttribute attribute in attributes)
                {
                    var hidden = attribute.Hidden;

                    // Property
                    var prop = member as PropertyInfo;
                    if (prop != null)
                    {
                        RegisterProperty(prop, attribute.Name, hidden);
                    }
                    else
                    {
                        // Field
                        var field = member as FieldInfo;
                        if (field != null)
                        {
                            RegisterField(field, attribute.Name, hidden);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Registers the given property
        /// </summary>
        /// <param name="propertyInfo">Property info</param>
        /// <param name="name">Property name</param>
        /// <param name="hidden">If true, the property is registered as hidden</param>
        private void RegisterProperty(PropertyInfo propertyInfo, string name, bool hidden)
        {
            // Do not register indexer property
            var parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0)
            {
                return;
            }

            name = name ?? propertyInfo.Name;

            // Get getter and setter
            Func<TParent, object> getter = null;
            if (propertyInfo.CanRead)
            {
                getter = o => propertyInfo.GetValue(o, null);
            }

            Action<TParent, object> setter = null;
            if (propertyInfo.CanWrite)
            {
                setter = (o, v) => propertyInfo.SetValue(o, v, null);
            }

            // Register the property
            if ((getter != null) || (setter != null))
            {
                var prop = Add<object>(name, getter, setter);
                prop.PropertyType = propertyInfo.PropertyType;

                // Hide if hidden
                if (hidden)
                {
                    Hide(name);
                }
            }
        }


        /// <summary>
        /// Registers the given property
        /// </summary>
        /// <param name="fieldInfo">Field info</param>
        /// <param name="name">Field name</param>
        /// <param name="hidden">If true, the field is registered as hidden</param>
        private void RegisterField(FieldInfo fieldInfo, string name, bool hidden)
        {
            name = name ?? fieldInfo.Name;

            // Get getter
            Func<TParent, object> getter = o => fieldInfo.GetValue(o);

            // Register the field
            var prop = Add<object>(name, getter, null);
            prop.PropertyType = fieldInfo.FieldType;

            // Hide if hidden
            if (hidden)
            {
                Hide(name);
            }
        }

        #endregion
    }
}