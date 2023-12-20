using CMS.Core;
using CMS.Modules;

using CMS;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;

[assembly: RegisterImplementation(typeof(IUILinkProvider), typeof(UILinkProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.PortalEngine.Internal
{
    /// <summary>
    /// Provides methods for generating links to access single objects within the module. (e.g. single Site).
    /// </summary>
    public interface IUILinkProvider
    {
        /// <summary>
        /// Gets link to the single object if supported by the current state of UI elements.
        /// Can return link to the object creation if object identifier in <see cref="ObjectDetailLinkParameters"/> is left empty.
        /// Leaving out the <see cref="ObjectDetailLinkParameters"/> to null will return simple link for the object creation.
        /// </summary>
        /// <example>
        /// Following example shows how to obtain link to access single site.
        /// <code>
        /// var siteEditElement = UIElementInfoProvider.GetUIElementInfo("CMS", "EditSiteProperties");
        ///
        /// var retriever = <see cref="Service.Resolve{TService}()"/>;
        ///
        /// string link = retriever.GetSingleObjectLink(siteEditElement, new ObjectDetailLinkParemeters {
        ///     ObjectIdentifier = "siteName",
        ///     AllowNavigationToListing = true
        /// });
        /// </code>
        /// </example>
        /// <param name="detailElement">UI element displaying the object detail</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>Link to single object defined in <paramref name="detailElement"/> and <paramref name="parameters"/>.</returns>
        string GetSingleObjectLink(UIElementInfo detailElement, ObjectDetailLinkParameters parameters = null);


        /// <summary>
        /// Gets link to the single object if supported by the current state of UI elements.<br />
        /// Can return link to the object creation if object identifier in <see cref="ObjectDetailLinkParameters"/> is left empty.
        /// Leaving out the <see cref="ObjectDetailLinkParameters"/> to null will return simple link for the object creation.
        /// </summary>
        /// /// <example>
        /// Following example shows how to obtain link to access single site.
        /// <code>
        /// var retriever = <see cref="Service.Resolve{TService}()"/>;
        ///
        /// string link = retriever.GetSingleObjectLink("CMS", "EditSiteProperties" new ObjectDetailLinkParemeters {
        ///     ObjectIdentifier = "siteName",
        ///     AllowNavigationToListing = true
        /// });
        /// </code>
        /// </example>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementDetailName">Object detail element name</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>Link to single object defined in <paramref name="moduleName"/>, <paramref name="elementDetailName"/> and <paramref name="parameters"/>.</returns>
        string GetSingleObjectLink(string moduleName, string elementDetailName, ObjectDetailLinkParameters parameters = null);
    }
}