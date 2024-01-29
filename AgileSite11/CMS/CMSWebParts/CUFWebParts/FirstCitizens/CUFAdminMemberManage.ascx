<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFAdminMemberManage.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.FirstCitizens.CUFAdminMemberManage" %>

<%@ Register Src="~/CMSFormControls/Cultures/SiteCultureSelector.ascx" TagName="SiteCultureSelector"
    TagPrefix="cms" %>
<%@ Register Src="~/CMSModules/Membership/FormControls/Users/UserName.ascx" TagName="UserName"
    TagPrefix="cms" %>
<%@ Register Src="~/CMSFormControls/System/EnumSelector.ascx" TagName="EnumSelector"
    TagPrefix="cms" %>

<div class="form-horizontal">
    <asp:Label ID="lblInfoMessage" runat="server"></asp:Label>
    <div id="divEditMain" runat="server" visible="true">
        <div id="divUserName" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="ucUserName" CssClass="control-label" ID="lblUserName"
                    runat="server" EnableViewState="false" ResourceString="general.username"
                    DisplayColon="true" ShowRequiredMark="true" />
            </div>
            <div class="editing-form-value-cell">
                <cms:UserName ID="ucUserName" runat="server" />
            </div>
        </div>
        <div id="divFullName" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel ShowRequiredMark="True" AssociatedControlID="txtFullName" CssClass="control-label"
                    ID="lblFullName" runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.FullName" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSTextBox ID="txtFullName" runat="server" MaxLength="200" />
            </div>
        </div>
        <div id="divFirstName" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="txtFirstName" CssClass="control-label" ID="lblFirstName"
                    runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.FirstName" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSTextBox ID="txtFirstName" runat="server" MaxLength="100" />
            </div>
        </div>
        <div id="divMiddleName" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="txtMiddleName" CssClass="control-label"
                    ID="lblMiddleName" runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.MiddleName" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSTextBox ID="txtMiddleName" runat="server" MaxLength="100" />
            </div>
        </div>
        <div id="divLastName" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="txtLastName" CssClass="control-label" ID="lblLastName"
                    runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.LastName" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSTextBox ID="txtLastName" runat="server" MaxLength="100" />
            </div>
        </div>
        <div id="divEmail" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="txtEmail" CssClass="control-label" ID="LabelEmail"
                    runat="server" EnableViewState="false" Text="Email Address" ResourceString=""
                    DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSTextBox ID="txtEmail" runat="server" MaxLength="100" />
            </div>
        </div>
        <div id="divEnable" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="CheckBoxEnabled" CssClass="control-label"
                    ID="lblEnabled" runat="server" EnableViewState="false" ResourceString="general.enabled"
                    DisplayColon="true" />
                <cms:CMSCheckBox ID="CheckBoxEnabled" runat="server" />
            </div>
            <div class="editing-form-value-cell">
            </div>
        </div>
        <div id="divPrivilege" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="drpPrivilege" CssClass="control-label" ID="lblPrivilege"
                    runat="server" EnableViewState="false" ResourceString="user.privilegelevel"
                    DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <cms:EnumSelector runat="server" ID="drpPrivilege" AssemblyName="CMS.Membership"
                    TypeName="CMS.Membership.UserPrivilegeLevelEnum" />
            </div>
        </div>
        <div id="divIsExternal" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="chkIsExternal" CssClass="control-label"
                    ID="lblIsExternal" runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.IsExternal" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSCheckBox ID="chkIsExternal" runat="server" />
            </div>
        </div>
        <div id="divIsDomain" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="chkIsDomain" CssClass="control-label" ID="lblIsDomain"
                    runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.UserIsDomain"
                    DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSCheckBox ID="chkIsDomain" runat="server" />
            </div>
        </div>
        <div id="divIsHidden" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="chkIsHidden" CssClass="control-label" ID="lblIsHidden"
                    runat="server" EnableViewState="false" ResourceString="User_Edit_General.IsHidden"
                    DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSCheckBox ID="chkIsHidden" runat="server" />
            </div>
        </div>
        <div id="divCultureSelector" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="cultureSelector" CssClass="control-label"
                    ID="lblCulture" runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.Culture" />
            </div>
            <div class="editing-form-value-cell">
                <cms:SiteCultureSelector runat="server" ID="cultureSelector" IsLiveSite="false" />
            </div>
        </div>
        <div id="divUICulture" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="lstUICulture" CssClass="control-label" ID="lblUICulture"
                    runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.UICulture" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSDropDownList ID="lstUICulture" runat="server" CssClass="DropDownField" />
            </div>
        </div>
        <div id="divCreateInfo" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="lblCreatedInfo" CssClass="control-label"
                    ID="lblCreated" runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.UserCreated" />
                <asp:Label CssClass="form-control-text" ID="lblCreatedInfo" runat="server" />
            </div>
            <div class="editing-form-value-cell">
            </div>
        </div>
        <div id="divIsMFRequired" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="chkIsMFRequired" CssClass="control-label"
                    ID="lblIsRequiredMF" runat="server" EnableViewState="false" ResourceString="mfauthentication.label.isRequired"
                    DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSCheckBox ID="chkIsMFRequired" runat="server" EnableViewState="false" />
            </div>
        </div>
        <div id="divResetToken" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="btnResetToken" EnableViewState="false" CssClass="control-label"
                    ID="lblResetToken" runat="server" DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <div class="control-group-inline">
                    <cms:LocalizedButton ID="btnResetToken" runat="server" OnClick="btnResetToken_Click"
                        ButtonStyle="Default" EnableViewState="false" ResourceString="mfauthentication.tokenconfiguration.reset" />
                </div>
            </div>
        </div>
        <div id="divLastLogonTime" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="lblLastLogonTime" CssClass="control-label"
                    ID="lblLastLogon" runat="server" EnableViewState="false" ResourceString="Administration-User_Edit_General.LastLogon" />
                <asp:Label CssClass="form-control-text" ID="lblLastLogonTime" runat="server" />
            </div>
            <div class="editing-form-value-cell">
            </div>
        </div>
        <div id="divlastLogonInfo" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="plcUserLastLogonInfo" CssClass="control-label"
                    ID="lblUserLastLogonInfo" runat="server" EnableViewState="false"
                    ResourceString="adm.user.lblUserLastLogonInfo" />
                <asp:PlaceHolder runat="server" ID="plcUserLastLogonInfo" EnableViewState="false" />
            </div>
            <div class="editing-form-value-cell">
            </div>
        </div>
        <div id="divInvalidLogonAttempt" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="btnResetLogonAttempts" CssClass="control-label"
                    ID="lblInvalidLogonAttempts" runat="server" EnableViewState="false"
                    ResourceString="Administration-User_Edit_General.InvalidLogonAttempts" DisplayColon="true" />

                <asp:Label CssClass="form-control-text" ID="lblInvalidLogonAttemptsNumber" runat="server" />
                <cms:LocalizedButton ID="btnResetLogonAttempts" runat="server" OnClick="btnResetLogonAttempts_Click"
                    ButtonStyle="Default" EnableViewState="false" ResourceString="invalidlogonattempts.unlockaccount.reset" />
                <div class="control-group-inline">
                </div>
            </div>
            <div class="editing-form-value-cell">
            </div>
        </div>
        <div id="divExtendValidity" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="btnExtendValidity" CssClass="control-label"
                    ID="lblPassExpiration" runat="server" DisplayColon="true" />
            </div>
            <div class="editing-form-value-cell">
                <div class="control-group-inline">
                    <asp:Label CssClass="form-control-text" ID="lblExpireIn" runat="server" />
                    <cms:LocalizedButton ID="btnExtendValidity" runat="server" OnClick="btnExtendValidity_Click"
                        ButtonStyle="Default" EnableViewState="false" ResourceString="passwordexpiration.extendvalidity" />
                </div>
            </div>
        </div>
        <div id="divUserStartingPath" class="form-group" runat="server" visible="false">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="txtUserStartingPath" CssClass="control-label"
                    ID="lblUserStartingPath" runat="server" EnableViewState="false"
                    ResourceString="Administration-User_Edit_General.UserStartingPath" />
            </div>
            <div class="editing-form-value-cell">
                <cms:CMSTextBox runat="server" ID="txtUserStartingPath" />
            </div>
        </div>
        <div id="divResetPassword" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="btnResetPassword" CssClass="control-label"
                    ID="lblResetPassword" runat="server" EnableViewState="false" Text="Send Password Link via Email"
                    ResourceString="" DisplayColon="true" />
                <cms:FormSubmitButton ID="btnResetPassword" runat="server" OnClick="btnResetPassword_Click"
                    EnableViewState="false" Text="Send Now" ResourceString="" />
            </div>
            <div class="editing-form-value-cell">
            </div>
            <div class="editing-form-value-cell editing-form-value-cell-offset">
            </div>
        </div>
        <div id="divResetSecurityAnswers" class="form-group" runat="server" visible="true">
            <div class="editing-form-label-cell">
                <cms:LocalizedLabel AssociatedControlID="btnResetSecurityAnswer" CssClass="control-label"
                    ID="LocalizedLabel1" runat="server" EnableViewState="false" Text="Reset Security Challenge Answers"
                    ResourceString="" DisplayColon="true" />
                <cms:FormSubmitButton ID="btnResetSecurityAnswer" runat="server" OnClick="btnResetSecurityAnswer_Click"
                    EnableViewState="false" Text="Reset" ResourceString="" />
            </div>
            <div class="editing-form-value-cell">
            </div>
            <div class="editing-form-value-cell editing-form-value-cell-offset">
            </div>
        </div>
    </div>
    <div id="divButtons" class="form-group" runat="server" visible="true">
        <div class="editing-form-value-cell editing-form-value-cell-offset">
            <cms:FormSubmitButton ID="btnOk" runat="server" OnClick="btnOk_Click"
                EnableViewState="false" ResourceString="" Text="Save" />
            <cms:FormSubmitButton ID="btnCancel" runat="server" OnClick="btnCancel_Click" Visible="true"
                EnableViewState="false" ResourceString="general.cancel" />
        </div>
    </div>
    <asp:Label ID="lblMessage" runat="server"></asp:Label>
    <asp:Label ID="lblError" runat="server"></asp:Label>
</div>