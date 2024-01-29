using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.IO;
using CMS.PortalEngine;
using CMS.Base.Web.UI;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSEditModeButtonEditDelete class.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSEditModeButtonEditDelete : WebControl
    {
        #region "Variables"

        private string mEditText;
        private string mDeleteText;
        private string mPath;
        private string mCulture;
        private EditModeButtonEnum mEditMode = EditModeButtonEnum.Both;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether controls was added by automatic conversion virtual file -> physical file.
        /// Property is not used in code.
        /// </summary>
        public bool AddedAutomatically
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether control should be converted to the ##editdelete## macro within the conversion physical file -> virtual file.
        /// Property is not used in code.
        /// </summary>
        public bool AddedByMacro
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether parent setting should enable control. Used for transformations saved on file system.
        /// </summary>
        public bool EnableByParent 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Returns current UI culture.
        /// </summary>
        private string Culture
        {
            get
            {
                if (mCulture == null)
                {
                    CurrentUserInfo ui = MembershipContext.AuthenticatedUser;
                    if (ui != null)
                    {
                        mCulture = ui.PreferredUICultureCode;
                    }
                    else
                    {
                        mCulture = Thread.CurrentThread.CurrentUICulture.Name;
                    }
                }

                return mCulture;
            }
        }


        /// <summary>
        /// Custom caption.
        /// </summary>
        public string EditText
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mEditText;
                }

                if (mEditText == null)
                {
                    mEditText = ResHelper.GetString("general.edit", Culture);
                }
                return mEditText;
            }
            set
            {
                mEditText = value;
            }
        }


        /// <summary>
        /// Edit mode
        /// </summary>
        public EditModeButtonEnum EditMode
        {
            get
            {
                return mEditMode;
            }
            set
            {
                mEditMode = value;
            }
        }


        /// <summary>
        /// Custom caption.
        /// </summary>
        public string DeleteText
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mDeleteText;
                }

                if (mDeleteText == null)
                {
                    mDeleteText = ResHelper.GetString("general.delete", Culture);
                }

                return mDeleteText;
            }
            set
            {
                mDeleteText = value;
            }
        }


        /// <summary>
        /// Aliaspath of the edited/deleted document
        /// </summary>
        public string Path
        {
            get
            {
                return ValidationHelper.GetString(mPath, "");
            }
            set
            {
                mPath = value;
            }
        }


        /// <summary>
        /// Render.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            output.Write(PreBuild(Path));
        }

        #endregion


        /// <summary>
        /// PreBuild.
        /// </summary>
        public string PreBuild(string mPreBuildPath)
        {
            // Render the control if in edit mode
            if (PortalContext.ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive))
            {
                return Build(mPreBuildPath);
            }

            return "";
        }


        /// <summary>
        /// Build hyperlink.
        /// </summary>
        /// <returns>return hyperlink string</returns>
        public string Build(string mBuildPath)
        {
            StringBuilder result = new StringBuilder();

            bool enabled = true;
            if (EnableByParent)
            {
                Control ctrl = ControlsHelper.GetParentControl(this, typeof(ICMSBaseProperties));
                if (ctrl != null)
                {
                    enabled = ValidationHelper.GetBoolean(RequestStockHelper.GetItem("EditModeButtonEditDelete", ctrl.ClientID), false);
                }
            }

            result.Length = 0;
            int mNodeId = TreePathUtils.GetNodeIdByAliasPath(SiteContext.CurrentSiteName, mBuildPath);

            if ((mNodeId > 0) && (enabled))
            {
                var buttonsWriter = new StringWriter();
                
                if ((EditMode == EditModeButtonEnum.Both) || EditMode == EditModeButtonEnum.Edit)
                {
                    // Edit
                    var btnEdit = new CMSButton();
                    btnEdit.Text = EditText;
                    btnEdit.OnClientClick = "EditDocument(" + mNodeId + ", 'editform'); return false;";
                    btnEdit.ButtonStyle = ButtonStyle.Default;

                    btnEdit.RenderControl(new HtmlTextWriter(buttonsWriter));
                }

                if ((EditMode == EditModeButtonEnum.Both) || EditMode == EditModeButtonEnum.Delete)
                {
                    // Delete
                    var btnDelete = new CMSButton();
                    btnDelete.Text = DeleteText;
                    btnDelete.OnClientClick ="DeleteDocument(" + mNodeId + "); return false;";
                    btnDelete.ButtonStyle = ButtonStyle.Default;

                    btnDelete.RenderControl(new HtmlTextWriter(buttonsWriter));
                }

                var buttonsText = buttonsWriter.GetStringBuilder().ToString();
                if (!string.IsNullOrEmpty(buttonsText))
                {
                    result.Append("<div class=\"cms-bootstrap\">");
                    result.Append(buttonsText);
                    result.Append("</div>");
                }
                
                result.Append("<div class=\"CMSEditModeButtonClear\"></div>");
            }

            return result.ToString();
        }
    }
}