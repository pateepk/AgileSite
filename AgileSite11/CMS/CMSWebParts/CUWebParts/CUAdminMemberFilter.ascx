<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUAdminMemberFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.CUWebParts.CUAdminMemberFilter" %>

<asp:Label ID="lblMsg" runat="server" />
Search for a specific member #, member name, or account #:	
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse"/>