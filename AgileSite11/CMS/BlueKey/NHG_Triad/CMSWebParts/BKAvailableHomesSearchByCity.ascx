<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BKAvailableHomesSearchByCity.ascx.cs" Inherits="NHG_T.BlueKey_CMSWebParts_BKAvailableHomesSearchByCity" %>

<%@ Register Assembly="CMS.Controls" Namespace="CMS.Controls" TagPrefix="cms" %>

<script type="text/javascript">
    var j = jQuery.noConflict();
    
    j(document).ready(function () {
        var baseHref = j("#viewByCityBUtton").attr('href');
        j("#viewByCityButton").attr('href', baseHref = "#filter");

        j("#ddCity").change(function () {
            if (this.value > 0) {
                j("#viewByCityButton").attr('href', baseHref + "?city=" + this.value + "#filter");
            }
            else {
                j("#viewByCityButton").attr('href', baseHref + "#filter");
            }
        });
    });
</script>

<div class="homeLandingBox">
    <div class="homeLandingBoxTitle">Search By City</div>
    <div class="homeLandingBoxContent">
        <div class="homeLandingBoxIcon"><img src="../BlueKey/Templates/images/iconHomeLandingSc.png" alt=""></div>
        <div class="dropDownWrapper">
            <asp:DropDownList ID="ddCity" runat="server" ClientIDMode="Static" AppendDataBoundItems="true">                
                <asp:ListItem Text="Any City" Value="0" />
            </asp:DropDownList>
        </div>
    <%--<cms:CMSWebPartZone ZoneID="zoneThree" runat="server" />--%>
    <cms:CMSEditableRegion ID="CMSEditableContent" runat="server" DialogHeight="200" RegionType="HTMLEditor" RegionTitle="" />    
          
    </div>    
    <%--<div class="homeLandingBoxAction"><a href="/Available-Homes" id="viewByCityButton" class="btn btnAccent1 stroke">View By City</a></div>--%>
    <div class="homeLandingBoxAction"><cms:CMSEditableRegion ID="CMSEditableContent1" runat="server" DialogHeight="200" RegionType="HTMLEditor" RegionTitle="" /></div>
</div>
