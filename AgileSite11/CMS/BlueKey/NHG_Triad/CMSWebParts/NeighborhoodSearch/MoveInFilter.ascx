<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MoveInFilter.ascx.cs" Inherits="NHG_T.BlueKey_CMSWebParts_NeighborhoodSearch_MoveInFilter" %>

<%@ Register Assembly="CMS.DocumentEngine.Web.UI" Namespace="CMS.DocumentEngine.Web.UI" TagPrefix="cms" %>

<script type="text/javascript" src="/App_Themes/Charleston/assets/js/populatemoveinfiltermap.min.js"></script>

<script type="text/javascript">
    var j = jQuery.noConflict();
</script>

<asp:Literal ID="ltlDebug" runat="server" />
<script type="text/javascript">
    j.extend({
        getUrlVars: function () {
            var vars = [], hash;
            var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
            for (var i = 0; i < hashes.length; i++) {
                hash = hashes[i].split('=');
                vars.push(hash[0]);
                vars[hash[0]] = hash[1];
            }
            return vars;
        },
        getUrlVar: function (name) {
            return $.getUrlVars()[name];
        }
    });


    var geocoder = new google.maps.Geocoder();

    var latlng = new google.maps.LatLng(32.98102, -80.109558);

    var myOptions = {
        zoom: 10,
        center: latlng,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    var map = new google.maps.Map(document.getElementById("google_map"), myOptions);


    j(document).ready(function() {

	        if (j.getUrlVars()["map"] == 1) {
	            j('.mapWrapper').addClass('expanded');
	            j('#google_map').height(600);
	            j('.btnViewLargerMap').attr('href', window.location.protocol + '//' + window.location.host + window.location.pathname).html('View Smaller Map');
	            google.maps.event.trigger(map, 'resize');
	            map.setZoom(map.getZoom());
	        }

	        //j(".builderListingTable").tablesorter({ widgets: ['zebra'] });

	    //    addLandmark("Charleston Airport", 32.882687, -80.036945, "/productionFiles/images/iconAirport.aspx");
	        var position = getLocationPosition("", '32.882687', '-80.036945');
	        map.setCenter(position);
	        //addLandmark("Charleston Airforce Base", 32.892821, -80.041323, "/productionFiles/images/iconAirForce.aspx");
	        //addLandmark("Boeing", 32.877317, -80.033941, "/productionFiles/images/iconBoeing.aspx");
	        //addLandmark("Spawar", 32.91461, -79.975625, "/productionFiles/images/iconSpawar.aspx");
	        //addLandmark("Bosch", 32.905811, -80.099444, "/productionFiles/images/iconBosch.aspx");
    });
</script>

<script type="text/javascript">
    function ClearFilters(sender, args) {
        var id = j(sender).attr("id");
        __doPostBack(id, id);
    }

    function btnFilter_Click(sender, args)
    {
        var id = j(sender).attr("id");
        __doPostBack(id, id);
    }

    j(document).ready(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequest);

        j('.sorterPrice').addClass('up');
        j('.sorterName').removeClass('up');
        //sortUsingNestedText(j('.sortList'), "div", '.sortPriceNumber');

        function endRequest() {
            var low = getUrlVars()["low"];
            var high = getUrlVars()["high"];

            j(".slider-range").slider({
                range: true,
                min: 100,
                max: 750,
                values: [low == null ? 0 : low, high == null ? 750 : high],
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


                        if (ui.values[1] == 750) {
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
<a name="filter"></a>

<asp:UpdatePanel ID="updPanelFilters" class="moveInFilters" runat="server">
    <Triggers>  
        <asp:AsyncPostBackTrigger ControlID="ddCounty" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddCity" EventName="SelectedINdexChanged" />
    </Triggers>
    <ContentTemplate>
        <div style="clear: both;"></div>
        <br />
        <div class="mobileFilterToggle">Neighborhood Search Filters</div>
            
        <div class="filterWrapper">

            <div class="filterByPriceLabel">By Price</div>
            <div class="mainSearchSlider">
                    <div class="mainSearchSliderValues">
                    <div class="mainSearchSliderValue lowValue"><span class="atMin"></span></div>
                    <div class="mainSearchSliderValue highValue"><span class="atMax"></span></div>
                    </div>
                    <div id="slider-range" class="slider-range moveinFilterSliderRange"></div>
                </div>
                <div style="clear: both;"> </div>
                <asp:HiddenField ID="hfLowValue" runat="server" ClientIDMode="Static" Value="0" />
                <asp:HiddenField ID="hfHighValue" runat="server" ClientIDMode="Static" Value="100000"/>

            <div class="dropDownContainer">
            
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
<%--                <span>By Type</span>
                <asp:ListBox ID="ddType" runat="server" CssClass="neigborhoodFilterType" SelectionMode="Multiple"></asp:ListBox>--%>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddSqFt" runat="server" class="moveinFilterSqFt"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddBedrooms" runat="server" class="moveinFilterBedBath"></asp:DropDownList>
            </div>

            </div>
            
            <asp:Label ID="ddDebugLabel" runat="server"></asp:Label>

            <div class="filterActions">
                    
            <%--<asp:Button ID="btnFilterNeighborhoods" CssClass="btn_quicksearch"  runat="server" OnClick="btnFilter_Click" Text="Search" />--%>
            <div id="btnFilter" class="btn btnAccent1 stroke" runat="server" ClientIDMode="static" onclick="btnFilter_Click(this)">Search</div>
            <div class="clearFiltersBtn" id="btnClearFilters" runat="server" ClientIDMode="static" onclick="ClearFilters(this)">Clear Filters</div>

            </div>

        </div>

        <div class="sortWrapper moveInSorts"><span><span>Sort Alphabetically or High/Low</span></span>
            <ul>
	            <li>
	            <div class="sorter sorterPrice">Price</div>
	            </li>
	            <li>
	            <div class="sorter sorterName">Neighborhood</div>
	            </li>
	            <li>
	            <div class="sorter sorterCity">City</div>
	            </li>
	            <li>
	            <div class="sorter sorterPlan">Plan</div>
	            </li>
	            <li>
	            <div class="sorter sorterBuilder">Builder</div>
	            </li>
	            <li>
	            <div class="sorter sorterLot">Lot #</div>
	            </li>
	            <li>
	            <div class="sorter sorterStories">Stories</div>
	            </li>
	            <li>
	            <div class="sorter sorterBedBath">Bed/Bath</div>
	            </li>
	            <li>
	            <div class="sorter sorterSqFt">Sq. Ft.</div>
	            </li>
            </ul>
        </div>

        <%--<p class="searchResultsFine">*Renderings below are not the actual home and may vary. Square footage is approximate, if important - measure. Please call on-site agents for details.</p>--%>
        <span class="searchResultsFine"><cms:CMSEditableRegion ID="editableFinePrint" runat="server" DialogHeight="100" RegionType="HTMLEditor" RegionTitle="" /></span>

        <div class="showingHomesTotal">
            Showing <asp:Literal ID="ltlShowingCount" runat="server" Text="XXXXX" /> <span>of <asp:Literal ID="ltlTotalCount" runat="server" Text="XXXXX" /></span> Homes
        </div>
        <div style="clear: both;"></div>
    </ContentTemplate>
</asp:UpdatePanel>