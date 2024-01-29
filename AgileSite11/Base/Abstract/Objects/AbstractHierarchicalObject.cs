using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Abstract class for the hierarchical objects
    /// </summary>
    public abstract class AbstractHierarchicalObject<TObject> : AbstractDataContainer<TObject>, IVirtualHierarchicalObject
        where TObject : AbstractHierarchicalObject<TObject>
    {
        #region "Variables"

        // Properties available through the context.
        private static List<string> mProperties;

        // Local properties available through the context.
        private List<string> mLocalProperties;

        // Registered properties
        private static RegisteredProperties<TObject> mRegisteredProperties;

        // Local registered properties
        private RegisteredProperties<TObject> mLocalRegisteredProperties;

        // PropertyList SyncRoot
        private static readonly object propertyListSyncRoot = new object();

        // RegisteredProperties SyncRoot
        private static readonly object registeredPropSyncRoot = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the object uses local properties
        /// </summary>
        protected bool UseLocalProperties
        {
            get;
            set;
        }


        /// <summary>
        /// Used property list
        /// </summary>
        protected List<string> PropertyList
        {
            get
            {
                if (UseLocalProperties)
                {
                    return mLocalProperties;
                }
                else
                {
                    return mProperties;
                }
            }
            set
            {
                if (UseLocalProperties)
                {
                    mLocalProperties = value;
                }
                else
                {
                    mProperties = value;
                }
            }
        }


        /// <summary>
        /// Properties available through the context.
        /// </summary>
        [XmlIgnore]
        public virtual List<string> Properties
        {
            get
            {
                if (PropertyList == null)
                {
                    lock (propertyListSyncRoot)
                    {
                        if (PropertyList == null)
                        {
                            // Get the registered properties
                            List<string> properties = RegisteredProperties.GetRegisteredProperties();

                            // Add all columns
                            List<string> columns = new List<string>();

                            foreach (string col in ColumnNames)
                            {
                                // Add column
                                columns.Add(col);
                            }

                            // Order the properties alphabetically
                            properties.Sort();

                            // Order the columns alphabetically
                            columns.Sort();

                            columns.AddRange(properties);

                            PropertyList = columns;
                        }
                    }
                }

                return PropertyList;
            }
        }

        #endregion


        #region "Property registration"

        /// <summary>
        /// Registered properties object
        /// </summary>
        protected RegisteredProperties<TObject> RegisteredPropertiesObject
        {
            get
            {
                if (UseLocalProperties)
                {
                    return mLocalRegisteredProperties;
                }

                return mRegisteredProperties;
            }
            set
            {
                if (UseLocalProperties)
                {
                    mLocalRegisteredProperties = value;
                }
                else
                {
                    mRegisteredProperties = value;
                }
            }
        }


        /// <summary>
        /// Registered properties
        /// </summary>
        protected RegisteredProperties<TObject> RegisteredProperties
        {
            get
            {
                if (RegisteredPropertiesObject == null)
                {
                    lock (registeredPropSyncRoot)
                    {
                        if (RegisteredPropertiesObject == null)
                        {
                            RegisteredPropertiesObject = new RegisteredProperties<TObject>(RegisterProperties);
                        }
                    }
                }

                return RegisteredPropertiesObject;
            }
        }


        /// <summary>
        /// Registers the given parameterized property to the object.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="parameter">Parameter for the lambda expression</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<TObject> RegisterProperty(string propertyName, object parameter, Func<TObject, object, object> ex)
        {
            return RegisteredProperties.Add<object>(propertyName, parameter, ex, null);
        }


        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<TObject> RegisterProperty(string propertyName, Func<TObject, object> ex)
        {
            return RegisteredProperties.Add<object>(propertyName, ex, null);
        }


        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<TObject> RegisterProperty<TProperty>(string propertyName, Func<TObject, object> ex) 
        {
            return RegisteredProperties.Add<TProperty>(propertyName, ex, null);
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected virtual void RegisterProperties()
        {
            RegisteredProperties.CollectProperties(typeof(TObject));
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractHierarchicalObject()
        {
            TypeManager.RegisterGenericType(typeof(AbstractHierarchicalObject<TObject>));
        }


        /// <summary>
        /// Combines current instance with other properties (modifies current instace!). 
        /// List properties are merged from all the participating typeinfos, other properties are taken as first non-null value (non-null is determined by nullValues collection).
        /// </summary>
        /// <param name="excludedProperties">Properties which should be ignored by the merging process</param>
        /// <param name="nullValues">Values which are considered as null values (value not specified)</param>
        /// <param name="objToMergeWith">Objects to be merged with current instance</param>
        public void CombineWith(List<string> excludedProperties, List<object> nullValues, params TObject[] objToMergeWith)
        {
            var objToMerge = new List<AbstractHierarchicalObject<TObject>>();
            objToMerge.Add(this);
            objToMerge.AddRange(objToMergeWith);

            // Go through all the properties and if the result is List, merge it
            foreach (var p in Properties)
            {
                if (HasSetter(p) && ((excludedProperties == null) || !excludedProperties.Contains(p)))
                {
                    // Check all the values for being a list (if at least one value is a list, we need to merge)
                    bool isList = false;
                    foreach (var typeInfo in objToMerge)
                    {
                        object valObj;
                        typeInfo.TryGetProperty(p, out valObj);
                        if (valObj is IList)
                        {
                            isList = true;
                            break;
                        }
                    }

                    if (isList)
                    {
                        IList resultVal = null;
                        foreach (var typeInfo in objToMerge)
                        {
                            object valObj;
                            typeInfo.TryGetProperty(p, out valObj);

                            var val = (IList)valObj;
                            if (val != null)
                            {
                                if (resultVal == null)
                                {
                                    // First non-empty item will be the base for merging
                                    resultVal = val;
                                }
                                else
                                {
                                    foreach (var item in val)
                                    {
                                        // Copy all the items which are not there yet to the final list
                                        if (!resultVal.Contains(item))
                                        {
                                            resultVal.Add(item);
                                        }
                                    }
                                }
                            }
                        }

                        SetProperty(p, resultVal);
                    }
                    else
                    {
                        // For other properties take first which is not [Unknown]
                        foreach (var typeInfo in objToMerge)
                        {
                            object valObj;
                            typeInfo.TryGetProperty(p, out valObj);

                            // Value is considered null if it's null or equals to any of the values specified in nullValues collection
                            bool isNull = (valObj == null);
                            if (!isNull && (nullValues != null))
                            {
                                foreach (var nullVal in nullValues)
                                {
                                    if (nullVal.Equals(valObj))
                                    {
                                        isNull = true;
                                    }
                                }
                            }

                            if (!isNull)
                            {
                                // Set the first non-null value
                                SetProperty(p, valObj);
                                break;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if given property has a setter
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool HasSetter(string columnName)
        {
            return RegisteredProperties.HasSetter(columnName) || RegisteredColumns.HasSetter(columnName);
        }


        /// <summary>
        /// Gets the context property.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Value to set</param>
        public virtual bool SetProperty(string name, object value)
        {
            var result = RegisteredProperties.Set((TObject)this, name, value);
            if (result)
            {
                return true;
            }

            return RegisteredColumns.Set((TObject)this, name, value);
        }


        /// <summary>
        /// Gets the context property.
        /// </summary>
        /// <param name="name">Property name</param>
        public virtual object GetProperty(string name)
        {
            object value;
            TryGetProperty(name, out value);

            return value;
        }


        /// <summary>
        /// Gets the context property.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Returning the value</param>
        public virtual bool TryGetProperty(string name, out object value)
        {
            // Try to get from registered properties
            bool result = RegisteredProperties.Evaluate((TObject)this, name, out value);
            if (result)
            {
                return true;
            }

            // Try to get from registered values
            return TryGetValue(name, out value);
        }


        /// <summary>
        /// Gets the type of the given property
        /// </summary>
        /// <param name="columnName">Property name</param>
        protected virtual Type GetPropertyType(string columnName)
        {
            // Try to get from registered properties
            Type result = RegisteredProperties.GetPropertyType(columnName);
            if (result == null)
            {
                result = RegisteredColumns.GetPropertyType(columnName);
            }

            return result;
        }


        /// <summary>
        /// Gets the empty object of the given property
        /// </summary>
        /// <param name="columnName">Property name</param>
        private object GetPropertyEmptyObject(string columnName)
        {
            // Try to get from registered properties
            var result = RegisteredProperties.GetPropertyEmptyObject(columnName);
            if (result == null)
            {
                result = RegisteredColumns.GetPropertyEmptyObject(columnName);
            }

            // Get empty object the default way
            if (result == null)
            {
                Type propertyType = GetPropertyType(columnName);
                result = ClassHelper.GetEmptyObject(propertyType);
            }

            return result;
        }


        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <param name="notNull">If true, the property attempts to return non-null values, at least it returns the empty object of the correct type</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetProperty(string columnName, out object value, bool notNull)
        {
            bool result = TryGetProperty(columnName, out value);

            // Handle the null value
            if (notNull && result && (value == null))
            {
                value = GetPropertyEmptyObject(columnName);
            }

            return result;
        }


        /// <summary>
        /// Gets the value of particular property
        /// </summary>
        /// <param name="name">Property name</param>
        public override object this[string name]
        {
            get
            {
                return GetProperty(name);
            }
            set
            {
                SetProperty(name, value);
            }
        }

        #endregion
    }
}