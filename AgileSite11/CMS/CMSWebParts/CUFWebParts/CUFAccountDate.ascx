<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFAccountDate.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.CUFAccountDate" %>

<asp:Label ID="lblMsg" runat="server" />
Choose End Date:
<asp:DropDownList ID="ddlAccountDate" runat="server" AutoPostBack="false" onchange="document.location.href = this.value;" />
