<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LOCProductSearchDialog.ascx.cs" Inherits="CMSApp.CMSWebParts.WTEWebParts.PSE.LOCProductSearchDialog" %>

<asp:Label ID="lblMessage" runat="server" EnableViewState="false"></asp:Label>
<table>
    <tr>
        <td>
            <label for="txtProductName">Product Name</label> <asp:TextBox ID="txtProductName" runat="server"></asp:TextBox>
        </td>
        <td>
            <label for="txtProductNumber">Product Number</label> <asp:TextBox ID="txtProductNumber" runat="server"></asp:TextBox>
        </td>
        <td>
            <label for="txtSKU">SKU</label> <asp:TextBox ID="txtSKU" runat="server"></asp:TextBox>
        </td>
    </tr>
    <tr>
        <th colspan="3">
            <asp:Button ID="search" runat="server" Text="Search"  CssClass="btn btn-primary" EnableViewState="false" />
        </th>
        <td>
            <asp:Button ID="btnClear" runat="server" Text="Clear"  CssClass="btn" EnableViewState="false" CausesValidation="false" Visible="false" OnClick="btnClear_Click" />
        </td>
    </tr>
   
</table>