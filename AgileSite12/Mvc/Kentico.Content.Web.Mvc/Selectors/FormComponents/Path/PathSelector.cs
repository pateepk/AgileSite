using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;

using Kentico.Components.Web.Mvc.Dialogs;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Components.Web.Mvc.FormComponents.Internal;
using Kentico.Forms.Web.Mvc;


[assembly: RegisterFormComponent(PathSelector.IDENTIFIER, typeof(PathSelector), "{$kentico.components.pathselector.name$}", ViewName = "~/Views/Shared/Kentico/Selectors/FormComponents/_PathSelector.cshtml", IsAvailableInFormBuilderEditor = false)]

namespace Kentico.Components.Web.Mvc.FormComponents
{
    /// <summary>
    /// Represents a path selector form component.
    /// </summary>
    public class PathSelector : PageSelectorBase<PathSelectorProperties, PathSelectorItem>
    {
        /// <summary>
        /// Represents the <see cref="PathSelector"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.PathSelector";


        /// <summary>
        /// Gets the page specified by the <paramref name="pageIdentifier"/>.
        /// </summary>
        internal override TreeNode GetPage(PathSelectorItem pageIdentifier)
        {
            if (pageIdentifier == null)
            {
                return null;
            }

            return new PageSelectorPagesRepository().GetPage(
                pageIdentifier.NodeAliasPath,
                SiteContext.CurrentSiteName,
                CultureHelper.GetPreferredCulture()
            );
        }
    }
}
