<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.UserControls.WTE_MyProfile"  Codebehind="WTE_MyProfile.ascx.cs" %>

<cms:MessagesPlaceHolder ID="plcMess" runat="server" />

<asp:Panel ID="RegForm" runat="server" CssClass="my-profile-panel">
    <cms:DataForm ID="editProfileForm" runat="server" ClassName="cms.user" />    
</asp:Panel>
