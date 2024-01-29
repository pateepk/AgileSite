<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSApp.CMSWebParts.WTE.DPC.WTE_Custom_DPC_JWT_SSO" CodeBehind="Custom_DPC_JWT_SSO.ascx.cs" %>

<asp:Label ID="lblError" runat="server"></asp:Label>
<asp:HyperLink ID="hplLogin" CssClass="btn btn-primary" target="_blank" runat="server" Text="Enter The Training Pit"></asp:HyperLink>
<cms:CMSButton class="btn btn-primary" target="_blank" ID="btnLogin" runat="server" Text="Login" ButtonStyle="Default" Visible="false" />