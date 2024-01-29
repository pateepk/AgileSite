using System;
using System.Collections;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Property transformation for a collection. Enumerates the properties of the collection objects and does transformation according to that property.
    /// </summary>
    public class CollectionPropertyTransformation<ResultType> : IHierarchicalObject
    {
        #region "Variables"

        /// <summary>
        /// Cached results of the transformation
        /// </summary>
        protected Hashtable mCachedResults = new Hashtable();

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the results of the transformation are cached
        /// </summary>
        public bool CacheResults
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the transformation is case sensitive (default is case insensitive)
        /// </summary>
        public bool CaseSensitive
        {
            get;
            set;
        }


        /// <summary>
        /// Parent collection
        /// </summary>
        public IInfoObjectCollection ParentCollection
        {
            get;
            protected set;
        }


        /// <summary>
        /// Transformation function
        /// </summary>
        protected Func<IInfoObjectCollection, string, ResultType> Transformation
        {
            get;
            set;
        }


        /// <summary>
        /// Transformation function for list of properties
        /// </summary>
        protected Func<IInfoObjectCollection, List<string>> PropertiesTransformation
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Parent collection</param>
        /// <param name="transformation">Transformation function</param>
        /// <param name="propertiesTransformation">Transformation to get list of properties</param>
        public CollectionPropertyTransformation(IInfoObjectCollection collection, Func<IInfoObjectCollection, string, ResultType> transformation, Func<IInfoObjectCollection, List<string>> propertiesTransformation)
        {
            ParentCollection = collection;
            Transformation = transformation;
            PropertiesTransformation = propertiesTransformation;
        }

        #endregion


        #region "IHierarchicalObject Members"

        /// <summary>
        /// Properties of the object available through GetProperty.
        /// </summary>
        public List<string> Properties
        {
            get
            {
                if (PropertiesTransformation != null)
                {
                    return PropertiesTransformation(ParentCollection);
                }

                return ParentCollection.Object.ColumnNames;
            }
        }


        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetProperty(string columnName)
        {
            object result = null;
            TryGetProperty(columnName, out result);

            return result;
        }


        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetProperty(string columnName, out object value)
        {
            string name = null;
            value = null;

            // Try to use cached results
            if (CacheResults)
            {
                // Handle case sensitivity
                if (CaseSensitive)
                {
                    name = columnName;
                }
                else
                {
                    name = columnName.ToLowerCSafe();
                }

                value = mCachedResults[name];
            }

            if (value == null)
            {
                // Transform the property
                value = Transformation.Invoke(ParentCollection, columnName);
            }

            // Save the result to cache
            if (CacheResults)
            {
                mCachedResults[name] = value;
            }

            return true;
        }

        #endregion


        #region "INameIndexable Members"

        /// <summary>
        /// Interface to access collections through string indexers by name
        /// </summary>
        object INameIndexable.this[string name]
        {
            get
            {
                return this[name];
            }
        }


        /// <summary>
        /// Interface to access collections through string indexers by name
        /// </summary>
        public ResultType this[string name]
        {
            get
            {
                return (ResultType)GetProperty(name);
            }
        }

        #endregion
    }
}