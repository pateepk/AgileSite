<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sso_strategic_comp.aspx.cs" Inherits="sso_strategic_comp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblDebug" runat="server" Text=""></asp:Label>
            <asp:Literal ID="ViewLiteral" runat="server"></asp:Literal>
        </div>
        <div>
            <strong>REDIRECT URL: </strong>
            <asp:Label ID="lblRedirectURL" runat="server" Text=""></asp:Label>
        </div>
        <hr />
        <div>
            <table style="width: 650px">
                <tr>
                    <td><strong>FORM DATA</strong>:<br />
                        <asp:TextBox ID="txtFormData" runat="server" Rows="5" TextMode="MultiLine" Style="height: 250px; width: 100%" Enabled="false"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><strong>INPUT STREAM</strong>:<br />
                        <asp:TextBox ID="txtInputStream" runat="server" Rows="5" TextMode="MultiLine" Style="height: 250px; width: 100%" Enabled="false"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><strong>RAW SAML</strong>:<br />
                        <asp:TextBox ID="txtRAWSAML" runat="server" Rows="5" TextMode="MultiLine" Style="height: 250px; width: 100%" Enabled="false"></asp:TextBox></td>
                </tr>
                <tr>
                    <td>
                        <strong>Decoded SAML</strong>:<br />
                        <asp:TextBox ID="TxtDecodeSAML" runat="server" Rows="5" TextMode="MultiLine" Style="height: 250px; width: 100%" Enabled="false"></asp:TextBox>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>