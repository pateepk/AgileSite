﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_MyAccount.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_MyAccount" %>

<%@ Register Src="~/CMSModules/Membership/FormControls/Passwords/ChangePassword.ascx" TagName="ChangePassword" TagPrefix="cms" %>
<%@ Register Src="~/CMSModules/Membership/Controls/MyProfile.ascx" TagName="MyProfile" TagPrefix="cms" %>

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