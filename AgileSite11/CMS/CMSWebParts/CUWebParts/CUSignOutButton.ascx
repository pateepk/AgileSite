<%@ Control Language="C#" AutoEventWireup="true" CodeBeind="CUSignOutButton.ascx.cs" Inherits="CMSApp.CMSWebParts.CUWebParts.CUSignOutButton" %>

<asp:Literal ID="ltlScript" runat="server" EnableViewState="false" />
<cms:CMSButton ID="btnSignOut" runat="server" OnClick="btnSignOut_Click" CssClass="signoutButton"
    ValidationGroup="SignOut" EnableViewState="false" />
<asp:LinkButton ID="btnSignOutLink" runat="server" OnClick="btnSignOut_Click" CssClass="signoutLink"
    EnableViewState="false"  ValidationGroup="SignOut" />