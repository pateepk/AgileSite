<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountAllocationGraph.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.AccountAllocationGraph" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>
<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<telerik:RadChart ID="rcAssetAllocation" runat="server" Width="700px" OnItemDataBound="rcAssetAllocation_ItemDataBound"
    PlotArea-Appearance-FillStyle-MainColor="White" PlotArea-Appearance-Border-Visible="false"
    PlotArea-Appearance-FillStyle-FillType="Solid" Skin="None">
    <Series>
        <telerik:ChartSeries Type="Pie" DataYColumn="chartlabel" DefaultLabelValue="#%">
            <Appearance LegendDisplayMode="ItemLabels">
            </Appearance>
        </telerik:ChartSeries>        
    </Series>    
</telerik:RadChart>
