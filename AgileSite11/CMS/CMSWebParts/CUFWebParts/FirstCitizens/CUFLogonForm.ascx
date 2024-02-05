<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFLogonForm.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.FirstCitizens.CUFLogonForm" %>

<asp:Panel ID="pnlBody" runat="server" CssClass="LogonPage">
    <asp:Login ID="Login1" runat="server" DestinationPageUrl="~/Default.aspx">
        <LayoutTemplate>
            <asp:Panel runat="server" ID="pnlLogin" DefaultButton="LoginButton">
                <div class="form-group">
                    <cms:LocalizedLabel ID="lblUserName" runat="server" AssociatedControlID="UserName"
                        EnableViewState="false" />
                    <cms:CMSTextBox ID="UserName" runat="server" MaxLength="100" CssClass="LogonTextBox" />
                    <cms:CMSRequiredFieldValidator ID="rfvUserNameRequired" runat="server" ControlToValidate="UserName"
                        EnableViewState="false">*</cms:CMSRequiredFieldValidator>
                </div>

                <div class="form-group">
                    <cms:LocalizedLabel ID="lblPassword" runat="server" AssociatedControlID="Password"
                        EnableViewState="false" />
                    <cms:CMSTextBox ID="Password" runat="server" TextMode="Password" MaxLength="110"
                        CssClass="LogonTextBox" />
                </div>
                <div class="remember-chk">
                    <cms:LocalizedCheckBox ID="chkRememberMe" runat="server" />
                </div>
                <cms:LocalizedLabel ID="FailureText" runat="server" EnableViewState="False" CssClass="ErrorLabel" />
                <cms:LocalizedButton ID="LoginButton" class="btn" runat="server" CommandName="Login"
                    EnableViewState="false" />
            </asp:Panel>
        </LayoutTemplate>
    </asp:Login>

    <cms:CMSUpdatePanel runat="server" ID="pnlUpdatePasswordRetrievalLink" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:LinkButton ID="lnkPasswdRetrieval" runat="server" EnableViewState="false"
                OnClick="lnkPasswdRetrieval_Click" />
        </ContentTemplate>
    </cms:CMSUpdatePanel>

    <cms:CMSUpdatePanel runat="server" ID="pnlUpdatePasswordRetrieval" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlPasswdRetrieval" runat="server" CssClass="LoginPanelPasswordRetrieval"
                DefaultButton="btnPasswdRetrieval" Visible="False">

                <asp:Label ID="lblPasswdRetrieval" runat="server" EnableViewState="false" AssociatedControlID="txtPasswordRetrieval" />

                <cms:CMSTextBox ID="txtPasswordRetrieval" runat="server" />
                <cms:CMSButton ID="btnPasswdRetrieval" runat="server" EnableViewState="false" class="btn" />
                <cms:CMSRequiredFieldValidator ID="rqValue" runat="server" ControlToValidate="txtPasswordRetrieval"
                    EnableViewState="false" />

                <asp:Label ID="lblResult" runat="server" Visible="false" EnableViewState="false" />
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger EventName="Click" ControlID="lnkPasswdRetrieval" />
        </Triggers>
    </cms:CMSUpdatePanel>
</asp:Panel>