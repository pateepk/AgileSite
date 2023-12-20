<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSWebParts_Filters_Dashboard_Dashboard_Filter" CodeFile="Dashboard_Filter.ascx.cs" %>

<div style="background-color:#f0f0f0;border:2.5rem;padding:.3rem;">
    <div CssClass="" style="display:flex;flex:auto;justify-content:space-between">
        <!-- First Row -->
        <div style="max-width:50%;">
            <div style="padding:.3rem;">
                <!-- DOB Text Field 11/11/1111 -->
                <asp:TextBox style="margin:.2rem" ID="txtDOB" Columns="1" MexLength="10" Placeholder="mm/dd/yyyy" runat="server"></asp:TextBox>
                <!-- STATE Selection Field -->
            </div>
            <div style="padding:.3rem;">
                <asp:DropDownList style="margin:.2rem" ID="ddState" runat="server" OnChange="toggleStateDropDowns(this)">
                </asp:DropDownList>

                <!-- STATE Selection Field [SUB] {{ REGION }}-->
                <asp:DropDownList style="margin:.2rem" ID="ddRegion" runat="server">
                </asp:DropDownList>
            </div>
            <div style="padding:.3rem;max-width:100%">
                <!-- Second Row-->
                <!-- State [SUB] {{ Insurance ONE }}-->
                <asp:DropDownList style="margin:.2rem;max-width:100%;display:none;" ID="ddStateInsOne" runat="server"></asp:DropDownList>
                <!-- State [SUB] {{ Insurance TWO }}-->
                <asp:DropDownList style="margin:.2rem;max-width:100%;display:none;" ID="ddStateInsTwo" runat="server"></asp:DropDownList>
            </div>
            <!-- Third Row -->
            <div style="padding:.3rem;max-width:100%;display:inline-flex;">
                <div style="padding:.3rem;">
                    <asp:TextBox style="margin:.2rem" ID="txtSearch" Columns="1" MexLength="10" Placeholder="Search..." runat="server"></asp:TextBox>
                </div>
                <div style="padding:.3rem;">
                    <asp:Button runat="server" id="btnSearch" Text="Filter" class="btn btn-primary" OnClick="GenericOnClick" />

                </div>
                <div style="padding:.3rem;">
                    <asp:Button runat="server" id="btnClearFilters" Text="Clear Filters" class="btn btn-primary" OnClick="BTN_ClearFilters" />
                </div>
            </div>
            <!-- Fifth Row -->
        </div>
        <div style="max-width:50%">
            <div style="padding:.3rem;display:inline-flex;">
                <!-- Medical Provider Checkbox -->
                <div style="display: -webkit-inline-box;padding-right:.5rem;">
                <asp:CheckBox style="margin:.2rem" id="chisMedProv" AutoPostBack="false" OnClick="toggleCheckbox(true)" Text-Aligh="Right" runat="server"></asp:CheckBox>
                <Label>Medical</Label>
                </div>
                <!-- Therapy -->
                <div style="display: -webkit-inline-box;padding-right:.5rem;">
                <asp:CheckBox style="margin:.2rem" id="chisTherapy" AutoPostBack="false" OnClick="toggleCheckbox(false)" Text-Aligh="Right" runat="server"></asp:CheckBox>
                <Label>Therapy</Label>
                </div>
                <!-- isAPP (Medical Dependant) -->
                <div style="display: -webkit-inline-box;padding-right:.5rem;display:none;" id="isAppDiv">
                <asp:CheckBox style="margin:.2rem;" id="chisIsAPP" AutoPostBack="false" Text-Aligh="Right" runat="server"></asp:CheckBox>
                <Label>Is APP</Label>
                </div>
            </div>
            <div style="padding:.3rem;max-width:100%;">
                <!-- Medical Provider [SUB] {{ PRIMARY MEDICAL CONDITION}} -->
                <asp:DropDownList style="margin:.2rem;display:none;max-width:100%;" ID="ddPri_MedProvCond" runat="server"></asp:DropDownList>
                <!-- Medical Provider [SUB] {{ SECONDARY MEDICAL CONDITION}} -->
                <asp:DropDownList style="margin:.2rem;display:none;max-width:100%;" ID="ddSec_MedProvCond" runat="server"></asp:DropDownList>
            </div>
            <div style="padding:.3rem;">
                <!-- Therapy [SUB] {{ Condition PRIMARY }} -->
                <asp:DropDownList style="margin:.2rem;display:none;max-width:100%;" ID="ddTherapyCondPri" runat="server"></asp:DropDownList>
                <!-- Therapy [SUB] {{ Condition SECONDARY }} -->
                <asp:DropDownList style="margin:.2rem;display:none;max-width:100%;" ID="ddTherapyCondSec" runat="server"></asp:DropDownList>
            </div>
        </div>
    </div>
    <!-- Sixth Row -->

    <asp:Label ID="lblDEBUG" runat="server"></asp:Label>
