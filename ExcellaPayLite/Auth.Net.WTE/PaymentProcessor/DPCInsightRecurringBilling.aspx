<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DPCInsightRecurringBilling.aspx.cs" Inherits="PaymentProcessor.DPCInsightRecurringBilling" %>

<!DOCTYPE html">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dark Pool Insights</title>
    <meta charset="utf" />
    <meta name="robots" content="noindex" />
    <meta name="googlebot" content="noindex" />
    <link rel="stylesheet" href="https://thedarkpools.com/App_Themes/DarkPoolCrypto/assets/css/basic.css" type="text/css" media="screen" />
</head>
<body>
    <div class="membership container">

        <h3 class="text-center m-b-1">Dark Pool Insights<br />
            Membership Billing</h3>

        <h4 class="text-center">All Fields are required</h4>

        <form id="form1" runat="server">
            <div class="m-1">
                <table id="tblFormFields" runat="server" class="excella-table">
                    <tr>
                        <td>
                            <label for="txtFirstName">First Name:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtFirstName"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtLastName">Last Name:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtLastName"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtAddress">Street:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtAddress"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtCity">City:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtCity"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtState">State:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtState" MaxLength="2" class="input-mini"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtPostalCode">Zip/Postal Code:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtPostalCode"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtEmailAddres">Email:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtEmailAddress"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="ddlPaymentOption">Payment Option:</label></td>
                        <td>
                            <asp:DropDownList ID="ddlPaymentOption" runat="server">
                                <asp:ListItem Value="13" Text="$57.00 monthly"></asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtCCN">Card Number:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtCCN"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="ddlMonth">Expiration Date:</label></td>
                        <td style="white-space: nowrap;">
                            <asp:DropDownList runat="server" ID="ddlMonth" Style="display: inline-block;">
                                <asp:ListItem Value="0" Text=" -- Month -- "></asp:ListItem>
                                <asp:ListItem Value="01" Text="January"></asp:ListItem>
                                <asp:ListItem Value="02" Text="February"></asp:ListItem>
                                <asp:ListItem Value="03" Text="March"></asp:ListItem>
                                <asp:ListItem Value="04" Text="April"></asp:ListItem>
                                <asp:ListItem Value="05" Text="May"></asp:ListItem>
                                <asp:ListItem Value="06" Text="June"></asp:ListItem>
                                <asp:ListItem Value="07" Text="July"></asp:ListItem>
                                <asp:ListItem Value="08" Text="August"></asp:ListItem>
                                <asp:ListItem Value="09" Text="September"></asp:ListItem>
                                <asp:ListItem Value="10" Text="October"></asp:ListItem>
                                <asp:ListItem Value="11" Text="November"></asp:ListItem>
                                <asp:ListItem Value="12" Text="December"></asp:ListItem>
                            </asp:DropDownList>&nbsp;&nbsp;
                    <asp:DropDownList runat="server" ID="ddlYear" Style="display: inline-block;">
                        <asp:ListItem Value="0" Text=" -- Year -- "></asp:ListItem>
                    </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="txtSecurityCode">Security Code:</label></td>
                        <td>
                            <asp:TextBox runat="server" ID="txtSecurityCode" MaxLength="4" class="input-small"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>
                            <label for="ddlTeamMember">
                                Which team member assisted
                                <br />
                                you in joining our room?</label>
                        </td>
                        <td>
                            <asp:DropDownList runat="server" ID="ddlTeamMember" Style="display: inline-block;">
                                <asp:ListItem Value="0" Text=" -- Select Team Member -- "></asp:ListItem>
                                <asp:ListItem Value="2" Text="Paul aka the Alien"></asp:ListItem>
                                <asp:ListItem Value="3" Text="Jane aka Jungle Jane"></asp:ListItem>
                                <asp:ListItem Value="4" Text="Stefanie Kammerman"></asp:ListItem>
                                <asp:ListItem Value="5" Text="Donda"></asp:ListItem>
                                <asp:ListItem Value="7" Text="Wealth Summit"></asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <asp:CheckBox runat="server" ID="chkAgreeToTerm" />&nbsp;<label for="chkAgreeToTerm">I agree to the&nbsp;<a href="https://thedarkpools.com/Legal/Terms-and-Conditions" target="_blank">terms and conditions</a>&nbsp;and
                                 <a href="https://thedarkpools.com/Legal/Refund-Policy" target="_blank">refund policy</a></label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <asp:Label runat="server" ID="lblMessage" EnableViewState="false"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <asp:Button runat="server" class="btn btn-primary btn-lg" ID="btnRun" Text="Process My Payment" OnClick="btnRun_Click" />
                        </td>
                    </tr>
                </table>
            </div>
        </form>
    </div>
</body>
</html>