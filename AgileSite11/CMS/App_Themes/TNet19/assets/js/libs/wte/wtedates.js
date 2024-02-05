/***********************************************
* wtedates.js
*
* required
* <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
* <script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
***********************************************/


if (typeof getFirstDayOfYear !== 'function' || typeof getFirstDayOfYear === 'undefined') {
    // get the first day of the year
    getFirstDayOfYear = function getFirstDayOfYear(dateFrom) {
        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }
        var ret = new Date(dateFrom.getFullYear(), 0, 1);

        ret = getFormattedDate(ret);
        return ret;
    }
}

if (typeof getLastDayOfYear !== 'function' || typeof getLastDayOfYear === 'undefined') {
    // get the last day of the year
    getLastDayOfYear = function getLastDayOfYear(dateFrom) {
        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }
        var ret = new Date(dateFrom.getFullYear(), 11, 31);
        ret = getFormattedDate(ret);
        return ret;
    }
}

if (typeof getFirstOfMonth !== 'function' || typeof getFirstOfMonth === 'undefined') {
    // get first day of the month
    getFirstOfMonth = function getFirstOfMonth(dateFrom) {
        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }
        var ret = new Date(dateFrom.getFullYear(), dateFrom.getMonth(), 1);
        ret = getFormattedDate(ret);
        return ret;
    }
}

if (typeof getLastOfMonth !== 'function' || typeof getLastOfMonth === 'undefined') {
    // get last day of the month
    getLastOfMonth = function getLastOfMonth(dateFrom) {
        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }
        var ret = new Date(dateFrom.getFullYear(), dateFrom.getMonth() + 1, 0);
        ret = getFormattedDate(ret);
        return ret;
    }
}

if (typeof getYesterday !== 'function' || typeof getYesterday === 'undefined') {
    // Get Yesterday's date
    getYesterday = function getYesterday() {
        return getPastDate(1);
    }
}

if (typeof getTomorrow !== 'function' || typeof getTomorrow === 'undefined') {
    // Get Tomorrow's date
    getTomorrow = function getTomorrow() {
        return getFutureDate(1);
    }
}

if (typeof getToday !== 'function' || typeof getToday === 'undefined') {
    // Get Today's date
    getToday = function getToday() {
        return getFutureDate(0);
    }
}

if (typeof getThirtyDaysAgo !== 'function' || typeof getThirtyDaysAgo === 'undefined') {
    // Get 30 days ago
    getThirtyDaysAgo = function getThirtyDaysAgo() {
        return getPastDate(30);
    }
}

if (typeof getThirtyDaysFromNow !== 'function' || typeof getThirtyDaysFromNow === 'undefined') {
    // Get 30 Days from now
    getThirtyDaysFromNow = function getThirtyDaysFromNow() {
        return getFutureDate(30);
    }
}

if (typeof getNinetyDaysAgo !== 'function' || typeof getNinetyDaysAgo === 'undefined') {
    // Get 90 Days ago
    getNinetyDaysAgo = function getNinetyDaysAgo() {
        return getPastDate(90);
    }
}

if (typeof getNinetyDaysFromNow !== 'function' || typeof getNinetyDaysFromNow === 'undefined') {
    // Get 90 days from now
    getNinetyDaysFromNow = function getNinetyDaysFromNow() {
        return getFutureDate(90);
    }
}

if (typeof getOneYearAgo !== 'function' || typeof getOneYearAgo === 'undefined') {
    // get the date for 1 year ago
    getOneYearAgo = function getOneYearAgo() {
        var ret = getPastDate(0, 0, 1);
        return ret;
    }
}

if (typeof getOneYearFromNow !== 'function' || typeof getOneYearFromNow === 'undefined') {
    // get the date 1 year from now
    getOneYearFromNow = function getOneYearFromNow() {
        var ret = getFutureDate(0, 0, 1);
        return ret;
    }
}

if (typeof getPastDate !== 'function' || typeof getPastDate === 'undefined') {
    // Get date of number of days, month and year in the past (from today)
    getPastDate = function getPastDate(numDays, numMonths, numYears, dateFrom, formatDate) {
        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }

        var ret = dateFrom;

        if (typeof (numDays) == 'undefined' || numDays == '') {
            numDays = 0;
        }
        if (typeof (numMonths) == 'undefined' || numMonths == '') {
            numMonths = 0;
        }
        if (typeof (numYears) == 'undefined' || numYears == '') {
            numYears = 0;
        }

        ret.setDate(ret.getDate() - numDays);
        ret.setMonth(ret.getMonth() - numMonths);
        ret.setYear(ret.getFullYear() - numYears);
        ret = getFormattedDate(ret, formatDate);

        return ret;
    }
}

if (typeof getFutureDate !== 'function' || typeof getFutureDate === 'undefined') {
    // Get date of number of days, month and year in the future from today
    getFutureDate = function getFutureDate(numDays, numMonths, numYears, dateFrom, formatDate) {
        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }

        var ret = dateFrom;

        if (typeof (numDays) == 'undefined' || numDays == '') {
            numDays = 0;
        }
        if (typeof (numMonths) == 'undefined' || numMonths == '') {
            numMonths = 0;
        }
        if (typeof (numYears) == 'undefined' || numYears == '') {
            numYears = 0;
        }

        ret.setDate(ret.getDate() + numDays);
        ret.setMonth(ret.getMonth() + numMonths);
        ret.setYear(ret.getFullYear() + numYears);
        ret = getFormattedDate(ret, formatDate);

        return ret;
    }
}
if (typeof getFormattedDate !== 'function' || typeof getFormattedDate === 'undefined') {
    // Get date in mm/dd/yyyy format
    getFormattedDate = function getFormattedDate(dateFrom, formatDate) {
        var doFormat = true;

        if (formatDate == false || (typeof (formatDate) != 'undefined')) {
            doFormat = getBoolean(formatDate);
        }

        if (typeof (dateFrom) == 'undefined' || dateFrom == '') {
            dateFrom = new Date();
        }

        if (doFormat == true) {
            var dd = dateFrom.getDate();
            var mm = dateFrom.getMonth() + 1; //January is 0!
            var yyyy = dateFrom.getFullYear();

            if (dd < 10) { dd = '0' + dd }
            if (mm < 10) { mm = '0' + mm }

            ret = mm + '/' + dd + '/' + yyyy;
        }
        else {
            ret = dateFrom;
        }

        return ret;
    }
}