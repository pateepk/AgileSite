<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MoveInFilter-GS_WTE.ascx.cs" Inherits="NHG_C.BlueKey_CMSWebParts_NeighborhoodSearch_MoveInFilter_GS_WTE" %>

<a name="filter"></a> 
<asp:UpdatePanel ID="updPanelFilters" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ddCounty" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddCity" EventName="SelectedINdexChanged" />
    </Triggers>
    <ContentTemplate>
        <div class="moveInFilters">

            <div class="mobileFilterToggle">Search Filters</div>
            <div class="filterWrapper group">

                <div class="filterByPriceLabel">By Price</div>
                <div class="mainSearchSlider">
                    <div class="mainSearchSliderValues">
                        <div class="mainSearchSliderValue lowValue"><span class="atMin"></span></div>
                        <div class="mainSearchSliderValue highValue"><span class="atMax"></span></div>
                    </div>

                    <div id="slider-range" class="slider-range moveinFilterSliderRange"></div>
                </div>

                <asp:HiddenField ID="hfLowValue" runat="server" ClientIDMode="Static" Value="0" />
                <asp:HiddenField ID="hfHighValue" runat="server" ClientIDMode="Static" Value="100000" />

                <div class="dropDownContainer group">
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddArea" runat="server" CssClass="moveinFilterArea" OnSelectedIndexChanged="ddArea_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddCounty" runat="server" class="moveinFilterCounty" OnSelectedIndexChanged="ddCounty_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddCity" runat="server" class="moveinFilterCity" OnSelectedIndexChanged="ddCity_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddNeighborhood" runat="server" class="moveinFilterNeighborhood"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddBuilder" runat="server" class="moveinFilterBuilder"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddType" runat="server" class="moveinFilterType"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddSqFt" runat="server" class="moveinFilterSqFt"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddBedrooms" runat="server" class="moveinFilterBedBath"></asp:DropDownList>
                    </div>
                </div>

                <asp:Label ID="ddDebugLabel" runat="server"></asp:Label>

                <div class="filterActions group">
                    <div id="btnFilter" class="btn btnAccent1 stroke" runat="server" clientidmode="static" onclick="btnFilter_Click(this)">Search</div>
                    <div class="clearFiltersBtn" id="btnClearFilters" runat="server" clientidmode="static" onclick="ClearFilters(this)">Clear Filters</div>
                </div>

            </div>
        </div>

        <div class="moveInSorts"></div><!-- LEAVE FOR JQUERY -->

    </ContentTemplate>
</asp:UpdatePanel>
