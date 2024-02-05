<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountGrid.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CashAccountGrid" %>
<telerik:RadGrid ID="rgCashAccounts" runat="server" AutoGenerateColumns="False" GridLines="None"
    AllowPaging="True" OnItemCommand="rgCashAccounts_RowCommand" OnItemDataBound="rgCashAccounts_ItemDataBound"
    OnNeedDataSource="rgCashAccounts_NeedDataSource" Width="100%" CellPadding="4"
    AllowSorting="True" PageSize="50" Skin="None">
    <MasterTableView DataKeyNames="ShareAccountID">
        <Columns>
            <telerik:GridBoundColumn DataField="EndDate" HeaderText="End Date" DataType="System.DateTime"
                DataFormatString="{0:MM/dd/yyyy}" />
            <telerik:GridBoundColumn DataField="AccountName" HeaderText="Account Name" />
            <telerik:GridBoundColumn DataField="EndingBalance" HeaderText="Ending Balance" DataFormatString="{0:c}"
                ItemStyle-HorizontalAlign="Right" />
            <telerik:GridTemplateColumn DataField="ShareAccountID" HeaderText="View" UniqueName="View">
                <ItemTemplate>
                    <asp:HyperLink ID="hlView" runat="server" Text="View" />
                </ItemTemplate>
            </telerik:GridTemplateColumn>
        </Columns>
    </MasterTableView>
</telerik:RadGrid>
