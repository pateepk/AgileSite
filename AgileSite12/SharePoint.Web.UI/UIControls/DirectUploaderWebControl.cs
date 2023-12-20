using System;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.UIControls;

namespace CMS.SharePoint.Web.UI
{
    /// <summary>
    /// This control is only a means of having Uploader in the place of a button.
    /// None of the <see cref="CMSWebControl"/> properties are taken into account use only <see cref="Uploader"/>, <see cref="JavaScriptModuleName"/>, <see cref="JavascriptModuleParameters"/>,
    /// <see cref="Enabled"/>
    /// </summary>
    internal class DirectUploaderWebControl : CMSWebControl
    {
        private DirectFileUploader mUploader;


        /// <summary>
        /// Gets the actual uploader that will be rendered on the page
        /// </summary>
        public DirectFileUploader Uploader
        {
            get
            {
                return mUploader;
            }
        }


        /// <summary>
        /// Gets or sets ID/Name of the JavaScriptModule to be registered on the page.
        /// The Upload handler referenced in <see cref="Uploader"/> might need a javascript module.
        /// </summary>
        public string JavaScriptModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// <see cref="JavaScriptModuleName"/>'s server data object.
        /// </summary>
        public object JavascriptModuleParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the enabled flag of the <see cref="Uploader"/>
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return Uploader.Enabled;
            }
            set
            {
                Uploader.Enabled = value;
            }
        }


        /// <summary>
        /// The only constructor that should be used. Initializes the control and creates child controls.
        /// </summary>
        /// <param name="Page">Page on which the controls is located</param>
        public DirectUploaderWebControl(CMSPage Page)
        {
            this.Page = Page;
            EnsureChildControls();
        }


        /// <summary>
        /// Creates child controls if they have not been yet created.
        /// </summary>
        protected override void EnsureChildControls()
        {
            if (mUploader == null)
            {
                mUploader = ((CMSPage)Page).LoadUserControl("~/CMSModules/Content/Controls/Attachments/DirectFileUploader/DirectFileUploader.ascx") as DirectFileUploader;
                Controls.Add(mUploader);
            }
            base.EnsureChildControls();
        }


        /// <summary>
        /// PreRender event.
        /// Registers javascript module given by <see cref="JavaScriptModuleName"/> with <see cref="JavascriptModuleParameters"/> as parameters if required.
        /// </summary>
        /// <param name="e">Ignored</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Enabled && Uploader.Visible && !String.IsNullOrEmpty(JavaScriptModuleName))
            {
                ScriptHelper.RegisterModule(Page, JavaScriptModuleName, JavascriptModuleParameters);
            }
        }


        /// <summary>
        /// Renders the <see cref="Uploader"/> if visible.
        /// </summary>
        /// <param name="writer">Output writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Uploader.Visible)
            {
                mUploader.RenderControl(writer);
            }
        }
    }
}
