using System;
using System.Web.UI;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.OutputFilter;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for all web site (content) pages.
    /// </summary>
    public abstract class ContentPage : AbstractCMSPage
    {
        #region "Variables"

        /// <summary>
        /// Document base.
        /// </summary>
        protected DocumentBase mDocumentBase = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Document base.
        /// </summary>
        public DocumentBase DocumentBase
        {
            get
            {
                return mDocumentBase ?? (mDocumentBase = new DocumentBase(this));
            }
        }


        /// <summary>
        /// Current site.
        /// </summary>
        public SiteInfo CurrentSite
        {
            get
            {
                return DocumentBase.CurrentSite;
            }
        }


        /// <summary>
        /// Current user.
        /// </summary>
        public CurrentUserInfo CurrentUser
        {
            get
            {
                return DocumentBase.CurrentUser;
            }
        }


        /// <summary>
        /// Current page info.
        /// </summary>
        public PageInfo CurrentPage
        {
            get
            {
                return DocumentBase.CurrentPage;
            }
        }


        /// <summary>
        /// Page manager.
        /// </summary>
        public IPageManager PageManager
        {
            get
            {
                return DocumentBase.PageManager;
            }
            set
            {
                DocumentBase.PageManager = value;
            }
        }


        /// <summary>
        /// DocType.
        /// </summary>
        public string DocType
        {
            get
            {
                return DocumentBase.DocType;
            }
            set
            {
                DocumentBase.DocType = value;
            }
        }


        /// <summary>
        /// Body parameters.
        /// </summary>
        public string BodyParameters
        {
            get
            {
                return DocumentBase.BodyParameters;
            }
            set
            {
                DocumentBase.BodyParameters = value;
            }
        }


        /// <summary>
        /// Top HTML body node for custom HTML code.
        /// </summary>
        public string BodyScripts
        {
            get
            {
                return DocumentBase.BodyScripts;
            }
            set
            {
                DocumentBase.BodyScripts = value;
            }
        }


        /// <summary>
        /// Css file.
        /// </summary>
        public string CssFile
        {
            get
            {
                return DocumentBase.CssFile;
            }
            set
            {
                DocumentBase.CssFile = value;
            }
        }


        /// <summary>
        /// Extended tags.
        /// </summary>
        public string ExtendedTags
        {
            get
            {
                return DocumentBase.ExtendedTags;
            }
            set
            {
                DocumentBase.ExtendedTags = value;
            }
        }


        /// <summary>
        /// Header tags.
        /// </summary>
        public string HeaderTags
        {
            get
            {
                return DocumentBase.GetHeaderTags();
            }
        }


        /// <summary>
        /// Body class.
        /// </summary>
        public string BodyClass
        {
            get
            {
                return DocumentBase.BodyClass;
            }
            set
            {
                DocumentBase.BodyClass = value;
            }
        }


        /// <summary>
        /// Additional XML namespace to HTML tag.
        /// </summary>
        public string XmlNamespace
        {
            get
            {
                return DocumentBase.XmlNamespace;
            }
            set
            {
                DocumentBase.XmlNamespace = value;
            }
        }


        /// <summary>
        /// Description.
        /// </summary>
        public string Description
        {
            get
            {
                return DocumentBase.Description;
            }
            set
            {
                DocumentBase.Description = value;
            }
        }


        /// <summary>
        /// Key words.
        /// </summary>
        public string KeyWords
        {
            get
            {
                return DocumentBase.KeyWords;
            }
            set
            {
                DocumentBase.KeyWords = value;
            }
        }


        /// <summary>
        /// Title.
        /// </summary>
        public string PageTitle
        {
            get
            {
                return DocumentBase.Title;
            }
            set
            {
                DocumentBase.Title = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates new instance of <see cref="ContentPage"/>.
        /// </summary>
        protected ContentPage()
        {
            DocumentBase.SetCulture();
        }


        /// <summary>
        /// Gets document manager container
        /// </summary>
        protected override Control GetDocumentManagerContainer()
        {
            // Try to use portal manager
            if (PageManager != null)
            {
                return (Control)PageManager;
            }

            return base.GetDocumentManagerContainer();
        }


        /// <summary>
        /// PreInit event handler.
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnPreInit", null, 1);

            // Ensure document manager
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                EnsureDocumentManager = true;
            }

            base.OnPreInit(e);

            DocumentBase.PreInit();
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnInit", null, 1);

            // Set NodeID for document manager if current view mode is on-site edit
            if (PortalContext.ViewMode.IsEditLive())
            {
                PageInfo pi = DocumentContext.CurrentPageInfo;
                if (pi != null)
                {
                    DocumentManager.NodeID = pi.NodeID;
                }
            }

            base.OnInit(e);

            // Ensure content preferred culture
            EnsurePreferredCulture = true;

            // Initialize debug controls is master not found
            if (Master == null)
            {
                InitDebug();
            }
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnLoad", null, 1);

            base.OnLoad(e);

            DocumentBase.Load();
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnPreRender", null, 1);

            base.OnPreRender(e);

            DocumentBase.PreRender();
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            RequestDebug.LogRequestOperation("Render", null, 1);

            // Link component style sheets
            PortalHelper.AddComponentsCSS(DocumentBase.Page);

            // Filter output for special cases (page not found on End - request)
            if (OutputFilterContext.FilterResponseOnRender && (OutputFilterContext.CurrentFilter != null))
            {
                using (StringWriter sw = new StringWriter())
                {
                    using (HtmlTextWriter customWriter = new HtmlTextWriter(sw))
                    {
                        base.Render(customWriter);
                    }

                    // Apply output filter
                    string html = OutputFilterContext.CurrentFilter.FilterResponse(sw.ToString());

                    // Resolve substitutions
                    ResponseOutputFilter.ResolveSubstitutions(ref html);

                    writer.Write(html);
                }
                return;
            }

            base.Render(writer);
        }


        /// <summary>
        /// Unload event handler.
        /// </summary>
        protected override void OnUnload(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnUnload", null, 1);

            base.OnUnload(e);
        }


        /// <summary>
        /// Ensures the script manager on the page.
        /// </summary>
        public override ScriptManager EnsureScriptManager()
        {
            EnsureChildControls();

            // Try get current manager
            ScriptManager manager = ScriptManagerControl;
            if (manager == null)
            {
                // Get predefined managers container
                Control container = ManagersContainer;
                if (container == null)
                {
                    // Get portal manager as a container
                    container = (Control)PageManager;
                }

                if (container != null)
                {
                    // Create new script manager
                    manager = new ScriptManager();
                    manager.ID = "manScript";
                    manager.ScriptMode = ScriptMode.Release;

                    container.Controls.Add(manager);
                }
            }

            return manager;
        }

        #endregion
    }
}