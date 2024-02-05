<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFMemberTaxYearFilter.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.CUFMemberTaxYearFilter" %>

<asp:Label ID="lblMsg" runat="server" />
<div id="divYearDropDown" runat="server">
    Select Year:
    <asp:DropDownList ID="ddlTaxYear" runat="server" AutoPostBack="true" />
</div>