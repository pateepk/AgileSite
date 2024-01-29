using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;


namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Base class for the web part controls.
    /// </summary>
    public partial class CMSAbstractWebPart
    {
        /// <summary>
        /// Renders the control to the given writer
        /// </summary>
        /// <param name="writer">Writer for the output</param>
        /// <param name="webPart">Web part control</param>
        private static void RenderInternal(CMSAbstractWebPart webPart, HtmlTextWriter writer)
        {
            // Do not render if not visible
            if (!webPart.Visible)
            {
                return;
            }

            webPart.mPageCycle = PageCycleEnum.Rendering;

            // Do not render if removed
            if (!webPart.Removed)
            {
                // Move the variants to a separate list (to avoid default rendering)
                bool renderVariants = (webPart.RenderVariants && webPart.HasVariants);
                if (renderVariants)
                {
                    webPart.VariantControlsPlaceHolder.Visible = false;
                }

                bool renderActive = false;

                // If not variant or layout web part, render with header and other controls around
                if (!webPart.StandAlone && !webPart.IsVariant)
                {
                    // Prepare the values
                    ViewModeEnum viewMode = webPart.ParentZone.ViewMode;

                    // Get the current view mode (it could have been changed when changing workflow steps) if it's required
                    // Refresh only widgets to ensure displaying their header when required
                    if ((webPart.PortalManager != null)
                        && webPart.PortalManager.ReloadViewMode
                        // Do not reload viewmode for user widgets. They cannot change their view modes between postbacks
                        && webPart.IsWidget && (webPart.ParentZone != null) && (webPart.ParentZone.WidgetZoneType != WidgetZoneTypeEnum.User))
                    {
                        viewMode = webPart.GetCurrentViewmode();
                    }

                    bool render = true;
                    bool designMode = false;

                    if (PortalContext.IsDesignMode(viewMode))
                    {
                        // Design mode rendering (just the design panel)
                        renderActive = (webPart.PartInstance.ParentZone != null);
                        designMode = true;
                    }
                    else
                    {
                        // Other modes
                        switch (viewMode)
                        {
                            case ViewModeEnum.Edit:
                            case ViewModeEnum.GroupWidgets:
                            case ViewModeEnum.UserWidgets:
                            case ViewModeEnum.UserWidgetsDisabled:
                            case ViewModeEnum.DashboardWidgets:
                                {
                                    renderActive = true;
                                }
                                break;

                            case ViewModeEnum.EditLive:
                                {
                                    if (!webPart.HideOnCurrentPage)
                                    {
                                        renderActive = true;
                                    }
                                }
                                break;
                        }
                    }

                    // Preview anchor
                    webPart.RenderPreviewAnchor(writer);

                    // On-Site editing - render web part begin span tag
                    webPart.renderOnSiteEditSpanTags = webPart.RenderOnSiteEditSpanTags();
                    bool isLayoutWebPart = (webPart.WebPartType == WebPartTypeEnum.Layout) || (webPart is CMSAbstractLayoutWebPart);
                    bool isEditableWebPart = webPart is CMSAbstractEditableWebPart;

                    if (webPart.renderOnSiteEditSpanTags)
                    {
                        string editPageUrl = string.Empty;
                        string dialogWidth = string.Empty;

                        // Get the "edit page url" and "dialog width" for editable web parts
                        if (isEditableWebPart)
                        {
                            var editableWebPart = (CMSAbstractEditableWebPart)webPart;
                            editPageUrl = editableWebPart.EditPageUrl;
                            dialogWidth = editableWebPart.EditDialogWidth;
                            int dialogWidthFromWebPartProperties = ValidationHelper.GetInteger(webPart.GetValue("DialogWidth"), 0);

                            if (dialogWidthFromWebPartProperties > 0)
                            {
                                dialogWidth = dialogWidthFromWebPartProperties.ToString();
                            }
                        }

                        // Do not render highlight span tags when used in a placeholder with Default page template set
                        if (webPart.PagePlaceholder.UsingDefaultPageTemplate)
                        {
                            webPart.renderOnSiteEditSpanTags = false;

                            // Do not render the web part content and its highlight span tags in On-site editing when the web part is not visible
                            if (!webPart.IsVisible)
                            {
                                render = false;
                            }
                        }

                        if (webPart.renderOnSiteEditSpanTags)
                        {
                            var tagConfig = new OnSiteEditStartTagConfiguration
                            {
                                CurrentUser = MembershipContext.AuthenticatedUser,
                                Page = webPart.PagePlaceholder.PageInfo,
                                EditUrl = editPageUrl,
                                DialogWidth = Unit.Parse(dialogWidth),
                                WebPartInstance = webPart.PartInstance,
                                WebPartTitle = webPart.WebPartTitle,
                                WebPartIsEditable = isEditableWebPart
                            };

                            string startSpan = OnSiteEditHelper.GetOnSiteEditStartTag(tagConfig);

                            if (!string.IsNullOrEmpty(startSpan))
                            {
                                // Render the on-site editing start tag
                                writer.Write(startSpan);
                            }
                            else
                            {
                                // Ensure that the end span will not be rendered when the start span was not rendered neither
                                webPart.renderOnSiteEditSpanTags = false;
                            }

                            if (webPart.renderOnSiteEditSpanTags && (isLayoutWebPart || !webPart.IsVisible))
                            {
                                // Render the layout web parts as an empty web part
                                writer.Write(OnSiteEditHelper.GetOnSiteEditEndTag());
                                webPart.renderOnSiteEditSpanTags = false;

                                // Do not render content the web parts which are not visible (layout web parts need to be rendered as they contain other web parts)
                                if (!webPart.IsVisible)
                                {
                                    render = false;
                                }
                            }
                        }
                    }

                    if (render)
                    {
                        // Render the container
                        string containerEnd = null;
                        if (renderActive)
                        {
                            containerEnd = webPart.RenderContainerStart(writer, renderActive, viewMode, isLayoutWebPart);
                        }

                        // Render the identification
                        if (renderActive)
                        {
                            writer.Write(webPart.GetLocationScript());

                            webPart.locationRendered = true;
                        }

                        // Render the border
                        bool renderBorder = renderActive;
                        if (renderBorder)
                        {
                            webPart.RenderBorderStart(writer, designMode);
                        }

                        // Handle
                        bool renderHandle = designMode && !isLayoutWebPart;
                        if (renderHandle)
                        {
                            webPart.RenderHandleStart(writer);
                        }

                        // Render the web part itself
                        if (renderActive)
                        {
                            // Handle
                            bool renderHeader = ((webPart.mHeaderContainer == null) && (webPart.mHeaderControl != null));
                            if (renderHeader)
                            {
                                webPart.mHeaderControl.RenderControl(writer);
                            }

                            // Content
                            webPart.RenderContentStart(writer);

                            // Render the variant envelope
                            if (renderVariants)
                            {
                                writer.Write("<div id=\"" + webPart.GetVariantID() + "\" >");
                            }

                            // Render contents if visible
                            if (!webPart.HideOnCurrentPage)
                            {
                                RenderContent(webPart, writer, renderHeader);
                            }

                            // Close the variant envelope
                            if (renderVariants)
                            {
                                writer.Write("</div>");

                                // Set the visibility to ensure that all the variants will be rendered correctly
                                webPart.VariantControlsPlaceHolder.Visible = true;

                                // Render the other variants of this webpart.
                                foreach (Control ctrl in webPart.VariantControlsPlaceHolder.Controls)
                                {
                                    ctrl.RenderControl(writer);
                                }
                            }

                            writer.RenderEndTag(); // Content
                        }
                        else
                        {
                            // Standard rendering
                            StandardRender(webPart, writer);
                        }

                        // End of the handle
                        if (renderHandle)
                        {
                            writer.RenderEndTag(); // Handle
                        }

                        // End of the border
                        if (renderBorder)
                        {
                            writer.RenderEndTag(); // Border
                        }

                        // End of the container
                        writer.Write(containerEnd);
                    }

                    // On-Site editing - render web part end span tag
                    if (webPart.renderOnSiteEditSpanTags)
                    {
                        writer.Write(OnSiteEditHelper.GetOnSiteEditEndTag());
                    }
                }
                else
                {
                    // Standard rendering (no special controls around) in case the web part is standalone or a variant
                    StandardRender(webPart, writer);
                }
            }

            // Log the used evals
            webPart.LogEvals();

            webPart.mPageCycle = PageCycleEnum.Rendered;
        }


        /// <summary>
        /// Renders the control in a standard way.
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        /// <param name="webPart">Web part control</param>
        private static void StandardRender(CMSAbstractWebPart webPart, HtmlTextWriter writer)
        {
            bool renderLocation = false;
            bool renderCPDesignPanel = false;

            // Handle the container
            string containerEnd = webPart.RenderStandardContainerStart(writer);

            // Handle the envelope
            string envelopeEnd = webPart.RenderEnvelopeStart(writer, ref renderLocation, ref renderCPDesignPanel);

            // Render the variant envelope
            bool renderVariantsEnvelope = (webPart.RenderVariants && (webPart.HasVariants || webPart.IsVariant));
            if (renderVariantsEnvelope)
            {
                if (webPart.IsVariant)
                {
                    writer.Write("<div class=\"CMSVariant\" id=\"" + webPart.GetVariantID() + "\" >");

                    renderLocation = true;
                }
                else
                {
                    writer.Write("<div id=\"" + webPart.GetVariantID() + "\" >");
                }
            }

            // Render the location
            if (renderLocation && !webPart.locationRendered)
            {
                writer.Write(webPart.GetLocationScript());
            }

            // Standard rendering
            if (!webPart.HideOnCurrentPage)
            {
                // Content personalization header in the Edit mode
                if (renderCPDesignPanel)
                {
                    webPart.mHeaderControl.RenderControl(writer);
                    webPart.mHeaderControl.Visible = false;
                }

                // Container before
                bool renderContainer = ((webPart.PartInstance != null) && !webPart.ContainerHideOnCurrentPage);
                if (renderContainer)
                {
                    writer.Write(webPart.ContainerBefore);
                }

                // Content before
                writer.Write(webPart.ContentBefore);

                if (webPart.CssClass != "")
                {
                    writer.Write("<div class=\"" + HTMLHelper.HTMLEncode(webPart.CssClass) + "\">");

                    webPart.BaseRender(writer);

                    writer.Write("</div>");
                }
                else
                {
                    webPart.BaseRender(writer);
                }

                // Render "Empty editable web part" text
                if (webPart.renderOnSiteEditSpanTags && (webPart is CMSAbstractEditableWebPart) && ((webPart as CMSAbstractEditableWebPart).EmptyContent))
                {
                    string title = !string.IsNullOrEmpty(webPart.WebPartTitle) ? webPart.WebPartTitle : webPart.PartInstance.ControlID;
                    writer.Write(OnSiteEditHelper.GetEmptyEditableWebPartTag(HTMLHelper.HTMLEncode(title)));
                }

                // Content after
                writer.Write(webPart.ContentAfter);

                // Container after
                if (renderContainer)
                {
                    writer.Write(webPart.ContainerAfter);
                }
            }

            // Close a variant envelope
            if (renderVariantsEnvelope)
            {
                writer.Write("</div>");

                // Render all variants of the web part
                if (webPart.HasVariants)
                {
                    // Render all web part/widget variants
                    foreach (Control ctrl in webPart.VariantControlsPlaceHolder.Controls)
                    {
                        ctrl.RenderControl(writer);
                    }
                }
            }

            // Envelope end
            writer.Write(envelopeEnd);

            // Close the positioning
            writer.Write(containerEnd);
        }


        /// <summary>
        /// Renders the web part content
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        /// <param name="renderHeader">True if the header should be rendered</param>
        /// <param name="webPart">Web part control</param>
        private static void RenderContent(CMSAbstractWebPart webPart, HtmlTextWriter writer, bool renderHeader)
        {
            if (renderHeader)
            {
                webPart.mHeaderControl.Visible = false;
            }

            if ((webPart.PartInstance != null) && !webPart.ContainerHideOnCurrentPage)
            {
                writer.Write(webPart.ContainerBefore);
            }

            writer.Write(webPart.ContentBefore);

            // Control itself
            webPart.BaseRender(writer);

            writer.Write(webPart.ContentAfter);

            if ((webPart.PartInstance != null) && !webPart.ContainerHideOnCurrentPage)
            {
                writer.Write(webPart.ContainerAfter);
            }

            if (renderHeader)
            {
                webPart.mHeaderControl.Visible = true;
            }
        }


        /// <summary>
        /// Renders the control to a string
        /// </summary>
        /// <param name="webPart">Web part control</param>
        private static string RenderToString(CMSAbstractWebPart webPart)
        {
            // Prepare the string builder for the control
            StringBuilder sb = new StringBuilder();
            StringWriter tw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(tw);

            RenderInternal(webPart, hw);

            return sb.ToString();
        }


        /// <summary>
        /// Returns true if the web part should render its on-site editing envelope
        /// </summary>
        private bool RenderOnSiteEditSpanTags()
        {
            if ((PartInfo != null)
                && (PagePlaceholder != null) && (PagePlaceholder.PageInfo != null)
                && (ParentZone != null) && (ParentZone.ZoneInstance != null)
                && (PartInstance != null))
            {
                // Render span tags for visible web parts only
                bool renderOnSiteEditSpanTags = (PortalContext.ViewMode.IsEditLive()) && !HideOnCurrentPage && !IsWidget;

                if (renderOnSiteEditSpanTags)
                {
                    // Render only for required web part types
                    switch ((WebPartTypeEnum)PartInfo.WebPartType)
                    {
                        case WebPartTypeEnum.Standard:
                        case WebPartTypeEnum.Filter:
                        case WebPartTypeEnum.Basic:
                        case WebPartTypeEnum.Layout:
                        case WebPartTypeEnum.DataSource:
                        case WebPartTypeEnum.BasicViewer:
                            return true;
                    }
                }
            }

            // Do not  render by default
            return false;
        }


        /// <summary>
        /// Renders the preview mode anchor tag
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        private void RenderPreviewAnchor(HtmlTextWriter writer)
        {
            if (GeneratePreviewAnchor())
            {
                writer.AddAttribute("name", "previewanchor");
                writer.AddAttribute("style", "visibility:hidden");
                writer.RenderBeginTag("a");
                writer.Write("anchor");
                writer.RenderEndTag(); // a
            }
        }


        /// <summary>
        /// Returns true if preview anchor to be generated
        /// </summary>
        private bool GeneratePreviewAnchor()
        {
            String previewObjectName = QueryHelper.GetString("previewobjectname", String.Empty);
            if (previewObjectName != String.Empty)
            {
                String previewObjectIdentifier = QueryHelper.GetString("previewobjectidentifier", String.Empty);
                Guid guid = QueryHelper.GetGuid("previewguid", Guid.Empty);

                // First compare guid 
                if (InstanceGUID == guid)
                {
                    return true;
                }
                else
                    if (previewObjectIdentifier != String.Empty)
                    {
                        // Compare identifier based on object name
                        switch (previewObjectName.ToLowerCSafe())
                        {
                            case WebPartContainerInfo.OBJECT_TYPE:
                                return ((Container != null) && (Container.ContainerName == previewObjectIdentifier));

                            case TransformationInfo.OBJECT_TYPE:
                                // Loop through all properties to test if transformation is used
                                foreach (string prop in PartInstance.Properties.Keys)
                                {
                                    if (PartInstance.GetValue(prop).ToString().CompareToCSafe(previewObjectIdentifier, true) == 0)
                                    {
                                        return true;
                                    }
                                }
                                break;

                            case WebPartInfo.OBJECT_TYPE:
                                return ((PartInfo != null) && (previewObjectIdentifier == PartInfo.WebPartName));

                            case WebPartLayoutInfo.OBJECT_TYPE:
                                String layoutName = ValidationHelper.GetString(PartInstance.GetValue("WebPartLayout"), String.Empty);
                                return (layoutName == previewObjectIdentifier);
                        }
                    }
            }
            return false;
        }


        /// <summary>
        /// Renders the start of the container. Returns the string representing the end of the container
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        /// <param name="renderActive">Render as active element</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="layout">Layout web part</param>
        private string RenderContainerStart(HtmlTextWriter writer, bool renderActive, ViewModeEnum viewMode, bool layout)
        {
            // Get the outer style
            string innerStyle = null;
            string outerStyle = GetOuterContainerStyle(out innerStyle);

            // Prepare the container classes
            string outerContainerClass = "WebPart";
            string innerContainerClass = null;

            // Class for web parts inside layout web part
            if (mRenderWebPartClass)
            {
                if (PortalHelper.RenderWebPartIDCssClass)
                {
                    innerContainerClass += " WebPart_" + ID;
                }
            }

            if (renderActive)
            {
                switch (viewMode)
                {
                    case ViewModeEnum.UserWidgetsDisabled:
                        innerContainerClass += " UserWidget UserWidgetDisabled";
                        break;

                    case ViewModeEnum.UserWidgets:
                        innerContainerClass += " UserWidget";
                        break;

                    case ViewModeEnum.GroupWidgets:
                        innerContainerClass += " GroupWidget";
                        break;

                    case ViewModeEnum.Edit:
                    case ViewModeEnum.EditLive:
                        innerContainerClass += " EditorWidget";
                        break;

                    case ViewModeEnum.EditDisabled:
                        innerContainerClass += " EditorWidgetDisabled";
                        break;

                    case ViewModeEnum.DashboardWidgets:
                        innerContainerClass += " DashboardWidget";
                        break;

                    default:
                        innerContainerClass += " StandardWebPart";
                        break;
                }
                // Minimized class
                if (PartInstance.Minimized)
                {
                    innerContainerClass += " MinimizedWidget";
                }
            }

            // Outer container
            writer.Write("<div id=\"{0}\"", GetContainerId());

            if (!String.IsNullOrEmpty(outerContainerClass))
            {
                writer.Write(" class=\"{0}\"", outerContainerClass.Trim());
            }
            if (!String.IsNullOrEmpty(outerStyle))
            {
                writer.Write(" style=\"{0}\"", outerStyle);
            }

            writer.Write(">");


            // Inner container
            writer.Write("<div");

            innerContainerClass += " WebPartInner";
            innerContainerClass = innerContainerClass.Trim();

            if (!String.IsNullOrEmpty(innerContainerClass))
            {
                writer.Write(" class=\"{0}\"", innerContainerClass);
            }
            if (!String.IsNullOrEmpty(innerStyle))
            {
                writer.Write(" style=\"{0}\"", innerStyle);
            }

            writer.Write(">");

            return "</div></div>";
        }


        /// <summary>
        /// Gets the style for the outer container
        /// </summary>
        /// <param name="innerStyle">Returns inner container style</param>
        private string GetOuterContainerStyle(out string innerStyle)
        {
            // Prepare the outer style
            string outerStyle = null;
            innerStyle = null;

            switch (LayoutType)
            {
                case ZoneLayoutTypeEnum.FloatLeft:
                    // Float left
                    outerStyle = "float: left;";
                    break;

                case ZoneLayoutTypeEnum.FloatRight:
                    // Float right
                    outerStyle = "float: right;";
                    break;

                case ZoneLayoutTypeEnum.Free:
                    // Flow layout - absolute position
                    {
                        outerStyle = "position: relative; height: 0px;";
                        innerStyle = "position: absolute;";

                        int top = ValidationHelper.GetInteger(GetValue("PositionTop"), 0);
                        if (top > 0)
                        {
                            outerStyle += " top: " + top + "px;";
                        }

                        int left = ValidationHelper.GetInteger(GetValue("PositionLeft"), 0);
                        if (left > 0)
                        {
                            outerStyle += " left: " + left + "px;";
                        }
                    }
                    break;
            }

            return outerStyle;
        }





        /// <summary>
        /// Renders the start of an envelope
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        /// <param name="renderLocation">Returns the state if the location should be rendered</param>
        /// <param name="renderCPDesignPanel">Returns the state if the content personalization panel should be rendered</param>
        private string RenderEnvelopeStart(HtmlTextWriter writer, ref bool renderLocation, ref bool renderCPDesignPanel)
        {
            StringBuilder envelopeEnd = new StringBuilder();

            // Envelope and border
            if (!HideOnCurrentPage && (RenderEnvelope || CPWebPartInEdit))
            {
                writer.Write("<div id=\"" + ClientID + "_border\" class=\"WebPartBorder\">");

                if (!CPWebPartInEdit)
                {
                    // Normal mode
                    writer.Write("<div id=\"" + ClientID + "\" >");

                    envelopeEnd.Append("</div>");
                }
                else
                {
                    // Content personalization in editing mode
                    writer.Write("<div id=\"" + ClientID + "\" onmouseover=\"ActivateBorder('" + ClientID + "', this);\" onmouseout=\"DeactivateBorder('" + ClientID + "', 50);\" >");
                    writer.Write("<div id=\"" + ClientID + "_Container\" class=\"CPContainer\">");

                    renderLocation = true;
                    renderCPDesignPanel = true;

                    envelopeEnd.Append("</div></div>");

                }

                envelopeEnd.Append("</div>");
            }

            return envelopeEnd.ToString();
        }


        /// <summary>
        /// Renders the start of the standard container
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        private string RenderStandardContainerStart(HtmlTextWriter writer)
        {
            string containerEnd = null;

            string innerStyle = null;
            string outerStyle = GetOuterContainerStyle(out innerStyle);
            if (!String.IsNullOrEmpty(outerStyle))
            {
                writer.Write("<div class=\"WebPart\" style=\"{0}\">", outerStyle);

                containerEnd += "</div>";
            }
            if (!String.IsNullOrEmpty(innerStyle))
            {
                // Inner style
                writer.Write("<div style=\"{0}\" class=\"WebPartInner\">", innerStyle);
                containerEnd += "</div>";
            }

            return containerEnd;
        }


        /// <summary>
        /// Renders the start of the content tag
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        private void RenderContentStart(HtmlTextWriter writer)
        {
            writer.AddAttribute("id", ShortClientID + "_content");
            writer.AddAttribute("class", "WebPartContent" + AdditionalCssClass);
            writer.RenderBeginTag("div");
        }


        /// <summary>
        /// Renders the starting tag of the handle
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        private void RenderHandleStart(HtmlTextWriter writer)
        {
            writer.AddAttribute("class", "WebPartHandle");
            writer.AddAttribute("id", ShortClientID + "_hnd");
            writer.RenderBeginTag("div"); // Handle
        }


        /// <summary>
        /// Renders the start of the border
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        /// <param name="designMode">True if the view mode is design</param>
        private void RenderBorderStart(HtmlTextWriter writer, bool designMode)
        {
            // Border
            writer.AddAttribute("id", ShortClientID + "_border");
            writer.AddAttribute("class", "WebPartBorder");

            // Border activation
            if (!designMode)
            {
                writer.AddAttribute("onmouseover", "ActivateBorder('" + ShortClientID + "', this);");
                writer.AddAttribute("onmouseout", "DeactivateBorder('" + ShortClientID + "', 50);");
            }

            if ((IsWidget && ViewMode.IsDesign()) || ((ViewMode != ViewModeEnum.DesignDisabled) && !IsWidget && (mHeaderContainer == null)))
            {
                // Configuration on double click
                writer.AddAttribute("ondblclick", configureScript + " return false;");
            }

            writer.RenderBeginTag("div"); // Border
        }
    }
}
