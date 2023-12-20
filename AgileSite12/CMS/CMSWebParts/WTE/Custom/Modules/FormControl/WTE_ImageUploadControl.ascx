<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WTE_ImageUploadControl.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl.WTE_ImageUploadControl" %>
<asp:Label CssClass="ErrorLabel" runat="server" ID="lblError" Visible="false" EnableViewState="false" />

<script>
    function OnClientFilesUploaded(sender, eventArgs) {
        // var contentType = eventArgs.get_fileInfo().contentType;
        // alert(contentType);
        __doPostBack('ruMain', eventArgs);
    }
</script>

<style> 

.DropZone1 {
    width: 300px;
    height: 90px;
    background-color: #357A2B;
    border-color: #CCCCCC;
    color: #767676;
    float: left;
    text-align: center;
    font-size: 16px;
    color: white;
}
 
#DropZone2 {
    width: 300px;
    height: 90px;
    background-color: #357A2B;
    border-color: #CCCCCC;
    color: #767676;
    float: right;
    text-align: center;
    font-size: 16px;
    color: white;
}
 
.demo-container .RadAsyncUpload {
    text-align: center;
    margin-left: 0;
    margin-bottom: 28px;
}
 
.demo-container .RadAsyncUpload .ruFileWrap  {
    text-align: left;
}
 
.demo-container .RadUpload .ruUploadProgress {
    width: 210px;
    display: inline-block;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    vertical-align: top;
}
 
html .demo-container .ruFakeInput {
    width: 100px;
   
    
}
 
html .RadUpload .ruFileWrap {
    position: relative;
}

</style>

<asp:PlaceHolder runat="server" ID="plcUpload">
    <asp:Image ID="imgPreview" runat="server" />
    <cms:Uploader ID="uploader" runat="server" />
    <asp:Button ID="hdnPostback" CssClass="HiddenButton" runat="server" EnableViewState="false" />
    <div class="demo-container size-wide">
    <telerik:RadAsyncUpload ID="ruMain" runat="server" RenderMode="Lightweight" Visible="false"
        OnClientFilesUploaded="OnClientFilesUploaded" 
        OnFileUploaded="OnFileUploaded"
        MaxFileSize="2097152"
        AllowedFileExtensions="jpg,png,gif,bmp"
        AutoAddFileInputs="false" 
        DropZones=".DropZone1,#DropZone2"
        Localization-Select="Upload Photo"
    />
    </div>

</asp:PlaceHolder>
