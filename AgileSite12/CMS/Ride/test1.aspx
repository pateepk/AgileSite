<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test1.aspx.cs" Inherits="test1" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
	<telerik:RadScriptManager ID="RadScriptManager1" runat="server">
	</telerik:RadScriptManager>
	
	     <script>
             function onClientFileUploaded(sender, eventArgs) {
                 var contentType = eventArgs.get_fileInfo().contentType;
                 //alert(contentType);
                 alert("file uploaded, click ok to redirect to file");
                 __doPostBack('RadCloudUpload1', eventArgs);
             }
         </script>
		<h1>This is my test page</h1>
		<br/>
    <div class="demo-container size-narrow">
        <h3 class="additional-text">Upload Files to Amazon S3</h3>
        <telerik:RadCloudUpload ID="RadCloudUpload1" runat="server" RenderMode="Lightweight" MultipleFileSelection="Automatic" OnFileUploaded="RadCloudUpload1_FileUploaded" ProviderType="Amazon" MaxFileSize="3145728" OnClientFileUploaded="onClientFileUploaded">
        </telerik:RadCloudUpload>
    </div>		
    </form>
</body>
</html>
