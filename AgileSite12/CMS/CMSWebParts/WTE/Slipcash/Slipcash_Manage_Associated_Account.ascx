<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Slipcash_Manage_Associated_Account.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.Slipcash.Slipcash_Manage_Associated_Account" %>

<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<input type="hidden" id="hdnPostbackUrl" value="https://www.trainingnetworknow.com/System/TestMacros" />

<script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
<script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>



<style>
#button{
  display:block;
  margin:20px auto;
  padding:10px 30px;
  background-color:#eee;
  border:solid #ccc 1px;
  cursor: pointer;
}
#overlay{	
  position: fixed;
  top: 0;
  z-index: 100;
  width: 100%;
  height:100%;
  display: none;
  background: rgba(0,0,0,0.6);
}
.cv-spinner {
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;  
}
.spinner {
  width: 40px;
  height: 40px;
  border: 4px #ddd solid;
  border-top: 4px #2e93e6 solid;
  border-radius: 50%;
  animation: sp-anime 0.8s infinite linear;
}
@keyframes sp-anime {
  100% { 
    transform: rotate(360deg); 
  }
}
.is-hide{
  display:none;
}
</style>

    <div id="overlay">
        <div class="cv-spinner">
            <span class="spinner"></span>
        </div>
    </div>


<div id="divtest" runat="server" visible="false">
    <style>
        /**
            * Box model adjustments
            * `border-box`... ALL THE THINGS - http://cbrac.co/RQrDL5
        */
        *,
        *:before,
        *:after {
            -webkit-box-sizing: border-box;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
        }

        /**
            * 1. Force a vertical scrollbar - http://cbrac.co/163MspB
            * NOTE: Use `text-rendering` with caution - http://cbrac.co/SJt8p1
            * NOTE: Avoid the webkit anti-aliasing trap - http://cbrac.co/TAdhbH
            * NOTE: IE for Windows Phone 8 ignores `-ms-text-size-adjust` if the
            *       viewport <meta> tag is used - http://cbrac.co/1cFrAvl
        */

        html {
            font-size: 100%;
            overflow-y: scroll; /* 1 */
            min-height: 100%;
        }

        /**
            * 1. Inherits percentage declared on above <html> as base `font-size`
            * 2. Unitless `line-height`, which acts as multiple of base `font-size`
        */

        body {
            font-family: "Helvetica Neue", Arial, sans-serif;
            font-size: 1em; /* 1 */
            line-height: 1.5; /* 2 */
            color: #444;
        }

        /* Page wrapper */
        .wrapper {
            width: 90%;
            max-width: 800px;
            margin: 4em auto;
            text-align: center;
        }

        /* Icons */
        .icon {
            display: inline-block;
            width: 16px;
            height: 16px;
            vertical-align: middle;
            fill: currentcolor;
        }

        /* Headings */
        h1,
        h2,
        h3,
        h4,
        h5,
        h6 {
            color: #222;
            font-weight: 700;
            font-family: inherit;
            line-height: 1.333;
            text-rendering: optimizeLegibility;
        }

        /**
            * Modals ($modals)
        */
        /* 1. Ensure this sits above everything when visible */
        .modal {
            position: absolute;
            z-index: 10000; /* 1 */
            top: 0;
            left: 0;
            visibility: hidden;
            width: 100%;
            height: 100%;
        }

            .modal.is-visible {
                visibility: visible;
            }

        .modal-overlay {
            position: fixed;
            z-index: 10;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: hsla(0, 0%, 0%, 0.5);
            visibility: hidden;
            opacity: 0;
            transition: visibility 0s linear 0.3s, opacity 0.3s;
        }

        .modal.is-visible .modal-overlay {
            opacity: 1;
            visibility: visible;
            transition-delay: 0s;
        }

        .modal-wrapper {
            position: absolute;
            z-index: 9999;
            top: 6em;
            left: 50%;
            width: 32em;
            margin-left: -16em;
            background-color: #fff;
            box-shadow: 0 0 1.5em hsla(0, 0%, 0%, 0.35);
        }

        .modal-transition {
            transition: all 0.3s 0.12s;
            transform: translateY(-10%);
            opacity: 0;
        }

        .modal.is-visible .modal-transition {
            transform: translateY(0);
            opacity: 1;
        }

        .modal-header,
        .modal-content {
            padding: 1em;
        }

        .modal-header {
            position: relative;
            background-color: #fff;
            box-shadow: 0 1px 2px hsla(0, 0%, 0%, 0.06);
            border-bottom: 1px solid #e8e8e8;
        }

        .modal-close {
            position: absolute;
            top: 0;
            right: 0;
            padding: 1em;
            color: #aaa;
            background: none;
            border: 0;
        }

            .modal-close:hover {
                color: #777;
            }

        .modal-heading {
            font-size: 1.125em;
            margin: 0;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }

        .modal-content > *:first-child {
            margin-top: 0;
        }

        .modal-content > *:last-child {
            margin-bottom: 0;
        }
    </style>

    <div id="divAdd">
        <div class="modal">
            <div class="modal-overlay modal-toggle">
            </div>
            <div class="modal-wrapper modal-transition">
                <div class="modal-header">
                    <button class="modal-close modal-toggle">
                        <svg class="icon-close icon" viewBox="0 0 32 32">
                            <use xlink:href="#icon-close"></use></svg></button>
                    <!--<h2 class="modal-heading">This is a modal</h2>-->
                </div>
                <div class="modal-body">
                    <div class="modal-content">
                        <div class="centered-form">
                            <!-- pop up content here -->
                            <input type="text" id="input_handle_addnew" placeholder="enter slipcash handle" value="" />
                            <input type="hidden" id="hdn_memberid_addnew" value="1" />
                            <div class="center"><a class="modal-toggle btn btn-dark" href="#" onclick="saveNew_Clicked();return false;">Submit</a></div>
                        </div>
                        <!--<button class="modal-toggle">Update</button>-->
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="center">
        <button class="modal-toggle btn btn-dark" onclick="toggleAddNew('99999',true);">add new</button>
    </div>

