<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CloudImageUploader.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.PropTrax.CloudImageUploader" %>

<div id="divMain" runat="server">
    <script>
        function onClientFileUploaded(sender, eventArgs) {
            // var contentType = eventArgs.get_fileInfo().contentType;
            // alert(contentType);
            __doPostBack('rcuMain', eventArgs);
        }
    </script>

    <div id="divContainer" class="btn btn-large btn-brand-2" runat="server">
        <telerik:RadCloudUpload ID="rcuMain" runat="server"
            MultipleFileSelection="Automatic"
            ProviderType="Amazon"
            Skin="Bootstrap"
            OnFileUploaded="OnCloudFileUploaded"
            OnClientFilesUploaded="onClientFileUploaded"
            AllowedFileExtensions="jpg,jpeg,gif,png"
            DropZones=".DropZone1,#DropZone2"
            Localization-SelectButtonText="UPLOAD PHOTOS"
            Localization-DropZone="DROP FILES HERE">
        </telerik:RadCloudUpload>
    </div>
</div>