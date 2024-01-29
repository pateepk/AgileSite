<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sso_nebraska_test_old.aspx.cs" Inherits="sso_nebraska_test_old" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SSO: Nebraska - Test Page</title>
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
                    <asp:Button ID="btnSubmitLogin" runat="server" Enabled="false" PostBackUrl="~/sso/sso_nebraska.aspx" Text="SUBMIT" OnClick="btnSubmitLogin_Click" />
                    </td>
                </tr>
            </table>
            <br />
            Post URL is: https://www.trainingnetworknow.com/sso/sso_nebraska.aspx
            <br />
            Post Data: SAMLResponse<br />

            <br />
            <table>
                <tr>
                    <td>Partner Name</td>
                    <td>Company ID</td>
                </tr>
                <tr>
                    <td>Nebraska Safety Council:</td>
                    <td>1427</td>
                </tr>
                <tr>
                    <td>National Safety Council Nebraska:</td>
                    <td>1452</td>
                </tr>
                <tr>
                    <td>Nebraska Safety Center University Of Nebraska:</td>
                    <td>2217</td>
                </tr>
                <tr>
                    <td>National Safety Council Nebraska:</td>
                    <td>2297</td>
                </tr>
            </table>

            <br />
        </div>
    </form>
</body>
</html>