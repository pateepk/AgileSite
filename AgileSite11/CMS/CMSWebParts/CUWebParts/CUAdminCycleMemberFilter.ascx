<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUAdminCycleMemberFilter.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CUAdminCycleMemberFilter" %>
<asp:Label ID="lblMsg" runat="server" />
Search for a specific member #:
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse" />