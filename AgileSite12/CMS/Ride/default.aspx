<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Default" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <telerik:RadScriptManager ID="RadScriptManager1" runat="server">
            <Scripts>
                <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.Core.js" />
                <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQuery.js" />
                <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQueryInclude.js" />
            </Scripts>
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
        <br />
        <asp:Label ID="lblTest" runat="server" Text="Test here" EnableViewState="false" />
        <br />
        <asp:TextBox ID="tbDoc" runat="server" />
        <br />
        <asp:Button ID="btnTest" runat="server" Text="TEST1" OnClick="rbTest_Click" />
        <br />
        <telerik:RadTextBox ID="rtbDoc" runat="server" />
        <br />
        <telerik:RadButton ID="rbTest" runat="server" Text="TEST" OnClick="rbTest_Click">
        </telerik:RadButton>

        <div class="demo-container size-narrow">
            <h3 class="additional-text">Upload Files to Amazon S3</h3>
            <telerik:RadCloudUpload ID="RadCloudUpload1" runat="server" RenderMode="Lightweight" MultipleFileSelection="Automatic" OnFileUploaded="RadCloudUpload1_FileUploaded" ProviderType="Amazon" MaxFileSize="3145728" OnClientFileUploaded="onClientFileUploaded">
            </telerik:RadCloudUpload>
        </div>
    </form>
</body>
</html>