</div>
<style>
    /*  input[type=text] {
            background: none;
            font-weight: bold;
            border-color: #2e2e2e;
            border-style: solid;
            border-width: 2px 2px 2px 2px;
            outline: none;
            padding: 10px 20px 10px 20px;
            width: 250px;
        }*/
    /*Css to target the dropdownbox*/
    ul {
        background: black;
        width: 250px;
        list-style-type: none;
    }

        ul.ui-autocomplete {
            color: white !important;
            -moz-border-radius: 15px;
            border-radius: 15px;
            z-index: 99999
        }
</style>
<div class="center m-b-2  ">
    <div id="div_team_100" name="div_team_100">
        <a id='lnkPreview_100' href="https://slipcash.agilesite.com/$banana1" class="people-pay__btn" name='lnkPreview_100'>banana1</a>
        <input id="txtHandle_100" type="text" placeholder="enter handle" name="txtHandle_100" onkeyup="checkHandle('100', event);" />
        <a id='lnkUpdate_100' class="" title="Update" href="#" onclick="doSaveAssociatedAccount('100', event);" name='lnkUpdate_100' autocomplete="on">
            <i class="fa-sharp fa-solid fa-circle-check"></i>
        </a>
        <a id='lnkEdit_100' class="" title="Edit" href="#" onclick="doEditAssociatedAccount('100', event);" name='lnkEdit_100'>
            <i class="fa-duotone fa-user-pen"></i>
        </a>
        <a id='lnkEnable_100' class="" title="Disable" href="#" onclick="doEnableAssociatedAccount('100', 'false', event);" name='lnkEnable_100'>
            <i class="fa-solid fa-user-large-slash"></i>
        </a>
        <a id='lnkDelete_100' class="" title="Delete" href="#" onclick="doDeleteAssociatedAccount('100', event);" name='lnkDelete_100'>
            <i class="fa-sharp fa-regular fa-trash"></i>
        </a>
    </div>
    <div id='div_validationmsg_100'></div>
    <div id="div_team_9999" name="div_team_9999">
        <a id='lnkPreview_9999' class="people-pay__btn" href="https://slipcash.agilesite.com/$Add New" name='lnkPreview_9999'>Add New</a>
        <input id="txtHandle_9999" type="text" placeholder="enter handle" name="txtHandle_9999" onkeyup="checkHandle('9999', event);" autocomplete="on" />
        <a id='lnkUpdate_9999' class="" title="Update" href="#" onclick="doSaveAssociatedAccount('9999', event);" name='lnkUpdate_9999'>
            <i class="fa-sharp fa-solid fa-circle-check"></i>
        </a>
        <a id='lnkEdit_9999' class="" title="Edit" href="#" onclick="doEditAssociatedAccount('9999', event);" name='lnkEdit_9999'>
            <i class="fa-duotone fa-user-pen"></i>
        </a>
        <a id='lnkEnable_9999' class="" title="Disable" href="#" onclick="doEnableAssociatedAccount('9999', 'false', event)" name='lnkEnable_9999'>
            <i class="fa-solid fa-user-large-slash"></i>
        </a>
        <a id='lnkDelete_9999' class="" title="Delete" href="#" onclick="doDeleteAssociatedAccount('9999', event);" name='lnkDelete_9999'>
            <i class="fa-sharp fa-regular fa-trash"></i>
        </a>
    </div>

    <div id='div_validationmsg_9999'></div>
    <div class="center">
        <button class="btn btn-dark" onclick="toggleAddNew('9999','true', event);">add new</button>
    </div>
</div>


