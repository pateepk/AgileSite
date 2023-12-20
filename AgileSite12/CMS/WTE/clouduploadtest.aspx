<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="clouduploadtest.aspx.cs" Inherits="CMSApp.WTE.clouduploadtest" EnableEventValidation="false" %>

<%--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">--%>
<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <telerik:RadScriptManager ID="RadScriptManager1" runat="server" EnableScriptCombine="false"></telerik:RadScriptManager>
        <script>
            function onClientFileUploaded(sender, eventArgs) {
                var contentType = eventArgs.get_fileInfo().contentType;
                alert(contentType);
                __doPostBack('rcuMain', eventArgs);
            }
        </script>
        <script>
            function onCarmeetsClientFileUploaded(sender, eventArgs) {
                var contentType = eventArgs.get_fileInfo().contentType;
                alert(contentType);
                __doPostBack('rcuCarmeets', eventArgs);
            }
        </script>
        <script>
            function onRidesClientFileUploaded(sender, eventArgs) {
                var contentType = eventArgs.get_fileInfo().contentType;
                alert(contentType);
                __doPostBack('rcuRides', eventArgs);
            }
        </script>
        <script>
            function onModsClientFileUploaded(sender, eventArgs) {
                var contentType = eventArgs.get_fileInfo().contentType;
                alert(contentType);
                __doPostBack('rcuMods', eventArgs);
            }
        </script>
        <h1>DEMO: Upload single image (gif, png, or jpg) file to "Buckets" - need to validate the file.</h1>
        <div class="demo-container size-narrow">
            <h3 class="additional-text">Upload to Root rideology</h3>
            <telerik:RadCloudUpload ID="rcuMain" runat="server"
                MultipleFileSelection="Disabled"
                ProviderType="Amazon"
                Skin="Bootstrap"
                OnFileUploaded="OnCloudFileUploaded"
                OnClientFileUploaded="onClientFileUploaded"                
                AllowedFileExtensions="jpg,gif,png">
            </telerik:RadCloudUpload>
            <h3 class="additional-text">Upload to carmeets</h3>
            <telerik:RadCloudUpload ID="rcuCarmeets" runat="server"
                MultipleFileSelection="Disabled"
                ProviderType="Amazon"
                Skin="Black"
                OnFileUploaded="OnCloudFileUploaded"
                OnClientFileUploaded="onCarmeetsClientFileUploaded"
                HttpHandlerUrl="~/WTE/UploadToCarmeets.ashx"
                AllowedFileExtensions="jpg,gif,png">
            </telerik:RadCloudUpload>
            <h3 class="additional-text">Upload to mods</h3>
            <telerik:RadCloudUpload ID="rcuMods" runat="server"
                MultipleFileSelection="Disabled"
                ProviderType="Amazon"
                Skin="Bootstrap"
                OnFileUploaded="OnCloudFileUploaded"
                OnClientFileUploaded="onModsClientFileUploaded"                
                HttpHandlerUrl="~/WTE/UploadToMods.ashx"
                AllowedFileExtensions="jpg,gif,png">
            </telerik:RadCloudUpload>
            <h3 class="additional-text">Upload to rides</h3>
            <telerik:RadCloudUpload ID="rcuRides" runat="server"
                MultipleFileSelection="Disabled"
                ProviderType="Amazon"
                Skin="Bootstrap"
                OnFileUploaded="OnCloudFileUploaded"
                OnClientFileUploaded="onRidesClientFileUploaded"                
                HttpHandlerUrl="~/WTE/UploadToRides.ashx"
                AllowedFileExtensions="jpg,gif,png">
            </telerik:RadCloudUpload>
        </div>
    </form>
</body>
</html>