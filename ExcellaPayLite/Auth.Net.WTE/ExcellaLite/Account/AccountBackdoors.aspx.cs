using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;


namespace ExcellaLite.Account
{
    public partial class AccountBackdoors : BasePage
    {

        public struct Columns
        {
            public const string BackdoorLink = "BackdoorLink";
            public const string SendEmailButton = "SendEmailButton";
        }

        public struct Actions
        {
            public const string SendEmail = "SendEmail";
        }

        protected DRspUsers_Select allUsers = null;
        protected DRspUser_GetByID ForUser = null;
        protected string action = "";

        protected void Page_Load()
        {
            action = Utils.getQueryString("action").ToString();

            if ((user.isLogin) && (user.isUserAdministrator))
            {
                if (!IsPostBack)
                {
                    Repeater1.DataSource = null;
                    radEMail1.Visible = false;
                    radEMail2.Visible = false;
                    radEMail3.Visible = false;
                    phEmailForm.Visible = false;

                    // for sending email for
                    if (action == Actions.SendEmail)
                    {
                        int ForUserID = Utils.getQueryString("UserID").ToInt();
                        if (ForUserID > 0)
                        {
                            ForUser = SQLData.spUser_GetByID(ForUserID);
                            if (ForUser.Count > 0)
                            {
                                phEmailForm.Visible = true;
                                LiteralFullName.Text = ForUser.FullName(0);
                                TextBoxEmailTemplate.Text = Utils.getAppSettings(SV.AppSettings.EmailTemplate_BackdoorLink).ToString();
                                // same user
                                if (user.userID == ForUser.UserID(0))
                                {
                                    radEMail1.Visible = true;
                                    radEMail1.Enabled = true;
                                    radEMail1.Checked = true;
                                    LiteralEmail1.Text = ForUser.Email(0);

                                    radEMail2.Visible = false;
                                    radEMail3.Visible = false;
                                }
                                else
                                {
                                    radEMail1.Visible = true;
                                    radEMail2.Visible = true;
                                    radEMail3.Visible = true;

                                    radEMail1.Enabled = true;
                                    radEMail1.Checked = true;
                                    LiteralEmail1.Text = String.Format("Send to {0} (user)", ForUser.Email(0));

                                    radEMail2.Enabled = true;
                                    radEMail2.Checked = false;
                                    LiteralEmail2.Text = String.Format("Send to {0} (me)", user.Email);

                                    radEMail3.Enabled = true;
                                    radEMail3.Checked = false;
                                    LiteralEmail3.Text = String.Format("Send to {0} (user) and CC {1} (me)", ForUser.Email(0), user.Email);

                                }
                            }
                            else
                            {
                                ForUser = null;
                            }

                        }
                    }


                    allUsers = SQLData.spUsers_GetByKeywords("", "");

                    DataTable dt = allUsers.DataSource;
                    dt.Columns.Add(Columns.BackdoorLink);
                    dt.Columns.Add(Columns.SendEmailButton);

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        dt.Rows[i][Columns.BackdoorLink] = Utils.ResolveFullPathURL("/Account/Login?guid=" + allUsers.BackdoorGUID(i));
                        dt.Rows[i][Columns.SendEmailButton] = String.Format("<input type=\"button\" class=\"ui-button ui-widget ui-corner-all\" onclick=\"AccountBackdoorSendEmail({0});\" value=\"Send Email\">", allUsers.UserID(i));
                    }

                    Repeater1.DataSource = dt;
                }
            }
            else
            {
                Utils.responseRedirect("Default.aspx");
            }

            Repeater1.DataBind();

        }

        protected void SendNow_Click(object sender, EventArgs e)
        {
            int ForUserID = Utils.getQueryString("UserID").ToInt();
            if (ForUserID > 0)
            {
                ForUser = SQLData.spUser_GetByID(ForUserID);
                if (ForUser.Count > 0)
                {
                    phEmailForm.Visible = true;
                    LiteralFullName.Text = ForUser.FullName(0);
                    string EmailBody = "";
                    string EmailTo = "";
                    string EmailSubject = "WTE Excella Lite Login Information";
                    bool IsOK = true;
                    try
                    {
                        EmailBody = String.Format(TextBoxEmailTemplate.Text, ForUser.FullName(0), Utils.ResolveFullPathURL("/Account/Login?guid=" + ForUser.BackdoorGUID(0)));
                    }
                    catch (Exception)
                    {
                        IsOK = false;
                    }

                    if (IsOK)
                    {
                        EmailManager email = new EmailManager();
                        if(radEMail1.Checked)
                        {
                            EmailTo = ForUser.Email(0);
                        } else 
                        {
                            EmailTo = user.Email;
                        }
                        if(radEMail3.Checked)
                        {
                            email.AddEmailAddressCC(user.Email);
                        }

                        long EmailID = email.SendEmail(EmailTo, EmailSubject, EmailBody);
                        if (EmailID > 0)
                        {
                            LiteralEmailMessage.Text = String.Format("<span><b>Email sent!</b></span>");
                            BtnCancel.Text = "Done";
                            SendNow.Text = "Send Again";
                        }
                        else
                        {
                            LiteralEmailMessage.Text = String.Format("<span style=\"color:red;\"><b>Error sending email! Please contact administrator.</b></span>");
                        }

                    } else
                    {
                        LiteralEmailMessage.Text = String.Format("<span style=\"color:blue;\"><b>Error inserting values in the template. Make sure you did not modify the token placeholders.</b></span>");
                    }
                    
                }
            }
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            Utils.responseRedirect("Account/AccountBackdoors.aspx");
        }



    }
}