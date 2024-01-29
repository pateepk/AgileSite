<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="~/CMSWebParts/Membership/TwitterConnect/TwitterConnect.ascx.cs" Inherits="CMSWebParts_Membership_TwitterConnect" %>
<asp:Button runat="server" ClientIDMode="Static" ID="btnTwitterLogin"  OnClick="btnTwitterLogin_Click"/>
<asp:LinkButton ID="lnkTwitterLogin" runat="server" OnClick="btnTwitterLogin_Click" Visible="false" ></asp:LinkButton>