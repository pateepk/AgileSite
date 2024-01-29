/***********************************************
* wtedata.js
* Data helpers
*
* required
* <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
* <script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
***********************************************/


if (typeof getTextWithSuffix !== 'function' || typeof getTextWithSuffix === 'undefined') {
    // get text with suffix
    getTextWithSuffix = function getTextWithSuffix(text, suffix) {
        if (typeof (suffix) == 'undefined' || suffix == '') {
            suffix = '';
        }
        var ret = text + suffix;
        return ret;
    }
}

if (typeof getTextWithPrefix !== 'function' || typeof getTextWithPrefix === 'undefined') {
    // get text with prefix
    getTextWithPrefix = function getTextWithPrefix(text, prefix) {
        if (typeof (prefix) == 'undefined' || prefix == '') {
            prefix = '';
        }
        var ret = prefix + text;
        return ret;
    }
}

if (typeof getOneOrZero !== 'function' || typeof getOneOrZero === 'undefined') {
    // get "1" or "0" from a boolean value
    getOneOrZero = function getOneOrZero(val, hideFalse) {
        var ret = '0';
        if (val != null) {
            if (getBoolean(val) == true) {
                ret = '1';
            }
        }

        if (ret == '0' && hideFalse == true) {
            ret = '';
        }
        return ret;
    }
}

if (typeof getBoolean !== 'function' || typeof getBoolean === 'undefined') {
    // get boolean from values
    getBoolean = function getBoolean(val) {
        var ret = false;
        if (typeof (val) != 'undefined' && val != null && val != '') {
            if (val == true || val == 1 || val == 'true' || val == 'yes') {
                ret = true;
            }
        }
        return ret;
    }
}