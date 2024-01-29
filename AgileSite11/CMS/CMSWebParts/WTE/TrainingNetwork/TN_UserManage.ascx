<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_UserManage.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_UserManage" %>
<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<input type="hidden" id="hdnPostbackUrl" value="https://www.trainingnetworknow.com/System/TestMacros" />

<script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
<script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>

<div id="divtest" runat="server" visible="false">
    <!-- Test Scripts -->
    <script type="text/javascript">
        function btnTest_Clicked() {
            PerformMultipleAction('ddlUserActionTest', 'userselecttest', 'hdnPostbackUrl')
        }

        function btnTest2_Clicked() {
            PerformSingleAction('enable', '1001', 'hdnPostbackUrl')
        }

        function userselecttest_checkchanged() {
            doselectall('userselecttest_all', 'userselecttest');
        }
    </script>
    1:
    <input type="checkbox" id="userselecttest_1" checked="checked" value="1" />
    2:
    <input type="checkbox" id="userselecttest_2" checked="checked" value="2" />
    3:
    <input type="checkbox" id="userselecttest_3" checked="checked" value="3" />

    <div class="inline-form-group">
        <label for="ddlUserActionTest">Select an Action:</label>
        <select id="ddlUserActionTest" name="ddlUserActionTest" class="select-css">
            <option value="">--none--</option>
            <option value="enable">enable</option>
            <option value="disable">disable</option>
        </select>
    </div>
    <div class="inline-form-group">
        <label for="userselecttest_all">select all:</label><input type="checkbox" id="userselecttest_all" onchange="userselecttest_checkchanged();return false;" />
    </div>
    <div class="inline-form-group">
        <button onclick="btnTest_Clicked();return false;" class="btn-admin btn-admin-dark">Test</button>
        <button onclick="btnTest2_Clicked();return false;" class="btn-admin btn-admin-dark">Test2</button>
    </div>
</div>
<div id="divAction" class="filter-box" runat="server" visible="false">
    <div class="inline-form-group">
        <select id="ddlUserAction" name="ddlUserAction" class="select-css">
            <option value="">--none--</option>
            <option value="enable">enable</option>
            <option value="disable">disable</option>
        </select>
    </div>
    <div class="inline-form-group">
        <label for="chkSelectAllUsers">select all:</label><input type="checkbox" id="chkSelectAllUsers" onchange="chkSelectAllUsers_checkchanged();return false;" title="select all" />
    </div>
    <div class="inline-form-group">
        <button onclick="btnPerformMultipleAction_Clicked();return false;" class="btn-admin btn-admin-dark">Submit</button>
    </div>
</div>

<script type="text/javascript">

    // select all
    function chkSelectAllUsers_checkchanged() {
        doselectall('chkSelectAllUsers', 'userselect');
    }

    // button clicked
    function btnPerformMultipleAction_Clicked() {
        PerformMultipleAction('ddlUserAction', 'userselect', 'hdnPostbackUrl')
    }

    if (typeof PerformMultipleAction !== 'function' || typeof PerformMultipleAction === 'undefined') {
        // perform the selected action
        function PerformMultipleAction(actionDropdownName, checkboxName, postbackUrlName) {

            var action = getControlValue(actionDropdownName);
            var postbackUrl = getControlValue(postbackUrlName);
            var cbname = checkboxName + '_';
            postbackUrl = window.location.href.split("?")[0];

            if (typeof (postbackUrl) == 'undefined' || postbackUrl == '') {
                return false; // we can't proceed no postback url defined
            }

            if (typeof (action) == 'undefined' || action == '') {
                return false; // we can't proceed no action defined
            }

            var selectedid = "";
            $('input[type=checkbox][id*=' + cbname + ']').each(function () {
                var checked = this.checked;
                var id = $(this).val();
                if (checked == true) {
                    if (selectedid.length > 0) {
                        selectedid += ",";
                    }
                    selectedid += '"' + id + '"';
                }
            });

            if (selectedid.length > 0) {
                var actionstring = 'Are you sure you want to ' + action + ' all selected users?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","UserID":[' + selectedid + ']}';
                    var transResponse = JSON.parse(payload);
                    RedirectAndPostJSON(postbackUrl, transResponse);
                    return true;
                }
            }

            return false;
        }
    }

    if (typeof PerformSingleAction !== 'function' || typeof PerformSingleAction === 'undefined') {
        // perform the selected action
        function PerformSingleAction(actionName, id, postbackUrlName) {

            var action = actionName;
            var postbackUrl = getControlValue(postbackUrlName);
            //var cbname = checkboxName + '_';
            postbackUrl = window.location.href.split("?")[0];

            if (typeof (postbackUrl) == 'undefined' || postbackUrl == '') {
                return false; // we can't proceed no postback url defined
            }

            if (typeof (action) == 'undefined' || action == '') {
                return false; // we can't proceed no action defined
            }

            if (typeof (id) == 'undefined' || id == '') {
                return false; // id needs to be specified
            }

            var selectedid = '"' + id + '"';

            if (selectedid.length > 0) {
                var actionstring = 'Are you sure you want to ' + action + ' this user?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","UserID":[' + selectedid + ']}';
                    var transResponse = JSON.parse(payload);
                    RedirectAndPostJSON(postbackUrl, transResponse);
                    return true;
                }
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