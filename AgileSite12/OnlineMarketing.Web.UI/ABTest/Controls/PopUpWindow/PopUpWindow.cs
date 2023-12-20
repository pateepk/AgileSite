using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// This class represents popup window with item listing and actions in footer.
    /// </summary>
    internal class PopUpWindow : WebControl
    {
        /// <summary>
        /// Gets or sets the header text of the popup window.
        /// </summary>
        public string HeaderText { get; set; }


        /// <summary>
        /// Gets or sets the content header text.
        /// </summary>
        public string ContentHeaderText { get; set; }


        /// <summary>
        /// Control to be rendered in the middle part of the window.
        /// </summary>
        public Control ContentControl { get; set; }


        /// <summary>
        /// Controls to be rendered in the footer of the window.
        /// If left empty footer is omitted.
        /// </summary>
        public IList<Control> FooterControls { get; } = new List<Control>();


        /// <summary>
        /// Gets or sets position of the window. 
        /// </summary>
        public PopUpWindowPosition Position { get; set; }


        /// <summary>
        /// If true then a triangle is rendered to show trigger point of the window.
        /// Position of the triangle is affected by <see cref="Position"/>.
        /// </summary>
        public bool DisplayTriangle { get; set; }


        /// <summary>
        /// Gets or sets the color theme of the dialog.
        /// </summary>
        public PopUpWindowColorTheme ColorTheme { get; set; }


        /// <summary>
        /// Creates an instance of <see cref="PopUpWindow"/>.
        /// </summary>
        /// <remarks>
        /// To use click-away and closing functionality of the window you may use 'CMS.OnlineMarketing/PopUpWindow' helper module.
        /// </remarks>
        public PopUpWindow()
            : base("kentico-pop-up-container")
        {
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeAttributes();

            InitializeContent();

            InitializeFooter();

            ScriptHelper.RegisterWebComponentsScript(Page);
            InitializeJs();
        }


        private void InitializeAttributes()
        {
            Attributes.Add("header-title", HeaderText);
            Attributes.Add("position", Position.ToString().ToLowerInvariant());
            if (DisplayTriangle)
            {
                Attributes.Add("triangle", "");
            }
        }


        private void InitializeContent()
        {
            var contentSlot = new Panel();
            contentSlot.Attributes.Add("slot", "pop-up-content");

            var contentHeader = new HtmlGenericControl("div");
            contentHeader.Attributes.Add("class", "ktc-pop-up-content-header");
            contentHeader.InnerText = ContentHeaderText;

            contentSlot.Controls.Add(contentHeader);

            if (ContentControl != null)
            {
                contentSlot.Controls.Add(ContentControl);
            }

            Controls.Add(contentSlot);
        }


        private void InitializeFooter()
        {
            if (FooterControls.Count == 0)
            {
                return;
            }

            var footer = new Panel();
            footer.AddCssClass("ktc-popup-footer");
            footer.Attributes.Add("slot", "pop-up-footer");

            foreach (var fc in FooterControls)
            {
                footer.Controls.Add(fc);
            }

            Controls.Add(footer);
        }


        private void InitializeJs()
        {
            ScriptHelper.RegisterStartupScript(this, typeof(string), Guid.NewGuid().ToString(), @"
(function(document) {
    var thisWindow = document.getElementById('" + ClientID + @"');
    thisWindow.theme = '" + ColorTheme.ToString().ToLowerInvariant() + @"';
})(document);", true);
        }
    }
}
