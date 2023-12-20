<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WTE_CustomTableForm.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.Custom.WTE_CustomTableForm" %>

<cms:UIContextPanel runat="server" ID="pnlUIContext">
    <asp:Label ID="lblMsg" runat="server"></asp:Label>
    <cms:CustomTableForm ID="form" runat="server" />
</cms:UIContextPanel>


