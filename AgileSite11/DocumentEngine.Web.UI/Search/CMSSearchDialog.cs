using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Search;
using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSSearchDialog class.
    /// </summary>
    [ToolboxData("<{0}:CMSSearchDialog runat=server></{0}:CMSSearchDialog>"), Serializable()]
    public class CMSSearchDialog : WebControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// Search for label.
        /// </summary>
        protected Label mLblSearchFor;

        /// <summary>
        /// Search for textbox.
        /// </summary>
        protected TextBox mTxtSearchFor;

        /// <summary>
        /// Search mode label.
        /// </summary>
        protected Label mLblSearchMode;

        /// <summary>
        /// Search mode dropdown list.
        /// </summary>
        protected CMSDropDownList mDrpSearchMode;

        /// <summary>
        /// Search scope label.
        /// </summary>
        protected Label mLblSearchScope;

        /// <summary>
        /// Search scope dropdown list.
        /// </summary>
        protected CMSDropDownList mDrpSearchScope;

        /// <summary>
        /// Search button.
        /// </summary>
        protected Button mBtnSearch;

        /// <summary>
        /// Outer panel.
        /// </summary>
        protected Panel mSearchPanel;

        /// <summary>
        /// Stop processing.
        /// </summary>
        protected bool mStopProcesing;

        /// <summary>
        /// Occurres when user submits the dialog.
        /// </summary>
        public delegate void DoSearchEventHandler();

        /// <summary>
        /// Do search.
        /// </summary>
        public event DoSearchEventHandler DoSearch;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the custom query string which is placed after search querystring data, do not use &amp; or ? on start of the custom query string.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Gets or sets the custom query string which is placed after search querystring data, do not use & or ? on start of the custom query string.")]
        public string CustomQueryStringData
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CustomQueryStringData"], "");
            }
            set
            {
                ViewState["CustomQueryStringData"] = value;
            }
        }


        /// <summary>
        /// Stop processing 
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Stop processing.")]
        public virtual bool StopProcessing
        {
            get
            {
                return ValidationHelper.GetBoolean(mStopProcesing, false);
            }
            set
            {
                mStopProcesing = value;
            }
        }


        /// <summary>
        /// SearchFor label control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public Label SearchForLabel
        {
            get
            {
                return mLblSearchFor;
            }

            set
            {
                mLblSearchFor = value;
            }
        }


        /// <summary>
        /// SearchFor textbox.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public TextBox SearchForTextBox
        {
            get
            {
                return mTxtSearchFor;
            }
            set
            {
                mTxtSearchFor = value;
            }
        }


        /// <summary>
        /// SearchMode label.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public Label SearchModeLabel
        {
            get
            {
                return mLblSearchMode;
            }
            set
            {
                mLblSearchMode = value;
            }
        }


        /// <summary>
        /// SearchMode drop-down list.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public CMSDropDownList SearchModeList
        {
            get
            {
                return mDrpSearchMode;
            }
            set
            {
                mDrpSearchMode = value;
            }
        }


        /// <summary>
        /// SearchScope label.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public Label SearchScopeLabel
        {
            get
            {
                return mLblSearchScope;
            }
            set
            {
                mLblSearchScope = value;
            }
        }


        /// <summary>
        /// SearchScope drop-down list.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public CMSDropDownList SearchScopeList
        {
            get
            {
                return mDrpSearchScope;
            }
            set
            {
                mDrpSearchScope = value;
            }
        }


        /// <summary>
        /// Search button.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public Button SearchButton
        {
            get
            {
                return mBtnSearch;
            }
            set
            {
                mBtnSearch = value;
            }
        }

        #endregion


        #region "Search properties"

        /// <summary>
        /// Returns the search scope based on the given string.
        /// </summary>
        /// <param name="searchScope">String Scope representation</param>
        public static SearchScopeEnum GetSearchScope(string searchScope)
        {
            if (searchScope == null)
            {
                return SearchScopeEnum.SearchAllContent;
            }
            else
            {
                switch (searchScope.ToLowerCSafe())
                {
                    case "searchcurrentsection":
                        return SearchScopeEnum.SearchCurrentSection;
                    default:
                        return SearchScopeEnum.SearchAllContent;
                }
            }
        }


        /// <summary>
        /// Returns the search mode based on the given string.
        /// </summary>
        /// <param name="searchMode">String mode representation</param>
        public static SearchModeEnum GetSearchMode(string searchMode)
        {
            return searchMode.ToEnum<SearchModeEnum>();
        }


        ///<summary>Indicates if search mode settings should be displayed.</summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if search mode settings should be displayed."), NotifyParentProperty(true)]
        public bool ShowSearchMode
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowSearchMode"], false);
            }
            set
            {
                ViewState["ShowSearchMode"] = value;

                if (value)
                {
                    mLblSearchMode.Visible = true;
                    mDrpSearchMode.Visible = true;
                }
                else
                {
                    mLblSearchMode.Visible = false;
                    mDrpSearchMode.Visible = false;
                }
            }
        }


        ///<summary>Indicates if search scope settings should be displayed.</summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if search scope settings should be displayed."), NotifyParentProperty(true)]
        public bool ShowSearchScope
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowSearchScope"], false);
            }
            set
            {
                ViewState["ShowSearchScope"] = value;
                if (value)
                {
                    mLblSearchScope.Visible = true;
                    mDrpSearchScope.Visible = true;
                }
                else
                {
                    mLblSearchScope.Visible = false;
                    mDrpSearchScope.Visible = false;
                }
            }
        }


        ///<summary>Word(s) to be searched for.</summary>
        [Category("Behavior"), DefaultValue(""), Description("Word(s) to be searched for."), NotifyParentProperty(true)]
        public string SearchExpression
        {
            get
            {
                if (mTxtSearchFor != null)
                {
                    return mTxtSearchFor.Text;
                }
                return String.Empty;
            }
            set
            {
                mTxtSearchFor.Text = value;
            }
        }


        ///<summary>Search mode - any word, all words, exact phrase, etc.</summary>
        [Category("Behavior"), DefaultValue(1), Description("Search mode - any word, all words or exact phrase."), NotifyParentProperty(true)]
        public SearchModeEnum SearchMode
        {
            get
            {
                if (mDrpSearchMode != null)
                {
                    return mDrpSearchMode.SelectedValue.ToEnum<SearchModeEnum>();
                }

                return SearchModeEnum.AnyWord;
            }
            set
            {
                mDrpSearchMode.SelectedValue = value.ToStringRepresentation();
            }
        }


        ///<summary>Indicates if all content or only the current section should be searched.</summary>
        [Category("Behavior"), DefaultValue(0), Description("Returns 0 for all content or 1 for the current section."), NotifyParentProperty(true)]
        public SearchScopeEnum SearchScope
        {
            get
            {
                if (mDrpSearchScope != null)
                {
                    return mDrpSearchScope.SelectedValue.ToEnum<SearchScopeEnum>();
                }

                return SearchScopeEnum.SearchAllContent;
            }
            set
            {
                mDrpSearchScope.SelectedValue = value.ToStringRepresentation();
            }
        }

        #endregion


        /// <summary>
        /// Create child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            if (!StopProcessing)
            {
                Controls.Add(mSearchPanel);
                mSearchPanel.Controls.Add(new LiteralControl("<table> \n <tr><td>"));
                mSearchPanel.Controls.Add(mLblSearchFor);
                mSearchPanel.Controls.Add(new LiteralControl("</td><td>"));
                mSearchPanel.Controls.Add(mTxtSearchFor);
                mSearchPanel.Controls.Add(new LiteralControl("&nbsp;"));
                mSearchPanel.Controls.Add(mBtnSearch);
                mSearchPanel.Controls.Add(new LiteralControl("</td></tr> \n"));

                if (ShowSearchMode)
                {
                    mSearchPanel.Controls.Add(new LiteralControl("<tr><td>"));
                    mSearchPanel.Controls.Add(mLblSearchMode);
                    mSearchPanel.Controls.Add(new LiteralControl("</td><td>"));
                    mSearchPanel.Controls.Add(mDrpSearchMode);
                    mSearchPanel.Controls.Add(new LiteralControl("</td></tr> \n"));
                }

                if (ShowSearchScope)
                {
                    mSearchPanel.Controls.Add(new LiteralControl("<tr><td>"));
                    mSearchPanel.Controls.Add(mLblSearchScope);
                    mSearchPanel.Controls.Add(new LiteralControl("</td><td>"));
                    mSearchPanel.Controls.Add(mDrpSearchScope);
                    mSearchPanel.Controls.Add(new LiteralControl("</td></tr>\n"));
                }

                mSearchPanel.Controls.Add(new LiteralControl("</table>"));
            }

            base.CreateChildControls();
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (Context == null)
            {
                return;
            }

            EnsureChildControls();
            base.OnInit(e);
        }


        /// <summary>
        /// OnLoad override.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (Page != null)
            {
                string targetId = Page.Request.Params.Get(Page.postEventArgumentID);
                if (!String.IsNullOrEmpty(targetId))
                {
                    if (targetId.ToLowerCSafe() == ClientID.ToLowerCSafe())
                    {
                        StartSearch(mBtnSearch, null);
                    }
                }

                mBtnSearch.OnClientClick = Page.ClientScript.GetPostBackEventReference(mBtnSearch, ClientID);
            }

            base.OnLoad(e);
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[ CMSSearchDialog : " + ClientID + " ]");
                return;
            }

            if (!StopProcessing)
            {
                // SearchModeList.SelectedValue = 


                //this.EnableViewState = false;
                if (Context == null)
                {
                    output.Write("<table><tr><td>");
                    mLblSearchFor.RenderControl(output);
                    output.Write("</td><td>");
                    mTxtSearchFor.RenderControl(output);
                    output.Write("&nbsp;");
                    mBtnSearch.RenderControl(output);
                    output.Write("</td></tr>");
                    // If ShowSearchMode Then
                    output.Write("<tr><td>");
                    mLblSearchMode.RenderControl(output);
                    output.Write("</td><td>");
                    mDrpSearchMode.RenderControl(output);
                    output.Write("</td></tr>");
                    // End If
                    // If ShowSearchScope Then
                    output.Write("<tr><td>");
                    mLblSearchScope.RenderControl(output);
                    output.Write("</td><td>");
                    mDrpSearchScope.RenderControl(output);
                    output.Write("</td></tr>");
                    // End If
                    output.Write("</table>");
                }
                else
                {
                    base.Render(output);
                }
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSSearchDialog()
            : base()
        {
            if (Context == null)
            {
                return;
            }

            if (!StopProcessing)
            {
                mLblSearchFor = new Label();
                mLblSearchFor.Text = ResHelper.GetString("CMSSearchDialog.SearchFor");
                mLblSearchFor.CssClass = "CMSSearchDialogSearchForLabel";
                mLblSearchFor.AssociatedControlID = "txtSearchFor";
                mTxtSearchFor = new CMSTextBox();
                mTxtSearchFor.Width = new Unit(150);
                mTxtSearchFor.EnableViewState = true;
                mTxtSearchFor.CssClass = "CMSSearchDialogSearchForTextBox";
                mTxtSearchFor.ID = "txtSearchFor";
                mLblSearchMode = new Label();
                mLblSearchMode.Text = ResHelper.GetString("CMSSearchDialog.SearchMode");
                mLblSearchMode.CssClass = "CMSSearchDialogSearchModeLabel";
                mLblSearchMode.AssociatedControlID = "drpSearchMode";              
                mDrpSearchMode = new CMSDropDownList();
                mDrpSearchMode.Width = new Unit(150);
                mDrpSearchMode.Items.Add(new ListItem(ResHelper.GetString("CMSSearchDialog.SearchMode.AnyWord"), SearchModeEnum.AnyWord.ToStringRepresentation()));
                mDrpSearchMode.Items.Add(new ListItem(ResHelper.GetString("CMSSearchDialog.SearchMode.AllWords"), SearchModeEnum.AllWords.ToStringRepresentation()));
                mDrpSearchMode.Items.Add(new ListItem(ResHelper.GetString("CMSSearchDialog.SearchMode.ExactPhrase"), SearchModeEnum.ExactPhrase.ToStringRepresentation()));
                mDrpSearchMode.CssClass = "CMSSearchDialogSearchModeDropDownList";
                mDrpSearchMode.ID = "drpSearchMode";
                mLblSearchScope = new Label();
                mLblSearchScope.Text = ResHelper.GetString("CMSSearchDialog.SearchScope");
                mLblSearchScope.CssClass = "CMSSearchDialogSearchScopeLabel";
                mLblSearchScope.AssociatedControlID = "drpSearchScope";
                mDrpSearchScope = new CMSDropDownList();
                mDrpSearchScope.Items.Add(new ListItem(ResHelper.GetString("CMSSearchDialog.SearchScope.AllContent"), SearchScopeEnum.SearchAllContent.ToStringRepresentation()));
                mDrpSearchScope.Items.Add(new ListItem(ResHelper.GetString("CMSSearchDialog.SearchScope.OnlyThisSection"), SearchScopeEnum.SearchCurrentSection.ToStringRepresentation()));
                mDrpSearchScope.Width = new Unit(150);
                mDrpSearchScope.CssClass = "CMSSearchDialogSearchScopeDropDownList";
                mDrpSearchScope.ID = "drpSearchScope";
                mBtnSearch = new CMSButton();
                mBtnSearch.CausesValidation = false;
                mBtnSearch.ID = "btnSearch";
                mBtnSearch.Text = ResHelper.GetString("CMSSearchDialog.Go");
                mBtnSearch.CssClass = "CMSSearchDialogSearchButton";
                mBtnSearch.Click += StartSearch;
                mSearchPanel = new Panel();
                mSearchPanel.ID = "pnlSearch";
                mSearchPanel.DefaultButton = mBtnSearch.ID;

                EnsureChildControls();
            }
        }


        /// <summary>
        /// Raises the DoSearch event in case the text is entered.
        /// </summary>
        protected void StartSearch(object sender, EventArgs e)
        {
            if (Page != null)
            {
                mTxtSearchFor.Text = ValidationHelper.GetString(Page.Request.Params[mTxtSearchFor.UniqueID], mTxtSearchFor.Text);

                if (ShowSearchMode)
                {
                    mDrpSearchMode.SelectedValue = ValidationHelper.GetString(Page.Request.Params[mDrpSearchMode.UniqueID], mDrpSearchMode.SelectedValue);
                }

                if (ShowSearchScope)
                {
                    mDrpSearchScope.SelectedValue = ValidationHelper.GetString(Page.Request.Params[mDrpSearchScope.UniqueID], mDrpSearchScope.SelectedValue);
                }
            }

            if ((mTxtSearchFor.Text != null) && (mTxtSearchFor.Text.Trim() != ""))
            {
                if (null != DoSearch)
                {
                    DoSearch();
                }

                string url = RequestContext.CurrentURL;
                url = URLHelper.UpdateParameterInUrl(url, "searchtext", HttpUtility.UrlEncode(mTxtSearchFor.Text.Trim()));

                if (ShowSearchMode)
                {
                    url = URLHelper.UpdateParameterInUrl(url, "searchmode", mDrpSearchMode.SelectedValue);
                }

                if (ShowSearchScope)
                {
                    url = URLHelper.UpdateParameterInUrl(url, "searchscope", mDrpSearchScope.SelectedValue);
                }

                // Add custom query string data if are defined
                if (!String.IsNullOrEmpty(CustomQueryStringData))
                {
                    // Check whether querystring data starts with forbidden character, if so, remove it
                    if ((CustomQueryStringData.StartsWithCSafe("&")) || (CustomQueryStringData.StartsWithCSafe("?")))
                    {
                        CustomQueryStringData = CustomQueryStringData.Remove(0, 1);
                    }

                    // Add custom query to the url
                    url += "&" + CustomQueryStringData;
                }

                URLHelper.Redirect(url);
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return CMSControlsHelper.GetControlTagKey();
            }
        }
    }
}