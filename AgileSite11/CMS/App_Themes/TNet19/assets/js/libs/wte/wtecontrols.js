/***********************************************
* wtecontrols.js
* Control helpers
*
* required
* <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
* <script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
***********************************************/


if (typeof makeDatePicker !== 'function' || typeof makeDatePicker === 'undefined') {
    // make input a date picker
    makeDatePicker = function makeDatePicker(name) {
        var n = "#" + name;
        if ($(n) != null) {
            $(n).datepicker();
        }
    }
}

if (typeof clearSearchTermTextBox !== 'function' || typeof clearSearchTermTextBox === 'undefined') {
    // Clear the search box
    clearSearchTermTextBox = function clearSearchTermTextBox(name, text) {

        if (typeof (name) == 'undefined' || name == '') {
            name = 'txtSearch'
        }

        var search = getControlValue(name);
        if (search == text) {
            setControlValue(name, '', '', true);
        }
    }
}

if (typeof fillSearchTermTextBox !== 'function' || typeof fillSearchTermTextBox === 'undefined') {
    // Fill the search box with "water mark"
    fillSearchTermTextBox = function fillSearchTermTextBox(name, text) {

        if (typeof (name) == 'undefined' || name == '') {
            name = 'txtSearch'
        }

        var search = getSearchTerm(name);
        var placeholder = getControlPlaceHolderValue(name);
        if (typeof (placeholder) != 'undefined' && placeholder != text) {
            if (search == '') {
                setControlValue(name, text, '', true);
            }
        }
    }
}

if (typeof getSearchTerm !== 'function' || typeof getSearchTerm === 'undefined') {
    // Get search term
    getSearchTerm = function getSearchTerm(name, excludeText) {
        var ret = getControlValue(name);
        if (ret == excludeText) {
            ret = '';
        }
        return ret;
    }
}

if (typeof getSortID !== 'function' || typeof getSortID === 'undefined') {
    // get sort id
    getSortID = function getSortID(name, sortParamName, sortId, defaultSort) {

        var ret = getParameterByName(name);

        if (typeof (ret) == 'undefined' || ret == '') {
            ret = sortId;
        }
        else {
            if (sortId != '' && typeof (sortId) != 'undefined') {
                if (sortId == ret) {
                    ret = sortId + 'x';
                }
                else {
                    ret = sortId;
                }
            }
        }

        //if (typeof (ret) == 'undefined') {
        //    if (sortId != null) {
        //        ret = sortId;
        //    }
        //    else {
        //        ret = defaultSort;
        //    }
        //}
        //else if (typeof (sortParamName) != 'undefined' && name == sortParamName) {
        //    if (sortId == ret) {
        //        ret = sortId + 'x';
        //    }
        //}

        return ret;
    }
}

if (typeof hideControl !== 'function' || typeof shideControl === 'undefined') {
    // hide a control
    hideControl = function hideControl(name) {
        setControlVisibility(name, 'false');
    }
}

if (typeof showControl !== 'function' || typeof showControl === 'undefined') {
    // show a control
    showControl = function showControl(name) {
        setControlVisibility(name, 'true');
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

if (typeof getControlValueWithDefault !== 'function' || typeof getControlValueWithDefault === 'undefined') {
    // Get Control value with default
    getControlValueWithDefault = function getControlValueWithDefault(name, defaultval) {
        var ret = getControlValue(name);
        if (typeof (ret) == 'undefined' || ret == '' || ret == null) {
            ret = defaultval;
        }
        return ret;
    }
}

if (typeof setControlValue !== 'function' || typeof setControlValue === 'undefined') {
    // Set control value
    setControlValue = function setControlValue(name, value, size, setBlank) {
        var n = "#" + name;

        if (typeof (setBlank) == 'undefined' || setBlank == '') {
            setBlank = false;
        }

        if (typeof (value) != 'undefined' && value != null && (value != '' || setBlank == true)) {
            if ($(n) != null) {
                $(n).val(value);
                setControlFontSize(name, size);
            }
        }
    }
}

if (typeof getControlPlaceHolderValue !== 'function' || typeof getControlPlaceHolderValue === 'undefined') {
    // get value from the 'placeholder' property of a control
    getControlPlaceHolderValue = function getControlPlaceHolderValue(name) {
        var val = null;
        var n = "#" + name;
        if ($(n) != null) {
            val = $(n).attr('placeholder');
        }
        return val;
    }
}

if (typeof setControlPlaceHolderValue !== 'function' || typeof setControlPlaceHolderValue === 'undefined') {
    // get the value of the 'placeholder' property of a control
    setControlPlaceHolderValue = function setControlPlaceHolderValue(name, value) {
        var n = "#" + name;
        if (typeof (value) != 'undefined' && value != null && value != '') {
            if ($(n) != null) {
                $(n).attr('placeholder', value);
            }
        }
    }
}

if (typeof setControlFontSize !== 'function' || typeof setControlFontSize === 'undefined') {
    // Set font
    setControlFontSize = function setControlFontSize(name, size) {
        var n = "#" + name;
        if ($(n) != null) {
            if (typeof (size) != 'undefined' && size != '') {
                $(n).css('font-weight', size);
            }
        }
    }
}

if (typeof selectDropDownOption !== 'function' || typeof selectDropDownOption === 'undefined') {
    // select drop down option
    selectDropDownOption = function selectDropDownOption(name, option, checkzero, checkblank, size) {

        var n = "#" + name;
        if ((checkzero != 'true' || option != '0') && (checkblank != 'true' || option != '')) {
            if ($(n) != null) {
                var optionstring = n + " option[value='" + option + "']";
                $(optionstring).prop('selected', true);
                setControlFontSize(name, size);
            }
        }
    }
}

if (typeof isCheckBoxChecked !== 'function' || typeof isCheckBoxChecked === 'undefined') {
    // check to see if a check box is checked (also works for radio button)
    isCheckBoxChecked = function isCheckBoxChecked(name) {
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
    checkCheckBox = function checkCheckBox(name) {
        setCheckBoxValue(name, true);
    }
}

if (typeof uncheckCheckBox !== 'function' || typeof uncheckCheckBox === 'undefined') {
    // uncheck a check box (also works for radio button)
    uncheckCheckBox = function uncheckCheckBox(name) {
        setCheckBoxValue(name, false);
    }
}

if (typeof setCheckBoxValue !== 'function' || typeof setCheckBoxValue === 'undefined') {
    // set the "check" property (also works for radio button)
    setCheckBoxValue = function setCheckBoxValue(name, check) {
        var n = "#" + name;
        if ($(n) != null) {
            $(n).prop('checked', check);
        }
    }
}

if (typeof setRadioButtonValue !== 'function' || typeof setRadioButtonValue === 'undefined') {
    // select radio button value
    setRadioButtonValue = function setRadioButtonValue(name, value) {
        var iname = "input[name='" + name + "'][value='" + value + "']";
        if ($(iname) != null) {
            $(iname).prop('checked', true);
        }
    }
}

if (typeof getRadioButtonValue !== 'function' || typeof getRadioButtonValue === 'undefined') {
    // get radio button value
    getRadioButtonValue = function getRadioButtonValue(name) {
        var ret = '';
        var iname = "input:radio[name=" + name + "]:checked"
        if ($(iname) != null) {
            ret = $(iname).val();
        }
        return ret;
    }
}