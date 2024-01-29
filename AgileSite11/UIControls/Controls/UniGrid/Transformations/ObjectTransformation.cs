using System;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Object transformation control. 
    /// Transforms an object retrieved by ObjectID using specified transformation.
    /// Multiple object transformations on a single page are optimized to retrieve all used objects with a single query for better performance.
    /// </summary>
    public class ObjectTransformation : ObjectTransformationBase
    {
        #region "Variables"

        private int mObjectId;
        private ObjectTransformationDataProvider mDataProvider;

        #endregion


        #region "Properties"

        /// <summary>
        /// Object ID.
        /// </summary>
        public int ObjectID
        {
            get
            {
                return mObjectId;
            }
            set
            {
                mObjectId = value;

                RegisterObject();
            }
        }


        /// <summary>
        /// Data provider for this transformation
        /// </summary>
        public ObjectTransformationDataProvider DataProvider
        {
            get
            {
                return mDataProvider;
            }
            set
            {
                var newProvider = value;
                if (newProvider != null)
                {
                    // Copy requested objects to the new provider
                    Provider.CopyRequestedObjectsTo(newProvider);
                }

                mDataProvider = newProvider;
            }
        }


        /// <summary>
        /// DataProvider provider or default provider if DataProvider is not set.
        /// </summary>
        private ObjectTransformationDataProvider Provider
        {
            get
            {
                return DataProvider ?? ObjectTransformationDataProvider.Current;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectTransformation()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        public ObjectTransformation(string objectType, int objectId)
        {
            ObjectType = objectType;
            ObjectID = objectId;
        }


        /// <summary>
        /// Gets the default parameter for the transformation
        /// </summary>
        protected override object GetDefaultParameter()
        {
            return ObjectID;
        }


        /// <summary>
        /// Register required object.
        /// </summary>
        protected override void RegisterObject()
        {
            if (String.IsNullOrEmpty(ObjectType) || (ObjectID <= 0) || ObjectRequested)
            {
                return;
            }

            RequireObject(ObjectType, ObjectID);
        }


        /// <summary>
        /// Marks the given object as required.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        protected void RequireObject(string objectType, int objectId)
        {
            Provider.RequireObject(objectType, objectId);

            ObjectRequested = true;
        }


        /// <summary>
        /// Gets the given object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="directIfNotCached">If true, the method is allowed to get the objects one by one directly from provider if not found registered</param>
        protected IDataContainer GetObject(string objectType, int objectId, bool directIfNotCached)
        {
            try
            {
                using (new CMSActionContext { AllowLicenseRedirect = false })
                {
                    return Provider.GetObject(objectType, objectId, directIfNotCached);
                }
            }
            catch (LicenseException ex)
            {
                if (UseEmptyInfoForObjectLimitedByLicense)
                {
                    return ModuleManager.GetObject(objectType);
                }
                EventLogProvider.LogException("Object transformation", "GETOBJECT", ex);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Object transformation", "GETOBJECT", ex);
            }

            return null;
        }
        

        /// <summary>
        /// Gets the object used by the transformation
        /// </summary>
        protected override IDataContainer GetObject()
        {
            return GetObject(ObjectType, ObjectID, DirectIfNotCached || !ObjectRequested);
        }
        
        #endregion
    }
}
