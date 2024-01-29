using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.SiteProvider;

using CMS.PortalEngine.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.OnlineForms;



namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUCustomTableForm : CMSAbstractWebPart
    {
        #region "Properties"
        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CustomTable
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CustomTable"), "");
            }
            set
            {
                this.SetValue("CustomTable", value);
            }
        }

        /// <summary>
        /// Gets or sets the URL that the cancel button should redirect to.
        /// </summary>
        public string CancelURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CancelURL"), "");
            }
            set
            {
                this.SetValue("CancelURL", value);
            }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetupControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {

                //Item ID
                int key = 0;
                string customTable = CustomTable; //Code name of custom table
                if (QueryHelper.GetString("action", "").ToLower() == "edit")
                {
                    key = QueryHelper.GetInteger("RecordKey", 0); //Get query parameter from URL
                }

                //Definition of custom table in this case
                DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(customTable);

                custTableForm.CustomTableId = dataClassInfo.ClassID;
                //if we want an alternative form then add this in
                //custTableForm.AlternativeFormFullName = "customtable.SampleTable.test";
                custTableForm.SiteName = SiteContext.CurrentSite.SiteName;
                custTableForm.ItemID = key;
                custTableForm.ShowPrivateFields = true;

                if (key == 0)
                {
                    custTableForm.Mode = FormModeEnum.Insert;
                    //custTableForm.AddInformation("Fill in fields and click OK to add.", "<br/>");
                }
                else
                {
                    //custTableForm.AddInformation("Update information and click OK to update.", "<br/>");
                    custTableForm.Mode = FormModeEnum.Update;

                    // Custom code for modification of field    
                    custTableForm.OnAfterDataLoad += new EventHandler(custTableForm_OnAfterDataLoad);
                }
            }
        }

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (StopProcessing)
            {
                custTableForm.StopProcessing = true;
            }
            else
            {
                custTableForm.StopProcessing = false;
                if (custTableForm.Mode == FormModeEnum.Insert)
                {
                    // custTableForm.AddInformation("<font color=\"blue\">Fill in fields and click OK to add.</font>", "<br/>");
                }
                else
                {
                    custTableForm.AddInformation("<font color=\"blue\">Update information and click OK to update.</font>", "<br/>");
                }

                custTableForm.OnAfterSave += new EventHandler(custTableForm_OnAfterSave);

                LocalizedButton cancelButton = new LocalizedButton
                {
                    ID = "btnCancel",
                    Text = ResHelper.GetString("general.cancel"),
                    CssClass = custTableForm.FormButtonCssClass,
                    CausesValidation = false,
                    EnableViewState = false,
                    OnClientClick = string.Format("document.location.href='{0}';return false;", CancelURL)
                };


                FormSubmitButton submitButton = custTableForm.FindControl(custTableForm.SubmitButton.ID) as FormSubmitButton;
                if (submitButton != null)
                {
                    submitButton.Parent.Controls.AddAt(submitButton.Parent.Controls.IndexOf(submitButton) + 1, new LiteralControl("&nbsp;"));
                    submitButton.Parent.Controls.AddAt(submitButton.Parent.Controls.IndexOf(submitButton) + 2, cancelButton);
                }
            }
        }

        //in case there is an issue with alternate forms I saw this post
        //http://devnet.kentico.com/Forums/f45/fp7/t25714/Returning-Wrong-Form.aspx
        //public override void OnContentLoaded()
        //{
        //    base.OnContentLoaded();
        //    custTableForm.AlternativeFormFullName = "customtable.SampleTable.test";
        //}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            /*
            foreach (EditingFormControl ctrl1 in custTableForm.FieldEditingControls.Values)
            {
                lblMsg.Text += ctrl1.ID + ", ";
            }
            */

            if (custTableForm.FieldEditingControls != null)
            {
                //force site id to be current site id and disabled
                EditingFormControl ctrl = custTableForm.FieldEditingControls["siteid"] as EditingFormControl;
                if (ctrl != null)
                {
                    // Replace value of NameOfField field with value of Column2
                    ctrl.Value = SiteContext.CurrentSiteID;
                    ctrl.Enabled = false;
                }
            }
            else
            {
                lblMsg.Text = "You do not have permission to edit this item.";
            }
        }

        void custTableForm_OnAfterSave(object sender, EventArgs e)
        {
            if (custTableForm.Mode == FormModeEnum.Insert)
            {
                custTableForm.AddInformation("<font color=\"green\">Record added.</font>", "<br/>");
            }
            else
            {
                custTableForm.AddInformation("<font color=\"green\">Record updated.</font>", "<br/>");
            }
        }

        void custTableForm_OnAfterDataLoad(object sender, EventArgs e)
        {
            if (custTableForm.FieldControls != null)
            {
                // Get form control information 
                EditingFormControl ctrl = custTableForm.FieldEditingControls["to"] as EditingFormControl;
                if (ctrl != null)
                {
                    // Replace value of NameOfField field with value of Column2
                    ctrl.Value = ValidationHelper.GetString(custTableForm.GetFieldValue("from"), "");
                }
            }
        }
    }
}