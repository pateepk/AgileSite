<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListingsFilter-TR.ascx.cs" Inherits="NHG_T.CMSWebParts_ListingsFilter_TR" %>

<a name="filter"></a>
<asp:UpdatePanel ID="updPanelFilters" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ddArea" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddCounty" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddCity" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <div class="mobileFilterToggle">Search Filters</div>
            <div class="filterWrapper">
            <div class="filterByPriceLabel">By Price</div>
            <div class="mainSearchSlider group">
                <div class="mainSearchSliderValues">
                <div class="mainSearchSliderValue lowValue"><span class="atMin"></span></div>
                <div class="mainSearchSliderValue highValue"><span class="atMax"></span></div>
                </div>
                <div id="slider-range" class="slider-range neighborhoodFilterSliderRange"></div>
            </div>
            
            <asp:HiddenField ID="hfLowValue" runat="server" ClientIDMode="Static" Value="0" />
            <asp:HiddenField ID="hfHighValue" runat="server" ClientIDMode="Static" Value="100000"/>
			
			<div class="group">
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddArea" runat="server" class="moveinFilterArea" OnSelectedIndexChanged="ddArea_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddCounty" runat="server" class="moveinFilterCounty" OnSelectedIndexChanged="ddCounty_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddCity" runat="server" class="listingFilterCity"></asp:DropDownList>
            </div>

            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddNeighborhood" runat="server" class="listingFilterNeighborhood"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper" runat="server" id="ddBuilderDropDownWrapper">
                <asp:DropDownList ID="ddBuilder" runat="server" class="listingFilterBuilder"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddType" runat="server" CssClass="moveinFilterType"></asp:DropDownList>
            </div>

            <%--<asp:DropDownList ID="ddPrice" runat="server" class="listingFilterPrice"></asp:DropDownList> --%>
        
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddSqFt" runat="server" class="listingFilterSqFt"></asp:DropDownList>
            </div>

            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddBedrooms" runat="server" class="listingFilterBedrooms"></asp:DropDownList>
            </div>

            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddMoveInStatus" runat="server" class="listingFilterMoveInStatus">
                    <asp:ListItem Text="Any Status" Value="0" />
                    <asp:ListItem Text="Under Construction" Value="1" />
                    <asp:ListItem Text="Ready Now" Value="2" />
                </asp:DropDownList>
            </div>
			</div> <%-- end group --%>
			
            <%--<asp:Checkbox ID="chckMoveInSpecial" runat="server" class="listingFilterMoveInSpecial" Text="Move In Special"></asp:Checkbox>--%>
                        
            <asp:Label ID="ddDebugLabel" runat="server"></asp:Label>

            <div class="filterActions">
                <div id="btnFilter" class="btn btnAccent1 stroke" runat="server" ClientIDMode="Static" onclick="btnFilter_Click(this)">Search</div>
                <div class="clearFiltersBtn inner" id="btnClearFilters" runat="server" ClientIDMode="static" onclick="ClearFilters(this)">Clear Filters</div>
            </div>
        </div>        

        <%--<p class="searchResultsFine">*Renderings below are not the actual home and may vary. Square footage is approximate, if important - measure. Please call on-site agents for details.</p>--%>
        <cms:CMSEditableRegion ID="editableFinePrint" runat="server" DialogHeight="200" RegionType="HTMLEditor" REgionTitle="" />

        <div class="showingHomesTotal" runat="server" id="divShowingHomesTotal">
            Showing <asp:Literal ID="ltlShowingCount" runat="server" Text="XXXXX" /> <span>of <asp:Literal ID="ltlTotalCount" runat="server" Text="XXXXX" /></span> Homes
        </div>
    </ContentTemplate>
</asp:UpdatePanel>



<script type="text/javascript">
    var j = jQuery.noConflict();

    function ClearFilters(sender, args) {
        var id = j(sender).attr("id");
        __doPostBack(id, id);
    }

    function btnFilter_Click(sender, args) {
        var id = j(sender).attr("id");
        __doPostBack(id, id);
    }
</script>

<script type="text/javascript">
 j(document).ready(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequest);

        function endRequest() {
            var low = getUrlVars()["low"];
            var high = getUrlVars()["high"];

            j(".slider-range").slider({
                range: true,
                min: 100,
                max: 999,
                values: [low == null ? 0 : low, high == null ? 999 : high],
                slide: function (event, ui) {

                    j(".lowValue span").text(ui.values[0]);
                    var currMin = j(".slider-range").slider('option', 'min');
                    if (ui.values[0] <= currMin) {
                        j(".lowValue span").addClass('atMin');
                    } else {
                        j(".lowValue span").removeClass('atMin');
                    }

                    j(".highValue span").text(ui.values[1]);
                    var currMax = j(".slider-range").slider('option', 'max');
                    if (ui.values[1] >= currMax) {
                        j(".highValue span").addClass('atMax');
                    } else {
                        j(".highValue span").removeClass('atMax');
                    }

                    if (j(this).hasClass("neighborhoodFilterSliderRange") ||
                        j(this).hasClass("moveinFilterSliderRange") ||
                        j(this).hasClass("listingFilterSliderRange")) {
                        if (ui.values[0] == 100) {
                            j("#hfLowValue").val(0);
                        }
                        else {
                            j("#hfLowValue").val(ui.values[0]);
                        }


                        if (ui.values[1] == 999) {
                            j("#hfHighValue").val(100000);
                        }
                        else {
                            j("#hfHighValue").val(ui.values[1]);
                        }

                    }
                }
            });
            j(".lowValue span").text(j("#slider-range").slider("values", 0));
            j(".highValue span").text(j("#slider-range").slider("values", 1));
        }
    });
    
    function getUrlVars() {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }

        return vars;
    }
</script>
