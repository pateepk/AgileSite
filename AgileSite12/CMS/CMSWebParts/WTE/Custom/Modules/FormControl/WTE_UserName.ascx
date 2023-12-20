<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.WTE_UserNameControl"  Codebehind="WTE_UserName.ascx.cs" %>

<cms:CMSTextBox runat="server" ID="txtUserName" MaxLength="100" />
<cms:CMSRequiredFieldValidator ID="RequiredFieldValidatorUserName" runat="server" EnableViewState="false"
    Display="dynamic" ControlToValidate="txtUserName"  />
<cms:LocalizedLabel ID="lblUserName" AssociatedControlID="txtUserName" EnableViewState="false"
    ResourceString="general.username" Display="false" runat="server" />