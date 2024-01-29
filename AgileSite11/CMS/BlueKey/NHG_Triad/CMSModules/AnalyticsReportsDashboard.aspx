<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AnalyticsReportsDashboard.aspx.cs" Inherits="NHG_T.BlueKey_CMSModules_AnalyticsReportsDashboard" 
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Title="Analytics Dashboard" %>

<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">    

    <style type="text/css">

        .grdAnalytics {
            border: 2px solid black;    
        }

            .grdAnalytics th {
                padding: 2px;
                text-align: left;
                border: 1px solid #333;

            }

            .grdAnalytics td {
                padding: 2px;
                border: 1px solid #ccc;
                text-align: center;
            }


            .grdAnalytics td.nameColumn {
                text-align: left;
            }

    </style>

    <p><a href="AnalyticsReportsDashQuarterly.aspx">Quarterly Reports</a> | <a href="AnalyticsReportsDashboardAnnual.aspx">Annual Reports</a>

    </p>

    <h2 ID="hdrAnalyticsTitle" runat="server">Analytics Dashboard for the Current Month</h2>

    <label>See Records for: <asp:DropDownList ID="ddMonthSelector" runat="server" /></label><asp:Button ID="btnChangeToMonth" runat="server" OnClick="btnChangeToMonthClick" Text="Submit" />

    <h3>Records for <asp:Label ID="lblMnthRecords" runat="server" /></h3>

    <h3>Developers</h3>
    <asp:GridView ID="grdAnalyticsDeveloperSummary" runat="server" CssClass="grdAnalytics" 
        AutoGenerateColumns="false" 
        OnRowCommand="grdAnalyticsDeveloperSummary_RowCommand" 
        OnRowDataBound="grdAnalyticsDeveloperSummary_RowDataBound">
        <Columns>
            <asp:BoundField DataField="developer_name" ItemStyle-CssClass="nameColumn" HeaderText="Developer Name" />
            <asp:BoundField DataField="website_clicks" HeaderText="Website Clicks" />
            <asp:BoundField DataField="website_impressions" HeaderText="Website Impressions" />
            <asp:BoundField DataField="month" HeaderText="Month" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="btnSendReport" CommandArgument='<%# Eval("developer_id") %>' CommandName="SendReport" runat="server">Send Report</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <h3>Neighborhoods</h3>
    <asp:GridView ID="grdAnalyticsNeighborhoodSummary" runat="server" CssClass="grdAnalytics" 
        AutoGenerateColumns="false"
        OnRowCommand="grdAnalyticsNeighborhoodSummary_RowCommand" 
        OnRowDataBound="grdAnalyticsNeighborhoodSummary_RowDataBound">
        <Columns>
            <asp:BoundField DataField="neighborhood_name" ItemStyle-CssClass="nameColumn" HeaderText="Neighborhood Name" />
            <asp:BoundField DataField="website_clicks" HeaderText="Website Clicks" />
            <asp:BoundField DataField="website_impressions" HeaderText="Website Impressions" />
            <asp:BoundField DataField="month" HeaderText="Month" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="btnSendReport" CommandArgument='<%# Eval("neighborhood_id") %>' CommandName="SendReport" runat="server">Send Report</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    
    <h3 style="display:none;">Floorplans</h3>
    <asp:GridView ID="grdAnalyticsFloorplanSummary" runat="server" CssClass="grdAnalytics" 
        AutoGenerateColumns="false"
        Visible="false"
        OnRowCommand="grdAnalyticsFloorplanSummary_RowCommand" 
        OnRowDataBound="grdAnalyticsFloorplanSummary_RowDataBound">
        <Columns>
            <asp:BoundField DataField="floorplan_name" ItemStyle-CssClass="nameColumn" HeaderText="Floorplan Name" />
            <asp:BoundField DataField="website_clicks" HeaderText="Website Clicks" />
            <asp:BoundField DataField="website_impressions" HeaderText="Website Impressions" />
            <asp:BoundField DataField="month" HeaderText="Month" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="btnSendReport" CommandArgument='<%# Eval("floorplan_id") %>' CommandName="SendReport" runat="server">Send Report</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>