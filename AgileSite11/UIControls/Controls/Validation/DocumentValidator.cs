using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for validation controls.
    /// </summary>
    public abstract class DocumentValidator : CMSUserControl
    {
        #region "Variables"

        private TreeNode mTreeNode = null;
        private string mCultureCode = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// URL of document to be validated
        /// </summary>
        public string Url
        {
            get;
            set;
        }


        /// <summary>
        /// Key to store validation result
        /// </summary>
        protected abstract string ResultKey
        {
            get;
        }


        /// <summary>
        /// Gets or sets validation result
        /// </summary>
        public virtual DataSet DataSource
        {
            get
            {
                return WindowHelper.GetItem(ResultKey) as DataSet;
            }
            set
            {
                WindowHelper.Add(ResultKey, value);
            }
        }


        /// <summary>
        /// Identifier of the edited document.
        /// </summary>
        public int NodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Preferred culture of the edited document.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return mCultureCode ?? (mCultureCode = LocalizationContext.PreferredCultureCode);
            }
            set
            {
                mCultureCode = value;
            }
        }


        /// <summary>
        /// Get edited document.
        /// </summary>
        public TreeNode Node
        {
            get
            {
                return mTreeNode ?? (mTreeNode = DocumentHelper.GetDocument(NodeID, CultureCode, true, null));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Basic on external data bound event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="sourceName">Action what is called</param>
        /// <param name="parameter">Parameter</param>
        /// <returns>Result object</returns>
        protected object GridExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName)
            {
                case "view":
                case "viewcss":
                    // Edit action                
                    {
                        DataRowView drv = (DataRowView)((GridViewRow)parameter).DataItem;

                        int line = ValidationHelper.GetInteger(DataHelper.GetDataRowViewValue(drv, "line"), 0);
                        int ch = ValidationHelper.GetInteger(DataHelper.GetDataRowViewValue(drv, "column"), 0);

                        string message = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(drv, "message"), "");
                        if (String.IsNullOrEmpty(message))
                        {
                            message = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(drv, "error"), "");
                        }

                        string source = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(drv, "url"), "");

                        var btn = ((CMSGridActionButton)sender);

                        string format = (sourceName == "viewcss" ? "css" : null);

                        btn.OnClientClick = GetViewSourceActionClick(line, ch, message, source, format);
                    }
                    break;
            }

            return parameter;
        }


        /// <summary>
        /// Gets the onclick script for the view source action
        /// </summary>
        protected string GetViewSourceActionClick()
        {
            return GetViewSourceActionClick(0, 0, null, null, null);
        }


        /// <summary>
        /// Gets the onclick script for the view source action
        /// </summary>
        /// <param name="line">Line to set</param>
        /// <param name="ch">Character (column) to set</param>
        /// <param name="message">Message</param>
        /// <param name="url">URL</param>
        /// <param name="format">Format of the code</param>
        protected string GetViewSourceActionClick(int line, int ch, string message, string url, string format)
        {
            if (String.IsNullOrEmpty(url))
            {
                url = Url;
            }

            // Encode the URL
            string query = QueryHelper.BuildQuery(
                "url", url,
                "line", line.ToString(),
                "ch", ch.ToString(),
                "message", message,
                "format", format
            );

            query += "&hash=" + QueryHelper.GetHash(URLHelper.UrlEncodeQueryString(query));

            string click = "modalDialog('" + ResolveUrl("~/CMSModules/Content/CMSDesk/Validation/ViewCode.aspx") + ScriptHelper.GetString(query, false) + "', 'ViewHTMLCode', '95%', '95%'); return false;";

            return click;
        }


        /// <summary>
        /// Load event
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((Node != null) && (Node.RequiresSSL == 1) && !RequestContext.IsSSL)
            {
                string requestUrl = RequestContext.URL.AbsoluteUri;
                const string HTTP_PREFIX = "http://";

                // Replace the protocol if necessary
                if (requestUrl.StartsWithCSafe(HTTP_PREFIX, true))
                {
                    requestUrl = "https://" + requestUrl.Substring(HTTP_PREFIX.Length);

                    // Redirect to the secure page
                    URLHelper.Redirect(requestUrl);
                }
            }
        }


        /// <summary>
        /// Initialize scripts
        /// </summary>
        protected void InitializeScripts()
        {
            ScriptHelper.RegisterJQuery(Page);
            ScriptHelper.RegisterDialogScript(Page);

            // Register validator functions
            ScriptHelper.RegisterScriptFile(Page, "Validation.js");
        }

        #endregion
    }
}
