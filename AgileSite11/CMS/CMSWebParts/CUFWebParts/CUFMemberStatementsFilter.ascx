<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFMemberStatementsFilter.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.CUFMemberStatementsFilter" %>

<asp:Label ID="lblMsg" runat="server" />
View statements for:
<asp:DropDownList ID="ddlStatementDate" runat="server" AutoPostBack="true" />