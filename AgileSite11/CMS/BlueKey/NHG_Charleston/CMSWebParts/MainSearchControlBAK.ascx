<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainSearchControlBAK.ascx.cs" Inherits="NHG_C.BlueKey_CMSWebParts_MainSearchControlBAK" %>

<script type="text/javascript">
    var j = jQuery.noConflict();

    function Submit(sender, args) {
        var lowValue = "";
        var highValue = "";
        if (!j('div.lowValue > span').hasClass("atMin")) {
            lowValue = j('div.lowValue > span').html();
        }
        if (!j('div.lowValue > span').hasClass("atMax")) {
            highValue = j('div.highValue > span').html();
        }

        var id = j(sender).attr("id");
        __doPostBack(id, lowValue + "|" + highValue);

    }
</script>


<div class="mainSearchTool">


  <div class="bannerFlag mobileAccordionToggle">Quick Search</div>
  <div class="accordionContent mainSearchContent">
    <div class="mainSearchLeader">I am looking for a</div>
    <div class="mainSearchType dropDownWrapper">
        <asp:DropDownList ID="ddType" runat="server">            
            <asp:ListItem Text="Builder" Value="builder" />
            <asp:ListItem Text="Neighborhood" Value="neighborhood" selected="True"/>
            <asp:ListItem Text="Home" Value="home" />  
            <asp:ListItem Text="Homesite" Value="homesite" />          
        </asp:DropDownList>
    </div>
    <div class="mainSearchIn">in</div>
    <div class="mainSearchCity dropDownWrapper">
        <asp:DropDownList ID="ddCity" runat="server">
            
        </asp:DropDownList>
    </div>
    <div class="mainSearchInThe">between</div>
    <div class="mainSearchSlider">
      <div class="mainSearchSliderValues">
        <div class="mainSearchSliderValue lowValue"><span class="atMin"></span></div>
        <div class="mainSearchSliderValue highValue"><span class="atMax"></span></div>
      </div>
      <div id="slider-range" class="slider-range"></div>
    </div>
    <div id="btnMainSearchControlSubmit" runat="server" ClientIDMode="static" onclick="Submit(this)" class="mainSearchSubmit btn btnAccent1 stroke">Search</div>
  </div>
    <asp:Literal ID="ltlDebug" runat="server" />
</div>