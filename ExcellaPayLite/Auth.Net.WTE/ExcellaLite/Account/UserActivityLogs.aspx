<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserActivityLogs.aspx.cs" Inherits="ExcellaLite.Account.UserActivityLogs" %>
<%@ Import Namespace="PaymentProcessor.Web.Applications" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <br /><br />
    <%Title = "User Activity Logs"; %>
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <script type="text/javascript">

        var ActivityHistoryServiceURL = "<%=ActivityHistoryServiceURL %>";

        function showData(id) {
            var postdata = '';
            if (id > 0) {
                postdata += WSAddParameter('ActivityHistoryID', id);
                $.ajax({
                    url: ActivityHistoryServiceURL,
                    type: 'POST',
                    data: postdata,
                    success: function (data) {
                        var xmldata = WSGetJSONObjectFromReturn(data);
                        if (xmldata.success) {
                            var strHTML = '';
                            strHTML = '<table border="1"><tr><th>Data XML</th></tr><tr><td><b>Log ID : ' + id + '</b><br><br>' + xmldata.xml + '<br><br></td></tr></table>';
                            $('#XMLDataViewer').html(strHTML);
                            window.location.hash = "TableTop";
                        }
                    }
                });
            }
        }
</script>

        <caption>Last User Activity Logs (Max: 100 items)</caption>
        <a name="TableTop"></a>
        <table border="0" style="border:0px;">
        <tr><td valign="top">
        <asp:Repeater ID="Repeater1" runat="server">
        <HeaderTemplate>
             <table class="table-nice">
                <tr>
                   <th><b>Log ID</b></th>
                   <th><b>User</b></th>
                   <th><b>Activity</b></th>
                   <th><b>Time</b></th>
                   <th><b>Data</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr class="<%# Container.ItemIndex % 2 == 0 ? "odd" : "even" %>">
                <td> <%# DataBinder.Eval(Container.DataItem, DRspActivityHistory_Select.Columns.ActivityHistoryID)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspActivityHistory_Select.Columns.FullName)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspActivityHistory_Select.Columns.Activity)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.TimeAgo)%> </td>
                <td align="center"> <%# DataBinder.Eval(Container.DataItem, Columns.ShowData)%> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>
        </td><td width="20">&nbsp;</td><td valign="top">
        <div id="XMLDataViewer">
        </div>
        </td></tr>
        </table>

    
</asp:Content>
