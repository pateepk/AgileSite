using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Localization;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSEditModeButtonAdd.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSEditModeButtonAdd : WebControl
    {
        #region "Variables"

        private string mText;
        private string mPath;
        private string mClassName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Stop processing.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Custom caption.
        /// </summary>
        public string Text
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mText;
                }

                return DataHelper.GetNotEmpty(mText, ResHelper.GetString("CMSEditModeButtonAdd.AddNew"));
            }
            set
            {
                mText = value;
            }
        }


        /// <summary>
        /// Aliaspath of the parent document.
        /// </summary>
        public string Path
        {
            get
            {
                return DataHelper.GetNotEmpty(mPath, DocumentContext.CurrentAliasPath);
            }
            set
            {
                mPath = value;
            }
        }


        /// <summary>
        /// Document type in format cms.article that specifies the type of the document that should be created.
        /// </summary>
        public string ClassName
        {
            get
            {
                return ValidationHelper.GetString(mClassName, "");
            }
            set
            {
                mClassName = value;
            }
        }

        #endregion


        /// <summary>
        /// Render event.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (StopProcessing)
            {
                return;
            }

            // Render the control if is in edit mode
            if (PortalContext.ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive))
            {
                output.Write(GetHTML());
            }
        }


        /// <summary>
        /// Gets the HTML code of the control.
        /// </summary>
        public string GetHTML()
        {
            string code = String.Empty;

            var node = DocumentHelper.GetDocuments()
                            .Path(MacroResolver.ResolveCurrentPath(Path))
                            .OnCurrentSite()
                            .FirstObject;

            var dci = DataClassInfoProvider.GetDataClassInfo(ClassName);
            if ((node != null) && (dci != null))
            {
                string action;
                // Check allowed document type
                if (!DocumentHelper.IsDocumentTypeAllowed(node, dci.ClassID))
                {
                    action = "NotAllowed('child');";
                }
                // Check document permissions
                else if (!MembershipContext.AuthenticatedUser.IsAuthorizedToCreateNewDocument(node, dci.ClassName))
                {
                    action = "NotAllowed('new');";
                }
                else
                {
                    action = "NewDocument(" + node.NodeID + "," + dci.ClassID + ");";
                }

                var btnAdd = new CMSButton();
                btnAdd.Text = Text;
                btnAdd.OnClientClick = action + "; return false;";
                btnAdd.ButtonStyle = ButtonStyle.Default;

                var buttonsWriter = new StringWriter();
                btnAdd.RenderControl(new HtmlTextWriter(buttonsWriter));

                code = "<div class=\"cms-bootstrap\">" + buttonsWriter.GetStringBuilder() + "</div><div class=\"CMSEditModeButtonClear\"></div>";
            }
            return code;
        }
    }
}