<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NeighborhoodFilter-GS.ascx.cs" Inherits="NHG_T.CMSWebParts_NeighborhoodFilter_GS" %>


<a name="map"></a>
<div class="mapContainer">
    <div class="bannerFlag mobileAccordionToggle">Neighborhood Map</div>
    <div class="accordionContent">
        <div class="mapActions">
            <a href="" class="btn btnAccent1 ghost mapClose">Close Map</a>
            <a href="?map=1" class="btn btnAccent1 ghost btnViewLargerMap">View Larger Map</a>
            <a href="" class="btn btnAccent1 ghost mapKeyToggle">
                View Map Key
                <div class="mapKey">
                    <ul>
                        <li><span class="keyDot key1"></span><$100,000</li>
                        <li><span class="keyDot key2"></span>$100,001-$150,000</li>
                        <li><span class="keyDot key3"></span>$150,001-$200,000</li>
                        <li><span class="keyDot key4"></span>$200,001-$250,000</li>
                        <li><span class="keyDot key5"></span>$250,001-$300,000</li>
                        <li><span class="keyDot key6"></span>$300,001-$400,000</li>
                        <li><span class="keyDot key7"></span>$400,001-$500,000</li>
                        <li><span class="keyDot key8"></span>$500,001-$750,000</li>
                        <li><span class="keyDot key9"></span>$750,001+</li>
                    </ul>
                </div>
            </a>
        </div>        
        <div class="mapWrapper larger">
            <div id="google_map" style="width: 100%; height: 300px;"></div>
        </div>
    </div>
</div>

<br />

<script type="text/javascript">
    var j = jQuery.noConflict();
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
                max: 400,
                values: [low == null ? 0 : low, high == null ? 400 : high],
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


                        if (ui.values[1] == 400) {
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


<script type="text/javascript" src="/productionFiles/js/table_sorter.aspx"></script>
<!--script type="text/javascript" src="https://maps.google.com/maps/api/js?sensor=true"></script-->

<script type="text/javascript">

    //j(".mapKeyLink").tipTip({ content: "<div class='mapKey'><ul><li><img src='/BlueKey/Templates/images/mapMarker1.png' class='mapKeyColor' /><$100,000</li><li><img src='/productionFiles/images/iconHouse02Key.aspx' class='mapKeyColor' />$100,001-$150,000</li><li><img src='/productionFiles/images/iconHouse03Key.aspx' class='mapKeyColor' />$150,001-$200,000</li><li><img src='/productionFiles/images/iconHouse04Key.aspx' class='mapKeyColor' />$200,001-$250,000</li><li><img src='/productionFiles/images/iconHouse05Key.aspx' class='mapKeyColor' />$250,001-$300,000</li><li><img src='/productionFiles/images/iconHouse06Key.aspx' class='mapKeyColor' />$300,001-$400,000</li><li><img src='/productionFiles/images/iconHouse07Key.aspx' class='mapKeyColor' />$400,001-$500,000</li><li><img src='/productionFiles/images/iconHouse08Key.aspx' class='mapKeyColor' />$500,001-$750,000</li><li><img src='/productionFiles/images/iconHouse09Key.aspx' class='mapKeyColor' />$750,001+</li></ul></div>", defaultPosition: "bottom" });

    j.extend({
        getUrlVars: function() {
            var vars = [], hash;
            var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
            for (var i = 0; i < hashes.length; i++) {
                hash = hashes[i].split('=');
                vars.push(hash[0]);
                vars[hash[0]] = hash[1];
            }
            return vars;
        },
        getUrlVar: function(name) {
            return $.getUrlVars()[name];
        }
    });


    var geocoder = new google.maps.Geocoder();

    var latlng = new google.maps.LatLng(33.917294, -78.828779);

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

	    j(".builderListingTable").tablesorter({ widgets: ['zebra'] });

	    //addLandmark("Charleston Airport", 32.882687, -80.036945, "/productionFiles/images/iconAirport.aspx");
	    var position = getLocationPosition("", '33.917294', '-78.828779');
	    map.setCenter(position);
	    //addLandmark("Charleston Airforce Base", 32.892821, -80.041323, "/productionFiles/images/iconAirForce.aspx");
	    //addLandmark("Boeing", 32.877317, -80.033941, "/productionFiles/images/iconBoeing.aspx");
	    //addLandmark("Spawar", 32.91461, -79.975625, "/productionFiles/images/iconSpawar.aspx");
	    //addLandmark("Bosch", 32.905811, -80.099444, "/productionFiles/images/iconBosch.aspx");
	});


	function toggleMap() {
	    j('#map_container').slideToggle();

	    if (j('#toggle_map').text() == 'View Map') {

	        j('#toggle_map').removeClass('toggle_off').addClass('toggle_on');
	        j('#toggle_map').html('Close Map');

	    }
	    else {

	        j('#toggle_map').removeClass('toggle_on').addClass('toggle_off');
	        j('#toggle_map').html('View Map');

	    }
	    return false;
	}

	function addLandmark(name, lattitude, longitude, icon) {

        var markerContent = name;

        var infoWindow = new google.maps.InfoWindow({
            content: markerContent
        });

        //var markerImage = new google.maps.MarkerImage(icon, new google.maps.Size(32, 32), new google.maps.Point(0, 0), new google.maps.Point(0, 32));

        var marker = new google.maps.Marker({
            map: map,
            position: new google.maps.LatLng(lattitude, longitude),
            title: name,
            icon: icon
        });

        google.maps.event.addListener(marker, 'click', function() {
            infoWindow.open(map, marker);
        });

    }

	function getLocationPosition(address, lat, lon) {
        var position = null;

        if (lat != '' && lon != '') {
            position = new google.maps.LatLng(lat, lon);
        }
        else {

            geocoder.geocode({ 'address': address }, function(results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    position = results[0].geometry.location;
                }
            });

        }
        return position;
    }

    function addNewNeighborhood(name, position, details, icon) {
        if (position != null) {
            var infoWindow = new google.maps.InfoWindow({
                content: details
            });

            var marker = new google.maps.Marker({
                map: map,
                position: position,
                title: name,
                icon: icon
            });

            google.maps.event.addListener(marker, 'click', function() {
                infoWindow.open(map, marker);
            });

            //map.setCenter(position);
        }
    }

    function ClearFilters(sender, args) {
        var id = j(sender).attr("id");
        __doPostBack(id, id);
    }

    function btnFilter_Click(sender, args) {
        var id = j(sender).attr("id");
        __doPostBack(id, id);
    }
