<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CURICUAdminHistoricalStatementsFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.RICU.CURICUAdminHistoricalStatementsFilter" %>

Search for a member name or account #:
<br />
<asp:DropDownList ID="ddlSearchType" runat="server" AutoPostBack="false" EnableViewState="false">
    <asp:ListItem Text="Account #" Value="3"></asp:ListItem>
    <asp:ListItem Text="ANY" Value="0"></asp:ListItem>
    <asp:ListItem Text="Member Name" Value="2"></asp:ListItem>
    <asp:ListItem Text="Tax ID" Value="1"></asp:ListItem>
</asp:DropDownList>
<asp:TextBox ID="txtMemberNumber" runat="server" EnableViewState="false"/> 
EndDate:
<asp:DropDownList ID="ddlStatementDate" runat="server" AutoPostBack="false" EnableViewState="true"/>
<telerik:RadDatePicker ID="radDatetime" EnableViewState="true" runat="server" Visible="false"></telerik:RadDatePicker>
<asp:Button ID="btnFilter" runat="server" Text="Search" class="btn btn-inverse"/>
<br /><small><asp:Label ID="lblMsg" runat="server" /></small>