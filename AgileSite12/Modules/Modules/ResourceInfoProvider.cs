using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Core;

namespace CMS.Modules
{
    /// <summary>
    /// Provides access to information about resources.
    /// </summary>
    public class ResourceInfoProvider : AbstractInfoProvider<ResourceInfo, ResourceInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ResourceInfoProvider()
            : base(ResourceInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Variables/Constants"

        // List of low level system modules
        private static readonly HashSet<string> systemModules = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ModuleName.CMS, 
            ModuleName.CUSTOMSYSTEM
        };

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all resources.
        /// </summary>
        public static ObjectQuery<ResourceInfo> GetResources()
        {
            return ProviderObject.GetObjectQuery();
        }
        

        /// <summary>
        /// Returns the ResourceInfo structure for the specified resource.
        /// </summary>
        /// <param name="resourceId">Id of the resource to retrieve</param>
        public static ResourceInfo GetResourceInfo(int resourceId)
        {
            return ProviderObject.GetInfoById(resourceId);
        }


        /// <summary>
        /// Returns the ResourceInfo structure for the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource name to use for retrieving the resource data</param>
        public static ResourceInfo GetResourceInfo(string resourceName)
        {
            return ProviderObject.GetInfoByCodeName(resourceName);
        }


        /// <summary>
        /// Sets the specified resource data.
        /// </summary>
        /// <param name="resourceObj">Resource data object</param>
        public static void SetResourceInfo(ResourceInfo resourceObj)
        {
            ProviderObject.SetInfo(resourceObj);
        }


        /// <summary>
        /// Delete specified resource.
        /// </summary>
        /// <param name="resourceObj">Resource object</param>
        public static void DeleteResourceInfo(ResourceInfo resourceObj)
        {
            ProviderObject.DeleteInfo(resourceObj);
        }


        /// <summary>
        /// Delete specified resource.
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        public static void DeleteResourceInfo(int resourceId)
        {
            ResourceInfo resourceObj = GetResourceInfo(resourceId);
            DeleteResourceInfo(resourceObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets resources for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        public static ObjectQuery<ResourceInfo> GetResources(int siteId)
        {
            return ProviderObject.GetResourcesInternal(siteId);
        }


        /// <summary>
        /// Returns a value indicating whether the specified resource (module) is available.
        /// </summary>
        /// <remarks>
        /// A resource is available only if a database record exists and a module with the same is installed.
        /// A resource is also available if a negative identifier value is specified.
        /// </remarks>
        /// <param name="resourceId">A resource (module) identifier.</param>
        /// <returns>True, if the specified resource exists; otherwise, false.</returns>
        public static bool IsResourceAvailable(int resourceId)
        {
            if (resourceId <= 0)
            {
                return true;
            }

            var resource = GetResourceInfo(resourceId);
            if (resource == null)
            {
                return false;
            }

            return !resource.ResourceHasFiles || ModuleEntryManager.IsModuleLoaded(resource.ResourceName);
        }


        /// <summary>
        /// Returns true if resource(specified by resource name) is considered as system
        /// </summary>
        /// <param name="resourceName">Resource code name</param>
        public static bool IsSystemResource(string resourceName)
        {
            if (!String.IsNullOrEmpty(resourceName))
            {
                return systemModules.Contains(resourceName);
            }

            return false;
        }
        
        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ResourceInfo info)
        {
            // Check the unique name
            if ((info.ResourceID <= 0) && !CheckUniqueCodeName(info))
            {
                throw new Exception("[ResourceInfoProvider.SetResourceInfo]: Resource '" + info.ResourceName + "' already exists.");
            }

            // Set the object
            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ResourceInfo info)
        {
            // Delete the object
            base.DeleteInfo(info);

            // Clear site-resources hashtable
            ProviderHelper.ClearHashtables(ResourceSiteInfo.OBJECT_TYPE, false);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets resources for specified site based on the give parameters.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<ResourceInfo> GetResourcesInternal(int siteId)
        {
            return GetResources().OnSite(siteId);
        }
        
        #endregion
    }
}