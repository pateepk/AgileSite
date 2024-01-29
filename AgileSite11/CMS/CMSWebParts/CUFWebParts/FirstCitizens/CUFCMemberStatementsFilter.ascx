<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFCMemberStatementsFilter.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.FirstCitizens.CUFCMemberStatementsFilter" %>

<asp:Label ID="lblMsg" runat="server" />
View statements for:
<asp:DropDownList ID="ddlAccounts" runat="server" AutoPostBack="true" />
<asp:DropDownList ID="ddlStatementDate" runat="server" AutoPostBack="true" />