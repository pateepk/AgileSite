
// Dialog Alert With JQUERY (BEGIN)
var DialogAlertDivName = 'dialogAlert';

function OpenDialogAlert(message) {
    var checkDIV = document.getElementById(DialogAlertDivName);
    if (!checkDIV) {
        $('body').append('<div id="' + DialogAlertDivName + '" style="display:none; text-align:center; " title="Alert"><span id="' + DialogAlertDivName + 'Text"></span></div>');
    }
    $('#' + DialogAlertDivName).dialog("close"); // close previous dialog
    $('#' + DialogAlertDivName).dialog({
        modal: true
    });
    var strHTML = '<input type="button" onclick="CloseDialogAlert();" class="ui-button ui-widget ui-corner-all" value="Close Window" />';
    $('#' + DialogAlertDivName + 'Text').html('<b>' + message + '</b><br>' + strHTML);
}

function CloseDialogAlert() {
    $('#' + DialogAlertDivName).dialog("close"); // close previous dialog
}

// Dialog Alert With JQUERY (END)

// Dialog Confirm With JQUERY (BEGIN)
var DialogConfirmDivName = 'dialogConfirm';
var DialogConfirmCallbackString = '';

function OpenDialogConfirm(message, CallbackString) {
    
    var checkDIV = document.getElementById(DialogConfirmDivName);
    if (!checkDIV) {
        $('body').append('<div id="' + DialogConfirmDivName + '" style="display:none; text-align:center; " title="Confirm"><span id="' + DialogConfirmDivName + 'Text"></span></div>');
    }
    $('#' + DialogConfirmDivName).dialog("close"); // close previous dialog
    $('#' + DialogConfirmDivName).dialog({
        modal: true
    });
    DialogConfirmCallbackString = CallbackString;
    var strHTML = '<input type="button" onclick="CloseDialogConfirmOK();" class="ui-button ui-widget ui-corner-all" value="OK" />';
    strHTML += '<input type="button" onclick="CloseDialogConfirm();" class="ui-button ui-widget ui-corner-all" value="Cancel" />';
    $('#' + DialogConfirmDivName + 'Text').html('<b>' + message + '</b><br>' + strHTML);

}

function CloseDialogConfirmOK()
{
    if (DialogConfirmCallbackString.length > 0) {
        setTimeout(DialogConfirmCallbackString, 0);
    }
    DialogConfirmCallbackString = '';
    $('#' + DialogConfirmDivName).dialog("close");
}

function CloseDialogConfirm() {
    $('#' + DialogConfirmDivName).dialog("close"); // close previous dialog
}

// Dialog Confirm With JQUERY (END)

function WSAddParameter(varname, value) {
    return '&' + varname + '=' + value;
}

function WSGetJSONObjectFromReturn(data) {
    var obj;
    if (data.text) {
        try {
            obj = eval('(' + data.text + ')');
        } catch (err) { }
    } else { // chrome version LOL
        var $response = $(data);
        try {
            var s = $response.text();
            obj = eval('(' + s + ')');
        } catch (err) { }
    }
    return obj;
}
