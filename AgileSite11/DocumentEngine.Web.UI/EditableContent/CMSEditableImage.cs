using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Base.Web.UI;
using CMS.PortalEngine.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Editable image control.
    /// </summary>
    public class CMSEditableImage : CMSAbstractEditableControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// Image.
        /// </summary>
        protected Image imgImage = null;

        /// <summary>
        /// Region title.
        /// </summary>
        protected Label lblTitle = null;

        /// <summary>
        /// Error label.
        /// </summary>
        protected Label lblError = null;

        /// <summary>
        /// Region panel.
        /// </summary>
        protected Panel pnlEditor = null;

        /// <summary>
        /// Image selector.
        /// </summary>
        protected ImageSelector selPath = null;

        /// <summary>
        /// Image title.
        /// </summary>
        protected string mImageTitle = null;

        /// <summary>
        /// Alternate text.
        /// </summary>
        protected string mAlternateText = "";

        /// <summary>
        /// Display image selector text box.
        /// </summary>
        protected bool mDisplaySelectorTextBox = true;

        /// <summary>
        /// Image class.
        /// </summary>
        protected string mImageCssClass = "";

        /// <summary>
        /// Image style.
        /// </summary>
        protected string mImageStyle = "";


        private const int NOT_KNOWN = -1;
        private int mResizeToWidth = NOT_KNOWN;
        private int mResizeToHeight = NOT_KNOWN;
        private int mResizeToMaxSideSize = NOT_KNOWN;
        private bool mIsLiveSite = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Control title which is displayed in the editable mode.
        /// </summary>
        public string ImageTitle
        {
            get
            {
                return ValidationHelper.GetString(mImageTitle, ID);
            }
            set
            {
                mImageTitle = value;
            }
        }


        /// <summary>
        /// Image width.
        /// </summary>
        public int ImageWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public int ImageHeight
        {
            get;
            set;
        }


        /// <summary>
        /// Alternative image text.
        /// </summary>
        public string AlternateText
        {
            get
            {
                return mAlternateText;
            }
            set
            {
                mAlternateText = value;
            }
        }


        /// <summary>
        /// Display selector text box.
        /// </summary>
        public bool DisplaySelectorTextBox
        {
            get
            {
                return mDisplaySelectorTextBox;
            }
            set
            {
                mDisplaySelectorTextBox = value;
                SetTextBoxVisibility();
            }
        }


        /// <summary>
        /// Image CssClass.
        /// </summary>
        public string ImageCssClass
        {
            get
            {
                return mImageCssClass;
            }
            set
            {
                mImageCssClass = value;
            }
        }


        /// <summary>
        /// Image Style.
        /// </summary>
        public string ImageStyle
        {
            get
            {
                return mImageStyle;
            }
            set
            {
                mImageStyle = value;
            }
        }


        /// <summary>
        /// Width the image should be automatically resized to after it is uploaded.
        /// </summary>
        public int ResizeToWidth
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mResizeToWidth;
                }

                if (mResizeToWidth == NOT_KNOWN)
                {
                    mResizeToWidth = ImageHelper.GetAutoResizeToWidth(SiteContext.CurrentSiteName);
                }
                return mResizeToWidth;
            }
            set
            {
                mResizeToWidth = value;
            }
        }


        /// <summary>
        /// Height the image should be automatically resized to after it is uploaded.
        /// </summary>
        public int ResizeToHeight
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mResizeToHeight;
                }

                if (mResizeToHeight == NOT_KNOWN)
                {
                    mResizeToHeight = ImageHelper.GetAutoResizeToHeight(SiteContext.CurrentSiteName);
                }
                return mResizeToHeight;
            }
            set
            {
                mResizeToHeight = value;
            }
        }


        /// <summary>
        /// Max side size the image should be automatically resized to after it is uploaded.
        /// </summary>
        public int ResizeToMaxSideSize
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mResizeToMaxSideSize;
                }

                if (mResizeToMaxSideSize == NOT_KNOWN)
                {
                    mResizeToMaxSideSize = ImageHelper.GetAutoResizeToMaxSideSize(SiteContext.CurrentSiteName);
                }
                return mResizeToMaxSideSize;
            }
            set
            {
                mResizeToMaxSideSize = value;
            }
        }


        /// <summary>
        /// Returns instance of image control.
        /// </summary>
        public Image ImageControl
        {
            get
            {
                return imgImage;
            }
        }


        /// <summary>
        /// Indicates if control is used in live site mode.
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return mIsLiveSite;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Edit page URL which will be used for editing of the editable control. Used in On-site editing.
        /// </summary>
        protected override string EditPageUrl
        {
            get
            {
                return URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/OnSiteEdit/EditImage.aspx");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, initializes the parent portal manager.
        /// </summary>
        /// <param name="pageManager">Parent page manager</param>
        public CMSEditableImage(IPageManager pageManager)
            :this()
        {
            PageManager = pageManager;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSEditableImage()
        {
        }


        private void SetTextBoxVisibility()
        {
            if (selPath != null)
            {
                if (DisplaySelectorTextBox)
                {
                    selPath.ImagePathTextBox.Attributes.Remove("style");
                }
                else
                {
                    selPath.ImagePathTextBox.Attributes.Add("style", "display: none;");
                }

                selPath.ShowTextBox = DisplaySelectorTextBox;
            }
        }


        /// <summary>
        /// Creates the child control hierarchy.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            base.CreateChildControls();

            // Create controls by actual page mode
            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    // Main editor panel
                    pnlEditor = new Panel();
                    pnlEditor.ID = "pnlEditor";
                    pnlEditor.CssClass = "EditableImageEdit";
                    pnlEditor.Attributes.Add("data-tracksavechanges", "true");
                    if (ImageWidth > 0)
                    {
                        pnlEditor.Style.Add(HtmlTextWriterStyle.Width, ImageWidth + "px;");
                        //this.pnlEditor.Width = new Unit(this.DialogWidth); // Causes Invalid cast on Render
                    }
                    Controls.Add(pnlEditor);

                    // Title label
                    lblTitle = new Label();
                    lblTitle.EnableViewState = false;
                    lblTitle.CssClass = "EditableTextTitle";
                    pnlEditor.Controls.Add(lblTitle);

                    // Error label
                    lblError = new Label();
                    lblError.EnableViewState = false;
                    lblError.CssClass = "EditableTextError";
                    pnlEditor.Controls.Add(lblError);

                    // Add image selector
                    selPath = new ImageSelector(null, false);
                    selPath.Culture = MembershipContext.AuthenticatedUser.PreferredUICultureCode;
                    selPath.EnableOpenInFull = false;
                    selPath.ID = "selPath";
                    selPath.UseImagePath = true;
                    selPath.ImageCssClass = ImageCssClass;
                    selPath.IsLiveSite = IsLiveSite;
                    selPath.ImageStyle = ImageStyle;
                    selPath.ShowTextBox = false;
                    pnlEditor.Controls.Add(selPath);
                    selPath.AddCssClass("cms-bootstrap");

                    selPath.Enabled = (ViewMode.IsEdit());

                    // Set dialog configuration  
                    selPath.DialogConfig.ResizeToHeight = ResizeToHeight;
                    selPath.DialogConfig.ResizeToWidth = ResizeToWidth;
                    selPath.DialogConfig.ResizeToMaxSideSize = ResizeToMaxSideSize;

                    SetTextBoxVisibility();
                    break;

                default:
                    // Display content in non editing modes
                    imgImage = new Image();
                    imgImage.ID = "imgImage";
                    // Ensures XHTML validity
                    imgImage.GenerateEmptyAlternateText = true;
                    if (ImageCssClass != "")
                    {
                        imgImage.CssClass = ImageCssClass;
                    }
                    if (ImageStyle != "")
                    {
                        imgImage.Attributes.Add("style", ImageStyle);
                    }

                    imgImage.AlternateText = AlternateText;
                    imgImage.EnableViewState = false;
                    Controls.Add(imgImage);
                    break;
            }
        }


        /// <summary>
        /// Loads the control content.
        /// </summary>
        /// <param name="content">Content to load</param>
        /// <param name="force">Force new selector value</param>
        public override void LoadContent(string content, bool force = false)
        {
            // Load the properties
            EnsureChildControls();

            string path = null;
            string altText = null;

            // Load the image data
            if (!string.IsNullOrEmpty(content))
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(content);

                XmlNodeList properties = xml.SelectNodes("image/property");
                if (properties != null)
                {
                    foreach (XmlNode node in properties)
                    {
                        if (node.Attributes["name"] != null)
                        {
                            switch (node.Attributes["name"].Value.ToLowerCSafe())
                            {
                                case "imagepath":
                                    path = ResolveUrl(node.InnerText.Trim());
                                    break;

                                case "alttext":
                                    altText = node.InnerText.Trim();
                                    break;
                            }
                        }
                    }
                }
            }

            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    if (ImageWidth > 0)
                    {
                        selPath.ImageWidth = ImageWidth;
                    }
                    if (ImageHeight > 0)
                    {
                        selPath.ImageHeight = ImageHeight;
                    }
                    if (!RequestHelper.IsPostBack() || force)
                    {
                        selPath.Value = path;
                        selPath.AlternateText = altText;
                    }

                    if (!string.IsNullOrEmpty(ImageTitle) && (lblTitle != null))
                    {
                        lblTitle.Text = ImageTitle;
                    }
                    else
                    {
                        if (lblTitle != null)
                        {
                            lblTitle.Visible = false;
                        }
                    }
                    break;

                default:
                    Visible = true;

                    if (string.IsNullOrEmpty(path) || (imgImage == null))
                    {
                        if (ViewMode.IsEditLive() && (imgImage != null))
                        {
                            // Render the empty control in the OnSite editing mode
                            imgImage.Visible = false;
                        }
                        else
                        {
                            Visible = false;
                        }

                        return;
                    }

                    // Check authorized state
                    if (!CheckPermissions || PageManager.IsAuthorized)
                    {
                        if (ImageWidth > 0)
                        {
                            path = URLHelper.AddParameterToUrl(path, "width", ImageWidth.ToString());
                        }
                        if (ImageHeight > 0)
                        {
                            path = URLHelper.AddParameterToUrl(path, "height", ImageHeight.ToString());
                        }

                        // Use specific alternate text or default alternate text
                        imgImage.AlternateText = String.IsNullOrEmpty(altText) ? AlternateText : altText;
                        imgImage.ImageUrl = path;
                    }
                    break;
            }
        }


        /// <summary>
        /// Gets the current control content.
        /// </summary>
        public override string GetContent()
        {
            EnsureChildControls();

            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    // Get the path
                    var altText = selPath.AlternateText;
                    var imagePath = selPath.Value.Trim();

                    return GetContentXml(imagePath, altText);
            }

            return null;
        }


        /// <summary>
        /// Gets the image content XML
        /// </summary>
        /// <param name="imagePath">Image path</param>
        /// <param name="altText">Alternate text</param>
        public static string GetContentXml(string imagePath, string altText)
        {
            var path = URLHelper.UnResolveUrl(imagePath, SystemContext.ApplicationPath);
            if (!string.IsNullOrEmpty(path))
            {
                var xml = new XmlDocument();

                var imageNode = xml.CreateElement("image");
                xml.AppendChild(imageNode);

                var properties = new Dictionary<string, string>
                {
                    { "imagepath", path },
                    { "alttext", altText }
                };

                imageNode.AddChildElements(properties, "property");

                return xml.ToFormattedXmlString(true);
            }

            return "";
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    if (selPath != null)
                    {
                        selPath.Enabled = (PortalContext.ViewMode.IsEdit());
                    }
                    if (lblError != null)
                    {
                        lblError.Visible = (lblError.Text != "");
                    }
                    if (lblTitle != null && (ImageTitle != null))
                    {
                        lblTitle.Text = ImageTitle;
                    }
                    break;
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[CMSEditableImage: " + ClientID + "]");
                return;
            }

            base.Render(writer);
        }


        /// <summary>
        /// Gets the custom dialog parameters used in the On-site editing when opening the modal edit dialog.
        /// The URL parameters are in the following format: "name=value"
        /// </summary>
        public override string[] GetEditDialogParameters()
        {
            List<string> parameters = new List<string>();

            parameters.Add("aspxc=1");

            // Alternate text
            if (!String.IsNullOrEmpty(AlternateText))
            {
                parameters.Add("at=" + AlternateText);
            }

            // Image CSS class
            if (!String.IsNullOrEmpty(ImageCssClass))
            {
                parameters.Add("icc=" + ImageCssClass);
            }

            // Image height
            if (ImageHeight > 0)
            {
                parameters.Add("ih=" + ImageHeight);
            }

            // Image width
            if (ImageWidth > 0)
            {
                parameters.Add("iw=" + ImageWidth);
            }

            // Resize to height
            if (ResizeToHeight > 0)
            {
                parameters.Add("rth=" + ResizeToHeight);
            }

            // Resize to width
            if (ResizeToWidth > 0)
            {
                parameters.Add("rtw=" + ResizeToWidth);
            }

            // Resize to max-side size
            if (ResizeToMaxSideSize > 0)
            {
                parameters.Add("rtmss=" + ResizeToMaxSideSize);
            }

            return parameters.ToArray();
        }

        #endregion
    }
}