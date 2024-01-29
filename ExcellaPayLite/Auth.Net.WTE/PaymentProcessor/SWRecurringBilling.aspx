<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SWRecurringBilling.aspx.cs" Inherits="PaymentProcessor.SWRecurringBilling" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Stock Whisperer Reoccurring Billing</title>
    <meta charset="utf" />
    <meta name="robots" content="noindex" />
    <meta name="googlebot" content="noindex" />
    <link href="/favicon.ico" type="image/x-icon" rel="shortcut icon" />
    <link href="/favicon.ico" type="image/x-icon" rel="icon" />
    <link href="http://www.thestockwhisperer.com/App_Themes/ASC_StockWhisperer/assets/css/core.css" type="text/css" rel="stylesheet" />
</head>
<body>

    <div class="membership">

        <h2 style="text-align: center">The JavaPit<br />
            Membership Billing</h2>

        <h4 style="margin-top: 1em;">All Fields are required</h4>

        <form id="form1" runat="server">
            <table id="tblFormFields" runat="server">
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
                        <asp:TextBox runat="server" ID="txtState" MaxLength="2" Width="50px"></asp:TextBox></td>
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
                        <asp:TextBox runat="server" ID="txtSecurityCode" MaxLength="4" Width="60px"></asp:TextBox></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <asp:Button runat="server" class="btn btn-primary btn-large" ID="btnRun" Text="Process My Payment" OnClick="btnRun_Click" />
                    </td>
                </tr>
            </table>
            <asp:Label runat="server" ID="lblMessage" EnableViewState="false"></asp:Label>
        </form>
        <br />
        <br />
    </div>
</body>
</html>