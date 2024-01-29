<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TNProduct.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.CUFAdminCycleStatus" %>

<asp:Label ID="lblMsg" runat="server" />
<div class="filter-group">
    <label>Sort By: </label>
    <asp:DropDownList ID="ddlCycleStatus" runat="server" AutoPostBack="true" />
</div>

<div class="filter-group">
    <label>Filter By: </label>
    <asp:DropDownList ID="ddlCycleType" runat="server" AutoPostBack="true" />
</div>