</script>

<a name="filter"></a>
<asp:UpdatePanel ID="updPanelFilters" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ddArea" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddCounty" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <div class="searchFilterContainer neighborhoodSearchFilters">
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
                <asp:HiddenField ID="hfLowValue" runat="server" ClientIDMode="Static" Value="0" />
                <asp:HiddenField ID="hfHighValue" runat="server" ClientIDMode="Static" Value="100000"/>

                <div class="dropDownContainer">

                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddArea" runat="server" class="neighborhoodFilterArea" OnSelectedIndexChanged="ddArea_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddCounty" runat="server" class="neighborhoodFilterCounty" OnSelectedIndexChanged="ddCounty_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddCity" runat="server" class="neighborhoodFilterCity"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddBuilder" runat="server" class="neighborhoodFilterBuilder"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddType" runat="server" class="neighborhoodFilterType"></asp:DropDownList>
                    </div>
                    <div class="dropDownWrapper">
                        <asp:DropDownList ID="ddLifestyle" runat="server" class="neighborhoodFilterLifestyle"></asp:DropDownList>
                    </div>
                    <div class="checkboxWrapper">
                        <asp:CheckBox ID="chkCustom" runat="server" class="neighborhoodFilterCustom" Text="Custom" />
                    </div>

                </div>
                
                
                <div class="filterActions">
                    <div id="btnFilter" class="btn btnAccent1 stroke" runat="server" ClientIDMode="static" onclick="btnFilter_Click(this)">Search</div>
                    <div class="clearFiltersBtn" id="btnClearFilters" runat="server" ClientIDMode="static" onclick="ClearFilters(this)">Clear Filters</div>
                </div>

                

                <asp:Label ID="ddDebugLabel" runat="server"></asp:Label>
            </div>
        </div>

        <div class="showingHomesTotal">
            Showing <asp:Literal ID="ltlShowingCount" runat="server" Text="XXXXX" /> <span>of <asp:Literal ID="ltlTotalCount" runat="server" Text="XXXXX" /></span> Neighborhoods
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