<div id='div_team_<%# Eval("[DisplayOrder]") %>' name='div_team_<%# Eval("[DisplayOrder]") %>' class="form-group form-group_team-member">
  <label for="lnkPreview_<%# Eval("[DisplayOrder]") %>"><a id="lnkPreview_<%# Eval("[DisplayOrder]") %>" href="https://slipcash.agilesite.com/$<%# Eval("[DisplayAssociatedHandle]") %>" class="people-pay__btn" name="lnkPreview_<%# Eval("[DisplayOrder]") %>"><%# Eval("[DisplayAssociatedHandle]") %></a></label>
  <input id="txtHandle_<%# Eval("[DisplayOrder]") %>" type="text" placeholder="enter handle" name="txtHandle_<%# Eval("[DisplayOrder]") %>" value='<%# Eval("[AssociatedHandle]") %>' onkeyup="checkHandle('<%# Eval("[DisplayOrder]") %>', event);"/>
     <a id="lnkUpdate_<%# Eval("[DisplayOrder]") %>" title="Update" href="#" onclick="doSaveAssociatedAccount('<%# Eval("[DisplayOrder]") %>', event);" name="lnkUpdate_<%# Eval("[DisplayOrder]") %>">
	<i class="fa-sharp fa-solid fa-circle-check"></i> Save
  </a>
  <a id="lnkCancel_<%# Eval("[DisplayOrder]") %>" title="Cancel" href="#" onclick="doCancelSaveAssociatedAccount('<%# Eval("[DisplayOrder]") %>', event);" name="lnkCancel_<%# Eval("[DisplayOrder]") %>">
	<i class="fa-sharp fa-solid fa-circle-x"></i> Cancel
  </a>
  <div class="ui-icons">
  <a id="lnkEdit_<%# Eval("[DisplayOrder]") %>" title="Edit" href="#" onclick="doEditAssociatedAccount('<%# Eval("[DisplayOrder]") %>', event);" name="lnkEdit_<%# Eval("[DisplayOrder]") %>">
	<i class="fa-duotone fa-user-pen"></i>
  </a>
    <a id="lnkEnable_<%# Eval("[DisplayOrder]") %>" class="" title="<%# IfCompare(Eval("IsEnabled"), "0", "Disable", "Enable")%>" href="#" onclick="doEnableAssociatedAccount('<%# Eval("[DisplayOrder]") %>', '<%# IfCompare(Eval("IsEnabled"), "0", "false", "true")%>', event);" name="lnkEnable_<%# Eval("[DisplayOrder]") %>">
	<i class="<%# IfCompare(Eval("IsEnabled"), "0", "fa-solid fa-user-large-slash", "fa-solid fa-user-large")%>"></i>
    </a>
    <a id="lnkDelete_<%# Eval("[DisplayOrder]") %>" class="" title="Delete" href="#" onclick="doDeleteAssociatedAccount('<%# Eval("[DisplayOrder]") %>', event);" name="lnkDelete_<%# Eval("[DisplayOrder]") %>">
	 <i class="fa-sharp fa-regular fa-trash"></i>
  </a>
  </div>
  <input type='hidden' id='hdnrow_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[DisplayOrder]") %>' />
  <input type='hidden' id='hdnitemid_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[AssociatedAccountID]") %>' />
  <input type='hidden' id='hdnmemberuserid_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[MemberUserID]") %>' />
  <input type='hidden' id='hdnmemberid_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[MemberID]") %>' />
  <input type='hidden' id='hdnmemberhandle_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[MemberHandle]") %>' />
  <input type='hidden' id='hdnassociateduserid_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[AssociatedUserID]") %>' />
  <input type='hidden' id='associatedmemberid_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[AssociatedMemberID]") %>' />
  <input type='hidden' id='hdnassociatedmemberhandle_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[AssociatedHandle]") %>' />
  <input type='hidden' id='hdnisenabled_<%# Eval("[DisplayOrder]") %>' value='<%# Eval("[IsEnabled]") %>' />
</div>
<div id='div_validationmsg_<%# Eval("[DisplayOrder]") %>'></div>


