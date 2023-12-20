<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WTECustomOfficeParkWebPart.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.MOBLZ.WTECustomOfficeParkWebPart" %>

<style type="text/css">
.aisdemo-filters .ais-refinement-list--item label {
    padding: 1rem 2.1rem !important;
    line-height: 0.2 !important;
    background: lightgrey !important;
    color: darkolivegreen !important;
}

input[type="checkbox"]+label:before, input[type="radio"]+label:before {
    background: white !important;
    height: 1.30em !important;
    left: 0.3em !important;
    line-height: 1.30em !important;
    top: 0.3em !important;
    width: 1.30em !important;
}

input[type="checkbox"]:checked+label::before, input[type="radio"]:checked+label::before {
    background: #70B343 !important;
}
</style>

<asp:UpdatePanel runat="server" id="upnlMain" updatemode="Conditional">
 <ContentTemplate>
<div class="aisdemo-filters">
   <div class="aisdemo-filters_item">
      <div class="aisdemo-filters_title">City</div>
      <div id="room_types">
         <div data-reactroot="">
            <div class="ais-root ais-refinement-list">
               <div class="ais-body ais-refinement-list--body">
                  <div class="ais-refinement-list--list">
                    <asp:Repeater ID="citiesRepeater" runat="server">
                        <ItemTemplate>
                        <!-- repeat -->
                             <div class="ais-refinement-list--item col-sm">
                                <div>
                                    <asp:CheckBox ID="chkCity" runat="server" OnCheckedChanged="chkCity_CheckedChanged" Text='<%#Eval("CityName")%>' CityId='<%#Eval("CityID")%>' AutoPostBack="true"/>
                                </div>
                             </div>
                        <!-- end repeat -->
                        </ItemTemplate>
                    </asp:Repeater>
                  </div>
               </div>
            </div>
         </div>
      </div>
   </div>
   <div class="aisdemo-filters_item">
      <div class="aisdemo-filters_title"><asp:Literal ID="litOfficePark" runat="server" Text="Office Park" Visible="false"></asp:Literal></div>
      <div id="park_types">
         <div data-reactroot="">
            <div class="ais-root ais-refinement-list">
               <div class="ais-body ais-refinement-list--body">
                  <div class="ais-refinement-list--list">

                      <asp:Repeater ID="officeParkRepeater" runat="server">
                          <ItemTemplate>
                              <!-- repeat -->
                             <div class="ais-refinement-list--item col-sm">
                                <div>
                                    <asp:CheckBox ID="chkOfficePark" runat="server" OnCheckedChanged="chkOfficePark_CheckedChanged" Text='<%#Eval("Name")%>' OfficeParkId='<%#Eval("MOBLZ_OfficeParksID")%>' AutoPostBack="true"/>
                                </div>
                             </div>     
                              <!-- repeat -->
                         </ItemTemplate>
                      </asp:Repeater>
                  </div>
               </div>
            </div>
         </div>
      </div>
   </div>
   <div class="filter-map">
      <div id="map">&nbsp;</div>
   </div>
</div>

<div class="aisdemo-results" id="results">
<div id="hits">
<div data-reactroot="" class="ais-hits">

 <asp:Repeater ID="cardRepeater" runat="server">
      <ItemTemplate>
            <div class="ais-hits--item">
	            <div class="hit">
		              <div class="location" id='location-<%# Eval("Moblz_PropertyID") %>'>
			              <div class="location-header">		
				              <div class="location-top">
					              <div class="title"><%# Eval("PropertyName") %></div> 
					              <%# Eval("Address")%><br/> 
					              <%# Eval("City") %> , <%# Eval("ST") %>  <%# Eval("uszipcode") %><br />
				              </div>
		                        <div class="location-pm-only" style="visibility: hidden;"></div> 
		                </div> 
		              <div class="location-tags">
			              Allowed Types: 
						  <br/>
						  <asp:PlaceHolder runat="server" Visible='<%# Eval("AllowFoodTrucks") != null && Eval("AllowFoodTrucks").ToString().Equals("Yes")  %>'>
								<span title="Allowed">
											<i class="fa fa-check-circle green"></i>
												<span class="sr-only">Allowed</span>
										     Food Trucks
								</span><br/>
					      </asp:PlaceHolder>
						  <asp:PlaceHolder runat="server" Visible='<%# Eval("AllowFoodTrucks") != null && Eval("AllowFoodTrucks").ToString().Equals("No")  %>'>
								<span title="Not Allowed">
											<i class="fa fa-times-circle red"></i>
												<span class="sr-only">Not Allowed</span>
										     Food Trucks
								</span><br/>
					      </asp:PlaceHolder>
                          <asp:PlaceHolder runat="server" Visible='<%# Eval("AllowFoodTrucks") != null && Eval("AllowFoodTrucks").ToString().Equals("Events Only")  %>'>
								<span title="Events Only">
											<i class="fa fa-check-circle blue"></i>
												<span class="sr-only">Events Only</span>
										     Food Trucks
								</span><br/>
					      </asp:PlaceHolder>
						  
						  <asp:PlaceHolder runat="server" Visible='<%# Eval("AllowMobileVendors") != null && Eval("AllowMobileVendors").ToString().Equals("Yes")  %>'>
								<span title='Allowed'>
											<i class='fa fa-check-circle green'></i>
												<span class='sr-only'>Allowed</span>
										     Mobile Vendors
								</span><br/>
					      </asp:PlaceHolder>
						  <asp:PlaceHolder runat="server" Visible='<%# Eval("AllowMobileVendors") != null && Eval("AllowMobileVendors").ToString().Equals("No")  %>'>
								<span title="Not Allowed">
											<i class="fas fa-times-circle red"></i> 
												<span class='sr-only'>Not Allowed</span>
										    Mobile Vendors
								</span><br/>
					      </asp:PlaceHolder>
                          <asp:PlaceHolder runat="server" Visible='<%# Eval("AllowMobileVendors") != null && Eval("AllowMobileVendors").ToString().Equals("Events Only")  %>'>
								<span title="Events Only">
											<i class="fas fa-check-circle blue"></i> 
												<span class='sr-only'>Events Only</span>
										    Mobile Vendors
								</span><br/>
					      </asp:PlaceHolder>
		              </div>
	  
		              <div class="location-footer">
		               <a href='<%# Eval("BaseRegistrationURL").ToString()
                               .Substring(0, Eval("BaseRegistrationURL").ToString()
                               .LastIndexOf("/")) %>'>more info</a>
		              </div> 
	              </div>
              </div>
            </div>   
        </ItemTemplate>
    </asp:Repeater>
 </div></div></div>  
  </ContentTemplate>
</asp:UpdatePanel>
