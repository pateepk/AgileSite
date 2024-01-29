//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using CMS.Controls;
//using CMS.GlobalHelper;
//using CMS.Helpers;
//using CMS.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUBGStatementIDFilter : CMSAbstractQueryFilterControl
    {

        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            string statementID = SqlHelper.EscapeQuotes("StatementID");
            if (!string.IsNullOrWhiteSpace(statementID))
            {
                SessionHelper.SetValue("StatementID", statementID);
            }

            //init
            SetWhere();

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

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetWhere()
        {
            string where = "1=0";

            Object o = SessionHelper.GetValue("StatementID");
            string statementID = string.Empty;
            if (o != null)
            {
                statementID = Convert.ToString(o);
            }

            if (!string.IsNullOrWhiteSpace(statementID))
            {
                where = string.Format("StatementID = '{0}'", statementID);
            }

            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }

    }
}