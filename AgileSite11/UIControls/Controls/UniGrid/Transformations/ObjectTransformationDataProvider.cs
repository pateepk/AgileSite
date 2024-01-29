using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.UIControls
{
    using DataContainerDictionary = IGeneralIndexable<int, IDataContainer>;
    using DataHandlerFunc = Func<string, IEnumerable<int>, IGeneralIndexable<int, IDataContainer>>;
    using ObjectDictionary = SafeDictionary<int, IDataContainer>;
    using ObjectIDDictionary = SafeDictionary<int, int>;
    using SpecificDataHandlerFunc = Func<IEnumerable<int>, IGeneralIndexable<int, IDataContainer>>;


    /// <summary>
    /// Provider of object data for <see cref="ObjectTransformation" />
    /// </summary>
    public class ObjectTransformationDataProvider
    {
        #region "Variables"

        /// <summary>
        /// Current object transformation data provider
        /// </summary>
        public static RequestStockValue<ObjectTransformationDataProvider> Current = new RequestStockValue<ObjectTransformationDataProvider>("CurrentObjectTransformationDataProvider", () => new ObjectTransformationDataProvider());


        /// <summary>
        /// List of requested objects
        /// </summary>
        protected StringSafeDictionary<ObjectIDDictionary> mRequestedObjects = new StringSafeDictionary<ObjectIDDictionary>();
        
        /// <summary>
        /// Found objects.
        /// </summary>
        protected StringSafeDictionary<DataContainerDictionary> mFoundObjects = new StringSafeDictionary<DataContainerDictionary>();

        /// <summary>
        /// Data handlers for the given object type
        /// </summary>
        protected StringSafeDictionary<SpecificDataHandlerFunc> mDataHandlers = new StringSafeDictionary<SpecificDataHandlerFunc>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Handler function that provides the data to the dictionary
        /// </summary>
        protected DataHandlerFunc DefaultDataHandler
        { 
            get; 
            set; 
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectTransformationDataProvider()
        {
            DefaultDataHandler = GetInfosByIds;
        }


        /// <summary>
        /// Marks the given object as required.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        public void RequireObject(string objectType, int objectId)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new Exception("[ObjectTransformation] - Object type is required.");
            }

            if (objectId <= 0)
            {
                return;
            }

            var allObjects = mRequestedObjects;

            // Ensure the particular objects table
            var objects = allObjects[objectType];
            if (objects == null)
            {
                objects = new ObjectIDDictionary();
                allObjects[objectType] = objects;
            }

            objects[objectId] += 1;
        }


        /// <summary>
        /// Sets the default data handler for all object types
        /// </summary>
        /// <param name="handler">Handler to set</param>
        public void SetDefaultDataHandler(DataHandlerFunc handler)
        {
            DefaultDataHandler = handler;
        }


        /// <summary>
        /// Sets the default data handler for the given object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="handler">Handler to set</param>
        public void SetDataHandlerForType(string objectType, SpecificDataHandlerFunc handler)
        {
            mDataHandlers[objectType] = handler;
        }


        /// <summary>
        /// Gets the given object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="directIfNotCached">If true, the method is allowed to get the objects one by one directly from provider if not found registered</param>
        public IDataContainer GetObject(string objectType, int objectId, bool directIfNotCached)
        {
            if (objectId <= 0)
            {
                return null;
            }

            var allObjects = mFoundObjects;

            // Ensure the particular objects table
            var objects = allObjects[objectType];
            if (objects == null)
            {
                var requestedObjects = mRequestedObjects[objectType];
                if (requestedObjects != null)
                {
                    objects = GetData(objectType, requestedObjects.TypedKeys);
                }
                else
                {
                    objects = new ObjectDictionary();
                }

                allObjects[objectType] = objects;
            }

            // Try to find object in loaded objects
            var result = objects[objectId];
            if ((result == null) && directIfNotCached)
            {
                // Get from the provider the regular way
                result = GetSingleObject(objectType, objectId);
                if (result != null)
                {
                    objects[objectId] = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets a single object by object type and object ID
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        protected virtual IDataContainer GetSingleObject(string objectType, int objectId)
        {
            return ProviderHelper.GetInfoById(objectType, objectId);
        }


        /// <summary>
        /// Gets the objects for the given object type from database
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">Object IDs to retrieve</param>
        protected virtual DataContainerDictionary GetData(string objectType, IEnumerable<int> ids)
        {
            var handler = mDataHandlers[objectType];
            if (handler != null)
            {
                return handler(ids);
            }

            return DefaultDataHandler(objectType, ids);
        }


        /// <summary>
        /// Gets the objects for the given object type from database
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">Object IDs to retrieve</param>
        protected virtual DataContainerDictionary GetInfosByIds(string objectType, IEnumerable<int> ids)
        {
            return ProviderHelper.GetInfosByIds(objectType, ids).AsIndexable<int, IDataContainer>();
        }


        /// <summary>
        /// Copies requested objects to the new provider
        /// </summary>
        /// <param name="newProvider">Object transformation data provider</param>
        internal void CopyRequestedObjectsTo(ObjectTransformationDataProvider newProvider)
        {
            newProvider.mRequestedObjects = (StringSafeDictionary<ObjectIDDictionary>)mRequestedObjects.Clone();
        }
        
        #endregion
    }
}
