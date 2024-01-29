<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUCFAdminMemberFilter.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.ComFirst.CUCFAdminMemberFilter" %>

<asp:Label ID="lblMsg" runat="server" />
Search for a member name or account #:
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse" />