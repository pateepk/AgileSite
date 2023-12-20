<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSApp.CMSWebParts.WTE.Custom.WTE_MyProfile"  Codebehind="WTE_MyProfile.ascx.cs" %>
<%@ Register Src="./Modules/UserControls/WTE_MyProfile.ascx" TagName="MyProfile" TagPrefix="cms" %>

<asp:Label ID="lblError" CssClass="ErrorLabel" runat="server" Visible="false" EnableViewState="false" />
<asp:PlaceHolder id="plcContent" runat="server">
    <cms:MyProfile ID="myProfile" runat="server" />
</asp:PlaceHolder>
