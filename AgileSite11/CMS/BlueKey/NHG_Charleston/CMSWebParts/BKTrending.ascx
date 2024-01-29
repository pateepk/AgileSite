<%@ Control Language="C#" AutoEventWireup="true" Codebehind="BKTrending.ascx.cs" Inherits="NHG_C.BlueKey_CMSWebParts_BKTrending" %>

<asp:Literal ID="ltlDebug" runat="server" />

<asp:Panel ID="pnlNeighborhoods" runat="server" Visible="false">
    <h4>Featured Neighborhoods</h4>
    <asp:Repeater ID="rptNeighborhoods" runat="server">
        <ItemTemplate>
            <a class="asideBox trendingAside trendingNeighborhood" href="/CMSPages/TrackClick.aspx?url=<%# HttpContext.Current.Request.Url.Host %><%# GetNeighborhood(Eval("DocumentNodeID"), "", true, true) %>&type=Trending&id=<%# Eval("DocumentID") %>&LinkNote=Side Bar Neighborhood List">
                <div style="background: url('<%# GetThumb(Eval("NeighborhoodProfilePhoto")) %>') 50% 50% no-repeat; background-size: cover;" class="trendingThumb"></div>
                <div class="trendingName"><%# Eval("NeighborhoodName") %></div>
                <div class="trendingPrice"><%# Eval("NeighborhoodCity") %></div>
                <div class="trendingSubPrice"><%# Eval("NeighborhoodPriceRangeText") %></div>
                <div class="trendingSecondary"><%# GetDeveloper(Eval("NeighborhoodDevelopers")) %></div>
                <div class="trendingAction">Learn More >></div>
              </a>
        </ItemTemplate>
    </asp:Repeater>
</asp:Panel>

<asp:Panel ID="pnlBuilders" runat="server" Visible="false">
    <h4>Featured Builders</h4>
    <asp:Repeater ID="rptDevelopers" runat="server">
        <ItemTemplate>
            <a class="asideBox trendingAside" href="/CMSPages/TrackClick.aspx?url=<%# HttpContext.Current.Request.Url.Host %><%# GetDeveloper(Eval("DevelopersID"), false, false, "", true) %>&type=Trending&id=<%# Eval("DocumentID") %>&LinkNote=Side Bar Developer List">
                <div style="background: url('<%# GetThumb(Eval("DeveloperPhoto")) %>') 50% 50% no-repeat; background-size: contain;" class="trendingThumb"></div>
                <div class="trendingName"><%# Eval("DeveloperName") %></div>
                <div class="trendingPrice"><%# Eval("DeveloperPriceRangeText") %></div>
                <div class="trendingAction">Learn More >></div>
            </a>
        </ItemTemplate>
    </asp:Repeater>
</asp:Panel>

<asp:Panel ID="pnlHomes" runat="server" Visible="false">
    <h4>Featured Homes</h4>
    <asp:Repeater ID="rptHomes" runat="server">
        <ItemTemplate>
            <a class="asideBox trendingAside trendingHomeAside" href="/CMSPages/TrackClick.aspx?url=<%# HttpContext.Current.Request.Url.Host %><%# GetListing(Eval("DocumentNodeID")) %>&type=Trending&id=<%# Eval("DocumentNodeID") %>&LinkNote=Side Bar Listing List">
                <div style="background: url('<%# GetThumb(Eval("ListingMoveInSpecialThumb")) %>') 50% 50% no-repeat; background-size: cover;" class="trendingThumb"></div>
                <div class="trendingName trendingNamePrice"><%# Eval("ListingPrice") %></div>
                <div class="trendingTitle">Neighborhood</div>
                <div class="trendingNeighborhood"><%# GetParentNeighborhood(Eval("DocumentNodeID"), "", false, true) %></div>
                <div class="trendingTitle">Builder</div>
                <div class="trendingBuilder"><%#GetDeveloper(Eval("ListingDeveloper"), false) %></div>
                <div class="trendingCity"><%# Eval("ListingCity") %></div>
                <div class="trendingAction">Learn More >></div>
            </a>
        </ItemTemplate>
    </asp:Repeater>
</asp:Panel>