/***********************************************
* wterequests.js
*
* Request helpers
*
* requires
* <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
* <script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
***********************************************/


if (typeof getParamWithDefault !== 'function' || typeof getParamWithDefault === 'undefined') {
    // get param with default value
    getParamWithDefault = function getParamWithDefault(name, defaultval) {
        var ret = getParameterByName(name);
        if (typeof (ret) == 'undefined' || ret == '' || ret == null) {
            ret = defaultval;
        }
        return ret;
    }
}

if (typeof getParameterByName !== 'function' || typeof getParameterByName === 'undefined') {
    // name value from query string
    getParameterByName = function getParameterByName(name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
        return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    }
}

if (typeof resetStats !== 'function' || typeof resetStats === 'undefined') {
    // remove all param from url
    resetStats = function resetStats() {
        var clearURL = window.location.href.substr(0, window.location.href.indexOf("?"));
        if (!clearURL) {
            clearURL = window.location.href;
        }
        window.location.href = clearURL;
    }
}

if (typeof reloadWithoutQueryString !== 'function' || typeof reloadWithoutQueryString === 'undefined') {
    // reload page without query string
    reloadWithoutQueryString = function reloadWithoutQueryString() {
        window.location = window.location.href.split("?")[0];
    }
}

if (typeof reloadWithQueryStringVars !== 'function' || typeof reloadWithQueryStringVars === 'undefined') {
    // reload with query string (does not add item with empty values)
    reloadWithQueryStringVars = function reloadWithQueryStringVars(queryStringVars) {
        var existingQueryVars = location.search ? location.search.substring(1).split("&") : [],
          currentUrl = location.search ? location.href.replace(location.search, "") : location.href,
            newQueryVars = {},
              newUrl = currentUrl + "?";

        if (existingQueryVars.length > 0) {
            for (var i = 0; i < existingQueryVars.length; i++) {
                var pair = existingQueryVars[i].split("=");
                newQueryVars[pair[0]] = pair[1];
            }
        }

        if (queryStringVars) {
            for (var queryStringVar in queryStringVars) {
                newQueryVars[queryStringVar] = queryStringVars[queryStringVar];
            }
        }

        if (newQueryVars) {
            for (var newQueryVar in newQueryVars) {
                if (typeof (newQueryVars[newQueryVar]) == 'undefined' || newQueryVars[newQueryVar] == '') {
                    // do nothing
                }
                else {
                    newUrl += newQueryVar + "=" + newQueryVars[newQueryVar] + "&";
                }
            }
            newUrl = newUrl.substring(0, newUrl.length - 1);
            window.location.href = newUrl;
        } else {
            window.location.href = location.href;
        }
    }
}

if (typeof reloadWithQueryStringVars2 !== 'function' || typeof reloadWithQueryStringVars2 === 'undefined') {
    // reload with query string (version 2)
    reloadWithQueryStringVars2 = function reloadWithQueryStringVars2(queryStringVars) {
        var existingQueryVars = location.search ? location.search.substring(1).split("&") : [],
          currentUrl = location.search ? location.href.replace(location.search, "") : location.href,
            newQueryVars = {},
              newUrl = currentUrl + "?";
        if (existingQueryVars.length > 0) {
            for (var i = 0; i < existingQueryVars.length; i++) {
                var pair = existingQueryVars[i].split("=");
                newQueryVars[pair[0]] = pair[1];
            }
        }
        if (queryStringVars) {
            for (var queryStringVar in queryStringVars) {
                newQueryVars[queryStringVar] = queryStringVars[queryStringVar];
            }
        }
        if (newQueryVars) {
            for (var newQueryVar in newQueryVars) {
                newUrl += newQueryVar + "=" + newQueryVars[newQueryVar] + "&";
            }
            newUrl = newUrl.substring(0, newUrl.length - 1);
            window.location.href = newUrl;
        } else {
            window.location.href = location.href;
        }
    }
}

if (typeof reloadWithQueryStringVars3 !== 'function' || typeof reloadWithQueryStringVars3 === 'undefined') {
    // reload with query string (version 3)
    reloadWithQueryStringVars3 = function reloadWithQueryStringVars3(queryStringVars) {
        var existingQueryVars = location.search ? location.search.substring(1).split("&") : [],
            currentUrl = location.search ? location.href.replace(location.search, "") : location.href,
            newQueryVars = {},
            newUrl = currentUrl + "?";

        if (existingQueryVars.length > 0) {
            for (var i = 0; i < existingQueryVars.length; i++) {
                var pair = existingQueryVars[i].split("=");
                newQueryVars[pair[0]] = pair[1];
            }
        }
        if (queryStringVars) {
            for (var queryStringVar in queryStringVars) {
                if (typeof queryStringVar[queryStringVar] != 'undefined') {
                    newQueryVars[queryStringVar] = queryStringVars[queryStringVar];
                }
            }
        }
        if (newQueryVars) {
            for (var newQueryVar in newQueryVars) {
                if (typeof newQueryVars[queryStringVar] != 'undefined') {
                    newUrl += newQueryVar + "=" + newQueryVars[newQueryVar] + "&";
                }
            }
            newUrl = newUrl.substring(0, newUrl.length - 1);
            window.location.href = newUrl;
        } else {
            window.location.href = location.href;
        }
    }
}

if (typeof createCookie !== 'function' || typeof createCookie === 'undefined') {
    // create cookie for the page
    createCookie = function createCookie(name, value, days) {
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            var expires = "; expires=" + date.toGMTString();
        }
        else var expires = "";

        document.cookie = name + "=" + value + expires + "; path=/";
    }
}

if (typeof readCookie !== 'function' || typeof readCookie === 'undefined') {
    // read cookie from the page
    readCookie = function readCookie(name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }
}