<%@ Control Language="C#" AutoEventWireup="true"  Codebehind="WTE_UploadControl.ascx.cs"
    Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl.WTE_UploadControl" %>
<asp:Label CssClass="ErrorLabel" runat="server" ID="lblError" Visible="false" EnableViewState="false" />
<asp:PlaceHolder runat="server" ID="plcUpload">
    <cms:Uploader ID="uploader" runat="server" />
    <asp:Button ID="hdnPostback" CssClass="HiddenButton" runat="server" EnableViewState="false" />
</asp:PlaceHolder>
