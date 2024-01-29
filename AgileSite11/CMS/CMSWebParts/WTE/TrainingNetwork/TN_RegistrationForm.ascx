<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_RegistrationForm.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_RegistrationForm" %>

<%@ Register Src="TN_SecurityCode.ascx" TagName="SecurityCode" TagPrefix="cms" %>
<%@ Register Src="~/CMSModules/Membership/FormControls/Passwords/PasswordStrength.ascx" TagName="PasswordStrength" TagPrefix="cms" %>

<asp:Label ID="lblError" runat="server" ForeColor="red" EnableViewState="false" />
<asp:Label ID="lblText" runat="server" Visible="false" EnableViewState="false" />
<asp:Panel ID="pnlForm" runat="server" DefaultButton="btnOK">
    <div class="registration-form">
        <div class="form-horizontal">
            <div id="divTitle" runat="server">
                <h2>
                    <asp:Label ID="lblTitle" runat="server"></asp:Label></h2>
            </div>
            <div id="divUserName" runat="server" class="form-group">
                <div class="editing-form-label-cell">
                    <asp:Label CssClass="control-label" ID="lblUserName" runat="server" AssociatedControlID="txtEmail" EnableViewState="false" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox ID="txtUserName" runat="server" MaxLength="100" /><br />
                    <cms:CMSRequiredFieldValidator ID="rfvUserName" runat="server" ControlToValidate="txtUserName"
                        Display="Dynamic" EnableViewState="false" />
                </div>
            </div>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <asp:Label CssClass="control-label" ID="lblFirstName" runat="server" AssociatedControlID="txtFirstName" EnableViewState="false" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox ID="txtFirstName" EnableEncoding="true" runat="server" MaxLength="100" /><br />
                    <cms:CMSRequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName"
                        Display="Dynamic" EnableViewState="false" />
                </div>
            </div>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <asp:Label CssClass="control-label" ID="lblLastName" runat="server" AssociatedControlID="txtLastName" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox ID="txtLastName" EnableEncoding="true" runat="server" MaxLength="100" /><br />
                    <cms:CMSRequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName"
                        Display="Dynamic" EnableViewState="false" />
                </div>
            </div>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <asp:Label CssClass="control-label" ID="lblEmail" runat="server" AssociatedControlID="txtEmail" EnableViewState="false" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox ID="txtEmail" runat="server" MaxLength="100" />&nbsp;<asp:Label ID="lblEmailNote" runat="server"></asp:Label><br />
                    <cms:CMSRequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        Display="Dynamic" EnableViewState="false" />
                </div>
            </div>
            <div id="divUpdatePassword" runat="server" class="form-group">
                <div class="editing-form-value-cell editing-form-value-cell-offset">
                    <cms:CMSCheckBox ID="chkUpdatePassword" runat="server" ResourceString="" Text="Change Password" AutoPostBack="true" OnCheckedChanged="chkUpdatePassword_CheckedChanged" />
                </div>
            </div>
            <div id="divPassword" runat="server" class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblPassword" runat="server" EnableViewState="false" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:PasswordStrength runat="server" ID="txtPassword" ShowValidationOnNewLine="true" />
                </div>
            </div>
            <div id="divConfirmPassword" runat="server" class="form-group">
                <div class="editing-form-label-cell">
                    <asp:Label CssClass="control-label" ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword"
                        EnableViewState="false" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox ID="txtConfirmPassword" runat="server" TextMode="Password" MaxLength="100" /><br />
                    <cms:CMSRequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                        Display="Dynamic" EnableViewState="false" />
                </div>
            </div>
            <div id="divEnabledUser" runat="server" class="form-group">
                <div class="editing-form-value-cell editing-form-value-cell-offset">
                    <cms:CMSCheckBox ID="chkEnableUser" runat="server" ResourceString="" Text="Enable User" />
                </div>
            </div>
            <asp:PlaceHolder runat="server" ID="plcMFIsRequired" Visible="false">
                <div class="form-group">
                    <div class="editing-form-value-cell editing-form-value-cell-offset">
                        <cms:CMSCheckBox ID="chkUseMultiFactorAutentization" runat="server" ResourceString="webparts_membership_registrationform.mfrequired" />
                    </div>
                </div>
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="plcCaptcha">
                <div class="form-group">
                    <div class="editing-form-label-cell">
                        <asp:Label CssClass="control-label" ID="lblCaptcha" runat="server" AssociatedControlID="scCaptcha" EnableViewState="false" />
                    </div>
                    <div class="editing-form-value-cell">
                        <cms:SecurityCode ID="scCaptcha" GenerateNumberEveryTime="false" ShowInfoLabel="False" runat="server" />
                    </div>
                </div>
            </asp:PlaceHolder>
            <div class="form-group form-group-submit">
                <cms:CMSButton ID="btnOk" runat="server" OnClick="btnOK_Click" ButtonStyle="Default" EnableViewState="false" />
                <cms:CMSButton ID="btnCancel" runat="server" OnClick="btnCancel_Click" ButtonStyle="Default" EnableViewState="false" />
            </div>
        </div>
    </div>
</asp:Panel>