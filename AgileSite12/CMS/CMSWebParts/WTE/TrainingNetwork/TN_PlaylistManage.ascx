<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_PlaylistManage.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_PlaylistManage" %>

<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<input type="hidden" id="hdnPostbackUrl" value="https://www.trainingnetworknow.com/System/TestMacros" />

<script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
<script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>

<div id="divtest" runat="server" visible="false">

    <!-- Test Scripts -->
    <script type="text/javascript">
        function btnTest_Clicked() {
            PerformMultipleManageAction('ddlManageActionTest', 'playlistselecttest', 'hdnPostbackUrl')
        }

        function btnTest2_Clicked() {
            PerformSingleManageAction('enable', '1001', 'hdnPostbackUrl')
        }

        function playlistselectalltest_checkchanged() {
            doselectall('playlistselecttest_all', 'playlistselecttest');
        }
    </script>

    <div class="inline-form-group">
        1:
    <input type="checkbox" id="playlistselecttest_1" checked="checked" value="1" />
        2:
    <input type="checkbox" id="playlistselecttest_2" checked="checked" value="2" />
        3:
    <input type="checkbox" id="playlistselecttest_3" checked="checked" value="3" />
    </div>

    <div class="inline-form-group">
        <label for="ddlManageActionTest">Select an Action:</label>
        <select id="ddlManageActionTest" name="ddlManageActionTest" class="select-css">
            <option value="">--none--</option>
            <option value="enable">enable</option>
            <option value="disable">disable</option>
        </select>
    </div>
    <div class="inline-form-group">
        <label for="playlistselecttest_all">select all:</label><input type="checkbox" id="playlistselecttest_all" onchange="playlistselectalltest_checkchanged();return false;" />
    </div>

    <div class="inline-form-group">
        <button onclick="btnTest_Clicked();return false;" class="btn-admin btn-admin-dark">Test</button>
        <button onclick="btnTest2_Clicked();return false;" class="btn-admin btn-admin-dark">Test2</button>
    </div>
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

<div id="divProductAssigmentAction" class="filter-box" runat="server" visible="false">

    <div class="inline-form-group">
        <select id="ddlProductAssignmentAction" name="divProductAssignmentAction" class="select-css">
            <option value="">--none--</option>
            <option value="assign">assign</option>
            <option value="unassign">unassign</option>
        </select>
    </div>
    <div class="inline-form-group">
        <label for="chkProductAssignmentSelectAll">select all:</label><input type="checkbox" id="chkProductAssignmentSelectAll" onchange="chkProductAssignmentSelectAll_checkchanged();return false;" title="select all" />
    </div>
    <div class="inline-form-group">
        <button onclick="btnPerformMultipleProductAssignmentAction_Clicked();return false;" class="btn-admin btn-admin-dark">Submit</button>
    </div>
</div>

<div id="divCompanyAssignmentAction" class="filter-box" runat="server" visible="false">

    <div class="inline-form-group">
        <select id="ddlCompanyAssignmentAction" name="ddlCompanyAssignmentAction" class="select-css">
            <option value="">--none--</option>
            <option value="assign">assign</option>
            <option value="unassign">unassign</option>
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

    // select all
    function chkManageSelectAll_checkchanged() {
        doselectall('select_all', 'playlistselect');
    }

    // button clicked
    function btnPerformMultipleManageAction_Clicked() {
        PerformMultipleManageAction('ddlManageAction', 'playlistselect', 'hdnPostbackUrl')
    }

    if (typeof PerformMultipleManageAction !== 'function' || typeof PerformMultipleManageAction === 'undefined') {
        // perform the selected action
        function PerformMultipleManageAction(actionDropdownName, checkboxName, postbackUrlName) {

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
                var actionstring = 'Are you sure you want to ' + action + ' all selected playlist?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","PlaylistID":[' + selectedid + ']}';
                    var transResponse = JSON.parse(payload);
                    RedirectAndPostJSON(postbackUrl, transResponse);
                    return true;
                }
            }

            return false;
        }
    }

    if (typeof PerformSingleManageAction !== 'function' || typeof PerformSingleManageAction === 'undefined') {
        // perform the selected action
        function PerformSingleManageAction(actionName, id, postbackUrlName) {

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
                var actionstring = 'Are you sure you want to ' + action + ' this playlist?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","PlaylistID":[' + selectedid + ']}';
                    var transResponse = JSON.parse(payload);
                    RedirectAndPostJSON(postbackUrl, transResponse);
                    return true;
                }
            }

            return false;
        }
    }
</script>