</div>
<div style="display:flex;max-width:50%;">
</div>
</div>


<script type="text/javascript" lang="javascript"> //=============== [Get state, region, ins Drop Down Items ] ====================================== //
    var ddState = document.getElementById('<%=ddState.ClientID%>');
    var ddRegion = document.getElementById('<%=ddRegion.ClientID%>');
    var ddInsOne = document.getElementById('<%=ddStateInsOne.ClientID%>');
    var ddInsTwo = document.getElementById('<%=ddStateInsTwo.ClientID%>');

    // ON INIT i was to hide the regions and insurance that do not match my state selected
    stateInsuranceRegex();

    function stateInsuranceRegex() {
        // Lets reassign the state insurance one to data attribute
        for (var i=1;i<ddInsOne.options.length;i++) {
            ddInsOne.options[i].setAttribute("data", ddInsOne.options[i].text.replace("-", "~").split("~")[0].trim());
            ddInsOne.options[i].text = ddInsOne.options[i].text.replace("-", "~").split("~")[1].trim();
        }

        // Lets reassign the state insurance two to data attribute
        for (var i=1;i<ddInsTwo.options.length;i++) {
            ddInsTwo.options[i].setAttribute("data", ddInsTwo.options[i].text.replace("-", "~").split("~")[0].trim());
            ddInsTwo.options[i].text = ddInsTwo.options[i].text.replace("-", "~").split("~")[1].trim();
        }

        // Lets reassign the region  to data attribute
        for (var i=1;i<ddRegion.options.length;i++) {
                ddRegion.options[i].setAttribute("data", ddRegion.options[i].text.split(",")[1].trim());
        ddRegion.options[i].text = ddRegion.options[i].text.split(",")[0].trim();
        }
    }

    var regionDropDown = document.getElementById('<%=ddRegion.ClientID%>');
    if (window.location.search.includes('LocID') || window.location.search.includes('STID')) {
        var stateReplacement = '';
        var params = window.location.search;

        params.replace('?','').split('&').forEach(x => {
            if (x.includes('LocID')) { // Set Location DD
                if (x.replace('LocID=', '') == "-1") {
                    regionDropDown.selectedIndex = 0;
                } else {
                var regionReplacement = x.replace('LocID=', '');
                regionDropDown.selectedIndex = regionReplacement;
                }
            } else if (x.includes('STID')) { // Get state
                stateReplacement = x.replace('STID=', '')
            }
        });
        if (stateReplacement != '') {
            for (i=0;i<ddState.options.length;i++) {
                if (ddState.options[i].value.toUpperCase() == stateReplacement.toUpperCase()) {
                    ddState.selectedIndex = i;
                }
            }
        }
    } 

        if (window.location.search.replace("?").includes("LocID")) {
        var urlParamsLoad = window.location.search.replace("?").split("&");

        urlParamsLoad.forEach(x => {
            if (x.includes("LocID")) {
                for (i=0;i<ddRegion.children.length;i++) {
                  if (ddRegion.children[i].value == x.replace("LocID=", ""))
                    ddRegion.children[i].selected = true;
                }
            }
        });
    }

    if (ddState.selectedIndex > 0) {
        for (i=1;i<ddRegion.children.length;i++) {
            //console.log(ddRegion.children[i])
            if (ddRegion.children[i].attributes.data.textContent != ddState.options[ddState.selectedIndex].value) {
                ddRegion.children[i].hidden = true;
            }   else {
                ddRegion.children[i].hidden = false;
            }
        } // End of Region for loop
            
            // Insurance 1
        for (i=1;i<ddInsOne.children.length;i++) {
            //console.log(ddInsOne.children[i])
            if (ddInsOne.children[i].attributes.data.textContent != ddState.options[ddState.selectedIndex].value) {
                ddInsOne.children[i].hidden = true;
            }   else {
                ddInsOne.children[i].hidden = false;
            }
        } // End of Region for loop
            
        // Insurance 2
        for (i=1;i<ddInsTwo.children.length;i++) {
            //console.log(ddInsTwo.children[i])
            if (ddInsTwo.children[i].attributes.data.textContent != ddState.options[ddState.selectedIndex].value) {
                ddInsTwo.children[i].hidden = true;
            }   else {
                ddInsTwo.children[i].hidden = false;
            }
        } // End of Region for loop
        ddInsOne.style.display = "";
        ddInsTwo.style.display = "";
        ddRegion.disabled = false;       
        ddInsOne.disabled = false;       
        ddInsTwo.disabled = false;       
    } else {
        ddInsOne.style.display = "hidden";
        ddInsTwo.style.display = "hidden";    
        ddRegion.disabled = true;       
        ddInsOne.disabled = true;       
        ddInsTwo.disabled = true;       
    }

    
    // ================= [[  State, Region, Ins DO OnCLick toggles ]] ================= //


    function toggleStateDropDowns(state) { // only give state string [value]
        // Check region
        if (ddState.selectedIndex > 0) {
            for (i=1;i<ddRegion.children.length;i++) {
                //console.log(ddRegion.children[i])
                if (ddRegion.children[i].attributes.data.textContent != ddState.options[ddState.selectedIndex].value) {
                    ddRegion.children[i].hidden = true;
                }   else {
                    ddRegion.children[i].hidden = false;
                }
            } // End of Region for loop
            // Insurance 1
            for (i=1;i<ddInsOne.children.length;i++) {
                //console.log(ddInsOne.children[i])
                if (ddInsOne.children[i].attributes.data.textContent != ddState.options[ddState.selectedIndex].value) {
                    ddInsOne.children[i].hidden = true;
                }   else {
                    ddInsOne.children[i].hidden = false;
                }
            } // End of Region for loop
            // Insurance 2
            for (i=1;i<ddInsTwo.children.length;i++) {
                //console.log(ddInsTwo.children[i])
                if (ddInsTwo.children[i].attributes.data.textContent != ddState.options[ddState.selectedIndex].value) {
                    ddInsTwo.children[i].hidden = true;
                }   else {
                    ddInsTwo.children[i].hidden = false;
                }
            } // End of Region for loop
                ddInsOne.style.display = "";
                ddInsTwo.style.display = "";
                ddRegion.disabled = false;       
                ddInsOne.disabled = false;       
                ddInsTwo.disabled = false;   
                // now set indexes to 0
                ddRegion.selectedIndex = 0;
                ddInsOne.selectedIndex = 0;
                ddInsTwo.selectedIndex = 0;
        } else {
                ddRegion.selectedIndex = 0;
                ddRegion.disabled = true;   
                ddInsOne.style.display = "hidden";
                ddInsTwo.style.display = "hidden";    
                ddInsOne.selectedIndex = 0;
                ddInsOne.disabled = true;       
                ddInsTwo.selectedIndex = 0;
                ddInsTwo.disabled = true;   
                // clear url of STID and locID    
    
        }
        
    }
     //================== Medical & Therapy Description Checkboxes =====================//
    var medicalChecBx = document.getElementById('<%=chisMedProv.ClientID%>');
    var therapyChecBx = document.getElementById('<%=chisTherapy.ClientID%>'); //=== Medical & Therapy Description DropDownList
    var isAPPChecBx = document.getElementById('isAppDiv');
    var ddPri_MedProvCond = document.getElementById('<%=ddPri_MedProvCond.ClientID%>');
    var ddSec_MedProvCond = document.getElementById('<%=ddSec_MedProvCond.ClientID%>');
    var ddTherapyCondPri = document.getElementById('<%=ddTherapyCondPri.ClientID%>');
    var ddTherapyCondSec = document.getElementById('<%=ddTherapyCondSec.ClientID%>');
    
    // ================= [[  Medical & Therapy DO OnCLick toggles ]] ================= //

    function toggleCheckbox(isMed) {
        if (isMed == false) {
            // Enable
            ddTherapyCondPri.disabled = false;
            ddTherapyCondPri.style.display = "";
            ddTherapyCondSec.disabled = false;
            ddTherapyCondSec.style.display = "";
            // Disable
            medicalChecBx.checked = false;
            ddPri_MedProvCond.selectedIndex = 0;
            ddPri_MedProvCond.disabled = true;
            ddPri_MedProvCond.style.display = "none";
            ddSec_MedProvCond.selectedIndex = 0;
            ddSec_MedProvCond.disabled = true;
            ddSec_MedProvCond.style.display = "none";
            isAPPChecBx.style.display = "none";
        } else if (isMed == true) {
            // Enable
            ddPri_MedProvCond.disabled = false;
            ddPri_MedProvCond.style.display = "";
            ddSec_MedProvCond.disabled = false;
            ddSec_MedProvCond.style.display = "";
            isAPPChecBx.style.display = "inline-flex";
            // Disable
            therapyChecBx.checked = false;
            ddTherapyCondPri.selectedIndex = 0;
            ddTherapyCondPri.disabled = true;
            ddTherapyCondPri.style.display = "none";
            ddTherapyCondSec.selectedIndex =0;
            ddTherapyCondSec.disabled = true;
            ddTherapyCondSec.style.display = "none";
        }
    }

    </script>