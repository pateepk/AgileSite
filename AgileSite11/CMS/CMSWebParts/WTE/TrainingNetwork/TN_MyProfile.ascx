<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_MyProfile.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_MyProfile" %>

<%@ Register Src="MyProfile.ascx" TagName="MyProfile" TagPrefix="cms" %>

<asp:Label ID="lblError" CssClass="ErrorLabel" runat="server" Visible="false" EnableViewState="false" />
<asp:PlaceHolder id="plcContent" runat="server">
    <cms:MyProfile ID="myProfile" runat="server" />
</asp:PlaceHolder>
