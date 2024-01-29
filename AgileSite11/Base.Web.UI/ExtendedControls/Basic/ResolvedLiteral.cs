using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Literal control with value resolved for inline controls.
    /// </summary>
    [ToolboxItem(false)]
    public class ResolvedLiteral : PlaceHolder
    {
        private string mAllowedControls = null;


        /// <summary>
        /// List of allowed inline controls separated by semicolon.
        /// </summary>
        public string AllowedControls
        {
            get
            {
                return mAllowedControls;
            }
            set
            {
                mAllowedControls = value;
            }
        }


        /// <summary>
        /// Sets the literal text.
        /// </summary>
        public string Text
        {
            set
            {
                // Clear the current controls collection
                Controls.Clear();

                if (!string.IsNullOrEmpty(value))
                {
                    // Add the inner literal and resolve the controls
                    Controls.Add(new LiteralControl(value));

                    ControlsHelper.ResolveDynamicControls(this, AllowedControls);
                }
            }
        }
    }
}