<script type="text/javascript">

    var flag = false;
    var debounceTimer;

    $(document).ready(function () {

        if (flag == false) {
            // hide on the input
            $('input[type=text][id*=' + 'txtHandle' + ']').each(function () {
                var id = this.id;
                var name = this.name;
                if (name != 'txtHandle_9999' && name != 'txtHandle_9999') {
                    setControlVisibility(name, 'false');
                }
            });

            // hide all the check button
            $('a[id*=' + 'lnkUpdate' + ']').each(function () {
                var id = this.id;
                var name = this.name;
                if (name != 'lnkUpdate_9999' && name != 'lnkUpdate_9999') {
                    setControlVisibility(name, 'false');
                }
            });

            // hide all the cancel button
            $('a[id*=' + 'lnkCancel_' + ']').each(function () {
                var id = this.id;
                var name = this.name;
                if (name != 'lnkCancel_9999' && name != 'lnkCancel_9999') {
                    setControlVisibility(name, 'false');
                }
            });

            // hide the "add new items"

            // test section
            setControlVisibility('div_team_9999', 'false');
            setControlVisibility('lnkEdit_9999', 'false');
            setControlVisibility('lnkDelete_9999', 'false');
            setControlVisibility('lnkEnable_9999', 'false');

            // live section
            setControlVisibility('div_team_99999', 'false');
            setControlVisibility('lnkEdit_99999', 'false');
            setControlVisibility('lnkDelete_99999', 'false');
            setControlVisibility('lnkEnable_99999', 'false');

            flag = true;
        }
    });

    // do save
    function doSaveAssociatedAccount(name, e) {
        // call the save function and refresh the page...
        if (e != null) {
            e.preventDefault();
        }
        toggleTextBox(name, 'false'); // hide the text box
        setControlVisibility('lnkUpdate_' + name, 'false'); // hide the update button
        setControlVisibility('lnkCancel_' + name, 'false'); // hide the update button
        if (name != '99999') {
            setControlVisibility('lnkEdit_' + name, 'true'); // show the edit button
            setControlVisibility('lnkDelete_' + name, 'true'); // show the delete button
            setControlVisibility('lnkEnable_' + name, 'true'); //show the enable button
        }
        PerformSingleSaveAction(name);
    }

    // do cancel
    function doCancelSaveAssociatedAccount(name, e) {
        // call the save function and refresh the page...
        if (e != null) {
            e.preventDefault();
        }
        toggleTextBox(name, 'false'); // hide the text box
        if (name === '99999' || name === '9999') {
            var fullname = 'div_team_' + name;
            setControlVisibility(fullname, 'false'); // show the add new div
        }
        setControlVisibility('lnkUpdate_' + name, 'false'); // hide the update button
        setControlVisibility('lnkCancel_' + name, 'false'); // hide the update button
        if (name != '99999') {
            setControlVisibility('lnkEdit_' + name, 'true'); // show the edit button
            setControlVisibility('lnkDelete_' + name, 'true'); // show the delete button
            setControlVisibility('lnkEnable_' + name, 'true'); //show the enable button
        }
        //PerformSingleSaveAction(name);
    }


    // show the text box.
    function doEditAssociatedAccount(name, e) {
        if (e != null) {
            e.preventDefault();
        }
        toggleTextBox(name, 'true'); // show the text box
        setControlVisibility('lnkUpdate_' + name, 'false'); // show the "check" icon
        setControlVisibility('lnkCancel_' + name, 'true'); // hide the update button
        setControlVisibility('lnkEdit_' + name, 'false'); // show the edit button
        setControlVisibility('lnkDelete_' + name, 'false'); // show the delete button
        setControlVisibility('lnkEnable_' + name, 'false'); //show the enable button
        checkHandle(name, e);
    }

    function doEnableAssociatedAccount(name, enable, e) {
        if (e != null) {
            e.preventDefault();
        }
        var action = 'enable'
        if (enable == 'true') {
            action = 'enable';
        }
        else {
            action = 'disable';
        }
        PerformSingleSaveAction(name, action);
    }

    function doDeleteAssociatedAccount(name, e) {
        if (e != null) {
            e.preventDefault();
        }
        PerformSingleSaveAction(name, 'delete');
    }

    // Show/Hide add new section
    function toggleAddNew(name, show, e) {
        if (e != null) {
            e.preventDefault();
        }
        var fullname = 'div_team_' + name;
        setControlVisibility(fullname, show); // show the add new div
        toggleTextBox(name, 'true'); // show the text box
        setControlVisibility('lnkUpdate_' + name, 'true'); // show the "check" icon
        checkHandle(name, e);
    }

    // toggle input textbox visibility
    function toggleTextBox(name, show) {
        var fullname = 'txtHandle_' + name;
        setControlVisibility(fullname, show);
    }

    // save new clicked (not used)
    function saveNew_Clicked() {
        PerformSaveAction('addnew', 'hdnrow', 'hdnPostbackUrl')
    }

    function savebutton_Clicked() {
        PerformSavePayProfileAction('ddlManageActionTest', 'hdnrow', 'hdnPostbackUrl')
    }

    if (typeof PerformSingleSaveAction !== 'function' || typeof PerformSingleSaveAction === 'undefined') {
        // perform the selected action
        function PerformSingleSaveAction(name, action, postbackUrlName) {

            var postbackUrl = getControlValue(postbackUrlName);
            postbackUrl = window.location.href.split("?")[0];

            if (typeof (action) == 'undefined') {
                action = 'update';
            }

            if (typeof (postbackUrl) == 'undefined' || postbackUrl == '') {
                return false; // we can't proceed no postback url defined
            }

            if (typeof (action) == 'undefined' || action == '') {
                return false; // we can't proceed no action defined
            }

            //{"Action":"test",
            //"AccountData":
            //[
            //{"AssociatedAccountID":"1", "MemberUserID":"1", "MemberID": "1", "MemberHandle":"1", "AssociatedUserID":"1", "AssociatedMemberID":"1", "AssoociatedMemberHandle":"1",IsEnabled":"true" },
            //{"AssociatedAccountID":"1", "MemberUserID":"1", "MemberID": "1", "MemberHandle":"1", "AssociatedUserID":"1", "AssociatedMemberID":"1", "AssoociatedMemberHandle":"1",IsEnabled":"true" },
            //{"AssociatedAccountID":"1", "MemberUserID":"1", "MemberID": "1", "MemberHandle":"1", "AssociatedUserID":"1", "AssociatedMemberID":"1", "AssoociatedMemberHandle":"1",IsEnabled":"true" },
            //]
            //}

            var selectedid = "";
            if (selectedid.length > 0) {
                selectedid += ",";
            }
            var itemid = getControlValue('hdnitemid_' + name);
            var memberuserid = getControlValue('hdnmemberuserid_' + name);
            var memberid = getControlValue('hdnmemberid_' + name);
            var memberhandle = getControlValue('hdnmemberhandle_' + name);
            var associateduserid = getControlValue('hdnassociateduserid_' + name);
            var associatedmemberid = getControlValue('hdnassociatedmemberid_' + name);
            var associatedmemberhandle = getControlValue('txtHandle_' + name);
            var isenabled = getControlValue('hdnisenabled_' + name);

            if (action == 'enable') {
                isenabled = true;
            }
            else if (action == 'disable') {
                isenabled = false;
            }

            var assignmentdata = '{"AssociatedAccountID":"' + itemid
                + '","MemberUserID":"' + memberuserid
                + '","MemberID":"' + memberid
                + '","MemberHandle":"' + memberhandle
                + '","AssociatedUserID":"' + associateduserid
                + '","AssociatedMemberID":"' + associatedmemberid
                + '","AssociatedMemberHandle":"' + associatedmemberhandle
                + '","IsEnabled":"' + isenabled + '"}';
            selectedid += assignmentdata;

            if (selectedid.length > 0) {
                if (action == 'enable' || action == 'disable' || action == 'delete') {
                    // do confirmation.
                    var actionstring = 'Are you sure you want to ' + action + ' this team member?'
                    if (!confirm(actionstring)) {
                        return false;
                    }
                }

                // submit the payload
                var payload = '{"Action":"' + action + '","AccountData":[' + selectedid + ']}';
                var transResponse = JSON.parse(payload);
                RedirectAndPostJSON(postbackUrl, transResponse);
                return true;
            }

            return false;
        }
    }

    if (typeof PerformSaveAction !== 'function' || typeof PerformSaveAction === 'undefined') {
        // perform the selected action
        function PerformSaveAction(id, checkboxName, postbackUrlName) {

            var action = getControlValue(actionDropdownName);
            var postbackUrl = getControlValue(postbackUrlName);
            var cbname = checkboxName + '_';
            postbackUrl = window.location.href.split("?")[0];

            action = "update";

            if (typeof (postbackUrl) == 'undefined' || postbackUrl == '') {
                return false; // we can't proceed no postback url defined
            }

            if (typeof (action) == 'undefined' || action == '') {
                return false; // we can't proceed no action defined
            }

            //{"Action":"test",
            //"AccountData":
            //[
            //{"AssociatedAccountID":"1", "MemberUserID":"1", "MemberID": "1", "MemberHandle":"1", "AssociatedUserID":"1", "AssociatedMemberID":"1", "AssoociatedMemberHandle":"1",IsEnabled":"true" },
            //{"AssociatedAccountID":"1", "MemberUserID":"1", "MemberID": "1", "MemberHandle":"1", "AssociatedUserID":"1", "AssociatedMemberID":"1", "AssoociatedMemberHandle":"1",IsEnabled":"true" },
            //{"AssociatedAccountID":"1", "MemberUserID":"1", "MemberID": "1", "MemberHandle":"1", "AssociatedUserID":"1", "AssociatedMemberID":"1", "AssoociatedMemberHandle":"1",IsEnabled":"true" },
            //]
            //}

            var selectedid = "";
            $('input[type=hidden][id*=' + cbname + ']').each(function () {
                var checked = this.checked;
                var id = $(this).val(); // this is the row id...
                /*if (checked == true) {*/
                if (selectedid.length > 0) {
                    selectedid += ",";
                }
                var itemid = getControlValue('hdnitemid_' + id);
                var memberuserid = getControlValue('hdnmemberuserid_' + id);
                var memberid = getControlValue('hdnmemberid_' + id);
                var memberhandle = getControlValue('hdnmemberhandle_' + id);
                var associateduserid = getControlValue('hdnassociateduserid_' + id);
                var associatedmemberid = getControlValue('hdnassociatedmemberid_' + id);
                var associateduserhandle = getControlValue('input_' + id);
                var isenabled = getControlValue('hdnisenabled_' + id);
                var assignmentdata = '{"AssociatedAccountID":"' + itemid
                    + '","MemberUserID":"' + memberuserid
                    + '","MemberID":"' + memberid
                    + '","MemberHandle":"' + memberhandle
                    + '","AssociatedUserID":"' + associateduserid
                    + '","AssociatedMemberID":"' + associatedmemberid
                    + '","AssociatedMemberHandle":"' + associatedmemberhandle
                    + '","IsEnabled":"' + isenabled + '"}';
                selectedid += assignmentdata;
            });

            if (selectedid.length > 0) {
                //var actionstring = 'Are you sure you want to ' + action + ' your information?'
                //if (confirm(actionstring)) {
                var payload = '{"Action":"' + action + '","AccountData":[' + selectedid + ']}';
                var transResponse = JSON.parse(payload);
                RedirectAndPostJSON(postbackUrl, transResponse);
                return true;
                //}
            }

            return false;
        }
    }
