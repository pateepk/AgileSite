<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccountUsers.aspx.cs" Inherits="ExcellaLite.Account.AccountUsers" %>
<%@ Import Namespace="PaymentProcessor.Web.Applications" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">

        var NewGUID = '<%=Guid.NewGuid() %>';

        function CreateNewGUID() {
            $('#' + '<%=BackdooorGUID.ClientID%>').val(NewGUID);
            $('#NewGUIDButton').hide();
        }

        function main() {

        }

        $(document).ready(function () {
            main();
        });


    </script>

    <%Title = "Users"; %>
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

        <asp:Repeater ID="Repeater1" runat="server">
        <HeaderTemplate>
             <table border="1" class="table-nice">
                <tr>
                   <th><b>EDIT</b></th>
                   <th><b>Username</th>
                   <th><b>Full Name</b></th>
                   <th><b>Email</th>
                   <th><b>Phone</b></th>
                   <th><b>CustNum</b></th>
                   <th><b>Is Active</b></th>
                   <th><b>Last Login</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr>
                <td><a href="?EditUserID=<%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.UserID)%>">EDIT</a> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.LoginID)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.FullName)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.Email)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.Phone)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.AdminOrCust)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.IsActive)%> </td>
                <td> <%# (Eval(DRspUsers_Select.Columns.LastLoginDate).ToString().Length > 0) ? DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.LastLoginDate, SV.Common.SimpleDateFormat) : "Never" %> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>

<% if (IsUpdateAction)
   { %>
    <h2>Updating a User</h2>
<% } else { %>
    <h2>Add New User</h2>
<% } %>

<fieldset>
        <table border="0" cellpadding="0" cellspacing="10">
        <tr><td>
        <table border="0" cellpadding="3" cellspacing="3">
            <tr>
                <td>Full Name</td>
                <td>
                    <asp:TextBox ID="FullName" Width="330" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>Username</td>
                <td>
                    <asp:TextBox ID="LoginID" Width="330" runat="server"></asp:TextBox>
                </td>
            </tr>

            <tr>
                <td>Email</td>
                <td>
                    <asp:TextBox ID="Email" Width="330" runat="server"></asp:TextBox>&nbsp;
                </td>
            </tr>
            <tr>
                <td>Phone</td>
                <td>
                    <asp:TextBox ID="Phone" Width="330" Text="" runat="server"></asp:TextBox>
                </td>
            </tr>
            <%if (!IsUpdateAction)
              { %>
            <tr>
                <td>Password</td>
                <td>
                    <asp:TextBox ID="ExternalPassword" Width="330" Text="" TextMode="Password"  runat="server"></asp:TextBox>
                </td>
            </tr>
            <% } %>
            <%if (IsUpdateAction)
              { %>
            <tr>
                <td>Is Active</td>
                <td>
                    <asp:CheckBox ID="IsActive" runat="server" />Active (Enable for using this application)
                </td>
            </tr>
            <tr>
                <td>Backdoor GUID</td>
                <td>
                    <asp:TextBox ID="BackdooorGUID" Width="330" Text="" runat="server"></asp:TextBox>&nbsp;<input type="button" class="ui-button ui-widget ui-corner-all" value="New GUID" id="NewGUIDButton" onclick="CreateNewGUID()" />
                </td>
            </tr>
            <% } %>
            <tr>
                <td>CustNum</td>
                <td>
                    <asp:TextBox ID="CustNum" Width="100" Text="" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td valign="top">Role</td>
                <td>
                    <asp:CheckBox ID="IsAdministrator" runat="server" Text="" />&nbsp;<b>Admin</b>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="left">
            <%if (IsUpdateAction)
              { %>
                    <asp:Button ID="CancelEntry" CssClass="ui-button ui-widget ui-corner-all" runat="server" Text="Cancel" OnClick="CancelEntry_Click"  />
            <% } %>
                    &nbsp;
                    <asp:Button ID="SaveNew" runat="server" CssClass="ui-button ui-widget ui-corner-all" Text="Save This User" OnClick="SaveNew_Click" />
                </td>
            </tr>
        </table>
    </td></tr>
    </table>
    </fieldset>

</asp:Content>
