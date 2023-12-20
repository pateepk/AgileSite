using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;

using Kentico.Components.Web.Mvc.Dialogs;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Components.Web.Mvc.FormComponents.Internal;
using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(PageSelector.IDENTIFIER, typeof(PageSelector), "{$kentico.components.pageselector.name$}", ViewName = "~/Views/Shared/Kentico/Selectors/FormComponents/_PageSelector.cshtml", IsAvailableInFormBuilderEditor = false)]

namespace Kentico.Components.Web.Mvc.FormComponents
{
    /// <summary>
    /// Represents a page selector form component.
    /// </summary>
    public class PageSelector : PageSelectorBase<PageSelectorProperties, PageSelectorItem>
    {
        /// <summary>
        /// Represents the <see cref="PageSelector"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.PageSelector";


        /// <summary>
        /// Gets the page specified by the <paramref name="pageIdentifier"/>.
        /// </summary>
        internal override TreeNode GetPage(PageSelectorItem pageIdentifier)
        {
            if (pageIdentifier == null)
            {
                return null;
            }

            return new PageSelectorPagesRepository().GetPage(
                pageIdentifier.NodeGuid,
                SiteContext.CurrentSiteName,
                CultureHelper.GetPreferredCulture()
            );
        }
    }
}
