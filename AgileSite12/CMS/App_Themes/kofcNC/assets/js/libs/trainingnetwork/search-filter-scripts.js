/***********************************************
* search-filter-scripts.js
*
* TNN Search filter script - note requires the following wte libs
*
* required
* <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
* <script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
* <script src="https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wterequests.js"></script>
* <script src="https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wtedata.js"></script>
* <script src="https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wtedates.js"></script>
* <script src="https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wtecontrols.js"></script>
***********************************************/


//$.getScript("../wte/wterequests.js");
//$.getScript("../wte/wtedata.js");
//$.getScript("../wte/wtedates.js");
//$.getScript("../wte/wtecontrols.js");

//$.getScript("https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wterequests.js");
//$.getScript("https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wtedata.js");
//$.getScript("https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wtedates.js");
//$.getScript("https://www.trainingnetworknow.com/App_Themes/TNet19/assets/js/libs/wte/wtecontrols.js");

if (typeof applyFiltersContract !== 'function' || typeof applyFiltersContract === 'undefined') {
    // Apply contract period
    applyFiltersContract = function applyFiltersContract(ctnamesuffix, paramprefix, doRefresh) {
        if (typeof (doRefresh) == 'undefined' || doRefresh == '') {
            doRefresh = false;
        }

        tfdropdownname = getTextWithSuffix('ddlTimeFrame', ctnamesuffix);
        divsdtname = getTextWithSuffix('divSdt', ctnamesuffix);
        lblsdtname = getTextWithSuffix('lblSdt', ctnamesuffix);
        dpsdtname = getTextWithSuffix('dpStartDate', ctnamesuffix);
        divedtname = getTextWithSuffix('divEdt', ctnamesuffix);
        lbledtname = getTextWithSuffix('lblEdt', ctnamesuffix);
        dpedtname = getTextWithSuffix('dpEndDate', ctnamesuffix);
        divperiodname = getTextWithSuffix('divPeriod', ctnamesuffix);

        tfparamname = getTextWithPrefix('tf', paramprefix);
        sdtparamname = getTextWithPrefix('sdt', paramprefix);
        edtparamname = getTextWithPrefix('edt', paramprefix);

        var sdt = getControlValue('hdnContractStart');
        var edt = getControlValue('hdnContractEnd');
        setControlValue(dpsdtname, sdt, '');
        setControlValue(dpedtname, edt, '');

        selectDropDownOption(tfdropdownname, '8');

        if (doRefresh == true) {
            applyFilters();
        }
    }
}

if (typeof setTimeFrameSelection !== 'function' || typeof setTimeFrameSelection === 'undefined') {
    // set time frame
    setTimeFrameSelection = function setTimeFrameSelection(ctnamesuffix, paramprefix) {
        tfdropdownname = getTextWithSuffix('ddlTimeFrame', ctnamesuffix);
        divsdtname = getTextWithSuffix('divSdt', ctnamesuffix);
        lblsdtname = getTextWithSuffix('lblSdt', ctnamesuffix);
        dpsdtname = getTextWithSuffix('dpStartDate', ctnamesuffix);
        divedtname = getTextWithSuffix('divEdt', ctnamesuffix);
        lbledtname = getTextWithSuffix('lblEdt', ctnamesuffix);
        dpedtname = getTextWithSuffix('dpEndDate', ctnamesuffix);
        divperiodname = getTextWithSuffix('divPeriod', ctnamesuffix);

        tfparamname = getTextWithPrefix('tf', paramprefix);
        sdtparamname = getTextWithPrefix('sdt', paramprefix);
        edtparamname = getTextWithPrefix('edt', paramprefix);

        var timeFrame = getControlValue(tfdropdownname);
        var curSdt = getControlValueWithDefault(dpsdtname, getParamWithDefault(sdtparamname, getPastDate(90)));
        var curEdt = getControlValueWithDefault(dpedtname, getParamWithDefault(edtparamname, getToday()));
        var sdt = getControlValueWithDefault(dpsdtname, getParamWithDefault(sdtparamname, getPastDate(90)));
        var edt = getControlValueWithDefault(dpedtname, getParamWithDefault(edtparamname, getToday()));

        //1  -- Past 30 Days
        //2  -- Month to Date (MTD)
        //3  -- Year to Date (YTD)
        //4  -- Past/Prior Month
        //5  -- Last 90 Days
        //8  -- Contract Period
        //9  -- Custom Dates

        if (timeFrame == '5') {
            sdt = getPastDate(90);
            edt = getToday();
        }
        else if (timeFrame == '1') {
            sdt = getPastDate(30);
            edt = getToday();
        }
        else if (timeFrame == '2') {
            sdt = getFirstOfMonth();
            edt = getToday();
        }
        else if (timeFrame == '3') {
            sdt = getFirstDayOfYear();
            edt = getToday();
        }
        else if (timeFrame == '4') {
            sdt = getFirstOfMonth(getPastDate(0, 1, 0, new Date(), false));
            edt = getLastOfMonth(getPastDate(0, 1, 0, new Date(), false));
        }

        setControlValue(dpsdtname, sdt);
        setControlValue(dpedtname, edt);

        if (timeFrame == '8' || timeFrame == '9') {
            showControl(divperiodname);
            showControl(dpsdtname);
            showControl(dpedtname);
            showControl(lblsdtname);
            showControl(lbledtname);
            showControl(divsdtname);
            showControl(divedtname);

            if (timeFrame == '8') {
                applyFiltersContract(ctnamesuffix, paramprefix, false);
            }
            else {
                setControlValue(dpsdtname, sdt);
                setControlValue(dpedtname, edt);
            }
        }
        else {
            // hide when not in used
            hideControl(divperiodname);
            hideControl(dpsdtname);
            hideControl(dpedtname);
            hideControl(lblsdtname);
            hideControl(lbledtname);
            hideControl(divsdtname);
            hideControl(divedtname);
        }
    }
}