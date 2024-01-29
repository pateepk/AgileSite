<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MobileAdvancedSearchForm.ascx.cs" Inherits="NHG_C.BlueKey_CMSWebParts_MobileAdvancedSearchForm" %>


<script type="text/javascript">

    var lat;
    var lon;

    function setFormLocations(position) {
        j('#plc_lt_zonePlaceholder_pageplaceholder_pageplaceholder_lt_zoneMobileSubContent_usercontrol_userControlElem_hdnLat').attr('value', position.coords.latitude);
        j('#plc_lt_zonePlaceholder_pageplaceholder_pageplaceholder_lt_zoneMobileSubContent_usercontrol_userControlElem_hdnLon').attr('value', position.coords.longitude);
        lat = position.coords.latitude;
        lon = position.coords.longitude;
    }

    j(document).ready(function () {
        navigator.geolocation.getCurrentPosition(setFormLocations);

        j(".btnAdvancedSearchSubmit").click(function () {
            console.log('form submitted');

            var location = j('.frm_location option:selected').val();
            var price_range = j('.frm_pricerange option:selected').val();
            var developer = j('.frm_builder option:selected').val();
            var lifestyle = j('.frm_lifestyle option:selected').val();
            var type = j('.frm_type option:selected').val();

            var price_bits = price_range.split('|');

            var price_low = price_bits[0];
            var price_high = price_bits[1];

            if (location == -1) {
                location = "";
            }


            var url = "/Mobile/Neighborhoods.aspx?lat=" + lat + "&lon=" + lon + "&type=" + type + "&lifestyle=" + lifestyle + "&price_low=" + price_low + "&price_high=" + price_high + "&builder=" + developer + "&city=" + location;

            console.log("Changing page to: " + url);

            j.mobile.changePage(url);

            return false;
        });
    });


</script>


<asp:Panel runat="server" ID="pnlAdvancedSearch">

    <asp:HiddenField ID="hdnLat" runat="server" />
    <asp:HiddenField ID="hdnLon" runat="server" />

    <div class="locationSelector">
        <asp:DropDownList ID="ddLocation" CssClass="frm_location" runat="server" />
    </div>

    <asp:DropDownList ID="ddPriceRange" runat="server" CssClass="frm_pricerange" />

    <asp:DropDownList ID="ddBuilders" runat="server" CssClass="frm_builder" />

    <asp:DropDownList ID="ddLifestyle" runat="server" CssClass="frm_lifestyle" />

    <asp:DropDownList ID="ddType" runat="server" CssClass="frm_type" />

    <asp:Button runat="server" CssClass="btnAdvancedSearchSubmit" Text="Search Now" data-theme="b" />

</asp:Panel>