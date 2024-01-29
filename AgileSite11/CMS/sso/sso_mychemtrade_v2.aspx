<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sso_mychemtrade_v2.aspx.cs" Inherits="sso_mychemtrade_v2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            Redirecting...
        <asp:Label ID="lblDebug" runat="server" Text="DEBUG LABEL"></asp:Label>
            <asp:Literal ID="ViewLiteral" runat="server"></asp:Literal>
        </div>
        <div>
            <asp:Label ID="lblRedirectURL" runat="server" Text=""></asp:Label>
        </div>
    </form>
</body>
</html>