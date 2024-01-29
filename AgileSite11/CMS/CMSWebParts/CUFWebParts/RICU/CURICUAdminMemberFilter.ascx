<%@ Control Language="C#" AutoEventWireup="true" CodeF="CURICUAdminMemberFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.RICU.CURICUAdminMemberFilter" %>

<asp:Label ID="lblMsg" runat="server" />
Search for a member name or account #:	
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse"/>