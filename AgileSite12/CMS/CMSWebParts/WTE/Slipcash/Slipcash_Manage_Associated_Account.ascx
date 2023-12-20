<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Slipcash_Manage_Associated_Account.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.Slipcash.Slipcash_Manage_Associated_Account" %>

<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<input type="hidden" id="hdnPostbackUrl" value="https://www.trainingnetworknow.com/System/TestMacros" />

<script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
<script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>



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
        <button class="modal-toggle btn btn-dark">add new</button>
    </div>
    <script>
        // Quick & dirty toggle to demonstrate modal toggle behavior
        $('.modal-toggle').on('click', function (e) {
            e.preventDefault();
            $('.modal').toggleClass('is-visible');
        });
    </script>

</div>


<div id="divManageAction" class="filter-box" runat="server" visible="false">
    <div class="inline-form-group">
        <select id="ddlManageAction" name="ddlManageAction" class="select-css">
            <option value="">--none--</option>
            <option value="enable">enable</option>
            <option value="disable">disable</option>
        </select>
    </div>
    <div class="inline-form-group">
        <label for="chkManageSelectAll">select all:</label><input type="checkbox" id="chkManageSelectAll" onchange="chkManageSelectAll_checkchanged();return false;" title="select all" />
    </div>
    <div class="inline-form-group">
        <button onclick="btnPerformMultipleManageAction_Clicked();return false;" class="btn-admin btn-admin-dark">Submit</button>
    </div>
</div>

<div id="divUpdateAction" class="filter-box" runat="server" visible="false">
    <div class="inline-form-group">
        <select id="ddlCompanyAssignmentAction" name="ddlCompanyAssignmentAction" class="select-css">
            <option value="">--none--</option>
            <option value="assign">save</option>
        </select>
    </div>
    <div class="inline-form-group">
        <label for="chkCompanyAssignmentSelectAll">select all:</label><input type="checkbox" id="chkCompanyAssignmentSelectAll" onchange="chkCompanyAssignmentSelectAll_checkchanged();return false;" title="select all" />
    </div>
    <div class="inline-form-group">
        <button onclick="btnPerformMultipleCompanyAssignmentAction_Clicked();return false;" class="btn-admin btn-admin-dark">Submit</button>
    </div>
</div>

<script type="text/javascript">

    function saveNew_Clicked() {
        PerformSaveAction('addnew', 'hdnrow', 'hdnPostbackUrl')
    }

    function savebutton_Clicked() {
        PerformSavePayProfileAction('ddlManageActionTest', 'hdnrow', 'hdnPostbackUrl')
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
            //{"UserPaymentMethodID":"1", "PaymentTypeID":"1", "MemberID": "1", "PaymentLink":"1", "UserID":"1", "IsEnabled":"true" },
            //{"UserPaymentMethodID":"", "PaymentTypeID":"1", "MemberID": "1", "PaymentLink":"1"  "UserID":"1","IsEnabled":"true" },
            //{"UserPaymentMethodID":"1", "PaymentTypeID":"1", "MemberID": "1", "PaymentLink":"1" "UserID":"1","IsEnabled":"true" }
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
                var assignmentdata = '{"AssociatedAccountID":"' + itemid + '","MemberUserID":"' + memberuserid + '","MemberID":"' + memberid + '","MemberHandle":"' + memberhandle + '","AssociatedUserID":"' + associateduserid + '","AssociatedMemberID":"' + associatedmemberid + '","AssociatedMemberHandle":"' + associatedmemberhandle + '","IsEnabled":"' + isenabled + '"}';
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
                setTimeout(redirectToSelf, 1000);
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