</script>

<!-- look up scripts -->
<script>

    function checkHandle(name, e) {

        if (e != null) {
            e.preventDefault();
        }

        // Regular expression for allowed characters
        var regex = /^[a-zA-Z0-9_]*$/;
        //var inputVal = $(this).val(); // this is uncessary?
        var inputname = '#txtHandle_' + name;
        var updatelinkname = '#lnkUpdate_' + name;

        var messagedivname = '#div_validationmsg_' + name;
        var statusdivname = '#div_status_' + name;
        var autocompletedivname = '#div_autocomplete_' + name;
        var matcheddivname = '#div_autocomplete_' + name;


        //if (!regex.test(inputVal)) {
        //    // Remove the last character if it's not allowed
        //    $(this).val(inputVal.slice(0, -1));
        //    return;
        //}

        // Update the label with the mirrored username
        //if (inputVal.length > 0) {
        //    $('#memberidLabel').text(inputVal);
        //} else {
        //    $('#memberidLabel').empty(); // Clear the label if input is empty
        //}

        var lengthCheck = $(inputname).val().trim();
        if (lengthCheck.length === 0) {
            setControlVisibility('lnkUpdate_' + name, 'false'); // hide the save icon
            setControlVisibility('lnkCancel_' + name, 'true'); // show the cancel icon
        }

        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () {
            var handle = $(inputname).val().trim();
            if (handle.length === 0) {
                //$('#icon').empty();
                $(messagedivname).empty();
                return;
            }

            // Construct the URL with the username as a query parameter
            var checkUrl = 'https://slipcash.agilesite.com/secure/associated-accounts/check-handle?handle=' + encodeURIComponent(handle);

            // Send a POST request to check the username availability
            $.post(checkUrl, function (response) {
                // Check the response for specific keywords within HTML
                var rmessage = $(response).find('#rmessage');
                var rstatus = $(response).find('#rstatus');
                var rautocomplete = $(response).find('#rautocomplete');
                var rmatched = $(response).find('#rmatched');

                var message = '';
                var status = '';
                var autocompletedata = '';
                var matched = '';

                if (rmessage != null) {
                    message = rmessage.val();
                }

                if (rstatus != null) {
                    status = rstatus.val();
                }

                if (rautocomplete != null) {
                    autocompletedata = rautocomplete.val();
                }

                if (rmatched != null) {
                    matched = rmatched.val();
                }

                $(messagedivname).text(message);
                $(statusdivname).text(status);
                $(autocompletedivname).text(autocompletedata);
                $(matcheddivname).text(matched);



                //var availableTags = autocompletedata;
                //availableTags = ["1", "2", "3"]; 
                var data1 = JSON.parse(autocompletedata);
                $(inputname).autocomplete({
                    source: data1,
                    autofocus: true,
                    disable: false,
                    close: function (event, ui) {
                        checkHandle(name, event);
                    },
                    change: function (event, ui) {
                        checkHandle(name, event);
                    },
                    messages: {
                        //noResults: 'no results',
                        noResults: '',
                        results: function (amount) {
                            //return amount + 'results.';
                            return '';
                        }
                    }
                }); // this turns it off because we are passing string?

                /*$(inputname).attr("autocomplete", "on");*/
                //$(inputname).autocomplete({ source: data1 });

                //if (status === 'valid') {
                //    if (matched === 'matched') { }
                //}

                //setControlVisibility('lnkUpdate_' + name, 'true'); // show the "check" icon
                //setControlVisibility('lnkCancel_' + name, 'false'); // hide the update button
                //setControlVisibility('lnkEdit_' + name, 'false'); // show the edit button
                //setControlVisibility('lnkDelete_' + name, 'false'); // show the delete button
                //setControlVisibility('lnkEnable_' + name, 'false'); //show the enable button

                if (matched === 'no matched') {
                    $(messagedivname).text('the slipcash use does not exists.');
                    //$(updatelinkname).hide();
                    //$(updatelinkname).css('visibility', 'hidden');
                    setControlVisibility('lnkUpdate_' + name, 'false');
                    setControlVisibility('lnkCancel_' + name, 'true');
                    //$(messagedivname).text(message);
                    //$(statusdivname).text(status);
                    //$(autocompletedivname).text(autocompletedata);
                    //$(matcheddivname).text(matched);
                } else if (matched === 'matched') {
                    $(messagedivname).text('You are good to go.');
                    //$(updatelinkname).show();
                    //$(updatelinkname).css('visibility', 'visible');
                    setControlVisibility('lnkUpdate_' + name, 'true');
                    setControlVisibility('lnkCancel_' + name, 'false');
                    //$(messagedivname).text(message);
                    //$(statusdivname).text(status);
                    //$(autocompletedivname).text(autocompletedata);
                    //$(matcheddivname).text(matched);
                }
                else if (status === 'OK') {
                    // this never happens (code from another app)
                    $('#usermessage').text('found');
                    setControlVisibility('')
                    $(updatelinkname).show();
                    $(updatelinkname).css('visibility', 'visible');
                    //$(messagedivname).text(message);
                    //$(statusdivname).text(status);
                    //$(autocompletedivname).text(autocompletedata);
                    //$(matcheddivname).text(matched);
                }
                else {
                    // do nothing.
                }
            }).fail(function () {
                // Handle any errors (e.g., server not responding)
                //$('#icon').html('<i class="fas fa-exclamation-triangle orange"></i>');
                $('#message').text('We are experiencing technical difficulty, please contact support at support@wte.net');
            });
        }, 100); // Wait for 1 second after user stops typing
    };

    //$(document).ready(function () {
    //    var debounceTimer;
    //    $('#btnRegister').hide();
    //    $('#btnRegister').css('visibility', 'hidden');

    //    $('#memberid').on('keyup', function (event) {
    //        // Regular expression for allowed characters
    //        var regex = /^[a-zA-Z0-9_]*$/;

    //        var inputVal = $(this).val();
    //        //if (!regex.test(inputVal)) {
    //        //    // Remove the last character if it's not allowed
    //        //    $(this).val(inputVal.slice(0, -1));
    //        //    return;
    //        //}

    //        // Update the label with the mirrored username
    //        //if (inputVal.length > 0) {
    //        //    $('#memberidLabel').text(inputVal);
    //        //} else {
    //        //    $('#memberidLabel').empty(); // Clear the label if input is empty
    //        //}

    //        clearTimeout(debounceTimer);
    //        debounceTimer = setTimeout(function () {
    //            var username = $('#memberid').val().trim();
    //            if (username.length === 0) {
    //                //$('#icon').empty();
    //                $('#usermessage').empty();
    //                return;
    //            }

    //            // Construct the URL with the username as a query parameter
    //            var checkUrl = 'https://kofcnc.agilesite.com/wip/registration/member-lookup?mbn=' + encodeURIComponent(username);

    //            // Send a POST request to check the username availability
    //            $.post(checkUrl, function (response) {
    //                // Check the response for specific keywords within HTML
    //                var rmessage = $(response).find('#rmessage');
    //                var rstatus = $(response).find('#rstatus');
    //                var rmembername = $(response).find('#rmembername');

    //                var message = '';
    //                var status = '';
    //                var membername = '';

    //                if (rmessage != null) {
    //                    message = rmessage.val();
    //                }

    //                if (rstatus != null) {
    //                    status = rstatus.val();
    //                }

    //                if (rmembername != null) {
    //                    membername = rmembername.val();
    //                }

    //                $('#message').text(message);
    //                $('#status').text(status);
    //                $('#membername').text(membername);

    //                if (status === 'NOT FOUND') {
    //                    $('#usermessage').text('We are unabled to locate your membership information, please verify your membership number.');
    //                    $('#btnRegister').hide();
    //                    $('#btnRegister').css('visibility', 'hidden');
    //                    //$('#message').text(message);
    //                    //$('#status').text(status);
    //                    //$('#membername').text(membername);
    //                } else if (status === 'FOUND') {
    //                    $('#usermessage').text('Welcome Brother ' + membername + '. <br />We can not register you to the site at this time. It looks like you already have an account, please contact support at info@kofcnc.org.');
    //                    $('#btnRegister').hide();
    //                    $('#btnRegister').css('visibility', 'hidden');
    //                    //$('#message').text(message);
    //                    //$('#status').text(status);
    //                    //$('#membername').text(membername);
    //                }
    //                else if (status === 'OK') {
    //                    $('#usermessage').text('Welcome Brother ' + membername + '. You may proceed to registration.');
    //                    $('#btnRegister').show();
    //                    $('#btnRegister').css('visibility', 'visible');
    //                    //$('#message').text(message);
    //                    //$('#status').text(status);
    //                    //$('#membername').text(membername);
    //                }
    //                else {
    //                    // do nothing.
    //                }
    //            }).fail(function () {
    //                // Handle any errors (e.g., server not responding)
    //                //$('#icon').html('<i class="fas fa-exclamation-triangle orange"></i>');
    //                $('#message').text('We are experiencing technical difficulty, please contact support at info@kofcnc.org.');
    //            });
    //        }, 1000); // Wait for 1 second after user stops typing
    //    });
    //});
