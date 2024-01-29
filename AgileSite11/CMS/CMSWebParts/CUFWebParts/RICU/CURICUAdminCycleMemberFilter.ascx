<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CURICUAdminCycleMemberFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.RICU.CURICUAdminCycleMemberFilter" %>

<asp:Label ID="lblMsg" runat="server" />
Search for a member name or account #:
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse" />