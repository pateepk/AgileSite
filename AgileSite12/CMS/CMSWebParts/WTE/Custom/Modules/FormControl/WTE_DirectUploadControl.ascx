<%@ Control Language="C#" AutoEventWireup="true"  Codebehind="WTE_DirectUploadControl.ascx.cs"
Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl.WTE_DirectUploadControl" %>
<%@ Register Src="~/CMSModules/Content/Controls/Attachments/DocumentAttachments/DirectUploader.ascx"
    TagName="DirectUploader" TagPrefix="cms" %>
<cms:DirectUploader ID="directUpload" runat="server" />
