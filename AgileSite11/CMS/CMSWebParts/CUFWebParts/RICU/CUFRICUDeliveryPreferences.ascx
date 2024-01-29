﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUFRICUDeliveryPreferences.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.RICU.CUFRICUDeliveryPreferences" %>

<asp:Label ID="lblMessage" runat="server"></asp:Label>
<telerik:RadGrid ID="rgAccounts" runat="server" Width="270px" Height="34px" AutoGenerateColumns="False"
    AllowPaging="True" PageSize="100" OnNeedDataSource="RGAccounts_NeedDataSource"
    OnItemDataBound="RGAccounts_ItemDataBound">
    <MasterTableView DataKeyNames="MemberID, IsE, IsP"
        CommandItemDisplay="None" ShowHeader="false">
        <CommandItemSettings ShowAddNewRecordButton="false" ShowRefreshButton="false" />
        <Columns>
            <telerik:GridTemplateColumn HeaderText="SelectionColumn" UniqueName="SelectionColumn">
                <ItemTemplate>
                    <asp:RadioButtonList ID="rblDeliveryPreferenceList" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Value="0" Selected="true">eStatement</asp:ListItem>
                        <asp:ListItem Value="1">Paper Statement</asp:ListItem>
                        <asp:ListItem Value="2" Enabled="false">Both eStatement and Paper Statement</asp:ListItem>
                    </asp:RadioButtonList>
                </ItemTemplate>
            </telerik:GridTemplateColumn>
        </Columns>
        <NoRecordsTemplate>
            No statements found.
        </NoRecordsTemplate>
    </MasterTableView>
</telerik:RadGrid>
<br />
<table>
    <tr>
        <td>Email:</td>
        <td>
            <telerik:RadTextBox ID="rtbEmail" runat="server"></telerik:RadTextBox>
        </td>
    </tr>
</table>

<script type="text/javascript">
    var controlID1 = '<%= cbAgree.ClientID %>';
    function IsNotChecked1(obj, args) {
        var checkbox = $("#" + controlID1);
        args.IsValid = checkbox.attr('checked');
    }

</script>
<table style="padding-left: 5px;">
    <tr>
        <td style="vertical-align: top;">
            <asp:CheckBox ID="cbAgree" CssClass="nowrap_list" runat="server" Text="" ViewStateMode="Disabled" />
        </td>
        <td style="padding-left: 10px;">
            By checking this box I agree and fully understand that:<br>
            <ul style="margin-left: 15px;">
                <li>Changing the status on an account from Paper Statements to eStatements means
                    that I will no longer received a paper statement in the mail.
                </li>
                <li>Changing the status on an account from eStatements to Paper Statements means
                    that I will receive my next statement in the mail.
                </li>
            </ul>
            <div>
                <asp:CustomValidator ID="cvAgree" Display="Dynamic" runat="server" 
                    OnServerValidate="cvAgree_ServerValidate" ErrorMessage="You must check the agreement checkbox before changes can be saved."
                    ValidateEmptyText="True"></asp:CustomValidator>
            </div>
        </td>
    </tr>
</table>
<telerik:RadButton ID="rbSaveChanges" runat="server" Text="Save Changes" OnClick="rbSaveChanges_Click">
</telerik:RadButton>
