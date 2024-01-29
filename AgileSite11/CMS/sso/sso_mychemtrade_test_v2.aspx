<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sso_mychemtrade_test_v2.aspx.cs" Inherits="sso_mychemtrade_test_v2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SSO: mychemtrade - SAML Test Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table>
                <tr>
                    <td colspan="2">
                        <span style="color: red;">
                            <asp:Literal ID="LiteralMessage" runat="server"></asp:Literal></span>
                    </td>
                </tr>
                <tr>
                    <td>Full Name</td>
                    <td width="100%">
                        <asp:TextBox ID="txtFullName" runat="server" Width="350px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>Email/Username</td>
                    <td width="100%">
                        <asp:TextBox ID="txtEmailAddress" runat="server" Width="350px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td></td>
                    <td class="auto-style1">Email will be the username.</td>
                </tr>
                <tr>
                    <td>CompanyID</td>
                    <td width="100%">
                        <asp:TextBox ID="txtCompanyID" runat="server"></asp:TextBox>
                        <asp:Button ID="cmdCheckCompany" runat="server" Text="Check CompanyID" OnClick="cmdCheckCompany_Click" />
                    </td>
                </tr>
                <tr>
                    <td valign="top">Company Name</td>
                    <td valign="top" class="auto-style1">
                        <asp:Label ID="lblCompanyName" runat="server" Text=""></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td colspan="2"><i style="color: #FF0000; font-weight: bold">click generate SAML and then click submit to login</i></td>
                </tr>
                <tr>
                    <td valign="top">SAML</td>
                    <td>

                        <strong>SAML</strong>:<br />
                        <asp:TextBox ID="SAMLRespEncoded" runat="server" Rows="5" TextMode="MultiLine"
                            Height="251px" Width="600px" Enabled="false"></asp:TextBox>

                        <br />
                        <br />

                        <strong>SAML (ENCODED)</strong>:<br />
                        <asp:TextBox ID="SAMLRespDecoded" runat="server" Rows="5" TextMode="MultiLine"
                            Height="251px" Width="600px" Enabled="false"></asp:TextBox>
                        <asp:HiddenField ID="SAMLResponse" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <br />
                        <asp:Button ID="btnGenerateSAML" runat="server" Text="GENERATE SAML" OnClick="btnGenerateSAML_Click" />&nbsp;
                    <asp:Button ID="btnSubmitLogin" runat="server" Enabled="false" PostBackUrl="~/sso/sso_mychemtrade.aspx" Text="SUBMIT" OnClick="btnSubmitLogin_Click" />
                    </td>
                </tr>
            </table>
            <br />
            Post URL is: https://www.trainingnetworknow.com/sso/sso_mychemtrade.aspx
            <br />
            Post Data: SAMResponse<br />
            <asp:Label ID="lblPrivate" runat="server"></asp:Label>
            <asp:Label ID="lblPublic" runat="server"></asp:Label>
            <asp:Label ID="lblTHumbprint" runat="server"></asp:Label>
            <br />
            <table>
                <tr>
                    <td>Partner Name</td>
                    <td>Company ID</td>
                </tr>
                <tr>
                    <td>Chemtrade Logistics:</td>
                    <td>5051</td>
                </tr>
            </table>
            <br />
            <div>
                <asp:Label ID="keyVerified" Style="color: green;" runat="server" Visible="false">XML confirmed signed!</asp:Label>
            </div>
            <br />
            <div>
                <asp:Label ID="lblErrorTitle" runat="server" Visible="false">SAML ERROR</asp:Label>
                <asp:Label ID="lblError" runat="server" Visible="false"></asp:Label>
            </div>
            <div>
                <asp:Label ID="lblRsa" runat="server" Visible="false">RSA</asp:Label>
                <asp:Label ID="lblRSACode" runat="server" Visible="false"></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>