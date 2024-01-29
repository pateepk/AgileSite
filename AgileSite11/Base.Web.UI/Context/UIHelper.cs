using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// UI helper methods.
    /// </summary>
    public class UIHelper
    {
        #region "Variables

        /// <summary>
        /// If true, progress script is allowed on page.
        /// </summary>
        private static bool? mAllowUpdateProgress;

        /// <summary>
        /// If true, dialogs are being opened in new windows.
        /// </summary>
        private static bool? mClassicDialogs;


        /// <summary>
        /// UniGrid icons
        /// </summary>
        public const string UNIGRID_ICONS = "/ug/";


        /// <summary>
        /// Tree icons
        /// </summary>
        public const string TREE_ICONS = "/t/";


        /// <summary>
        /// Tree icons for RTL layout
        /// </summary>
        public const string TREE_ICONS_RTL = "/rt/";


        /// <summary>
        /// Small flag icons
        /// </summary>
        public const string FLAG_ICONS = "/f/";


        /// <summary>
        /// Flag icons for size 48x48
        /// </summary>
        public const string FLAG_ICONS_48 = "/f48/";

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, progress script is allowed on page.
        /// </summary>
        public static bool AllowUpdateProgress
        {
            get
            {
                if (mAllowUpdateProgress == null)
                {
                    mAllowUpdateProgress = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowProgressScript"], true);
                }

                return mAllowUpdateProgress.Value && ValidationHelper.GetBoolean(AbstractStockHelper<RequestStockHelper>.GetItem("CMSAllowProgressScript"), true);
            }
            set
            {
                AbstractStockHelper<RequestStockHelper>.Add("CMSAllowProgressScript", value);
            }
        }


        /// <summary>
        /// Indicates whether classic or new (iframe) dialogs should be used throughout UI.
        /// </summary>
        public static bool ClassicDialogs
        {
            get
            {
                if (mClassicDialogs == null)
                {
                    mClassicDialogs = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSClassicDialogs"], false);
                }

                return mClassicDialogs.Value;
            }
        }

        #endregion


        #region "Image url methods"

        /// <summary>
        /// Gets accessible-friendly font icon and screen reader representation.
        /// </summary>
        /// <param name="className">Font icon class name</param>
        /// <param name="tooltip">Icon description (Only if not decorative image)</param>
        /// <param name="size">Icon size</param>
        /// <param name="additionalClass">Additional CSS class</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public static string GetAccessibleIconTag(string className, string tooltip = null, FontIconSizeEnum size = FontIconSizeEnum.NotDefined, string additionalClass = null, string additionalAttributes = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<i aria-hidden=\"true\" class=\"" + className);
            if (size != FontIconSizeEnum.NotDefined)
            {
                sb.Append(" " + size.ToStringRepresentation());
            }

            if (!String.IsNullOrEmpty(additionalClass))
            {
                sb.Append(" " + additionalClass);
            }

            sb.Append("\"");
            bool hasTooltip = !string.IsNullOrEmpty(tooltip);
            if (hasTooltip)
            {
                tooltip = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(tooltip));
                sb.Append(" title=\"" + tooltip + "\"");
            }
            if (!String.IsNullOrEmpty(additionalAttributes))
            {
                sb.Append(" " + additionalAttributes);
            }
            sb.Append("></i>");

            if (hasTooltip)
            {
                sb.Append("<span class=\"sr-only\">" + tooltip + "</span>");
            }

            return sb.ToString();
        }
        

        /// <summary>
        /// Gets the 
        /// </summary>
        /// <param name="location">Location of icon (possible values: /ug/, /t/, /rt/, /f/, /f48/)</param>
        /// <param name="fileName">Name of file</param>
        public static string GetShortImageUrl(string location, string fileName)
        {
            return UrlResolver.ResolveUrl("~/a.aspx?cmsimg=" + location + fileName);
        }


        /// <summary>
        /// Gets UI image path.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetImageUrl(Page page, string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            string path = GetImagePath(page, imagePath, isLiveSite, ensureDefaultTheme);

            return ResolveImageUrl(path);
        }


        /// <summary>
        /// Resolves the path to an image
        /// </summary>
        /// <param name="path">Image path</param>
        public static string ResolveImageUrl(string path)
        {
            path = StorageHelper.GetImageUrl(path);

            return UrlResolver.ResolveUrl(path);
        }


        /// <summary>
        /// Gets UI image path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetImageUrl(string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            string path = AdministrationUrlHelper.GetImagePath(URLHelper.CustomTheme, imagePath, isLiveSite, ensureDefaultTheme);

            return ResolveImageUrl(path);
        }


        /// <summary>
        /// Redirects the file to the images folder.
        /// </summary>
        public static bool ShortImageRedirect()
        {
            string cmsimg = QueryHelper.GetString("cmsimg", null);
            if ((cmsimg != null) && cmsimg.StartsWithCSafe("/"))
            {
                if (cmsimg.StartsWithCSafe(UNIGRID_ICONS))
                {
                    // Unigrid actions
                    cmsimg = "Design/Controls/UniGrid/Actions" + cmsimg.Substring(3);
                }
                else if (cmsimg.StartsWithCSafe(TREE_ICONS))
                {
                    // Tree icons
                    cmsimg = "Design/Controls/Tree" + cmsimg.Substring(2);
                }
                else if (cmsimg.StartsWithCSafe(TREE_ICONS_RTL))
                {
                    // Tree icons RTL
                    cmsimg = "RTL/Design/Controls/Tree" + cmsimg.Substring(3);
                }
                else if (cmsimg.StartsWithCSafe(FLAG_ICONS))
                {
                    // Flag icons
                    cmsimg = "Flags/16x16" + cmsimg.Substring(2);
                }
                else if (cmsimg.StartsWithCSafe(FLAG_ICONS_48))
                {
                    // Large flag icons
                    cmsimg = "Flags/48x48" + cmsimg.Substring(4);
                }

                // Redirect to the correct location
                URLHelper.RedirectPermanentInternal(GetImageUrl(cmsimg));

                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets UI image path.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetImagePath(Page page, string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            var customTheme = GetCustomTheme(page);
            return AdministrationUrlHelper.GetImagePath(customTheme, imagePath, isLiveSite, ensureDefaultTheme);
        }


        /// <summary>
        /// Returns resolved path to the icon image for the specified document type.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="className">Name of the class</param>
        /// <param name="checkFile">Indicates if the required icon exists in the filesystem</param>
        public static string GetDocumentTypeIconUrl(Page page, string className, bool checkFile)
        {
            return GetDocumentTypeIconUrl(page, className, "", checkFile);
        }


        /// <summary>
        /// Returns resolved path to the icon image for the specified document type.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="className">Name of the class</param>
        /// <param name="iconSet">Name of the subfolder where icon images are located</param>
        /// <param name="checkFile">Indicates if the required icon exists in the filesystem</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetDocumentTypeIconUrl(Page page, string className, string iconSet = null, bool checkFile = true, bool ensureDefaultTheme = true)
        {
            string path = GetDocumentTypeIconPath(page, className, iconSet, checkFile, ensureDefaultTheme);

            return ResolveImageUrl(path);
        }


        /// <summary>
        /// Returns HTML markup representing document type icon.
        /// </summary>
        /// <param name="page">Page.</param>
        /// <param name="className">Class name.</param>
        /// <param name="iconClass">CSS class for font icon.</param>
        /// <param name="iconSize">Icon size for font icon.</param>
        /// <param name="iconSet">Icon set for image icon.</param>
        /// <param name="tooltip">Tooltip.</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public static string GetDocumentTypeIcon(Page page, string className, string iconClass = "", FontIconSizeEnum iconSize = FontIconSizeEnum.NotDefined, string iconSet = "", string tooltip = "", string additionalAttributes = "")
        {
            var path = GetDocumentTypeIconPath(page, className, iconSet);
            return GetAccessibleImageMarkup(page, iconClass, path, tooltip, iconSize, defaultIconClass: "icon-doc-o", additionalClass: "icon-doctype", additionalAttributes: additionalAttributes);
        }


        /// <summary>
        /// Returns path to the icon image for the specified document type.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="className">Name of the class</param>
        /// <param name="iconSet">Name of the subfolder where icon images are located</param>
        /// <param name="checkFile">Indicates if the required icon exists in the file system</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetDocumentTypeIconPath(Page page, string className, string iconSet = null, bool checkFile = true, bool ensureDefaultTheme = true)
        {
            string customTheme = GetCustomTheme(page);
            return AdministrationUrlHelper.GetDocumentTypeIconPath(customTheme, className, iconSet, checkFile, ensureDefaultTheme);
        }


        /// <summary>
        /// Returns HTML markup representing document type icon.
        /// </summary>
        /// <param name="page">Page.</param>
        /// <param name="extension">Class name.</param>
        /// <param name="iconSize">Icon size for font icon.</param>
        /// <param name="tooltip">Tooltip.</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public static string GetFileIcon(Page page, string extension, FontIconSizeEnum iconSize = FontIconSizeEnum.NotDefined, string tooltip = "", string additionalAttributes = "")
        {
            return GetAccessibleImageMarkup(page, GetFileIconClass(extension), null, tooltip, iconSize, additionalAttributes: additionalAttributes);
        }


        /// <summary>
        /// Returns font icon class for specified file extension
        /// </summary>
        /// <param name="extension">File extension</param>
        public static string GetFileIconClass(string extension)
        {
            extension = !String.IsNullOrEmpty(extension) ? ImageHelper.UnifyFileExtension(extension.ToLowerCSafe()) : "default";
            return "icon-file-" + extension + " icon-file-default";
        }


        /// <summary>
        /// Returns resolved path to the flag image for the specified culture.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="iconSet">Name of the subfolder where icon images are located</param>
        public static string GetFlagIconUrl(Page page, string cultureCode, string iconSet)
        {
            if (iconSet == null)
            {
                iconSet = "16x16";
            }

            // Get images directory and extension
            string imageDirectory = "Flags/" + iconSet;

            // Get image full path
            string imagePath = GetImagePath(page, imageDirectory.TrimEnd('/') + "/" + cultureCode + ".png");

            // File is not physically present, use default
            if (!File.Exists(URLHelper.GetPhysicalPath(imagePath)))
            {
                // Use default icon
                imagePath = GetImagePath(page, imageDirectory.TrimEnd('/') + "/" + "default.png");
            }

            return ResolveImageUrl(imagePath);
        }

        #endregion


        /// <summary>
        /// Gets tooltip body HTML.
        /// </summary>
        /// <param name="imageUrl">Image URL</param>
        /// <param name="imageWidth">Actual width of the image</param>
        /// <param name="imageHeight">Actual height of the image</param>
        /// <param name="title">File title</param>
        /// <param name="name">File name</param>
        /// <param name="description">File description</param>
        /// <param name="divInlineStyle">Inline style (e.g. 'width: 100px;')</param>
        /// <param name="tooltipWidth">Maximal width of the tooltip</param>
        public static string GetTooltip(string imageUrl, int imageWidth, int imageHeight, string title, string name, string description, string divInlineStyle, ref int tooltipWidth)
        {
            // Ensure default tooltip width
            if (tooltipWidth <= 0)
            {
                tooltipWidth = 200;
            }

            // If title is null or empty, set name
            if (String.IsNullOrEmpty(title))
            {
                title = name;
            }

            // Trim HTML tags
            title = HTMLHelper.StripTags(title);
            description = HTMLHelper.StripTags(description);

            // Ensure length
            title = TextHelper.LimitLength(title, 90);
            description = TextHelper.LimitLength(description, 200);

            // Encode text
            title = ScriptHelper.FormatTooltipString(title, false);
            description = ScriptHelper.FormatTooltipString(description, false);

            if (!String.IsNullOrEmpty(title))
            {
                title = HTMLHelper.EncodeForHtmlAttribute(title);
            }
            if (!String.IsNullOrEmpty(description))
            {
                description = HTMLHelper.EncodeForHtmlAttribute(description);
            }

            bool hasContent = false;
            StringBuilder sb = new StringBuilder();

            // Start div
            if (!String.IsNullOrEmpty(divInlineStyle))
            {
                sb.Append("<div style=\\'");
                sb.Append(divInlineStyle);
                sb.Append("\\'>");
            }

            // Add image tag
            if (!String.IsNullOrEmpty(imageUrl))
            {
                hasContent = true;
                sb.Append("<img");

                imageUrl = URLHelper.RemoveParameterFromUrl(imageUrl, "maxsidesize");
                if ((imageHeight > 0) && (imageWidth > 0))
                {
                    int[] dimensions = ImageHelper.EnsureImageDimensions(0, 0, tooltipWidth, imageWidth, imageHeight);
                    tooltipWidth = dimensions[0];
                    imageWidth = dimensions[0];
                    imageHeight = dimensions[1];
                    imageUrl = URLHelper.AddParameterToUrl(imageUrl, "width", imageWidth.ToString());
                    imageUrl = URLHelper.AddParameterToUrl(imageUrl, "height", imageHeight.ToString());
                }
                else
                {
                    imageUrl = URLHelper.AddParameterToUrl(imageUrl, "maxsidesize", tooltipWidth.ToString());
                }
                sb.Append(" class=\\'TooltipImage\\' style=\\'", MediaHelper.GetImageStyleAtt(imageWidth, imageHeight, null, 0, null, -1, -1, null), "\\'");
                // Escape apostrofs and qoutes from url for javascript integrity
                sb.Append(" src=\\'", UrlResolver.ResolveUrl(imageUrl).Replace("\'", "%27").Replace("\"", "%22"), "\\'");
                sb.Append(" alt=\\'", title, "\\' />");
            }
            else
            {
                // Ensure default width for tooltip without image
                tooltipWidth = 300;
            }

            if (!String.IsNullOrEmpty(title))
            {
                hasContent = true;
                // Add div tag for title
                sb.Append("<div class=\\'TooltipTitle\\'>");
                sb.Append(title);
                sb.Append("</div>");
            }

            if (!String.IsNullOrEmpty(description))
            {
                hasContent = true;
                // Add div tag for decription
                sb.Append("<div class=\\'TooltipDescription\\'>");
                sb.Append(description);
                sb.Append("</div>");
            }

            // End div
            if (!String.IsNullOrEmpty(divInlineStyle))
            {
                sb.Append("</div>");
            }

            return hasContent ? sb.ToString() : null;
        }


        /// <summary>
        /// Gets tooltip onmouseover and onmouseout attributes.
        /// </summary>
        /// <param name="imageUrl">Image URL</param>
        /// <param name="imageWidth">Actual width of the image</param>
        /// <param name="imageHeight">Actual height of the image</param>
        /// <param name="title">File title</param>
        /// <param name="name">File name</param>
        /// <param name="extension">File extension</param>
        /// <param name="description">File description</param>
        /// <param name="divInlineStyle">Inline style (e.g. 'width: 100px;')</param>
        /// <param name="tooltipWidth">Maximal width of the tooltip</param>
        /// <returns>Returns string in format 'onmouseout=\"UnTip()\" onmouseover=\"Tip('##TOOLTIP_BODY##', WIDTH, -##TOOLTIP_WIDTH##)\"'</returns>
        public static string GetTooltipAttributes(string imageUrl, int imageWidth, int imageHeight, string title, string name, string extension, string description, string divInlineStyle, int tooltipWidth)
        {
            string tooltip = null;
            // Force not to display the image for non-image file
            bool isImage = ImageHelper.IsImage(extension);
            string tooltipUrl = isImage ? imageUrl : null;
            int originalWidth = tooltipWidth;

            string tooltipBody = GetTooltip(tooltipUrl, imageWidth, imageHeight, title, name, description, null, ref tooltipWidth);
            if (!String.IsNullOrEmpty(tooltipBody))
            {
                tooltip = "onmouseout=\"UnTip()\" onmouseover=\"Tip('" + HTMLHelper.EncodeForHtmlAttribute(tooltipBody) + "', WIDTH, -" + originalWidth + ")\"";
            }
            return tooltip;
        }


        /// <summary>
        /// Ensures tooltip for given control.
        /// </summary>
        /// <param name="ctrl">Control to attach the tooltip</param>
        /// <param name="imageUrl">Image URL</param>
        /// <param name="imageWidth">Actual width of the image</param>
        /// <param name="imageHeight">Actual height of the image</param>
        /// <param name="title">File title</param>
        /// <param name="name">File name</param>
        /// <param name="extension">File extension</param>
        /// <param name="description">File description</param>
        /// <param name="divInlineStyle">Inline style (e.g. 'width: 100px;')</param>
        /// <param name="tooltipWidth">Maximal width of the tooltip</param>
        public static void EnsureTooltip(WebControl ctrl, string imageUrl, int imageWidth, int imageHeight, string title, string name, string extension, string description, string divInlineStyle, int tooltipWidth)
        {
            // Force not to display the image for non-image file
            bool isImage = ImageHelper.IsImage(extension);

            // Prepare the URL
            string tooltipUrl = isImage ? imageUrl : null;
            if (isImage)
            {
                tooltipUrl = StorageHelper.GetImageUrl(tooltipUrl);
            }

            int originalWidth = tooltipWidth;

            // Ensure tooltip
            string tooltip = GetTooltip(tooltipUrl, imageWidth, imageHeight, title, name, description, null, ref tooltipWidth);
            if (!String.IsNullOrEmpty(tooltip))
            {
                ctrl.Attributes.Add("onmouseover", "Tip('" + tooltip + "', WIDTH, -" + originalWidth + ");");
                ctrl.Attributes.Add("onmouseout", "UnTip();");
            }
        }


        /// <summary>
        /// Gets accessible-friendly image tag.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="path">Path to the image file</param>
        /// <param name="tooltip">Image description (Only if not decorative image)</param>
        /// <param name="size">Image size</param>
        /// <param name="additionalClass">Additional CSS class</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public static string GetAccessibleImageTag(Page page, string path, string tooltip = null, FontIconSizeEnum size = FontIconSizeEnum.NotDefined, string additionalClass = null, string additionalAttributes = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<img alt=\"" + tooltip + "\"");

            string cssClass = null;
            if (size != FontIconSizeEnum.NotDefined)
            {
                cssClass = size.ToStringRepresentation();
            }

            if (!String.IsNullOrEmpty(additionalClass))
            {
                cssClass += " " + additionalClass;
            }

            if (!String.IsNullOrEmpty(cssClass))
            {
                sb.Append(" class=\"" + cssClass, "\"");
            }

            if (!String.IsNullOrEmpty(additionalAttributes))
            {
                sb.Append(" " + additionalAttributes);
            }

            sb.Append(" src=\"" + GetImageUrl(page, path) + "\" />");
            return sb.ToString();
        }


        /// <summary>
        /// Returns accessible-friendly image markup based on image URL or font icon class.
        /// </summary>
        /// <param name="page">Page class</param> 
        /// <param name="iconClass">Icon class</param>
        /// <param name="imagePath">Image URL</param>
        /// <param name="tooltip">Description (Only if not decorative image)</param>
        /// <param name="size">Size to be used</param>
        /// <param name="iconColorClass">Icon color class</param>
        /// <param name="defaultIconClass">Default icon class.</param>
        /// <param name="additionalClass">Additional CSS class.</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public static string GetAccessibleImageMarkup(Page page, string iconClass, string imagePath, string tooltip = null, FontIconSizeEnum size = FontIconSizeEnum.NotDefined, string iconColorClass = null, string defaultIconClass = null, string additionalClass = null, string additionalAttributes = null)
        {
            // Then font icon has the highest priority
            if (!String.IsNullOrEmpty(iconClass))
            {
                iconClass = String.IsNullOrEmpty(additionalClass) ? iconClass : iconClass + " " + additionalClass;
                return GetAccessibleIconTag(iconClass, tooltip, size, iconColorClass, additionalAttributes);
            }

            // Image is next if it exists
            try
            {
                imagePath = GetImagePath(page, imagePath);
                if (!String.IsNullOrEmpty(imagePath) && FileHelper.FileExists(imagePath))
                {
                    return GetAccessibleImageTag(page, imagePath, tooltip, size, additionalClass, additionalAttributes);
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("UIHelper", "INVALIDIMAGEPATH", new ArgumentException(String.Format("Invalid image path: '{0}'", imagePath), ex));
                return null;
            }

            // Then default font icon
            if (!String.IsNullOrEmpty(defaultIconClass))
            {
                defaultIconClass = String.IsNullOrEmpty(additionalClass) ? defaultIconClass : defaultIconClass + " " + additionalClass;
                return GetAccessibleIconTag(defaultIconClass, tooltip, size, iconColorClass, additionalAttributes);
            }

            return null;
        }


        /// <summary>
        /// Sets breadcrumbs suffix
        /// </summary>
        public static void SetBreadcrumbsSuffix(string text)
        {
            RequestContext.ClientApplication.Add("breadcrumbsSuffix", text);
        }

        
        /// <summary>
        /// Indicates if friends functionality is enabled.
        /// </summary>
        /// <param name="siteName">Code name of the site to check</param>
        public static bool IsFriendsModuleEnabled(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSEnableFriends"].ToBoolean(false);
        }


        /// <summary>
        /// Returns Label control that displays font icon specified by ccs class
        /// </summary>
        /// <param name="cssClass">Font icon css class</param>
        /// <param name="toolTip">Icon tooltip</param>
        public static Label GetIcon(string cssClass, string toolTip)
        {
            Label iconWrapper = new Label
            {
                CssClass = "info-icon"
            };

            toolTip = ScriptHelper.FormatTooltipString(toolTip, false, false);

            CMSIcon helpIcon = new CMSIcon
            {
                CssClass = cssClass,
                ToolTip = toolTip
            };

            // Enable HTML formating in tooltip
            helpIcon.Attributes.Add("data-html", "true");

            iconWrapper.Controls.Add(helpIcon);

            return iconWrapper;
        }

        #region "Private methods"

        private static string GetCustomTheme(Page page)
        {
            return ((page != null) && (page.Theme != null)) ? page.Theme : URLHelper.CustomTheme;
        }

        #endregion
    }
}
