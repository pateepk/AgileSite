<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Statement.ascx.cs" Inherits="CMSApp.CMSWebParts.CUWebParts.Statement" %>

<div style="display: inline; float: left;">
    <table border="1" width="300px">
        <tr>
            <th colspan="2">
                Activity Summary
            </th>
        </tr>
        <tr>
            <td>
                Deposits & Credits
            </td>
            <td style="text-align: right;">
                <asp:Label ID="lblDepositsAndCredits" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                Withdrawals & Debits
            </td>
            <td style="text-align: right;">
                <asp:Label ID="lblWithdrawlsAndDebits" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <th style="text-align: left;">
                Available Balance
            </th>
            <th style="text-align: right;">
                <asp:Label ID="lblAvailableBalance" runat="server"></asp:Label>
            </th>
        </tr>
    </table>
</div>
<div style="float: right; display: inline;">
    <table width="400px">
        <tr>
            <th>
                <asp:Label ID="lblStatementDate" runat="server" />
            </th>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblAddress" runat="server" />
            </td>
        </tr>
        <tr>
            <th style="text-align:left;">
                <asp:Label ID="lblHeaderMessage" runat="server" />
            </th>
        </tr>
    </table>
</div>
<telerik:RadGrid ID="rgStatement" runat="server" AutoGenerateColumns="False" GridLines="None"
    AllowPaging="True" OnItemCommand="rgStatement_RowCommand" OnItemDataBound="rgStatement_ItemDataBound"
    OnNeedDataSource="rgStatement_NeedDataSource" Width="100%" CellPadding="4" AllowSorting="True"
    PageSize="50" Skin="Sunset">
    <MasterTableView DataKeyNames="ID,Description,Type">
        <Columns>
            <telerik:GridBoundColumn DataField="DisplayDate" HeaderText="Date" DataType="System.DateTime"
                DataFormatString="{0:MM/dd/yyyy}" />
            <telerik:GridTemplateColumn HeaderText="Description" UniqueName="Description">
                <ItemTemplate>
                    <asp:Label ID="lblDescription" runat="server" Visible="false" />
                    <asp:HyperLink ID="hlDescription" runat="server" Visible="false" />
                </ItemTemplate>
            </telerik:GridTemplateColumn>
            <telerik:GridBoundColumn DataField="Debit" HeaderText="Debit" DataFormatString="{0:c}"
                ItemStyle-HorizontalAlign="Right" />
            <telerik:GridBoundColumn DataField="Credit" HeaderText="Credit" DataFormatString="{0:c}"
                ItemStyle-HorizontalAlign="Right" />
            <telerik:GridBoundColumn DataField="RunningBalance" HeaderText="Balance" DataFormatString="{0:c}"
                ItemStyle-HorizontalAlign="Right" />
        </Columns>
    </MasterTableView>
</telerik:RadGrid>
