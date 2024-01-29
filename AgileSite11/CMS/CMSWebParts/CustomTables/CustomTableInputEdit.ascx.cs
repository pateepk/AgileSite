using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using System;

public partial class CMSWebParts_CustomTables_CustomTableInputEdit : CMSAbstractWebPart
{
    #region Private properties
    private bool isSuccess = false;
    private DataClassInfo dci = null;

    #endregion

    #region Public properties

    /// <summary>
    /// Determine if permissions should be checked or not
    /// </summary>
    public bool CheckPermissions
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("CheckPermissions"), false);
        }
        set
        {
            SetValue("CheckPermissions", value);
        }
    }

    /// <summary>
    /// Gets or sets the custom table name
    /// </summary>
    public string CustomTableName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("CustomTableName"), "");
        }
        set
        {
            SetValue("CustomTableName", value);
        }
    }

    /// <summary>
    /// Alternative form to use
    /// </summary>
    public string AlternativeFormFullName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AlternativeFormFullName"), "");
        }
        set
        {
            SetValue("AlternativeFormFullName", value);
        }
    }

    /// <summary>
    /// Gets or sets the value on whether to use a colon after the label
    /// </summary>
    public bool UseColonBehindLabel
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("UseColonBehindLabel"), true);
        }
        set
        {
            SetValue("UseColonBehindLabel", value);
        }
    }

    /// <summary>
    /// Submit buttons css class
    /// </summary>
    public string SubmitButtonCssClass
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SubmitButtonCssClass"), "");
        }
        set
        {
            SetValue("SubmitButtonCssClass", value);
        }
    }

    /// <summary>
    /// Submit buttons text
    /// </summary>
    public string SubmitButtonText
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SubmitButtonText"), "");
        }
        set
        {
            SetValue("SubmitButtonText", value);
        }
    }

    /// <summary>
    /// Displays the form after submission
    /// </summary>
    public bool DisplayFormAfterSubmit
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("DisplayFormAfterSubmit"), false);
        }
        set
        {
            SetValue("DisplayFormAfterSubmit", value);
        }
    }

    /// <summary>
    /// Display a message after submit
    /// </summary>
    public bool DisplayMessageAfterSubmit
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("DisplayMessageAfterSubmit"), false);
        }
        set
        {
            SetValue("DisplayMessageAfterSubmit", value);
        }
    }

    /// <summary>
    /// Message text to display if form is being inserted
    /// </summary>
    public string InsertMessageText
    {
        get
        {
            return ValidationHelper.GetString(GetValue("InsertMessageText"), "");
        }
        set
        {
            SetValue("InsertMessageText", value);
        }
    }

    /// <summary>
    /// Message text to display if form is being updated.
    /// </summary>
    public string UpdateMessageText
    {
        get
        {
            return ValidationHelper.GetString(GetValue("UpdateMessageText"), "");
        }
        set
        {
            SetValue("UpdateMessageText", value);
        }
    }

    /// <summary>
    /// Redirect after a submit
    /// </summary>
    public bool RedirectAfterSubmit
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("RedirectAfterSubmit"), false);
        }
        set
        {
            SetValue("RedirectAfterSubmit", value);
        }
    }

    /// <summary>
    /// Redirect URL to use
    /// </summary>
    public string RedirectUrl
    {
        get
        {
            return ValidationHelper.GetString(GetValue("RedirectUrl"), "");
        }
        set
        {
            SetValue("RedirectUrl", value);
        }
    }

    /// <summary>
    /// Gets/Sets the selected ItemID of the custom table record
    /// </summary>
    public int SelectedItemID
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("SelectedItemID"), 0);
        }
        set
        {
            SetValue("SelectedItemID", value);
        }
    }

    #endregion

    #region "Methods"
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        litMessage.Visible = (DisplayMessageAfterSubmit && isSuccess);

        if (RedirectAfterSubmit && isSuccess && IsPostBack)
        {
            URLHelper.Redirect(RedirectUrl);
        }

        if (isSuccess && IsPostBack)
        {
            customTableForm.Visible = DisplayFormAfterSubmit;
        }
    }

    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (StopProcessing)
        {
            customTableForm.StopProcessing = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(CustomTableName))
            {
                // Get form info
                dci = DataClassInfoProvider.GetDataClassInfo(CustomTableName);
                customTableForm.CustomTableId = dci.ClassID;
                
                // setting the selected item for editing
                if (SelectedItemID > 0)
                {
                    customTableForm.ItemID = SelectedItemID;
                    litMessage.Text = UpdateMessageText;
                }
                else
                {
                    litMessage.Text = InsertMessageText;
                }
            }

            customTableForm.OnBeforeSave += customTableForm_OnBeforeSave;
            customTableForm.OnAfterSave += customTableForm_OnAfterSave;
            customTableForm.ShowPrivateFields = true;
            customTableForm.AlternativeFormFullName = AlternativeFormFullName;
            customTableForm.SubmitButton.CssClass = SubmitButtonCssClass;
            customTableForm.SubmitButton.ResourceString = SubmitButtonText;
            customTableForm.SubmitButton.Text = SubmitButtonText;
        }
    }

    /// <summary>
    /// Allow user to reload data if needed.
    /// </summary>
    public override void ReloadData()
    {
        SetupControl();
    }

    private void customTableForm_OnBeforeSave(object sender, EventArgs e)
    {
        if (CheckPermissions)
        {
            // If editing item
            if (SelectedItemID > 0)
            {
                // Check 'Modify' permission
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.customtables", "Modify") &&
                    !MembershipContext.AuthenticatedUser.IsAuthorizedPerClassName(dci.ClassName, "Modify"))
                {
                    // Show error message
                    customTableForm.MessagesPlaceHolder.ClearLabels();
                    customTableForm.ShowError(String.Format(GetString("customtable.permissiondenied.modify"), dci.ClassName), null, null);
                    customTableForm.StopProcessing = true;
                }
            }
            else
            {
                // Check 'Create' permission
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.customtables", "Create") &&
                    !MembershipContext.AuthenticatedUser.IsAuthorizedPerClassName(dci.ClassName, "Create"))
                {
                    // Show error message
                    customTableForm.MessagesPlaceHolder.ClearLabels();
                    customTableForm.ShowError(String.Format(GetString("customtable.permissiondenied.create"), dci.ClassName), null, null);
                    customTableForm.StopProcessing = true;
                }
            }
        }
    }
        
    /// <summary>
    /// After successful save, set true value
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void customTableForm_OnAfterSave(object sender, EventArgs e)
    {
        isSuccess = true;
    }

    #endregion
}
