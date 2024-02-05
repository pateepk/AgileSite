<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NeighborhoodFilterNoMap.ascx.cs" Inherits="NHG_C.CMSWebParts_NeighborhoodFilterNoMap" %>

<asp:DropDownList ID="ddCity" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddPrice" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddBuilder" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddType" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddLifestyle" runat="server" class="product-depth"></asp:DropDownList>

<asp:Button ID="btnFilterNeighborhoods" CssClass="btn_quicksearch" runat="server" OnClick="btnFilter_Click" />

<asp:Label ID="ddDebugLabel" runat="server"></asp:Label>



