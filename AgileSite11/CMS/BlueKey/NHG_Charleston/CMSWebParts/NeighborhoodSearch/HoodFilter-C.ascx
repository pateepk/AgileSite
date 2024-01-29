<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HoodFilter-C.ascx.cs" Inherits="NHG_C.BlueKey_CMSWebParts_NeighborhoodSearch_HoodFilter_C" %>
<a name="filter"></a>

<asp:UpdatePanel ID="updPanelFilters" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ddCounty" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddCity" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        
        <div class="<%if (QueryHelper.GetInteger("filter", 0) == 1)
          {Response.Write("moveInFilters");}
          else
          {Response.Write("searchFilterContainer neighborhoodSearchFilters");}
              %>">
                        
            <div class="bannerFlag">Neighborhood Search</div>
            <div class="mobileFilterToggle">Neighborhood Search Filters</div>
                  
            <div class="filterWrapper">
                <div class="filterByPriceLabel">By Price</div>
                <div class="mainSearchSlider">
                  <div class="mainSearchSliderValues">
                    <div class="mainSearchSliderValue lowValue"><span class="atMin"></span></div>
                    <div class="mainSearchSliderValue highValue"><span class="atMax"></span></div>
                  </div>
                  <div id="slider-range" class="slider-range neighborhoodFilterSliderRange"></div>
                </div>
                <div style="clear: both;"> </div>
                <asp:HiddenField ID="hfLowValue" runat="server" ClientIDMode="Static" Value="0" />
                <asp:HiddenField ID="hfHighValue" runat="server" ClientIDMode="Static" Value="100000"/>

                <div class="dropDownContainer">
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddCounty" runat="server" CssClass="neighborhoodFilterCounty" OnSelectedIndexChanged="ddCounty_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddCity" runat="server" CssClass="neighborhoodFilterCity"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddBuilder" runat="server" CssClass="neighbrohoodFilterBuilder"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                         <asp:DropDownList ID="ddType" runat="server" CssClass="neighborhoodFilterType" ></asp:DropDownList>                    
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddLifestyle" runat="server" class="neighborhoodFilterLifestyle"></asp:DropDownList>
                    </div>
                </div>

                <div class="filterActions">
                                
                <div id="btnFilter" class="btn btnAccent1 stroke" runat="server" ClientIDMode="static" onclick="btnFilter_Click(this)">Search</div>
                <div class="clearFiltersBtn" id="btnClearFilters" runat="server" ClientIDMode="static" onclick="ClearFilters(this)">Clear Filters</div>
                </div>
            </div>
        </div>

        <div class="moveInSorts"></div><!-- LEAVE FOR JQUERY -->

    </ContentTemplate>
</asp:UpdatePanel>
