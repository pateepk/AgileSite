<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListingsFilter.ascx.cs" Inherits="NHG_C.CMSWebParts_ListingsFilter" %>

<%@ Register Assembly="CMS.DocumentEngine.Web.UI" Namespace="CMS.DocumentEngine.Web.UI" TagPrefix="cms" %>

<a name="filter"></a>
<asp:UpdatePanel ID="updPanelFilters" runat="server">
    <ContentTemplate>
         <div class="mobileFilterToggle">Hide Filters</div>
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
                <asp:DropDownList ID="ddCounty" runat="server" class="moveinFilterCounty" OnSelectedIndexChanged="ddCounty_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddCity" runat="server" class="listingFilterCity" OnSelectedIndexChanged="ddCity_filterSetAreas" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddNeighborhood" runat="server" class="listingFilterNeighborhood"></asp:DropDownList>
            </div>
            <div class="dropDownWrapper" runat="server" ID="ddBuilderDropDownWrapper">
                <asp:DropDownList ID="ddBuilder" runat="server" class="listingFilterBuilder"></asp:DropDownList>
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
            <div class="dropDownWrapper">
                <asp:DropDownList ID="ddMoveInStatus" runat="server" CssClass="listingFilterMoveInStatus">
                    <asp:ListItem Text="Any Status" Value="0" />
                    <asp:ListItem Text="Under Contruction" Value="1" />
                    <asp:ListItem Text="Ready Now" Value="2" />
                </asp:DropDownList>
            </div>

            </div>

            <asp:Label ID="ddDebugLabel" runat="server"></asp:Label>


            <div class="filterActions">

            <%--<asp:Button ID="btnFilter" runat="server" Text="Search" OnClick="btnFilter_Click" />--%>
            <div id="btnFilter" class="btn btnAccent1 stroke" runat="server" ClientIDMode="static" onclick="btnFilter_Click(this)">Search</div>


            <div class="clearFiltersBtn inner" id="btnClearFilters" runat="server" ClientIDMode="static" onclick="ClearFilters(this)">Clear Filters</div>

            </div>

            <%--<asp:Checkbox ID="chckMoveInSpecial" runat="server" class="listingFilterMoveInSpecial" Text="Move In Special"></asp:Checkbox>--%>

            <%--<asp:Button ID="btnFilterListings" CssClass="btn_quicksearch" runat="server" OnClick="btnFilter_Click" />--%>



        </div>

        
        <%--<span class="searchResultsFine"><cms:CMSEditableRegion ID="editableFinePrint" runat="server" DialogHeight="200" RegionType="HTMLEditor" RegionTitle="" /></span>--%>

        <div class="showingHomesTotal" runat="server" id="divShowingHomesTotal">
            Showing <asp:Literal ID="ltlShowingCount" runat="server" Text="XXXXX" /> <span>of <asp:Literal ID="ltlTotalCount" runat="server" Text="XXXXX" /></span> Homes
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript" src="/productionFiles/js/table_sorter.aspx"></script>

<script type="text/javascript">

    (function($, window, undefined) {
        $(document).ready(function() {
            $.tablesorter.addParser({
                // set a unique id
                id: 'currency',
                is: function(s) {
                    // return false so this parser is not auto detected
                    return false;
                },
                format: function(s) {
                    // format your data for normalization
                    return s.replace('$', '').replace(/,/g, '').replace('TBD','0');
                },
                // set type, either numeric or text
                type: 'numeric'
            });


            $(".listingsListingTable").tablesorter({
                widgets: ['zebra'],
                textExtraction: function(node) {

                    // for numbers formattted like $1,000.50 e.g. English
                    return $(node).text().replace(/[,$£€]|(TBD)/g, '');
                }
            });

            $('.sorterName').removeClass('up').removeClass('down');
            $('.sorterPrice').addClass('up');
           // sortUsingNestedText($('.sortList'), "div", '.sortPriceNumber');
        });

    })(jQuery, window);



</script>

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

    j(document).ready(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequest);

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
