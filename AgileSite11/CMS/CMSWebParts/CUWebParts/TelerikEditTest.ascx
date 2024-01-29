<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TelerikEditTest.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.TelerikEditTest" %>

    <telerik:RadEditor ID="reHTML" runat="server" ToolsFile="~/CMSWebParts/CUWebParts/RadEditorBasicToolsFile.xml">
        <ImageManager ViewPaths="~/Uploads" UploadPaths="~/Uploads" DeletePaths="~/Uploads"
            EnableAsyncUpload="true"></ImageManager>
    </telerik:RadEditor>

