<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MobileCitySearch.ascx.cs" Inherits="NHG_T.BlueKey_CMSWebParts_MobileCitySearch" %>
<script>
    jQuery.noConflict();
    jQuery(document).bind('pageinit', function(e) {
        jQuery('.ddlCity').live('change', function(e) {
            //console.log(jQuery(this).val());
            if ("-1" !== jQuery(this).val()) {
                var coords = jQuery(this).val().split(",");
                //jQuery.mobile.changePage("/mobile/Find-The-Guide.aspx?cs=" + jQuery(".ddlCity option:selected").text() + "&lat=" + coords[0] + "&lon=" + coords[1], {});
                jQuery.mobile.changePage("/mobile/Find-The-Guide.aspx?cs=1&lat=" + coords[0] + "&lon=" + coords[1], {});
            }
        });
    });
    function CitySearch() {
        jQuery.noConflict();
        if ("-1" !== jQuery(".ddlCity").val()) {
            var coords = jQuery(".ddlCity").val().split(",");
            //jQuery.mobile.changePage("/mobile/Find-The-Guide.aspx?cs=" + jQuery(".ddlCity option:selected").text() + "&lat=" + coords[0] + "&lon=" + coords[1], {});
            jQuery.mobile.changePage("/mobile/Find-The-Guide.aspx?cs=1&lat=" + coords[0] + "&lon=" + coords[1], {});
        }
    }
</script>
<asp:Panel runat="server" ID="pnlCitySearch">

    <div class="locationSelector">
        <asp:DropDownList ID="ddlCity" CssClass="frm_location ddlCity" runat="server" />
    </div>
    <!--
    <input type="submit" name="btncitysearch" value="Search Now" onclick="CitySearch(); return false;" class="btnCitySearch" data-theme="b" />
    -->
    <asp:Literal runat="server" id="ltlOutput"></asp:Literal>
    
    <br />

</asp:Panel>