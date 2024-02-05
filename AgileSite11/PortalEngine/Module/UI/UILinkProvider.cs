using System;
using System.Collections.Specialized;
using System.Web;

using CMS.Modules;
using CMS.PortalEngine.Internal;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Provides methods for generating links to access single objects within the module. (e.g. single Site).
    /// </summary>
    internal class UILinkProvider : IUILinkProvider
    {
        /// <summary>
        /// Gets link to the single object if supported by the current state of UI elements.
        /// Can return link to the object creation if object identifier in <see cref="ObjectDetailLinkParameters"/> is left empty.
        /// Leaving out the <see cref="ObjectDetailLinkParameters"/> will return simple link for the object creation.
        /// </summary>
        /// <param name="detailElement">UI element displaying the object detail</param>
        /// <param name="parameters">Additional parameters</param>
        /// <exception cref="ArgumentNullException"><paramref name="detailElement"/> is null</exception>
        /// <returns>Link to single object defined in <paramref name="detailElement"/> and <paramref name="parameters"/>.</returns>
        public string GetSingleObjectLink(UIElementInfo detailElement, ObjectDetailLinkParameters parameters = null)
        {
            if (detailElement == null)
            {
                throw new ArgumentNullException(nameof(detailElement));
            }

            var queryCollection = HttpUtility.ParseQueryString(string.Empty);
            queryCollection.Add("elementguid", detailElement.ElementGUID.ToString());
            
            if (parameters != null)
            {
                queryCollection = AddLinkParameters(queryCollection, parameters);
            }

            return ApplicationUrlHelper.GetApplicationUrl(detailElement.Application) + "&" + queryCollection;
        }


        private static NameValueCollection AddLinkParameters(NameValueCollection queryCollection, ObjectDetailLinkParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.ObjectIdentifier?.ToString()))
            {
                queryCollection.Add("objectid", parameters.ObjectIdentifier.ToString());
            }
            if (!string.IsNullOrEmpty(parameters.ParentObjectIdentifier?.ToString()))
            {
                queryCollection.Add("parentobjectid", parameters.ParentObjectIdentifier.ToString());
            }
            if (!string.IsNullOrEmpty(parameters.ParentTabName))
            {
                queryCollection.Add("parenttabname", parameters.ParentTabName);
            }
            if (parameters.AllowNavigationToListing)
            {
                queryCollection.Add("allownavigationtolisting", "1");
            }
            if (!string.IsNullOrEmpty(parameters.TabName))
            {
                queryCollection.Add("tabname", parameters.TabName);
            }
            queryCollection.Add(parameters.Persistent ? "persistent" : "ignorehash", "1");

            return queryCollection;
        }


        /// <summary>
        /// Gets link to the single object if supported by the current state of UI elements.<br />
        /// Can return link to the object creation if object identifier in <see cref="ObjectDetailLinkParameters"/> is left empty.
        /// Leaving out the <see cref="ObjectDetailLinkParameters"/> will return simple link for the object creation.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementDetailName">Object detail element name</param>
        /// <param name="parameters">Additional parameters</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="moduleName"/> is empty -or- <paramref name="elementDetailName"/> is empty -or- UI element with given <paramref name="moduleName"/> and <paramref name="elementDetailName"/> does not exist
        /// </exception>
        /// <returns>Link to single object defined in <paramref name="moduleName"/>, <paramref name="elementDetailName"/> and <paramref name="parameters"/>.</returns>
        public string GetSingleObjectLink(string moduleName, string elementDetailName, ObjectDetailLinkParameters parameters = null)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(moduleName));
            }

            if (string.IsNullOrEmpty(elementDetailName))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(elementDetailName));
            }

            UIElementInfo detailElement = UIElementInfoProvider.GetUIElementInfo(moduleName, elementDetailName);
            if (detailElement == null)
            {
                throw new ArgumentException($"Element with given name '{elementDetailName}' does not exist under specified '{moduleName}' module.");
            }

            return GetSingleObjectLink(detailElement, parameters);
        }
    }
}