<script type="text/javascript">
    // select all
    function chkProductAssignmentSelectAll_checkchanged() {
        doselectall('select_all', 'productselect');
    }

    // button clicked
    function btnPerformMultipleProductAssignmentAction_Clicked() {
        PerformMultipleProductAssignmentAction('ddlProductAssignmentAction', 'productselect', 'hdnPostbackUrl')
    }

    if (typeof PerformMultipleProductAssignmentAction !== 'function' || typeof PerformMultipleProductAssignmentAction === 'undefined') {
        // perform the selected action
        function PerformMultipleProductAssignmentAction(actionDropdownName, checkboxName, postbackUrlName) {

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

            //{"Action":"test",
            //"ProductAssignmentData":
            //[
            //{"AssignnmentID":"1", "CatalogID":"1", "CategoryID": "1", "VideoID":"1" },
            //{"AssignnmentID":"", "CatalogID":"1", "CategoryID": "1", "VideoID":"1" },
            //{"AssignnmentID":"1", "CatalogID":"1", "CategoryID": "1", "VideoID":"1" }
            //]
            //}

            var selectedid = "";
            $('input[type=checkbox][id*=' + cbname + ']').each(function () {
                var checked = this.checked;
                var id = $(this).val(); // this is the row id...
                if (checked == true) {
                    if (selectedid.length > 0) {
                        selectedid += ",";
                    }
                    var nodeid = getControlValue('hdnNodeID_' + id);
                    var videoid = getControlValue('hdnVideoID_' + id);
                    var assignmentid = getControlValue('hdnAssignmentID_' + id);
                    var assignmentactive = getControlValue('hdnAssignmentActive_' + id);
                    var playlistid = getControlValue('hdPlaylistID_' + id);

                    var assignmentdata = '{"AssignmentID":"' + assignmentid + '","PlaylistID":"' + playlistid + '","VideoID":"' + videoid + '"}';

                    selectedid += assignmentdata;
                }
            });

            if (selectedid.length > 0) {
                var actionstring = 'Are you sure you want to ' + action + ' all selected products/category to this playlist?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","ProductAssignmentData":[' + selectedid + ']}';
                    var transResponse = JSON.parse(payload);
                    RedirectAndPostJSON(postbackUrl, transResponse);
                    return true;
                }
            }

            return false;
        }
    }

    if (typeof PerformSingleProductAssignmentAction !== 'function' || typeof PerformSingleProductAssignmentAction === 'undefined') {
        // perform the selected action
        function PerformSingleProductAssignmentAction(actionName, assignmentid, videoid, playlistid, postbackUrlName) {

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

            if (typeof (playlistid) == 'undefined' || playlistid == '') {
                return false; // playlistid needs to be specified
            }

            if (typeof (videoid) == 'undefined' || videoid == '') {
                return false; // videoid needs to be specified
            }

            if (typeof (assignmentid) == 'undefined' || assignmentid == '') {
                return false; // id needs to be specified
            }

            var actionstring = 'Are you sure you want to ' + action + ' this video from the playlist?'
            if (confirm(actionstring)) {
                var assignmentdata = '{"AssignmentID":"' + assignmentid + '","PlaylistID":"' + playlistid + '","VideoID":"' + videoid + '"}';
                var payload = '{"Action":"' + action + '","ProductAssignmentData":[' + assignmentdata + ']}';
                var transResponse = JSON.parse(payload);
                RedirectAndPostJSON(postbackUrl, transResponse);
                return true;
            }

            return false;
        }
    }
</script>

<script type="text/javascript">

    // select all
    function chkCompanyAssignmentSelectAll_checkchanged() {
        doselectall('select_all', 'productselect');
    }

    // button clicked
    function btnPerformMultipleCompanyAssignmentAction_Clicked() {
        PerformMultipleCompanyAssignmentAction('ddlCompanyAssignmentAction', 'productselect', 'hdnPostbackUrl')
    }

    if (typeof PerformMultipleCompanyAssignmentAction !== 'function' || typeof PerformMultipleCompanyAssignmentAction === 'undefined') {
        // perform the selected action
        function PerformMultipleCompanyAssignmentAction(actionDropdownName, checkboxName, postbackUrlName) {

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

            //{"Action":"test",
            //"CompanyAssignmentData":
            //[
            //{"AssignnmentID":"1", "CompanyID":"1", "CatalogID":"1", "CategoryID": "1", "VideoID":"1" },
            //{"AssignnmentID":"", "CompanyID":"1", "CatalogID":"1", "CategoryID": "1", "VideoID":"1" },
            //{"AssignnmentID":"1", "CompanyID":"1", "CatalogID":"1", "CategoryID": "1", "VideoID":"1" }
            //]
            //}

            var selectedid = "";
            $('input[type=checkbox][id*=' + cbname + ']').each(function () {
                var checked = this.checked;
                var id = $(this).val(); // this is the row id...
                if (checked == true) {
                    if (selectedid.length > 0) {
                        selectedid += ",";
                    }
                    var nodeid = getControlValue('hdnNodeID_' + id);
                    var videoid = getControlValue('hdnVideoID_' + id);
                    var assignmentid = getControlValue('hdnAssignmentID_' + id);
                    var assignmentactive = getControlValue('hdnAssignmentActive_' + id);
                    var catalogid = getControlValue('hdnCatalogID_' + id);
                    var companyid = getControlValue('hdnCompanyID_' + id);

                    var assignmentdata = '{"AssignmentID":"' + assignmentid + '","CompanyID":"' + companyid + '","CatalogID":"' + catalogid + '","CategoryID":"' + nodeid + '","VideoID":"' + videoid + '"}';

                    selectedid += assignmentdata;
                }
            });

            if (selectedid.length > 0) {
                var actionstring = 'Are you sure you want to ' + action + ' all selected catalog/category/videos to this client?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","CompanyAssignmentData":[' + selectedid + ']}';
                    var transResponse = JSON.parse(payload);
                    RedirectAndPostJSON(postbackUrl, transResponse);
                    return true;
                }
            }

            return false;
        }
    }

    if (typeof PerformSingleCompanyAssignmentAction !== 'function' || typeof PerformSingleCompanyAssignmentAction === 'undefined') {
        // perform the selected action
        function PerformSingleCompanyAssignmentAction(actionName, id, postbackUrlName) {

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
                var actionstring = 'Are you sure you want to ' + action + ' this catalog?'
                if (confirm(actionstring)) {
                    var payload = '{"Action":"' + action + '","CatalogID":[' + selectedid + ']}';
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