using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.Modules;
using CMS.SiteProvider;

[assembly: RegisterImplementation(typeof(IUIElementObjectPropertiesProvider), typeof(UIElementObjectPropertiesProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for obtaining properties of related info object contained within the <see cref="UIElementInfo"/>.
    /// </summary>
    internal interface IUIElementObjectPropertiesProvider
    {
        /// <summary>
        /// Returns display name of the object matching given <paramref name="codeName"/> on <paramref name="siteID"/> for the <paramref name="uiElement"/>.
        /// </summary>
        /// <param name="codeName">Code name identifying the desired info object</param>
        /// <param name="siteID">ID of the <see cref="SiteInfo"/> the desired object is assigned to</param>
        /// <param name="uiElement">UI element containing desired object</param>
        /// <returns>Display name of found object; if not found, returns null.</returns>
        string GetDisplayName(string codeName, int siteID, UIElementInfo uiElement);


        /// <summary>
        /// Returns code name of the object identified by given <paramref name="objectID"/> contained within the <paramref name="uiElement"/>.
        /// </summary>
        /// <param name="objectID">ID of the identifying the desired info object</param>
        /// <param name="uiElement">UI element containing desired object</param>
        /// <returns>Code name of found object; if not found, returns null.</returns>
        string GetCodeName(int objectID, UIElementInfo uiElement);


        /// <summary>
        /// Returns object type of the object containing within the given <paramref name="uiElement"/>
        /// </summary>
        /// <param name="uiElement">UI element containing desired object type</param>
        /// <returns>Type of the found object; if not found, returns null</returns>
        string GetObjectTypeFromUIElement(UIElementInfo uiElement);
    }
}