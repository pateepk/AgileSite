<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ssotest_old.aspx.cs" Inherits="ssotest_old" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            width: 646px;
        }
    </style>
</head>
<body style="height: 279px; width: 1212px">
    <form id="form1" runat="server">
        <div>
            <fieldset>
                <legend>Enter Values: </legend>
                <table style="height: 271px;">
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
                        <td>SSO Link
                        </td>
                        <td width="100%">
                            <asp:Literal ID="LiteralSSOLink" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:Button ID="cmdCreateSSOLink" runat="server" Text="Create Sample Post URL" OnClick="cmdCreateSSOLink_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2"><i>Enter Full Name, Enter Email Address, Enter Company ID, you can check CompanyID for Company Name, click Create Sample to have SSO Link populated.</i></td>
                    </tr>
                </table>
            </fieldset>
        </div>
    </form>
</body>
</html>