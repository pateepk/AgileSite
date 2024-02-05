<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditVersionHistory.aspx.cs" Inherits="NHG_C.CMSApp.utils.EditVersionHistory" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button runat="server" Text="Update Charleton" OnClick="DeleteDocumentHistory_Click" Enabled="false" /> 
        <asp:Button runat="server" Text="Delete Object History" OnClick="DeleteObjectHistory_Click" Enabled="false" />
        <asp:Button runat="server" Text="Update Listing Dates" OnClick="UpdateListingDates_Click" Enabled="false" />

        <asp:Button runat="server" Text="Update Titles" OnClick="UpdateListingMetaTitles_Click" Enabled="true" />
    </form>
</body>
</html>
