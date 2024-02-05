/***********************************************
* wtewebrequesthandler.js
* Webrequest, postback functions
*
***********************************************/


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

if (typeof parseQueryString !== 'function' || typeof parseQueryString === 'undefined') {
    parseQueryString = function parseQueryString(str) {
        var vars = [];
        var arr = str.split('&');
        var pair;
        for (var i = 0; i < arr.length; i++) {
            pair = arr[i].split('=');
            vars[pair[0]] = unescape(pair[1]);
        }
        return vars;
    }
}

if (typeof jsonToFormData !== 'function' || typeof jsonToFormData === 'undefined') {
    jsonToFormData = function jsonToFormData(inJSON, inTestJSON, inFormData, parentKey) {
        // http://stackoverflow.com/a/22783314/260665
        // Raj: Converts any nested JSON to formData.
        var form_data = inFormData || new FormData();
        var testJSON = inTestJSON || {};
        for (var key in inJSON) {
            // 1. If it is a recursion, then key has to be constructed like "parent.child" where parent JSON contains a child JSON
            // 2. Perform append data only if the value for key is not a JSON, recurse otherwise!
            var constructedKey = key;
            if (parentKey) {
                constructedKey = parentKey + "." + key;
            }

            var value = inJSON[key];
            if (value && value.constructor === {}.constructor) {
                // This is a JSON, we now need to recurse!
                jsonToFormData(value, testJSON, form_data, constructedKey);
            } else {
                form_data.append(constructedKey, inJSON[key]);
                testJSON[constructedKey] = inJSON[key];
            }
        }
        return form_data;
    }
}

if (typeof addJSONFormData !== 'function' || typeof addJSONFormData === 'undefined') {
    addJSONFormData = function addJSONFormData(inJSON, inTestJSON, inFormData, parentKey, form) {
        // http://stackoverflow.com/a/22783314/260665
        // Raj: Converts any nested JSON to formData.
        var form_data = inFormData || new FormData();
        var formInput = form || ''
        var testJSON = inTestJSON || {};
        for (var key in inJSON) {
            // 1. If it is a recursion, then key has to be constructed like "parent.child" where parent JSON contains a child JSON
            // 2. Perform append data only if the value for key is not a JSON, recurse otherwise!
            var constructedKey = key;
            if (parentKey) {
                constructedKey = parentKey + "." + key;
            }

            var value = inJSON[key];
            if (value && value.constructor === {}.constructor) {
                // This is a JSON, we now need to recurse!
                addJSONFormData(value, testJSON, form_data, constructedKey, formInput);
            } else {
                form_data.append(constructedKey, inJSON[key]);
                testJSON[constructedKey] = inJSON[key];
                formInput += "<input type='hidden' name='" + constructedKey + "' value='" + inJSON[key] + "'></input>";
            }
        }
        return formInput;
    }
}

if (typeof RedirectAndPostJSON !== 'function' || typeof RedirectAndPostJSON === 'undefined') {
    // post convert json to form data to page
    RedirectAndPostJSON = function RedirectAndPostJSON(postUrl, data) {
        var testJSON = {};
        var formInput = addJSONFormData(data, testJSON);
        var formData = jsonToFormData(data);
        var formstr = "<form method='POST' action='" + postUrl + "'>\n";
        for (var key in testJSON) {
            var value = testJSON[key];
            formstr += "<input type='hidden' name='" + key + "' value='" + value + "'></input>";
        }
        formstr += "</form>";
        var formElement = $(formstr);
        $('body').append(formElement);
        $(formElement).submit();
    }
}

if (typeof RedirectAndPostJSON2 !== 'function' || typeof RedirectAndPostJSON2 === 'undefined') {
    RedirectAndPostJSON2 = function RedirectAndPostJSON2(postUrl, data) {
        var testJSON = {};
        var formInput = addJSONFormData(data, testJSON);
        var formData = jsonToFormData(data);
        //this crashes -- need to figure out why
        sendData(postUrl, formData);
        //var formstr = "<form method='POST' action='" + postUrl + "'>\n";
        //for (var key in testJSON) {
        //    var value = testJSON[key];
        //    formstr += "<input type='hidden' name='" + key + "' value='" + value + "'></input>";
        //}
        //formstr += "</form>";
        //var formElement = $(formstr);
        //$('body').append(formElement);
        //$(formElement).submit();
    }
}

