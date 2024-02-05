<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FrontPageSearch.ascx.cs" Inherits="NHG_C.CMSInlineControls_FrontPageSearch" %>

<asp:Label runat="server" ID="lblError"></asp:Label>

<asp:DropDownList ID="ddNeighborhood" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddBuilder" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddCity" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddPrice" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddType" runat="server" class="product-depth"></asp:DropDownList>
<asp:DropDownList ID="ddLifestyle" runat="server" class="product-depth"></asp:DropDownList>

<asp:Button CssClass="btn_quicksearch" runat="server" ID="btnSearch" OnClick="btnSearch_Click" />