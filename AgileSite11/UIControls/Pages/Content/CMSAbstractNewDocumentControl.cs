using System;
using System.Linq;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// NewDocumentControl abstract class
    /// </summary>
    public abstract class CMSAbstractNewDocumentControl : CMSUserControl
    {
        #region "Variables"

        private string mCaption;
        private string mNoDataMessage;
        private int mParentNodeId;
        private int mHeadingLevel = 4;
        private string mParentCulture;
        private TreeProvider mTree;
        private BaseInfo mParentNode;

        private DialogConfiguration mConfig;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the selected item ID
        /// </summary>
        public virtual int SelectedItemID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the configuration for Copy and Move dialog.
        /// </summary>
        private DialogConfiguration Config
        {
            get
            {
                if (mConfig == null)
                {
                    mConfig = new DialogConfiguration();
                    mConfig.ContentSelectedSite = SiteContext.CurrentSiteName;
                    mConfig.OutputFormat = OutputFormatEnum.Custom;
                    mConfig.SelectableContent = SelectableContentEnum.AllContent;
                    mConfig.HideAttachments = false;
                }
                return mConfig;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether current control is in dialog page
        /// </summary>
        protected bool IsInDialog
        {
            get
            {
                CMSContentPage page = Page as CMSContentPage;
                return ((page != null) && (page.RequiresDialog));
            }
        }


        /// <summary>
        /// ID of the TreeNode which the document types will be offered for.
        /// </summary>
        public int ParentNodeID
        {
            get
            {
                return mParentNodeId;
            }
            set
            {
                mParentNodeId = value;
                mParentNode = null;
            }
        }


        /// <summary>
        /// Culture of the parent TreeNode which will be used for new document.
        /// </summary>
        public string ParentCulture
        {
            get
            {
                return mParentCulture;
            }
            set
            {
                mParentCulture = value;
                mParentNode = null;
            }
        }


        /// <summary>
        /// TreeNode which the document types will be offered for.
        /// </summary>
        public virtual TreeNode ParentNode
        {
            get
            {
                return (TreeNode)InfoHelper.EnsureInfo(ref mParentNode, () => Tree.SelectNodes().TopN(1).WhereEquals("NodeID", mParentNodeId).CombineWithAnyCulture().Published(false).FirstOrDefault());
            }
            set
            {
                mParentNode = value;

                mParentNodeId = 0;
                if (value != null)
                {
                    mParentNodeId = value.NodeID;
                }
            }
        }


        /// <summary>
        /// TreeProvider object to be used for nodes handling.
        /// </summary>
        public TreeProvider Tree
        {
            get
            {
                return mTree ?? (mTree = new TreeProvider(MembershipContext.AuthenticatedUser));
            }
            set
            {
                mTree = value;
            }
        }


        /// <summary>
        /// Indicates whether 'Link an existing document' may be offered.
        /// </summary>
        public bool AllowNewLink
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether 'New AB test page variant' may be offered.
        /// </summary>
        public bool AllowNewABTest
        {
            get;
            set;
        }


        /// <summary>
        /// Title text.
        /// </summary>
        public string Caption
        {
            get
            {
                return mCaption ?? (mCaption = GetString("Content.NewInfo"));
            }
            set
            {
                mCaption = value;
            }
        }


        /// <summary>
        /// Gets and sets title level. By default set to 4, i.e. generates h4.
        /// </summary>
        public int HeadingLevel
        {
            get
            {
                return mHeadingLevel;
            }
            set
            {
                mHeadingLevel = value;
            }
        }


        /// <summary>
        /// Message to be displayed when no applicable document type found.
        /// </summary>
        public string NoDataMessage
        {
            get
            {
                return mNoDataMessage ?? (mNoDataMessage = GetString("Content.NoAllowedChildDocuments"));
            }
            set
            {
                mNoDataMessage = value;
            }
        }


        /// <summary>
        /// Selection link URL.
        /// </summary>
        public string SelectionUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Selection link URL for product document types.
        /// </summary>
        public string ProductSelectionUrl
        {
            get;
            set;
        }


        /// <summary>
        /// URL to redirect to when selecting 'New A/B test page variant'.
        /// </summary>
        public string NewVariantUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition used to restrict listed document types.
        /// </summary>
        public string Where
        {
            get;
            set;
        }


        /// <summary>
        /// Control automatically selects document type, when there are no other options (+ 'new A/B test' and 'new linked document' links are disabled).
        /// </summary>
        public bool RedirectWhenNoChoice
        {
            get;
            set;
        }


        /// <summary>
        /// Script called when user selects a document type.
        /// </summary>
        public string ClientTypeClick
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            int classId = QueryHelper.GetInteger("classid", 0);
            if (classId > 0)
            {
                // Redirect to specified document type
                if (!string.IsNullOrEmpty(SelectionUrl))
                {
                    URLHelper.Redirect(UrlResolver.ResolveUrl(GetSelectionUrl(classId)));
                }
            }


            // Register progress and dialog scripts
            ScriptHelper.RegisterLoader(Page);
            ScriptHelper.RegisterDialogScript(Page);

            base.OnLoad(e);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public CMSAbstractNewDocumentControl()
        {
            AllowNewABTest = true;
            AllowNewLink = true;
        }


        /// <summary>
        /// Gets the selected item URL
        /// </summary>
        /// <param name="classId">Class ID</param>
        public string GetSelectionUrl(int classId)
        {
            string baseUrl = SelectionUrl;

            // Get document type specific URL
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(classId);
            if (dci != null)
            {
                if (dci.ClassIsProduct && !string.IsNullOrEmpty(ProductSelectionUrl))
                {
                    baseUrl = ProductSelectionUrl;
                }

                // Check if new page URL is set
                if (!string.IsNullOrEmpty(dci.ClassNewPageURL))
                {
                    baseUrl = URLHelper.AppendQuery(ResolveUrl(dci.ClassNewPageURL), RequestContext.CurrentQueryString);
                }
            }

            // Add query string parameters
            string url = URLHelper.UpdateParameterInUrl(baseUrl, "classid", classId.ToString());

            url = URLHelper.UpdateParameterInUrl(url, "parentnodeid", ParentNode.NodeID.ToString());
            if (!string.IsNullOrEmpty(ParentCulture))
            {
                url = URLHelper.AddParameterToUrl(url, "parentculture", ParentCulture);
            }

            return url;
        }

        #endregion


        #region "Dialog handling"

        /// <summary>
        /// Returns Correct URL of the copy or move dialog.
        /// </summary>
        /// <param name="currentNodeId">ID Of the node to be copied or moved</param>
        public string GetLinkDialogUrl(int currentNodeId)
        {
            Config.CustomFormatCode = "linkdoc";
            string url = CMSDialogHelper.GetDialogUrl(Config, false, false, null, false);

            // Prepare url for link dialog
            url = URLHelper.RemoveParameterFromUrl(url, "hash");
            url = URLHelper.AddParameterToUrl(url, "sourcenodeids", currentNodeId.ToString());
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

            return url;
        }

        #endregion
    }
}
