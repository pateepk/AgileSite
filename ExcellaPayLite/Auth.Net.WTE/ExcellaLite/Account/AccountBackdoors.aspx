<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccountBackdoors.aspx.cs" Inherits="ExcellaLite.Account.AccountBackdoors" %>
<%@ Import Namespace="PaymentProcessor.Web.Applications" %>



<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">
        function AccountBackdoorSendEmail(UserID) {
            window.location = "AccountBackdoors.aspx?action=<%=Actions.SendEmail%>&UserID=" + UserID;
        }
    </script>

    <br /><br />
    <hgroup class="title">
        <h1>Backdoor Links
            <%if(ForUser!=null) { %>
            Send Email
            <% } %>
        </h1>
    </hgroup>
    <asp:PlaceHolder ID="phEmailForm" runat="server" Visible="false">

<fieldset>
    <asp:Literal ID="LiteralEmailMessage" runat="server"></asp:Literal>
        <table border="0" cellpadding="0" cellspacing="10">
        <tr><td>
        <table class="table-nice">
            <tr>
                <td>Full Name</td>
                <td>
                    <asp:Literal ID="LiteralFullName" runat="server"></asp:Literal>   
                </td>
            </tr>
            <tr>
                <td>Email Template</td>
                <td>
                    <asp:TextBox ID="TextBoxEmailTemplate" Width="600" Height="200" TextMode="MultiLine" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>Email</td>
                <td>
                    <table border="0">
                        <tr><td><asp:RadioButton ID="radEMail1" GroupName="EmailGroup" runat="server" /></td><td><asp:Literal ID="LiteralEmail1" runat="server"></asp:Literal></td></tr>
                        <tr><td><asp:RadioButton ID="radEMail2" GroupName="EmailGroup" runat="server" /></td><td><asp:Literal ID="LiteralEmail2" runat="server"></asp:Literal></td></tr>
                        <tr><td><asp:RadioButton ID="radEMail3" GroupName="EmailGroup" runat="server" /></td><td><asp:Literal ID="LiteralEmail3" runat="server"></asp:Literal></td></tr>
                    </table>
                    
                </td>
            </tr>  
            <tr>
                <td colspan="2" align="right">
                    <asp:Button ID="BtnCancel" runat="server" CssClass="ui-button ui-widget ui-corner-all" Text="Cancel" OnClick="BtnCancel_Click"  />&nbsp;<asp:Button ID="SendNow" runat="server" CssClass="ui-button ui-widget ui-corner-all" Text="Send Now" OnClick="SendNow_Click" />
                </td>
            </tr>
        </table>
    </td></tr>
    </table>
    </fieldset>

       

    </asp:PlaceHolder>

     
    <br />
        <asp:Repeater ID="Repeater1" runat="server">
        <HeaderTemplate>
             <table border="1" class="table-nice">
                <tr>
                   <th><b>ID</b></th>
                   <th><b>CustNum</th>
                   <th><b>Full Name</b></th>
                   <th><b>Link</b></th>
                   <th><b>Send Email</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.UserID)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.CustNum)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspUsers_Select.Columns.FullName)%> </td>
                <td> <a href="<%# DataBinder.Eval(Container.DataItem, Columns.BackdoorLink)%>"><%# DataBinder.Eval(Container.DataItem, Columns.BackdoorLink)%></a> </td>
                 <td> <%# DataBinder.Eval(Container.DataItem, Columns.SendEmailButton)%> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>

</asp:Content>
