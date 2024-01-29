using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Template control for displaying the link in databound controls.
    /// </summary>
    public class LinkItemTemplate : ITemplate
    {
        private string mNameID = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mName">Control name</param>
        public LinkItemTemplate(string mName)
        {
            mNameID = mName;
        }


        #region "ITemplate Members"

        /// <summary>
        /// InstantiateIn.
        /// </summary>
        public void InstantiateIn(Control container)
        {
            Control ctrl = null;
            ctrl = new HyperLink();
            ctrl.ID = mNameID;
            container.Controls.Add(ctrl);
        }

        #endregion
    }
}