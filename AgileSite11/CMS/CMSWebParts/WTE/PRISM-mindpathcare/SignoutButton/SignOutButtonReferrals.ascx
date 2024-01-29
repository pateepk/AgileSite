<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSWebParts_WTE_PRISM_mindpathcare_Registration_SignOutButtonReferrals" CodeBehind="SignOutButtonReferrals.ascx.cs" %>

<asp:Literal ID="ltlScript" runat="server" EnableViewState="false" />
<cms:CMSButton ID="btnSignOut" runat="server" OnClick="btnSignOut_Click" CssClass="signoutButton"
    ValidationGroup="SignOut" EnableViewState="false" ButtonStyle="Default" />
<asp:LinkButton ID="btnSignOutLink" runat="server" OnClick="btnSignOut_Click" CssClass="signoutLink"
    EnableViewState="false"  ValidationGroup="SignOut" />