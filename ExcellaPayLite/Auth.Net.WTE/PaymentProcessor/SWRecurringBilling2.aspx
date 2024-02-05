<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SWRecurringBilling2.aspx.cs" Inherits="PaymentProcessor.SWRecurringBilling2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Stock Whisperer Reoccurring Billing</title>
    <meta name="robots" content="noindex" />
    <meta name="googlebot" content="noindex" />
    <link href="/favicon.ico" type="image/x-icon" rel="shortcut icon" />
    <link href="/favicon.ico" type="image/x-icon" rel="icon" />
    <link href="https://www.thestockwhisperer.com/App_Themes/ASC_StockWhisperer/assets/css/core.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <div class="membership">

        <!--<h3>Reccurring Billing</h3> -->

        <h4>All Fields are required</h4>

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
                        <label for="txtCCN">Card Number:</label></td>
                    <td>
                        <asp:TextBox runat="server" ID="txtCCN"></asp:TextBox></td>
                </tr>
                <tr>
                    <td>
                        <label for="ddlMonth">Expiration Date:</label></td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlMonth">
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
                        </asp:DropDownList>&nbsp;&nbsp;&nbsp;

                    <asp:DropDownList runat="server" ID="ddlYear">
                        <asp:ListItem Value="0" Text=" -- Year -- "></asp:ListItem>
                        <asp:ListItem Value="2014" Text="2014"></asp:ListItem>
                        <asp:ListItem Value="2015" Text="2015"></asp:ListItem>
                        <asp:ListItem Value="2016" Text="2016"></asp:ListItem>
                        <asp:ListItem Value="2017" Text="2017"></asp:ListItem>
                        <asp:ListItem Value="2018" Text="2018"></asp:ListItem>
                        <asp:ListItem Value="2019" Text="2019"></asp:ListItem>
                        <asp:ListItem Value="2020" Text="2020"></asp:ListItem>
                        <asp:ListItem Value="2021" Text="2021"></asp:ListItem>
                    </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="txtSecurityCode">Security Code:</label></td>
                    <td>
                        <asp:TextBox runat="server" ID="txtSecurityCode" MaxLength="3" Width="60px"></asp:TextBox></td>
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
    </div>
</body>
</html>