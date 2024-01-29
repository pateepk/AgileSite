using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Modules
{
    /// <summary>
    /// Class providing ResourceLibraryInfo management.
    /// </summary>
    public class ResourceLibraryInfoProvider : AbstractInfoProvider<ResourceLibraryInfo, ResourceLibraryInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceLibraryInfoProvider()
            : base(ResourceLibraryInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ResourceLibraryInfo objects.
        /// </summary>
        public static ObjectQuery<ResourceLibraryInfo> GetResourceLibraries()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ResourceLibraryInfo with specified ID.
        /// </summary>
        /// <param name="id">ResourceLibraryInfo ID</param>
        public static ResourceLibraryInfo GetResourceLibraryInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ResourceLibraryInfo after verifying it is valid.
        /// </summary>
        /// <param name="infoObj">ResourceLibraryInfo to be set</param>
        public static void SetResourceLibraryInfo(ResourceLibraryInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ResourceLibraryInfo.
        /// </summary>
        /// <param name="infoObj">ResourceLibraryInfo to be deleted</param>
        public static void DeleteResourceLibraryInfo(ResourceLibraryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ResourceLibraryInfo with specified ID.
        /// </summary>
        /// <param name="id">ResourceLibraryInfo ID</param>
        public static void DeleteResourceLibraryInfo(int id)
        {
            ResourceLibraryInfo infoObj = GetResourceLibraryInfo(id);
            DeleteResourceLibraryInfo(infoObj);
        }


        /// <summary>
        /// Returns true when there is no <see cref="ResourceLibraryInfo"/> with given <paramref name="pathToLibrary"/> 
        /// referenced by <see cref="ResourceInfo"/> with <paramref name="resourceId"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="pathToLibrary"/> is internally normalized with <see cref="URLHelper.UnMapPath(string)"/> 
        /// method to the form of relative path.
        /// </remarks>
        /// <param name="resourceId"><see cref="ResourceInfo"/> id.</param>
        /// <param name="pathToLibrary">Relative path to library.</param>
        public static bool ValidateLibraryUniqueness(int resourceId, string pathToLibrary)
        {
            return ProviderObject.ValidateLibraryUniquenessInternal(resourceId, pathToLibrary);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Saves given info object to Database after verifying it is valid.
        /// </summary>
        /// <param name="info">Info object to be saved to DB</param>
        protected override void SetInfo(ResourceLibraryInfo info)
        {
            info.ResourceLibraryPath = NormalizeResourceLibraryPath(info.ResourceLibraryPath);
            if (!info.ResourceLibraryPath.StartsWith("~", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("ResourceLibrary path is in invalid format. Path has to be a physical path to a file within the application directory (must start with '~').");
            }

            // Do not add new library if given Library is already associated with given module, because CI restore caused exception to be thrown.
            if (!ValidateLibraryUniquenessInternal(info.ResourceLibraryResourceID, info.ResourceLibraryPath, info.ResourceLibraryID))
            {
                return;
            }

            var resourceInfo = ResourceInfoProvider.GetResourceInfo(info.ResourceLibraryResourceID);
            if ((resourceInfo != null) && String.Format("~\\bin\\{0}.dll", resourceInfo.ResourceName).ToLowerCSafe().EqualsCSafe(info.ResourceLibraryPath.ToLowerCSafe()))
            {
                throw new InvalidOperationException("Given Library is packed with module by default and cannot be added to the module.");
            }

            base.SetInfo(info);
        }


        /// <summary>
        /// Returns true when there is no <see cref="ResourceLibraryInfo"/> with different <paramref name="resourceLibraryId"/> with given <paramref name="pathToLibrary"/> 
        /// referenced by <see cref="ResourceInfo"/> with given <paramref name="resourceId"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="pathToLibrary"/> is internally normalized with <see cref="URLHelper.UnMapPath(string)"/> 
        /// method to the form of relative path.
        /// </remarks>
        /// <param name="resourceId"><see cref="ResourceInfo"/> id.</param>
        /// <param name="pathToLibrary">Relative path to library.</param>
        /// <param name="resourceLibraryId"><see cref="ResourceLibraryInfo"/> id.</param>
        protected virtual bool ValidateLibraryUniquenessInternal(int resourceId, string pathToLibrary, int resourceLibraryId = 0)
        {
            return !GetResourceLibraries().WhereEquals("ResourceLibraryResourceID", resourceId)
                .And().WhereEquals("ResourceLibraryPath", NormalizeResourceLibraryPath(pathToLibrary))
                .And().WhereNotEquals("ResourceLibraryID", resourceLibraryId).TopN(1).Any();
        }


        /// <summary>
        /// Normalizes path to relative form.
        /// </summary>
        /// <param name="path">Path to normalize.</param>
        private static string NormalizeResourceLibraryPath(string path)
        {
            return URLHelper.UnMapPath(path).Replace(@"/", @"\");
        }

        #endregion
    }
}