</script>

<!-- start common script -->
<script type="text/javascript">

    if (typeof loadScript !== 'function' || typeof loadScript === 'undefined') {
        loadScript = function loadScript(url, callback) {
            // Adding the script tag to the head as suggested before
            var head = document.getElementsByTagName('head')[0];
            var script = document.createElement('script');
            script.type = 'text/javascript';
            script.src = url;

            // Then bind the event to the callback function.
            // There are several events for cross browser compatibility.
            script.onreadystatechange = callback;
            script.onload = callback;

            // Fire the loading
            head.appendChild(script);
        }
    }

    if (typeof GetClientID !== 'function' || typeof GetClientID === 'undefined') {
        GetClientID = function GetClientID(id, context) {
            var el = $("#" + id, context);
            if (el.length < 1)
                el = $("[id$=_" + id + "]", context);
            return el;
        }
    }

    if (typeof GetControl !== 'function' || typeof GetControl === 'undefined') {
        GetControl = function GetControl(id, context) {
            var ctrl = document.getElementById(id);
            if (ctrl == null) {
                // it's a server control or doesn't exist on this page
                // try looking for it with the client id
                var clientid = GetClientID(id).attr("id");
                ctrl = document.getElementById(clientid);
            }
            return ctrl;
        }
    }

    if (typeof getControlValue !== 'function' || typeof getControlValue === 'undefined') {
        // Get value from a control (if exist)
        getControlValue = function getControlValue(name) {
            var val = null;
            var n = "#" + name;
            if ($(n) != null) {
                val = $(n).val();
            }
            return val;
        }
    }

    if (typeof isCheckBoxChecked !== 'function' || typeof isCheckBoxChecked === 'undefined') {
        // check to see if a check box is checked (also works for radio button)
        function isCheckBoxChecked(name) {
            var ret = false;
            var n = "#" + name;
            if ($(n) != null) {
                ret = $(n).is(':checked');
            }
            return ret;
        }
    }

    if (typeof checkCheckBox !== 'function' || typeof checkCheckBox === 'undefined') {
        // check a check box (also works for radio button)
        function checkCheckBox(name) {
            setCheckBoxValue(name, true);
        }
    }

    if (typeof uncheckCheckBox !== 'function' || typeof uncheckCheckBox === 'undefined') {

        // uncheck a check box (also works for radio button)
        function uncheckCheckBox(name) {
            setCheckBoxValue(name, false);
        }
    }

    if (typeof setCheckBoxValue !== 'function' || typeof setCheckBoxValue === 'undefined') {
        // set the "check" property (also works for radio button)
        function setCheckBoxValue(name, check) {
            var n = "#" + name;
            if ($(n) != null) {
                $(n).prop('checked', check);
            }
        }
    }

    if (typeof doselectall !== 'function' || typeof doselectall === 'undefined') {
        // do select all
        function doselectall(ctrlName, checkboxName) {
            var cbname = checkboxName + '_';
            var name = ctrlName;
            if (isCheckBoxChecked(name)) {
                $('input[type=checkbox][id*=' + cbname + ']').each(function () {
                    var id = $(this).attr("id");
                    checkCheckBox(id);
                });
            }
            else {
                $('input[type=checkbox][id*=' + cbname + ']').each(function () {
                    var id = $(this).attr("id");
                    uncheckCheckBox(id);
                });
            }
        }
    }

    if (typeof setControlVisibility !== 'function' || typeof setControlVisibility === 'undefined') {
        // show or hide control
        setControlVisibility = function setControlVisibility(name, show) {
            var n = "#" + name;

            if (typeof (show) == 'undefined' || show == '') {
                show = 'true';
            }

            if ($(n) != null) {
                if (show == 'true') {
                    $(n).show();
                    $(n).css('visibility', 'visible');
                }
                else {
                    $(n).hide();
                    $(n).css('visibility', 'hidden');
                }
            }
        }
    }

