<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFAdminCycleStatus.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.CUFAdminCycleStatus" %>
<asp:Label ID="lblMsg" runat="server" />
View:&nbsp;
<asp:DropDownList ID="ddlCycleType" runat="server" AutoPostBack="true" />
&nbsp;cycles with Web status:
&nbsp;<asp:DropDownList ID="ddlCycleStatus" runat="server" AutoPostBack="true" />