if (typeof RedirectAndPostData !== 'function' || typeof RedirectAndPostData === 'undefined') {
    RedirectAndPostData = function RedirectAndPostData(postUrl, key, data) {
        var formstr = "<form method='POST' action='" + postUrl + "'>\n";
        formstr += "<input type='hidden' name='" + key + "' value='" + data + "'></input>";
        formstr += "</form>";
        var formElement = $(formstr);
        $('body').append(formElement);
        $(formElement).submit();
    }
}

if (typeof sendJSON !== 'function' || typeof sendJSON === 'undefined') {
    sendJSON = function sendJSON(url, data) {
        xhr = new XMLHttpRequest();
        //var url = "url";
        //url = 'PaymentINSHandler.aspx?mode=AUTH'
        xhr.open("POST", url, true);
        xhr.setRequestHeader("Content-type", "application/json");
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                // var json = JSON.parse(xhr.responseText);
                //console.log(json.email + ", " + json.name)
            }
        }
        var data = JSON.stringify(data);
        xhr.send(data);
    }
}

if (typeof postData !== 'function' || typeof postData === 'undefined') {
    postData = function postData(url, data, redirect) {
        var XHR = new XMLHttpRequest();
        var urlEncodedData = "";
        var urlEncodedDataPairs = [];
        var name;

        urlEncodedDataPairs.push(encodeURIComponent('token') + '=' + encodeURIComponent(data));
        // Turn the data object into an array of URL-encoded key/value pairs.
        //for (name in data) {
        //    urlEncodedDataPairs.push(encodeURIComponent(name) + '=' + encodeURIComponent(data[name]));
        //}

        // Combine the pairs into a single string and replace all %-encoded spaces to
        // the '+' character; matches the behaviour of browser form submissions.
        urlEncodedData = urlEncodedDataPairs.join('&').replace(/%20/g, '+');

        // Define what happens on successful data submission
        XHR.addEventListener('load', function (event) {
            alert('success!');
            if (redirect == true) {
                location.replace(url);
            }
        });

        // Define what happens in case of error
        XHR.addEventListener('error', function (event) {
            alert('failed');
        });

        //var postUrl = 'https://example.com/cors.php'
        //postUrl = 'PaymentINSHandler.aspx?mode=AUTH'
        var postUrl = url;

        // Set up our request
        XHR.open('POST', postUrl);

        // Add the required HTTP header for form data POST requests
        XHR.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

        // Finally, send our data.
        XHR.send(urlEncodedData);
    }
}

if (typeof sendData !== 'function' || typeof sendData === 'undefined') {
    sendData = function sendData(url, data) {
        var XHR = new XMLHttpRequest();
        var urlEncodedData = "";
        var urlEncodedDataPairs = [];
        var name;

        // Turn the data object into an array of URL-encoded key/value pairs.
        for (name in data) {
            urlEncodedDataPairs.push(encodeURIComponent(name) + '=' + encodeURIComponent(data[name]));
        }

        // Combine the pairs into a single string and replace all %-encoded spaces to
        // the '+' character; matches the behaviour of browser form submissions.
        urlEncodedData = urlEncodedDataPairs.join('&').replace(/%20/g, '+');

        // Define what happens on successful data submission
        XHR.addEventListener('load', function (event) {
            alert('Yeah! Data sent and response loaded.');
        });

        // Define what happens in case of error
        XHR.addEventListener('error', function (event) {
            alert('Oops! Something goes wrong.');
        });

        //var postUrl = 'https://example.com/cors.php'
        //postUrl = 'PaymentINSHandler.aspx?mode=AUTH'
        var postUrl = url;

        // Set up our request
        XHR.open('POST', postUrl);

        // Add the required HTTP header for form data POST requests
        XHR.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

        // Finally, send our data.
        XHR.send(urlEncodedData);
    }
}