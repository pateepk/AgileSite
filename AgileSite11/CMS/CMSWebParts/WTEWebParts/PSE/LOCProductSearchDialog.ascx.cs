using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using System;
using System.Web.UI;

namespace CMSApp.CMSWebParts.WTEWebParts.PSE
{
    public partial class LOCProductSearchDialog : CMSAbstractQueryFilterControl
    {
        private Boolean mClearClicked = false;

        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            SetupControl();

            base.OnInit(e);
        }

        /// <summary>
        /// Pre render event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Set filter settings
            if (RequestHelper.IsPostBack())
            {
                SetFilter();
            }
            else
            {
                SetWhere();
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Setup the inner controls.
        /// </summary>
        private void SetupControl()
        {
            if (this.StopProcessing)
            {
                this.Visible = false;
            }
            else if (!RequestHelper.IsPostBack())
            {
                // ?
            }
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetFilter()
        {
            SetWhere();
            this.OrderBy = null;
            this.RaiseOnFilterChanged();
        }

        protected void btnClear_Click(Object sender, EventArgs e)
        {
            mClearClicked = true;

            txtProductName.Text = String.Empty;
            txtProductNumber.Text = String.Empty;
            txtSKU.Text = String.Empty;
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetWhere()
        {
            string where = "1 = 1";

            if (!Page.IsPostBack || mClearClicked)
            {
                //first time through or clear click do not return results
                where = "1 = 0";
            }

            bool hasQuery = false;
            if (!String.IsNullOrWhiteSpace(txtProductName.Text))
            {
                where += " AND p.Name LIKE '%" + txtProductName.Text.Trim() + "%' ";
                hasQuery = true;
            }

            if (!String.IsNullOrWhiteSpace(txtProductNumber.Text))
            {
                where += " AND p.ProductNum LIKE '%" + txtProductNumber.Text.Trim() + "%' ";
                hasQuery = true;
            }

            if (!String.IsNullOrWhiteSpace(txtSKU.Text))
            {
                where += " AND s.SKU LIKE '%" + txtSKU.Text.Trim() + "%' ";
                hasQuery = true;
            }

            if (!hasQuery)
            {
                where = "1 = 0 ";
            }

            // Set where condition
            this.WhereCondition = where;

            //lblMessage.Text = where;
        }
    }
}