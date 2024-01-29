<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSWebParts_WTE_PRISM_mindpathcare_Registration_PRISMCustomRegistrationForm" CodeBehind="PRISMCustomRegistrationForm.ascx.cs" %>


<cms:MessagesPlaceHolder ID="plcMess" runat="server" />
<asp:Panel ID="pnlRegForm" runat="server" DefaultButton="btnRegister">
    <cms:BasicForm ID="formUser" runat="server" IsLiveSite="true" />
    <asp:PlaceHolder runat="server" ID="plcCaptcha">
        <div class="form-horizontal">
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" runat="server" ID="lblCaptcha" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:SecurityCode ID="captchaElem" runat="server" />
                </div>
            </div>
        </div>
    </asp:PlaceHolder>
    <cms:CMSButton ID="btnRegister" runat="server" CssClass="button" />
</asp:Panel>