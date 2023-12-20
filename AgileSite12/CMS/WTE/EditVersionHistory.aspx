<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditVersionHistory.aspx.cs" Inherits="CMSApp.TrainingNetwork.EditVersionHistory" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button ID="btnClearTNNVideoVersionHistory" runat="server" Text="Clear Version History" OnClick="btnClearTNNVideoVersionHistory_Clicked" />
        <asp:Button ID="btnUpdateTNNVideoPages" runat="server" Text="Update pages" OnClick="btnUpdateTNNVideoPages_Clicked" />
        <asp:Button ID="btnUpdateTNNVideoData" runat="server" Text="Update Training Network Video Data" OnClick="btnUpdateTNNVideoData_Clicked" Visible="false" />
    </form>
</body>
</html>