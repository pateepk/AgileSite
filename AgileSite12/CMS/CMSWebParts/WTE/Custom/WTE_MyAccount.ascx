<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WTE_MyAccount.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.Custom.WTE_MyAccount" %>

<%@ Register Src="./Modules/UserControls/WTE_ChangePassword.ascx" TagName="ChangePassword" TagPrefix="cms" %>
<%@ Register Src="./Modules/UserControls/WTE_MyProfile.ascx" TagName="MyProfile" TagPrefix="cms" %>

<asp:Panel runat="server" ID="pnlBody" CssClass="MyAccount">
    <div class="TabsHeader">
        <cms:BasicTabControl ID="tabMenu" runat="server" />
    </div>
    <asp:Panel ID="pnlTabs" runat="server" CssClass="TabsContent">
        <asp:Label ID="lblError" CssClass="ErrorLabel" runat="server" Visible="false" EnableViewState="false" />
        <cms:MyProfile ID="myProfile" runat="server" Visible="false" StopProcessing="true" />
        <cms:ChangePassword ID="ucChangePassword" runat="server" Visible="false" />
        <asp:PlaceHolder runat="server" ID="plcOther" />
    </asp:Panel>
</asp:Panel>