﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFAdminCycleMemberFilter.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.CUFAdminCycleMemberFilter" %>
<asp:Label ID="lblMsg" runat="server" />
Search for a specific Tax ID, member name, or account #:
<asp:TextBox ID="txtMemberNumber" runat="server" />
<cms:LocalizedButton ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse" />