</script>

<script>
//jQuery(function($){
//  $(document).ajaxSend(function() {
//    $("#overlay").fadeIn(300);　
//  });		
//  $('#button').click(function(){
//    $.ajax({
//      type: 'GET',
//      success: function(data){
//        console.log(data);
//      }
//    }).done(function() {
//      setTimeout(function(){
//        $("#overlay").fadeOut(300);
//      },500);
//    });
//  });	
//});
</script>
<script type="text/javascript">
    if (typeof RedirectAndPostJSON !== 'function' || typeof RedirectAndPostJSON === 'undefined') {
        // post convert json to form data to page
        function RedirectAndPostJSON(postUrl, data) {
            sendJSON(postUrl, data);
        }
    }

    if (typeof sendJSON !== 'function' || typeof sendJSON === 'undefined') {
        function sendJSON(url, data) {
            $("#overlay").fadeIn(300);　
            xhr = new XMLHttpRequest();
            xhr.open("POST", url, true);
            xhr.setRequestHeader("Content-type", "application/json");
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    // do nothing, already posted?
                }
            }
            var data = JSON.stringify(data);
            xhr.send(data);
            xhr.onload = function () {
                //var responseobj = xhr.response;
                //alert(responseobj);
                $("#overlay").fadeOut(300);
                setTimeout(redirectToSelf, 100);
            }
        }
    }
    if (typeof redirectToSelf !== 'function' || typeof redirectToSelf === 'undefined') {
        function redirectToSelf() {
            var href = $(location).attr('href');
            window.location.replace(href);
        }
    }
</script>

