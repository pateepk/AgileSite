<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Egiving.Default" %>
<%@ Import Namespace ="PaymentProcessor.Web.Applications" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">

                <asp:Repeater ID="Repeater1" runat="server">
        <HeaderTemplate>
            <h2>Direct Transactions:</h2>
             <table border="1">
                <tr>
                   <th><b>Date</b></th> 
                   <th><b>Egivings Name</b></th>
                   <th><b>Amount</th>
                   <th><b>Card</b></th>
                   <th><b>Status</b></th>
                   <th><b>Transaction ID</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspTA_DirectTransactions_GetByUserGUID.Columns.TransactionDate, SV.Common.SimpleDateFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspTA_DirectTransactions_GetByUserGUID.Columns.EgivingsName)%> </td>
                <td align="right"> <%# DataBinder.Eval(Container.DataItem, DRspTA_DirectTransactions_GetByUserGUID.Columns.AMOUNT,SV.Common.CurrencyFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.Card)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.Status)%> </td>
                <td align="center"> <%# DataBinder.Eval(Container.DataItem, DRspTA_DirectTransactions_GetByUserGUID.Columns.PayTraceTRANSACTIONID)%> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>


        <br />



         <asp:Repeater ID="Repeater2" runat="server">
         <HeaderTemplate>
            <br />
            <h2>Check Transactions:</h2>
             <table border="1">
                <tr>
                   <th><b>Date</b></th> 
                   <th><b>Egivings Name</b></th>
                   <th><b>Amount</th>
                   <th><b>TR</b></th>
                   <th><b>Status</b></th>
                   <th><b>Check ID</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspTA_CheckTransactions_GetByUserGUID.Columns.TransactionDate, SV.Common.SimpleDateFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspTA_CheckTransactions_GetByUserGUID.Columns.EgivingsName)%> </td>
                <td align="right"> <%# DataBinder.Eval(Container.DataItem, DRspTA_CheckTransactions_GetByUserGUID.Columns.AMOUNT,SV.Common.CurrencyFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.TR1)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.Status)%> </td>
                <td align="center"> <%# DataBinder.Eval(Container.DataItem, DRspTA_CheckTransactions_GetByUserGUID.Columns.PayTraceCHECKIDENTIFIER)%> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>


        <asp:Repeater ID="Repeater3" Visible="false" runat="server">
        <HeaderTemplate>
            <br />
            <h2>Recurring Transactions:</h2>
             <table border="1">
                <tr>
                   <th><b>Date</b></th> 
                   <th><b>Egivings Name</b></th>
                   <th><b>Amount</th>
                   <th><b>Card</b></th>
                   <th><b>Status</b></th>
                   <th><b>Recurring ID</b></th>
                   <th><b>Frequency</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspTA_RecurringTransactions_GetByUserGUID.Columns.TransactionDate, SV.Common.SimpleDateFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspTA_RecurringTransactions_GetByUserGUID.Columns.EgivingsName)%> </td>
                <td align="right"> <%# DataBinder.Eval(Container.DataItem, DRspTA_RecurringTransactions_GetByUserGUID.Columns.AMOUNT,SV.Common.CurrencyFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.Card)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.Status)%> </td>
                <td align="center"> <%# DataBinder.Eval(Container.DataItem, DRspTA_RecurringTransactions_GetByUserGUID.Columns.PayTraceRECURID)%> </td>
                <td align="center"> <%# DataBinder.Eval(Container.DataItem, DRspTA_RecurringTransactions_GetByUserGUID.Columns.RecurringFrequency)%> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>

    </form>
</body>
</html>
