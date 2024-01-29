using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;


namespace ExcellaLite.Account
{
    public partial class AccountUsers : BasePage
    {
        protected bool IsUpdateAction = false;

        public struct Columns
        {
            public const string EditLink = "EditLink";
            public const string AdminOrCust = "AdminOrCust";
        }

        protected DRspUsers_Select allUsers = null;
        protected int EditUserID = 0;
        protected void Page_Load()
        {
            this.EnableViewState = false;
            Utils.DisablePageCaching();
            EditUserID = Utils.getQueryString("EditUserID").ToInt();
            IsUpdateAction = (EditUserID > 0);

            // admin only
            Repeater1.DataSource = null;
            if ((user.isLogin) && (user.isUserAdministrator))
            {

                allUsers = SQLData.spUsers_GetByKeywords("", "");

                DataTable dt = allUsers.DataSource;
                dt.Columns.Add(Columns.EditLink);
                dt.Columns.Add(Columns.AdminOrCust);


                for (int i = 0; i < allUsers.Count; i++)
                {
                    dt.Rows[i][Columns.EditLink] = "";
                    if (Utils.IsRoleIDsHasAdmin(allUsers.RoleIDs(i)))
                    {
                        dt.Rows[i][Columns.AdminOrCust] = "Admin";
                    }
                    else
                    {
                        dt.Rows[i][Columns.AdminOrCust] = allUsers.CustNum(i);
                    }

                    if ((!Page.IsPostBack) && (EditUserID > 0) && (allUsers.UserID(i) == EditUserID))
                    {
                         FullName.Text = allUsers.FullName(i);
                         LoginID.Text = allUsers.LoginID(i);
                         Email.Text = allUsers.Email(i);
                         Phone.Text = allUsers.Phone(i);
                         CustNum.Text = allUsers.CustNum(i);
                         IsAdministrator.Checked = Utils.IsRoleIDsHasAdmin(allUsers.RoleIDs(i));
                         IsActive.Checked = allUsers.IsActive(i);
                         BackdooorGUID.Text = allUsers.BackdoorGUID(i);
                    }

                }

                Repeater1.DataSource = dt;
            }
            else
            {
                Utils.responseRedirect("/Default.aspx", true);
            }

            Repeater1.DataBind();

        }

        protected void CancelEntry_Click(object sender, EventArgs e)
        {
            Utils.responseRedirect("/Account/AccountUsers", true);
        }

        protected void SaveNew_Click(object sender, EventArgs e)
        {
            if (EditUserID == 0)
            {
                if (ExternalPassword.Text.Length == 0)
                {
                    ExternalPassword.Text = Guid.NewGuid().ToString();
                }
                if (LoginID.Text.Length > 0)
                {
                    DRspUser_Insert np = SQLData.spUser_Insert(
                         user.userID
                        , FullName.Text
                        , LoginID.Text
                        , Email.Text
                        , Phone.Text
                        , true // adding new user, active it
                        , ExternalPassword.Text
                        , CustNum.Text
                        , IsAdministrator.Checked
                    );
                    if ((np.Count > 0) && (np.UserID(0) > 0))
                    {
                        Utils.responseRedirect("/Account/AccountUsers", true);
                    }
                }
            }
            else
            {
                DRspUser_Update du = SQLData.spUser_Update(
                        user.userID
                        , EditUserID
                        , FullName.Text
                        , LoginID.Text
                        , Email.Text
                        , IsActive.Checked
                        , false
                        , String.Empty // wont be update if above is false
                        , Phone.Text
                        , CustNum.Text
                        , BackdooorGUID.Text
                );
                if ((du.Count > 0) && (du.RowUpdated(0) > 0))
                {

                    if (IsAdministrator.Checked)
                    {
                        DRspUsersRoles_InsertByLoginID r1 = SQLData.spUsersRoles_InsertByLoginID(LoginID.Text, (int)roleIDs.Administrator);
                    }
                    else
                    {
                        DRspUsersRoles_DeleteByLoginID d1 = SQLData.spUsersRoles_DeleteByLoginID(LoginID.Text, (int)roleIDs.Administrator);
                    }

                    Utils.responseRedirect("/Account/AccountUsers", true);
                }

            }
        }



    }
}