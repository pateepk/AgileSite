<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUAccountDate.ascx.cs" Inherits="CMSApp.CMSWebParts.CUWebParts.CUAccountDate" %>

<asp:Label ID="lblMsg" runat="server" />
Choose End Date:
<asp:DropDownList ID="ddlAccountDate" runat="server" AutoPostBack="false" onchange="document.location.href = this.value;"  />
