<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUAdminTaxInfoMemberFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.CUWebParts.CUAdminTaxInfoMemberFilter" %>

<asp:Label ID="lblMsg" runat="server" />
Search by Member #, name, account#, or Address1:
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse" />

