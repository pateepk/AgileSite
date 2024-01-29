<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OnTimeBilling.aspx.cs" Inherits="PaymentProcessor.OnTimeBilling" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>One time Billing</title>
</head>
<body>
    <form id="form1" runat="server">
    <table>
    <tr>
        <td>First Name:</td>
        <td><asp:TextBox runat="server" ID="txtFirstName"></asp:TextBox></td>
    </tr>
    <tr>
        <td>Last Name:</td>
        <td><asp:TextBox runat="server" ID="txtLastName"></asp:TextBox></td>
    </tr>
    <tr>
        <td>Street 1:</td>
        <td><asp:TextBox runat="server" ID="txtAddress"></asp:TextBox></td>
    </tr>
    <tr>
        <td>City:</td>
        <td><asp:TextBox runat="server" ID="txtCity"></asp:TextBox></td>
    </tr>
    <tr>
        <td>State:</td>
        <td><asp:TextBox runat="server" ID="txtState" MaxLength="2" Width="25px"></asp:TextBox></td>
    </tr>
    <tr>
        <td>Postal Code:</td>
        <td><asp:TextBox runat="server" ID="txtPostalCode"></asp:TextBox></td>
    </tr>
    <tr>
        <td>Card Number:</td>
        <td><asp:TextBox runat="server" ID="txtCCN"></asp:TextBox></td>
    </tr>
    <tr>
        <td>Expiration:(YYYY-MM)</td>
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
        <asp:ListItem Value="2012" Text="2012"></asp:ListItem>
        <asp:ListItem Value="2013" Text="2013"></asp:ListItem>
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
        <td>Security Code:</td>
        <td><asp:TextBox runat="server" ID="txtSecurityCode" MaxLength="3" Width="30px"></asp:TextBox></td>
    </tr>
    <tr>
        <td colspan="2" align="center"><asp:Button runat="server" id="btnRun" Text="Process" OnClick="btnRun_Click" /></td>
    </tr>
    <tr>
        <td>
        <asp:Label runat="server" ID="lblMessage" EnableViewState="false"></asp:Label>
        </td>
    </tr>
    </table>
    </form>
</body>
</html>
