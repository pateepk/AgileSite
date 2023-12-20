using System;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Base.Web.UI;
using CMS.Modules;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for retrieving icon model for dashboard tiles.
    /// </summary>
    internal class TileIconModelProvider : ITileIconModelProvider
    {
        /// <summary>
        /// Creates icon model for given <paramref name="uiElement"/>.
        /// </summary>
        /// <param name="uiElement">UI element the icon model is retrieved for</param>
        /// <exception cref="ArgumentNullException"><paramref name="uiElement"/> is null</exception>
        /// <returns>Icon model for given <paramref name="uiElement"/></returns>
        public TileIconModel CreateTileIconModel(UIElementInfo uiElement)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException("uiElement");
            }

            TileIconModel tileIcon = new TileIconModel();
            string iconImagePath = UIHelper.GetImageUrl(null, uiElement.ElementIconPath);

            if (iconImagePath != null)
            {
                tileIcon.IconType = TileIconTypeEnum.Image;
                tileIcon.IconImagePath = iconImagePath;
            }
            else
            {
                tileIcon.IconType = TileIconTypeEnum.CssClass;
                tileIcon.IconCssClass = string.IsNullOrEmpty(uiElement.ElementIconClass) ? "icon-app-default" : uiElement.ElementIconClass;
            }

            return tileIcon;
        }
    }
}
