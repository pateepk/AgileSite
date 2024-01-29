using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Text comparison control tool.
    /// </summary>
    [ToolboxData("<{0}:TextComparison runat=server></{0}:TextComparison>")]
    public class TextComparison : CMSWebControl, INamingContainer
    {
        #region "Variables"

        // Wrapper panel
        private Panel pnlContainer = null;

        // List<DiffTextPart>s with text parts
        private List<DiffTextPart> mSourceDiffList = null;
        private List<DiffTextPart> mDestinationDiffList = null;

        // Paired control which uses this control Array lists as data source
        private TextComparison mPairedControl = null;

        private TextDiffList mDiffList = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if controls should stop processing.
        /// </summary>
        [Category("Behavior"), Description("Indicates if controls should stop processing.")]
        public bool StopProcessing
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["StopProcessing"], false);
            }
            set
            {
                ViewState["StopProcessing"] = value;
            }
        }


        /// <summary>
        /// Source string for diff comparison.
        /// </summary>
        [Category("Behavior"), Description("Source string for difference comparison.")]
        public string SourceText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SourceText"], "");
            }

            set
            {
                ViewState["SourceText"] = value;
            }
        }


        /// <summary>
        /// Destination string for diff comparison.
        /// </summary>
        [Category("Behavior"), Description("Destination string for difference comparison.")]
        public string DestinationText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DestinationText"], "");
            }

            set
            {
                ViewState["DestinationText"] = value;
            }
        }


        /// <summary>
        /// CSS class used for styling of HTML code in text.
        /// </summary>
        [Category("Appearance"), DefaultValue("HTMLMarkup"), Description("CSS class used for styling of HTML code in text.")]
        public string HTMLMarkupStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HTMLMarkupStyle"], "HTMLMarkup");
            }
            set
            {
                ViewState["HTMLMarkupStyle"] = value;
            }
        }


        /// <summary>
        /// CSS class used for styling of text removed from source.
        /// </summary>
        [Category("Appearance"), DefaultValue("RemovedFromSource"), Description("CSS class used for styling of text removed from source.")]
        public string RemovedFromSourceStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["RemovedFromSourceStyle"], "RemovedFromSource");
            }
            set
            {
                ViewState["RemovedFromSourceStyle"] = value;
            }
        }


        /// <summary>
        /// CSS class used for styling of text added to destination.
        /// </summary>
        [Category("Appearance"), DefaultValue("AddedToDestination"), Description("CSS class used for styling of text added to destination.")]
        public string AddedToDestinationStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["AddedToDestinationStyle"], "AddedToDestination");
            }
            set
            {
                ViewState["AddedToDestinationStyle"] = value;
            }
        }


        /// <summary>
        /// CSS class used for styling of not included text.
        /// </summary>
        [Category("Appearance"), DefaultValue("NotIncluded"), Description("CSS class used for styling of not included text.")]
        public string NotIncludedStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["NotIncludedStyle"], "NotIncluded");
            }
            set
            {
                ViewState["NotIncludedStyle"] = value;
            }
        }


        /// <summary>
        /// CSS class used for styling of not included HTML.
        /// </summary>
        [Category("Appearance"), DefaultValue("HTMLNotIncluded"), Description("CSS class used for styling of not included HTML.")]
        public string HTMLNotIncludedStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HTMLNotIncludedStyle"], "HTMLNotIncluded");
            }
            set
            {
                ViewState["HTMLNotIncludedStyle"] = value;
            }
        }


        /// <summary>
        /// CSS class used for styling of matched text.
        /// </summary>
        [Category("Appearance"), DefaultValue("Matched"), Description("CSS class used for styling of matched text.")]
        public string MatchedStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["MatchedStyle"], "Matched");
            }
            set
            {
                ViewState["MatchedStyle"] = value;
            }
        }


        /// <summary>
        /// Switch between rendering source text and destination text.
        /// </summary>
        [Category("Behavior"), DefaultValue(TextComparisonTypeEnum.SourceText), Description("Mode of rendering diff text.")]
        public TextComparisonTypeEnum RenderingMode
        {
            get
            {
                if (ViewState["RenderingMode"] == null)
                {
                    return TextComparisonTypeEnum.SourceText;
                }
                return (TextComparisonTypeEnum)ViewState["RenderingMode"];
            }
            set
            {
                ViewState["RenderingMode"] = value;
            }
        }


        /// <summary>
        /// If true, the HTML tags are ignored in comparison but produced in resulting text in their original version.
        /// </summary>
        [Category("Behavior"), DefaultValue(TextComparisonTypeEnum.SourceText), Description("Mode of rendering diff text.")]
        public bool IgnoreHTMLTags
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IgnoreHTMLTags"], false);
            }
            set
            {
                ViewState["IgnoreHTMLTags"] = value;
            }
        }


        /// <summary>
        /// If true, the found HTML tags are considered equal in ignore mode.
        /// </summary>
        [Category("Behavior"), DefaultValue(TextComparisonTypeEnum.SourceText), Description("Behavior for the HTML tags.")]
        public bool ConsiderHTMLTagsEqual
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ConsiderHTMLTagsEqual"], false);
            }
            set
            {
                ViewState["ConsiderHTMLTagsEqual"] = value;
            }
        }


        /// <summary>
        /// If true, the resulting content is balance to include the same content on both sides.
        /// </summary>
        [Category("Behavior"), DefaultValue(TextComparisonTypeEnum.SourceText), Description("If true, the resulting content is balance to include the same content on both sides.")]
        public bool BalanceContent
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["BalanceContent"], true);
            }
            set
            {
                ViewState["BalanceContent"] = value;
            }
        }


        /// <summary>
        /// DiffList for source text.
        /// </summary>
        [Category("Data"), Browsable(false), ReadOnly(true), Description("List<DiffTextPart> which can be source of data for related control.")]
        public List<DiffTextPart> SourceDiffList
        {
            get
            {
                if (mSourceDiffList == null)
                {
                    mSourceDiffList = DiffList.SrcDiffList;
                }
                return mSourceDiffList;
            }
            set
            {
                mSourceDiffList = value;
            }
        }


        /// <summary>
        /// DiffList for destination text.
        /// </summary>
        [Category("Data"), Browsable(false), ReadOnly(true), Description("List<DiffTextPart> which can be source of data for related control.")]
        public List<DiffTextPart> DestinationDiffList
        {
            get
            {
                if (mDestinationDiffList == null)
                {
                    mDestinationDiffList = DiffList.DstDiffList;
                }
                return mDestinationDiffList;
            }
            set
            {
                mDestinationDiffList = value;
            }
        }


        /// <summary>
        /// ID of paired control.
        /// </summary>
        [Category("Data"), Description("ID of paired control.")]
        public string PairedControlID
        {
            get
            {
                return ValidationHelper.GetString(ViewState["PairedControlID"], "");
            }
            set
            {
                ViewState["PairedControlID"] = value;
            }
        }


        /// <summary>
        /// PairedControl.
        /// </summary>
        [Browsable(false)]
        public TextComparison PairedControl
        {
            get
            {
                if (mPairedControl == null)
                {
                    if (!String.IsNullOrEmpty(PairedControlID))
                    {
                        mPairedControl = Parent.FindControl(PairedControlID) as TextComparison;
                    }
                }
                return mPairedControl;
            }
            set
            {
                mPairedControl = value;
            }
        }


        /// <summary>
        /// Destination string for diff comparison.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Enable or disable synchronized scrolling of paired control.")]
        public bool SynchronizedScrolling
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["SynchronizedScrolling"], true);
            }
            set
            {
                ViewState["SynchronizedScrolling"] = value;
            }
        }


        /// <summary>
        /// Indicates if whole source text will be compared including HTML.
        /// </summary>
        [Category("Behavior"), DefaultValue(TextComparisonModeEnum.HTML), Description("Indicates how should be comparison text treated.")]
        public TextComparisonModeEnum ComparisonMode
        {
            get
            {
                return ValidationHelper.GetValue<TextComparisonModeEnum>(ViewState["ComparisonMode"]);
            }
            set
            {
                ViewState["ComparisonMode"] = value;
            }
        }


        /// <summary>
        /// Indicates if whole source text will be compared including HTML.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if text line endings will be replaced by its HTML variant.")]
        public bool EnsureHTMLLineEndings
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EnsureHTMLLineEndings"], false);
            }
            set
            {
                ViewState["EnsureHTMLLineEndings"] = value;
            }
        }


        /// <summary>
        /// DiffList for comparing texts.
        /// </summary>
        private TextDiffList DiffList
        {
            get
            {
                if (mDiffList == null)
                {
                    switch (ComparisonMode)
                    {
                        case TextComparisonModeEnum.HTML:
                        case TextComparisonModeEnum.PlainText:

                            // Process the comparison
                            mDiffList = new HTMLTextDiffList(SourceText, DestinationText, ComparisonMode == TextComparisonModeEnum.PlainText);

                            ((HTMLTextDiffList)mDiffList).IgnoreHTMLTags = IgnoreHTMLTags;
                            ((HTMLTextDiffList)mDiffList).ConsiderHTMLTagsEqual = ConsiderHTMLTagsEqual;
                            ((HTMLTextDiffList)mDiffList).BalanceContent = BalanceContent;
                            break;

                        case TextComparisonModeEnum.PlainTextWithoutFormating:
                            mDiffList = new TextDiffList(SourceText, DestinationText);
                            break;
                    }

                    mDiffList.Analyze();
                }
                return mDiffList;
            }
        }

        #endregion


        #region "Control related methods"

        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Minimal rendered text in design mode 
            if (Context == null)
            {
                output.Write("[TextComparison: - " + ID + "]");
                return;
            }

            // Render only if is not stop processing or if is some HTML code generated
            if (!StopProcessing)
            {
                RenderContents(output);
            }
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            base.EnsureChildControls();

            // Add wrapper container panel
            pnlContainer = new Panel();
            pnlContainer.ID = "pnlContainer";
            Controls.Add(pnlContainer);
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Apply editor settings
            ApplySettings();

            // Register javascript methods if paired control is set and 
            if ((PairedControl != null) && (SynchronizedScrolling))
            {
                // Register TextComparison scroll javascript
                if (!ScriptHelper.IsClientScriptBlockRegistered("textComparisonFunctions"))
                {
                    ScriptHelper.RegisterJQuery(Page);

                    // Register script for srolling both parts
                    ScriptHelper.RegisterClientScriptBlock(Page, GetType(), "textComparisonFunctions", ScriptHelper.GetScript(
                        @"var textComparisonPairs = new Array();
                      window.setInterval(""checkScrolling()"",75);

                          function RegisterComparisonPair(divID1,divID2)
                          {
                             var div1 = new Array();
                             
                             div1['ID'] = divID1;
                             div1['scrollTop'] = $cmsj('div#' + divID1).scrollTop();
                             div1['scrollLeft'] = $cmsj('div#' + divID1).scrollLeft();

                             var div2 = new Array();
                             div2['ID'] = divID2;
                             div2['scrollTop'] = $cmsj('div#' + divID2).scrollTop();
                             div2['scrollLeft'] = $cmsj('div#' + divID2).scrollLeft();
                             
                             textComparisonPairs[textComparisonPairs.length] = new Array(div1,div2);

                          }

                          function checkScrolling()
                          {
                            for ( i=0; i< textComparisonPairs.length; i++ )
                            {
                                var div1 = $cmsj('div#' + textComparisonPairs[i][0].ID);
                                var div2 = $cmsj('div#' + textComparisonPairs[i][1].ID);

                                if((!div1) || (!div2))
                                {
                                    continue;
                                }
                                
                                // Helper control
                                var control = null;
                                
                                if(div1.scrollTop() != textComparisonPairs[i][0].scrollTop) 
                                {
                                    control = div1;
                                }
                                else if(div2.scrollTop() != textComparisonPairs[i][1].scrollTop)
                                {
                                    control = div2;
                                }

                                if(control == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    div1.scrollTop(control.scrollTop());
                                    div2.scrollTop(control.scrollTop());
                                    textComparisonPairs[i][0].scrollTop = div1.scrollTop();
                                    textComparisonPairs[i][1].scrollTop = div2.scrollTop();
                                }

                                if(div1.scrollLeft() != textComparisonPairs[i][0].scrollLeft) 
                                {
                                    control = div1;
                                }
                                else if(div2.scrollLeft() != textComparisonPairs[i][1].scrollLeft)
                                {
                                    control = div2;
                                }

                                if(control == null)
                                {
                                    continue;
                                }
                                else
                                {
                                     div1.scrollLeft(control.scrollLeft());
                                    div2.scrollLeft(control.scrollLeft());
                                    textComparisonPairs[i][0].scrollLeft = div1.scrollLeft();
                                    textComparisonPairs[i][1].scrollLeft = div2.scrollLeft();
                                }
                            }
                            
                        }"));
                }

                // Register TextComparison control pair
                ScriptHelper.RegisterClientScriptBlock(Page, GetType(), "TextComparisonScroll" + ClientID, ScriptHelper.GetScript(
                    @"$cmsj(function()
                      {
                          RegisterComparisonPair('" + ClientID + @"_pnlContainer','" + PairedControl.ClientID + @"_pnlContainer');   
                      });"));
            }
        }


        /// <summary>
        /// Apply settings to wrapper container panel and paired control.
        /// </summary>
        private void ApplySettings()
        {
            EnsureChildControls();

            // Apply settings on container panel
            pnlContainer.Width = Width;
            pnlContainer.Height = Height;
            pnlContainer.ScrollBars = ScrollBars.Auto;

            // Apply settings on paired control too
            if (PairedControl != null)
            {
                PairedControl.Width = Width;
                PairedControl.Height = Height;
            }
        }


        /// <summary>
        /// Sets paired control diff lists.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (PairedControl != null)
            {
                PairedControl.SourceDiffList = SourceDiffList;
                PairedControl.DestinationDiffList = DestinationDiffList;
            }
        }


        /// <summary>
        /// Render control content.
        /// </summary>
        /// <param name="output">HTMLTextWriter parameter</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            StringBuilder result = new StringBuilder();
            List<DiffTextPart> list = null;
            DiffTextPart oldPart = null;

            // Set source List<DiffTextPart> according to RenderingMode
            if (RenderingMode == TextComparisonTypeEnum.SourceText)
            {
                list = SourceDiffList;
            }
            else
            {
                list = DestinationDiffList;
            }

            //ComparisonTextPart.SortBy = ComparisonTextPartSortBy.SrcIndex;
            //list.Sort();

            // Go through the DiffTextPart list
            foreach (DiffTextPart part in list)
            {
                // Check for previous text part
                if (oldPart != null)
                {
                    // If actual part differs in status, render end span tag
                    if (oldPart.Status != part.Status)
                    {
                        // Finish proper previous textpart
                        if (GetTextStyle(oldPart.Status) != "")
                        {
                            result.Append(@"</span>");
                        }

                        // Begin new textpart
                        result.Append(GetSpanTag(part.Status));
                    }
                }
                else
                {
                    result.Append(GetSpanTag(part.Status));
                }

                // Encoded textpart string
                string renderText = null;
                if (IgnoreHTMLTags)
                {
                    switch (part.Status)
                    {
                        case DiffStatus.HTMLPart:
                        case DiffStatus.HTMLNotIncluded:
                            // Render original HTML
                            renderText = part.Text;
                            break;

                        default:
                            // Render encoded text
                            renderText = HTMLHelper.HTMLEncode(part.Text);
                            break;
                    }
                }
                else
                {
                    // Render encoded text
                    renderText = HTMLHelper.HTMLEncode(part.Text);
                }

                // Ensure line ending if neccessary
                if (EnsureHTMLLineEndings)
                {
                    renderText = HTMLHelper.EnsureHtmlLineEndings(renderText);
                }

                // Render the part
                result.Append(renderText);

                oldPart = part;
            }

            // Finish proper last textpart
            if ((oldPart != null) && (GetTextStyle(oldPart.Status) != ""))
            {
                result.Append(@"</span>");
            }

            // Render full control code
            pnlContainer.RenderBeginTag(output);
            output.Write(result);
            pnlContainer.RenderEndTag(output);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns start span tag in case CSS styling in necessary.
        /// </summary>
        /// <param name="status">Status of text part</param>
        /// <returns>String with span tag with appropriate CSS class</returns>
        private string GetSpanTag(DiffStatus status)
        {
            string spanTag = "";
            if (!((status == DiffStatus.HTMLPart) && IgnoreHTMLTags))
            {
                // Check for CSS class setting
                string style = GetTextStyle(status);
                if (!String.IsNullOrEmpty(style))
                {
                    spanTag += "<span class=\"" + style + "\"";
                    switch (status)
                    {
                        case DiffStatus.HTMLNotIncluded:
                        case DiffStatus.NotIncluded:
                            spanTag += " unselectable=\"on\"";
                            break;
                    }
                    spanTag += ">";
                }
            }
            return spanTag;
        }


        /// <summary>
        /// Check for CSS settings and return appropriate class name.
        /// </summary>
        /// <param name="status">DiffStatus of text part</param>
        /// <returns>Particular class name</returns>
        private string GetTextStyle(DiffStatus status)
        {
            switch (status)
            {
                    // Check for AddedToDestination CSS settings
                case DiffStatus.AddedToDestination:
                    return ValidationHelper.GetString(AddedToDestinationStyle, "");

                    // Check for HTMLPart CSS settings
                case DiffStatus.HTMLPart:
                    {
                        if (!IgnoreHTMLTags)
                        {
                            return ValidationHelper.GetString(HTMLMarkupStyle, "");
                        }
                        return "";
                    }

                    // Check for HTMLNotIncluded CSS settings
                case DiffStatus.HTMLNotIncluded:
                    return HTMLNotIncludedStyle;

                    // Check for NotIncluded CSS settings
                case DiffStatus.NotIncluded:
                    return NotIncludedStyle;

                    // Check for RemovedFromSource CSS settings
                case DiffStatus.RemovedFromSource:
                    return RemovedFromSourceStyle;

                    // Check for Matched CSS settings
                case DiffStatus.Matched:
                    return MatchedStyle;

                    // Default none
                default:
                    return "";
            }
        }

        #endregion
    }
}
