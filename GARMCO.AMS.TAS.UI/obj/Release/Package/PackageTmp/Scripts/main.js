
var currentClientHeight = 0;
var currentClientWidth = 0;
var dynamicTextBoxID = "";
var dynamicTextBoxValue = "";
var dynamicTextBox = new Array();
var dynamicTextBoxValues = new Array();
var dateText = null;
var attachmentWin;
var webpageWin;
var mgpWin;

function ConfigureClientHeight() {
    var mainBody;
    if (navigator.appName == "Microsoft Internet Explorer") {

        currentClientHeight = document.documentElement.clientHeight;
        currentClientWidth = document.documentElement.clientWidth;

        // Modify the height of the content placement
        mainBody = document.getElementById("mainBody");

        if (currentClientHeight > 594)
            mainBody.style.height = String(currentClientHeight - 130) + "px";
        else if (currentClientHeight <= 594 && currentClientHeight > 400)
            mainBody.style.height = String(currentClientHeight - 30) + "px";
        else
            mainBody.style.height = String(currentClientHeight + 150) + "px";
    }

    else if (navigator.appName == "Netscape") {

        currentClientHeight = window.innerHeight;
        currentClientWidth = window.innerWidth;

        // Modify the height of the content placement
        document.getElementById("mainBody").style.height = String(currentClientHeight - 140) + "px";

    }
}

function SetClientHeight() {
    var mainBody;
    if (navigator.appName == "Microsoft Internet Explorer") {

        currentClientHeight = document.documentElement.clientHeight;
        currentClientWidth = document.documentElement.clientWidth;

        // Modify the height of the content placement
        mainBody = document.getElementById("mainBody");
        mainBody.style.height = String(currentClientHeight - 155) + "px";
    }

    else if (navigator.appName == "Netscape") {

        currentClientHeight = window.innerHeight;
        currentClientWidth = window.innerWidth;

        // Modify the height of the content placement
        document.getElementById("mainBody").style.height = String(currentClientHeight - 165) + "px";

    }
}

function SetWindowSize() {
    ConfigureClientHeight();

    // Set the date and time
    SetDateTime();
}

function SetDateTime() {
    var dayList = new Array("Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday");
    var monthList = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September",
								"October", "November", "December");

    var currentDateTime = new Date();
    var dateTime = new Date();

    var dayWeek = currentDateTime.getDay();
    var day = currentDateTime.getDate();
    var month = currentDateTime.getMonth();
    var year = currentDateTime.getFullYear();
    var hours = currentDateTime.getHours();
    var mins = currentDateTime.getMinutes();
    var seconds = currentDateTime.getSeconds();

    // Display the time
    dateTime = new String(dayList[dayWeek] + ", " + FormatNumber(day) + " " + monthList[month] + " " + year + " " +
			FormatNumber(hours) + ":" + FormatNumber(mins) + ":" + FormatNumber(seconds));

    //	if (navigator.appName == "Microsoft Internet Explorer")
    document.getElementById("currentDateTime").innerHTML = dateTime;

    // Set the timeout
    setTimeout('SetDateTime()', 1000);
}

function HideScrollbar() {
    if (screen.availWidth > 1024)
        document.documentElement.style.overflowX = 'hidden'; //Hide the browser's horizontal scrollbar
    //    document.body.scroll = "no";
}

function ShowApplicationInFullScreen(url) {
    var width = 0;
    var height = 0;

    var garmcoWin = window.open(url, "SMS",
		"menubar=no, status=1, resizable=no, scrollbars=yes, toolbar=no, top=0, left=0");

    // Close the main window opener
    if (garmcoWin != null) {

        garmcoWin.focus();

        window.open('', '_parent', '');
        window.close();

    }

    if (document.all) {

        garmcoWin.resizeTo(screen.availWidth, screen.availHeight);
        //top.window.resizeTo(screen.availWidth,screen.availHeight);
    }

    else if (document.layers || document.getElementById) {
        if (top.window.outerHeight < screen.availHeight || top.window.outerWidth < screen.availWidth) {

            garmcoWin.outerHeight = screen.availHeight;
            garmcoWin.outerWidth = screen.availWidth;
            //		top.window.outerHeight = screen.availHeight;
            //		top.window.outerWidth = screen.availWidth;
        }
    }
}

function FormatNumber(number) {
    var newNumber = new String(number);

    if (number < 10)
        newNumber = "0" + number;

    return newNumber;
}

function OnDateSelected(sender, e) {
    var hidDynamicControlID = document.getElementById('mainContent_hidDynamicControlID');
    var hidDynamicControlValue = document.getElementById('mainContent_hidDynamicControlValue');
    var hidDynamicDate = document.getElementById('mainContent_hidDynamicDate');
    var hidDynamicTextBoxID1 = document.getElementById('mainContent_hidDynamicTextBoxID1');
    var hidDynamicTextBoxID2 = document.getElementById('mainContent_hidDynamicTextBoxID2');
    var hidDynamicTextBoxValue1 = document.getElementById('mainContent_hidDynamicTextBoxValue1');
    var hidDynamicTextBoxValue2 = document.getElementById('mainContent_hidDynamicTextBoxValue2');

    if (hidDynamicControlID != null)
        hidDynamicControlID.value = sender.get_id();

    if (hidDynamicControlValue != null)
        hidDynamicControlValue.value = e.get_newDate().toDateString();

    if (hidDynamicDate != null)
        hidDynamicDate.value = "0";

    if (hidDynamicTextBoxID1 != null)
        hidDynamicTextBoxID1.value = dynamicTextBox[0];

    if (hidDynamicTextBoxID2 != null)
        hidDynamicTextBoxID2.value = dynamicTextBox[2];

    if (hidDynamicTextBoxValue1 != null)
        hidDynamicTextBoxValue1.value = dynamicTextBox[1];

    if (hidDynamicTextBoxValue2 != null)
        hidDynamicTextBoxValue2.value = dynamicTextBox[3]

    // Clear the array
    dynamicTextBox.length = 0;

    if (document.getElementById('ctl00_mainContent_btnDynamic') != null)
        document.getElementById('ctl00_mainContent_btnDynamic').click();
}

function OnTimeSelected(sender, e) {
    var hidDynamicControlID = document.getElementById('mainContent_hidDynamicControlID');
    var hidDynamicControlValue = document.getElementById('mainContent_hidDynamicControlValue');
    var hidDynamicDate = document.getElementById('mainContent_hidDynamicDate');
    var hidDynamicTextBoxID1 = document.getElementById('mainContent_hidDynamicTextBoxID1');
    var hidDynamicTextBoxID2 = document.getElementById('mainContent_hidDynamicTextBoxID2');
    var hidDynamicTextBoxValue1 = document.getElementById('mainContent_hidDynamicTextBoxValue1');
    var hidDynamicTextBoxValue2 = document.getElementById('mainContent_hidDynamicTextBoxValue2');

    if (hidDynamicControlID != null)
        hidDynamicControlID.value = sender.get_id();

    if (hidDynamicControlValue != null)
        hidDynamicControlValue.value = e.get_newValue();

    if (hidDynamicDate != null)
        hidDynamicDate.value = "0";

    if (hidDynamicTextBoxID1 != null)
        hidDynamicTextBoxID1.value = dynamicTextBox[0];

    if (hidDynamicTextBoxID2 != null)
        hidDynamicTextBoxID2.value = dynamicTextBox[2];

    if (hidDynamicTextBoxValue1 != null)
        hidDynamicTextBoxValue1.value = dynamicTextBox[1];

    if (hidDynamicTextBoxValue2 != null)
        hidDynamicTextBoxValue2.value = dynamicTextBox[3]

    // Clear the array
    dynamicTextBox.length = 0;

    if (document.getElementById('ctl00_mainContent_btnDynamic') != null)
        document.getElementById('ctl00_mainContent_btnDynamic').click();
}

function OnClientTimeSelected(sender, e) {
    var hidDynamicControlID = document.getElementById('mainContent_hidDynamicControlID');
    var hidDynamicControlValue = document.getElementById('mainContent_hidDynamicControlValue');
    var hidDynamicTextBoxID = document.getElementById('mainContent_hidDynamicTextBoxID');
    var hidDynamicTextBoxValue = document.getElementById('mainContent_hidDynamicTextBoxValue');
    var hidDynamicDate = document.getElementById('mainContent_hidDynamicDate');

    if (hidDynamicControlID != null)
        hidDynamicControlID.value = sender._ownerDatePickerID;

    if (hidDynamicControlValue != null)
        hidDynamicControlValue.value = e.get_newTime();

    if (hidDynamicTextBoxID != null)
        hidDynamicTextBoxID.value = dynamicTextBoxID

    if (hidDynamicTextBoxValue != null)
        hidDynamicTextBoxValue.value = dynamicTextBoxValue

    if (hidDynamicDate != null)
        hidDynamicDate.value = "1";

    if (document.getElementById('ctl00_mainContent_btnDynamic') != null)
        document.getElementById('ctl00_mainContent_btnDynamic').click();
}


function OnClientSelectedIndexChanged(sender, args) {
    var selectedItem = args.get_item();
    var selectedItemText = selectedItem != null ? selectedItem.get_text() : sender.get_text();
    var hidDynamicControlID = document.getElementById('mainContent_hidDynamicControlID');
    var hidDynamicControlValue = document.getElementById('mainContent_hidDynamicControlValue');
    var hidDynamicDate = document.getElementById('mainContent_hidDynamicDate');
    var hidDynamicTextBoxID1 = document.getElementById('mainContent_hidDynamicTextBoxID1');
    var hidDynamicTextBoxID2 = document.getElementById('mainContent_hidDynamicTextBoxID2');
    var hidDynamicTextBoxValue1 = document.getElementById('mainContent_hidDynamicTextBoxValue1');
    var hidDynamicTextBoxValue2 = document.getElementById('mainContent_hidDynamicTextBoxValue2');

    if (hidDynamicControlID != null)
        hidDynamicControlID.value = sender.get_id();

    if (hidDynamicControlValue != null)
        hidDynamicControlValue.value = selectedItemText;

    if (hidDynamicDate != null)
        hidDynamicDate.value = "0";

    //    if (hidDynamicTextBoxID != null)
    //        hidDynamicTextBoxID.value = dynamicTextBoxID

    //    if (hidDynamicTextBoxValue != null)
    //        hidDynamicTextBoxValue.value = dynamicTextBoxValue

    if (hidDynamicTextBoxID1 != null)
        hidDynamicTextBoxID1.value = dynamicTextBox[0];

    if (hidDynamicTextBoxID2 != null)
        hidDynamicTextBoxID2.value = dynamicTextBox[2];

    if (hidDynamicTextBoxValue1 != null)
        hidDynamicTextBoxValue1.value = dynamicTextBox[1];

    if (hidDynamicTextBoxValue2 != null)
        hidDynamicTextBoxValue2.value = dynamicTextBox[3]

    // Clear the array
    dynamicTextBox.length = 0;

    if (document.getElementById('ctl00_mainContent_btnDynamic') != null)
        document.getElementById('ctl00_mainContent_btnDynamic').click();
}

function OnValueChanged(sender, args) {
    var hidDynamicControlID = document.getElementById('mainContent_hidDynamicControlID');
    var hidDynamicControlValue = document.getElementById('mainContent_hidDynamicControlValue');

    if (hidDynamicControlID != null)
        hidDynamicControlID.value = sender.get_id();

    if (hidDynamicControlValue != null)
        hidDynamicControlValue.value = args.get_newValue();

    //    dynamicTextBoxID = sender.get_id();
    //    dynamicTextBoxValue = args.get_newValue();

    if (dynamicTextBox.length == 0) {
        dynamicTextBox[0] = sender.get_id();
        dynamicTextBox[1] = args.get_newValue();
    }
    else {
        var index = GetArrayIndex(sender.get_id());
        if (index != -1) {
            dynamicTextBox[index] = sender.get_id();
            dynamicTextBox[index + 1] = args.get_newValue();
        }
        else {
            var newIndex = dynamicTextBox.length;
            dynamicTextBox[newIndex] = sender.get_id();
            dynamicTextBox[newIndex + 1] = args.get_newValue();
        }
    }
}

function GetArrayIndex(controlID) {
    var result = -1;

    for (var x = 0; x < dynamicTextBox.length; x++) {
        if (dynamicTextBox[x] == controlID) {
            result = x;
            break;
        }
    }
    return result;
}

function trim(stringToTrim) {
    return stringToTrim.replace(/^\s+|\s+$/g, "");
}

function ltrim(stringToTrim) {
    return stringToTrim.replace(/^\s+/, "");
}

function rtrim(stringToTrim) {
    return stringToTrim.replace(/\s+$/, "");
}


function ExecuteClickEvent(btnControl, hdnPostBackCode, postBackCodeValue, lbEntities, hdnSelectedValue) {
    if (document.getElementById(lbEntities) != null && document.getElementById(hdnSelectedValue) != null)
        document.getElementById(hdnSelectedValue).value = document.getElementById(lbEntities).value;

    if (document.getElementById(hdnPostBackCode) != null)
        document.getElementById(hdnPostBackCode).value = postBackCodeValue;

    if (document.getElementById(btnControl) != null)
        document.getElementById(btnControl).click();
}

function ConfirmDeletion(btnDelete, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnDelete) != null)
            document.getElementById(btnDelete).click();
    }
}

function ConfirmAction(hdnProceedDelete, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(hdnProceedDelete) != null)
            document.getElementById(hdnProceedDelete).value = "true";
    }
    return res;
}

function EnableDisableTargetControl(cbo, targetControlID) {
    var ddl = document.getElementsByTagName('select');
    var tb = document.getElementsByTagName('input');

    if (ddl != null && cbo != null && targetControlID != null) {
        for (var i = 0; i < ddl.length; i++) {
            if (ddl[i].id.indexOf(cbo) != -1) {
                var e = document.getElementById(ddl[i].id);
                var value = e.options[e.selectedIndex].text;
                if (value != null && value.toUpperCase() == "OTHERS") {
                    if (tb != null) {
                        for (var x = 0; x < tb.length; x++) {
                            if (tb[x].type == "text" || tb[x].type == "checkbox" || tb[x].type == "radio" || tb[x].type == "select") {
                                if (tb[x].id.indexOf(targetControlID) != -1) {
                                    tb[x].disabled = false;
                                    tb[x].focus();
                                    break;
                                }
                            }
                        }
                    }
                }
                else {
                    if (tb != null) {
                        for (var x = 0; x < tb.length; x++) {
                            if (tb[x].type == "text") {
                                if (tb[x].id.indexOf(targetControlID) != -1) {
                                    tb[x].disabled = true;
                                    tb[x].value = "";
                                    break;
                                }
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}

function EnableDisableTargetControlBaseOnValue(cbo, targetControlID, desiredValue) {
    var ddl = document.getElementsByTagName('select');
    var tb = document.getElementsByTagName('input');
    var isTargetFound = false;

    if (ddl != null && cbo != null && targetControlID != null && desiredValue != null && desiredValue != "") {
        for (var i = 0; i < ddl.length; i++) {
            if (ddl[i].id.indexOf(cbo) != -1) {
                var e = document.getElementById(ddl[i].id);
                var value = e.options[e.selectedIndex].text;
                if (value != null && value.toUpperCase() == desiredValue.toUpperCase()) {
                    if (tb != null) {
                        for (var x = 0; x < tb.length; x++) {
                            if (tb[x].type == "text" || tb[x].type == "checkbox" || tb[x].type == "radio" || tb[x].type == "select") {
                                if (tb[x].id.indexOf(targetControlID) != -1) {
                                    tb[x].disabled = false;
                                    tb[x].focus();
                                    isTargetFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isTargetFound == false) {
                        for (var x = 0; x < ddl.length; x++) {
                            if (ddl[x].id.indexOf(targetControlID) != -1) {
                                ddl[x].disabled = false;
                                ddl[x].focus();
                                break;
                            }
                        }
                    }
                }
                else {
                    if (tb != null) {
                        for (var x = 0; x < tb.length; x++) {
                            if (tb[x].type == "text") {
                                if (tb[x].id.indexOf(targetControlID) != -1) {
                                    tb[x].disabled = true;
                                    tb[x].value = "";
                                    isTargetFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isTargetFound == false) {
                        for (var x = 0; x < ddl.length; x++) {
                            if (ddl[x].id.indexOf(targetControlID) != -1) {
                                var ddlToDisable = document.getElementById(ddl[x].id);
                                if (ddlToDisable != null) {
                                    ddlToDisable.disabled = true;
                                    ddlToDisable.selectedIndex = 0;
                                }
                                break;
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}

function EnableDisableTargetControlBaseOnCheckValue(cbl, targetControlID, desiredValue) {
    var tb = document.getElementsByTagName('input');
    var checkBoxList = document.getElementById(cbl);

    if (tb != null) {
        for (var x = 0; x < tb.length; x++) {
            if (tb[x].type == "checkbox") {
                if (tb[x].checked) {

                }
                if (tb[x].id.indexOf(targetControlID) != -1) {
                    tb[x].disabled = false;
                    tb[x].focus();
                    break;
                }
            }
        }
    }

    //    if (checkBoxList != null) {
    //        var chkCheckBoxListItems= checkBoxList.getElementsByTagName("input");
    //        for (var counter = 0; counter <= chkCheckBoxListItems.lenght - 1; counter++) {
    //            var value = chkCheckBoxListItems[intCounter].text;
    //            if (value != null && value.toUpperCase() == desiredValue.toUpperCase()) {
    //                if (tb != null) {
    //                    for (var x = 0; x < tb.length; x++) {
    //                        if (tb[x].type == "text" || tb[x].type == "checkbox" || tb[x].type == "radio" || tb[x].type == "select") {
    //                            if (tb[x].id.indexOf(targetControlID) != -1) {
    //                                tb[x].disabled = false;
    //                                tb[x].focus();
    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            else {
    //                if (tb != null) {
    //                    for (var x = 0; x < tb.length; x++) {
    //                        if (tb[x].type == "text") {
    //                            if (tb[x].id.indexOf(targetControlID) != -1) {
    //                                tb[x].disabled = true;
    //                                tb[x].value = "";
    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
}

function changeClass(ele, oldClass, newClass) {
    if (!hasClass(ele, oldClass)) {
        addClass(ele, newClass);
    }
    else {
        removeClass(ele, oldClass);
        addClass(ele, newClass);
    }
}

function hasClass(ele, cls) {
    return ele.className.match(new RegExp('(\\s|^)' + cls + '(\\s|$)'));
}

function addClass(ele, cls) {
    if (!this.hasClass(ele, cls)) ele.className += " " + cls;
}

function removeClass(ele, cls) {
    if (hasClass(ele, cls)) {
        var reg = new RegExp('(\\s|^)' + cls + '(\\s|$)'); ele.className = ele.className.replace(reg, ' ');
    }
}

function SetSelectedCalendarDate(calMain, cal) {
    var calMain = document.getElementById(calMain);
    var tb = document.getElementsByTagName('input');

    if (tb != null) {
        for (var x = 0; x < tb.length; x++) {
            if (tb[x].type == "text") {
                if (tb[x].id.indexOf(dateText) != -1) {
                    tb[x].value = "test";
                    break;
                }
            }
        }
    }
    return true;
}

function ShowCalendarPopup(dateTextBox) {
    var calendarPanel = document.getElementById('calendarDiv');
    dateText = dateTextBox;

    if (calendarPanel != null) {
        changeClass(calendarPanel, "DatePickerPanel", "ShowDatePickerPanel");
    }
    return true;
}

function ShowRadCalendar(dateTextBox) {
    // Store date textbox to variable
    dateText = dateTextBox;

    var calendarPanel = document.getElementById('calendarDiv');
    if (calendarPanel != null && dateText != null) {
        // Get the x & y coordinates, width of the date textbox
        var xCoord = findDateTextXPos(dateText);
        var yCoord = findDateTextYPos(dateText);
        var width = 100;  //findDateTextWidth(dateText);

        calendarPanel.style.top = yCoord;
        calendarPanel.style.left = xCoord + width + 3;
        calendarPanel.style.display = "";
    }
}

function HideRadCalendar() {
    var calendarPanel = document.getElementById('calendarDiv');
    if (calendarPanel != null) {
        calendarPanel.style.display = "none";
    }
}

function findPosX(obj) {
    var curleft = 0;
    if (obj.offsetParent)
        while (1) {
            curleft += obj.offsetLeft;
            if (!obj.offsetParent)
                break;
            obj = obj.offsetParent;
        }
    else if (obj.x)
        curleft += obj.x;
    return curleft;
}

function findPosY(obj) {
    var curtop = 0;
    if (obj.offsetParent)
        while (1) {
            curtop += obj.offsetTop;
            if (!obj.offsetParent)
                break;
            obj = obj.offsetParent;
        }
    else if (obj.y)
        curtop += obj.y;
    return curtop;
}

function findDateTextXPos(dateText) {
    var xPos = 0;
    var tb = document.getElementsByTagName('input');

    if (tb != null) {
        for (var x = 0; x < tb.length; x++) {
            if (tb[x].type == "text") {
                if (tb[x].id.indexOf(dateText) != -1) {
                    var obj = tb[x];
                    if (obj.offsetParent) {
                        while (1) {
                            xPos += obj.offsetLeft;
                            if (!obj.offsetParent)
                                break;
                            obj = obj.offsetParent;
                        }
                    }
                    else if (obj.x)
                        xPos += obj.x;
                    break;
                }
            }
        }
    }
    return xPos;
}

function findDateTextYPos(dateText) {
    var yPos = 0;
    var tb = document.getElementsByTagName('input');

    if (tb != null) {
        for (var x = 0; x < tb.length; x++) {
            if (tb[x].type == "text") {
                if (tb[x].id.indexOf(dateText) != -1) {
                    var obj = tb[x];
                    if (obj.offsetParent)
                        while (1) {
                            yPos += obj.offsetTop;
                            if (!obj.offsetParent)
                                break;
                            obj = obj.offsetParent;
                        }
                    else if (obj.y)
                        yPos += obj.y;
                    break;
                }
            }
        }
    }
    return yPos;
}

function findDateTextWidth(dateText) {
    var width = 0;
    var tb = document.getElementsByTagName('input');

    if (tb != null) {
        for (var x = 0; x < tb.length; x++) {
            if (tb[x].type == "text") {
                if (tb[x].id.indexOf(dateText) != -1) {
                    var obj = tb[x];
                    width = obj.style.width;
                    break;
                }
            }
        }
    }
    return width;
}

function Calendar_OnDateSelected(calendarInstance, args) {
    var selectedDate = "";
    var dates = calendarInstance.get_selectedDates();
    for (var i = 0; i < dates.length; i++) {
        var date = dates[i];
        var year = date[0];
        var month = date[1];
        var day = date[2];
        selectedDate = day + "/" + month + "/" + year;
    }

    if (selectedDate != "" || selectedDate != null) {
        var tb = document.getElementsByTagName('input');
        if (tb != null) {
            for (var x = 0; x < tb.length; x++) {
                if (tb[x].type == "text") {
                    if (tb[x].id.indexOf(dateText) != -1) {
                        tb[x].value = selectedDate;
                        break;
                    }
                }
            }
        }
    }

    //Hide the calendar
    HideRadCalendar();
}

function DisplayAlert(msg) {
    alert(msg);
}

function DisplayAlertWithPostback(msg, btnPostback, hdnPostbackAction, action) {
    alert(msg);

    if (document.getElementById(hdnPostbackAction) != null)
        document.getElementById(hdnPostbackAction).value = action;

    if (document.getElementById(btnPostback) != null)
        document.getElementById(btnPostback).click();
}

function DisplayAlertWithAction(msg, btnPostback) {
    alert(msg);

    if (document.getElementById(btnPostback) != null)
        document.getElementById(btnPostback).click();
}

function ConfirmCloseRequest(confirmationMsg, btnPostback, hdnPostbackAction, hdnCloseReqFlag, action) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(hdnCloseReqFlag) != null)
            document.getElementById(hdnCloseReqFlag).value = "1";
    }
    else {
        if (document.getElementById(hdnCloseReqFlag) != null)
            document.getElementById(hdnCloseReqFlag).value = "0";
    }

    if (document.getElementById(hdnPostbackAction) != null)
        document.getElementById(hdnPostbackAction).value = action;

    if (document.getElementById(btnPostback) != null)
        document.getElementById(btnPostback).click();
}

function ExecutePostback(btnPostback) {
    if (document.getElementById(btnPostback) != null)
        document.getElementById(btnPostback).click();
    return true;
}

function ToggleSearchType(btnPostback, hidSearchType, panBasicSearch, panAdvancedSearch) {
    var SearchType = document.getElementById(hidSearchType);
    var BasicSearchPanel = document.getElementById(panBasicSearch)
    var AdvancedSearchPanel = document.getElementById(panAdvancedSearch)

    if (SearchType != null && BasicSearchPanel != null && AdvancedSearchPanel != null) {
        if (rtrim(SearchType.value) == "Basic") {
            SearchType.value = "Advanced";
        }
        else {
            SearchType.value = "Basic";
        }

        if (document.getElementById(btnPostback) != null)
            document.getElementById(btnPostback).click();
    }
}

function ShowAlertErrorMessage(btn, msg) {
    alert(msg);

    if (document.getElementById(btn) != null)
        document.getElementById(btn).click();
}

function ConfirmRecordDeletion(btnDelete, btnRebind, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnDelete) != null)
            document.getElementById(btnDelete).click();
    }
    else {
        if (document.getElementById(btnRebind) != null)
            document.getElementById(btnRebind).click();
    }
}

function ConfirmButtonAction(btnAction, btnRebind, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnAction) != null)
            document.getElementById(btnAction).click();
    }
    else {
        if (document.getElementById(btnRebind) != null)
            document.getElementById(btnRebind).click();
    }
}

function ConfirmButtonActionNoPostback(btnAction, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnAction) != null)
            document.getElementById(btnAction).click();
    }
}


function ConfirmUserAction(hidRequestFlag, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (hidRequestFlag != null)
            document.getElementById(hidRequestFlag).value = '1';
    }
    else {
        if (hidRequestFlag != null)
            document.getElementById(hidRequestFlag).value = '';
    }
    return true;
}

function ConfirmCancelation(btnCancel, btnRebind, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnCancel) != null)
            document.getElementById(btnCancel).click();
    }
    else {
        if (document.getElementById(btnRebind) != null)
            document.getElementById(btnRebind).click();
    }
}

function CancelProcess(hidCancelRequest, confirmationMsg) {
    var res = confirm(confirmationMsg);
    if (document.getElementById(hidCancelRequest) != null)
        document.getElementById(hidCancelRequest).value = res == true ? "1" : "0";
    return res;
}

function ConfirmReassignment(btnReassign, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnReassign) != null)
            document.getElementById(btnReassign).click();
    }
}

function ToggleButtons(btnToggle, control) {
    if (document.getElementById(btnToggle) != null)
        document.getElementById(btnToggle).click();

    //    if (document.getElementById(control) != null)
    //        document.getElementById(control).focus();
}

function ApplyResolution(btnApplyResolution, confirmationMsg) {
    var res = false;
    res = confirm(confirmationMsg);

    if (res == true) {
        if (document.getElementById(btnApplyResolution) != null)
            document.getElementById(btnApplyResolution).click();
    }
}

function SetPostbackControlID(hidPostback, controlID) {
    if (document.getElementById(hidPostback) != null)
        document.getElementById(hidPostback).value = controlID;
}

function DisplayAttachment(url) {
    var width = screen.availWidth / 1.5;
    var height = screen.availHeight / 1.5;
    var xPos = (screen.availWidth - width) / 2;
    var yPos = (screen.availHeight - height) / 2;

    if (attachmentWin != null) {
        //alert("Day-in-Lieu User Guide is already open!");
        var userGuidenew = window.open("", "AttachmentPage", "");
        userGuidenew.close();

        //Re-open the page
        attachmentWin = window.open(url, "AttachmentPage", "menubar=no, status=1, resizable=yes, scrollbars=yes, toolbar=no, " +
            "top=" + yPos.toString() + ", left=" + xPos.toString() + ", heigt=" + height.toString() + ", width=" + width.toString());

        if (document.all) {
            attachmentWin.focus();
        }
    }
    else {
        attachmentWin = window.open(url, "AttachmentPage", "menubar=no, status=1, resizable=yes, scrollbars=yes, toolbar=no, " +
            "top=" + yPos.toString() + ", left=" + xPos.toString() + ", heigt=" + height.toString() + ", width=" + width.toString());

        if (document.all) {
            attachmentWin.focus();
        }
    }

    return false;
}

function DisplayWebpage(url) {
    var width = screen.availWidth / 1.5;
    var height = screen.availHeight / 1.5;
    var xPos = (screen.availWidth - width) / 2;
    var yPos = (screen.availHeight - height) / 2;

    if (webpageWin != null) {
        //alert("Day-in-Lieu User Guide is already open!");
        var userGuidenew = window.open("", "AttachmentPage", "");
        userGuidenew.close();

        //Re-open the page
        webpageWin = window.open(url, "AttachmentPage", "menubar=no, status=1, resizable=yes, scrollbars=yes, toolbar=no, " +
            "top=" + yPos.toString() + ", left=" + xPos.toString() + ", heigt=" + height.toString() + ", width=" + width.toString());

        if (document.all) {
            webpageWin.focus();
        }
    }
    else {
        webpageWin = window.open(url, "AttachmentPage", "menubar=no, status=1, resizable=yes, scrollbars=yes, toolbar=no, " +
            "top=" + yPos.toString() + ", left=" + xPos.toString() + ", heigt=" + height.toString() + ", width=" + width.toString());

        if (document.all) {
            webpageWin.focus();
        }
    }

    return false;
}

function OpenWebpageByPostback(btnPostback) {
    if (document.getElementById(btnPostback) != null)
        document.getElementById(btnPostback).click();
}

function CalculateBenefitValue(txtFinanceBaseLineValue, txtFinanceActualValue, txtFinanceBenefitValue, txtBenefitPercent) {
    var baseLineValue = 0;
    var actualValue = 0;
    var benefitValue = 0;   //(Note: Benefit Value = Base Line Value - Actual Value)
    var benefitPercent = 0;   //(Note: Benefit % = Benefit Value / Base Line Value)

    try {
        if (document.getElementById(txtFinanceBaseLineValue) != null)
            baseLineValue = parseFloat(document.getElementById(txtFinanceBaseLineValue).value.replace(/,/g, ''));

        if (document.getElementById(txtFinanceActualValue) != null)
            actualValue = parseFloat(document.getElementById(txtFinanceActualValue).value.replace(/,/g, ''));

        // Check if values are valid
        if (isNaN(baseLineValue))
            baseLineValue = 0;

        if (isNaN(actualValue))
            actualValue = 0;

        benefitValue = (baseLineValue - actualValue).toFixed(3);

        if (isNaN(benefitValue))
            benefitValue = 0;

        if (baseLineValue > 0)
            benefitPercent = ((benefitValue / baseLineValue) * 100).toFixed(3);
    }
    catch (Error) {
    }

    if (document.getElementById(txtFinanceBenefitValue) != null)
        document.getElementById(txtFinanceBenefitValue).value = benefitValue.toString();

    if (document.getElementById(txtBenefitPercent) != null)
        document.getElementById(txtBenefitPercent).value = benefitPercent.toString();

    if (document.getElementById(txtFinanceActualValue) != null)
        document.getElementById(txtFinanceActualValue).value = actualValue.toFixed(3).toString();
}

function CalculateEstimatedFinancialBenefit(txtFinanceBaseLineValue, txtFinanceTargetValue, txtFinanceBenefitValue, txtBenefitPercent) {
    var baseLineValue = 0;
    var targetValue = 0;
    var benefitValue = 0;
    var benefitPercent = 0;

    try {
        if (document.getElementById(txtFinanceBaseLineValue) != null)
            baseLineValue = parseFloat(document.getElementById(txtFinanceBaseLineValue).value.replace(/,/g, ''));

        if (document.getElementById(txtFinanceTargetValue) != null)
            targetValue = parseFloat(document.getElementById(txtFinanceTargetValue).value.replace(/,/g, ''));

        // Check if values are valid
        if (isNaN(baseLineValue))
            baseLineValue = 0;

        if (isNaN(targetValue))
            targetValue = 0;

        // Calculate the Benefit Value
        if (baseLineValue == 0) {
            benefitValue = targetValue.toFixed(3);
        }
        else {
            benefitValue = (baseLineValue - targetValue).toFixed(3);;
        }

        if (isNaN(benefitValue))
            benefitValue = 0;

        if (baseLineValue > 0)
            benefitPercent = ((benefitValue / baseLineValue) * 100).toFixed(3);
    }
    catch (Error) {
    }

    // Render the values to the controls with 3 decimal places
    if (document.getElementById(txtFinanceBenefitValue) != null)
        document.getElementById(txtFinanceBenefitValue).value = benefitValue.toString();

    if (document.getElementById(txtBenefitPercent) != null)
        document.getElementById(txtBenefitPercent).value = benefitPercent.toString();
}

function CalculateActualFinancialBenefit(txtFinanceBaseLineValue, txtFinanceTargetValue, txtFinanceActualValue, txtFinanceBenefitValue, txtBenefitPercent) {
    var baseLineValue = 0;
    var targetValue = 0;
    var actualValue = 0;
    var benefitValue = 0;
    var benefitPercent = 0;

    try {
        if (document.getElementById(txtFinanceBaseLineValue) != null)
            baseLineValue = parseFloat(document.getElementById(txtFinanceBaseLineValue).value.replace(/,/g, ''));

        if (document.getElementById(txtFinanceTargetValue) != null)
            targetValue = parseFloat(document.getElementById(txtFinanceTargetValue).value.replace(/,/g, ''));

        if (document.getElementById(txtFinanceActualValue) != null)
            actualValue = parseFloat(document.getElementById(txtFinanceActualValue).value.replace(/,/g, ''));

        // Check if values are valid
        if (isNaN(baseLineValue))
            baseLineValue = 0;

        if (isNaN(targetValue))
            targetValue = 0;

        if (isNaN(actualValue))
            actualValue = 0;

        // Calculate the Benefit Value
        if (baseLineValue == 0) {
            benefitValue = (targetValue - actualValue).toFixed(3);
        }
        else {
            benefitValue = (baseLineValue - actualValue).toFixed(3);
        }

        if (isNaN(benefitValue))
            benefitValue = 0;

        if (baseLineValue > 0)
            benefitPercent = ((benefitValue / baseLineValue) * 100).toFixed(3);
    }
    catch (Error) {
    }

    // Render the values to the controls with 3 decimal places
    if (document.getElementById(txtFinanceBenefitValue) != null)
        document.getElementById(txtFinanceBenefitValue).value = benefitValue.toString();

    if (document.getElementById(txtBenefitPercent) != null)
        document.getElementById(txtBenefitPercent).value = benefitPercent.toString();

    if (document.getElementById(txtFinanceActualValue) != null)
        document.getElementById(txtFinanceActualValue).value = actualValue.toFixed(3).toString();
}

function CalculateACTImprovementPercent(txtAchievedValue, txtBaseLineValue, txtImprovePercent) {
    var achievedValue = 0;
    var baseLineValue = 0;
    var actPercent = 0;   //(Formula: ((Achieved Value - Base Line Value) / Base Line Value) x 100)

    try {
        if (document.getElementById(txtAchievedValue) != null)
            achievedValue = parseFloat(document.getElementById(txtAchievedValue).value.replace(/,/g, ''));

        if (document.getElementById(txtBaseLineValue) != null)
            baseLineValue = parseFloat(document.getElementById(txtBaseLineValue).value.replace(/,/g, ''));

        // Check if values are valid
        if (isNaN(achievedValue))
            achievedValue = 0;

        if (isNaN(baseLineValue))
            baseLineValue = 0;

        if (baseLineValue > 0)
            actPercent = (((achievedValue - baseLineValue) / baseLineValue) * 100).toFixed(3);

        if (isNaN(actPercent))
            actPercent = 0;
    }
    catch (Error) {
    }

    if (document.getElementById(txtImprovePercent) != null)
        document.getElementById(txtImprovePercent).value = actPercent.toString();

    if (document.getElementById(txtAchievedValue) != null)
        document.getElementById(txtAchievedValue).value = achievedValue.toFixed(3).toString();
}

function DisplayMGPAuthorization(url) {
    var width = screen.availWidth / 1.5;
    var height = screen.availHeight / 1.5;
    var xPos = (screen.availWidth - width) / 2;
    var yPos = (screen.availHeight - height) / 2;

    if (mgpWin != null) {
        var userGuidenew = window.open("", "MGPPage", "");
        userGuidenew.close();

        //Re-open the page
        mgpWin = window.open(url, "MGPPage", "menubar=no, status=1, resizable=yes, scrollbars=yes, toolbar=no, " +
            "top=" + yPos.toString() + ", left=" + xPos.toString() + ", heigt=" + height.toString() + ", width=" + width.toString());

        if (document.all) {
            mgpWin.focus();
        }
    }
    else {
        mgpWin = window.open(url, "MGPPage", "menubar=no, status=1, resizable=yes, scrollbars=yes, toolbar=no, " +
            "top=" + yPos.toString() + ", left=" + xPos.toString() + ", heigt=" + height.toString() + ", width=" + width.toString());

        if (document.all) {
            mgpWin.focus();
        }
    }
    return false;
}

function CalculateBPIMemberOperationalBenefit(txtBaseLineValue, txtTargetValue, txtImprovePercent, cboOperationalImproveType) {
    var baseLineValue = 0;
    var targetValue = 0;
    var improvementType = "";
    var improvementPercent = 0;

    try {
        if (document.getElementById(txtBaseLineValue) != null)
            baseLineValue = parseFloat(document.getElementById(txtBaseLineValue).value.replace(/,/g, ''));

        if (document.getElementById(txtTargetValue) != null)
            targetValue = parseFloat(document.getElementById(txtTargetValue).value.replace(/,/g, ''));

        if (document.getElementById(cboOperationalImproveType) != null) {
            improvementType = document.getElementById(cboOperationalImproveType).control._value;
        }

        // Check if values are valid
        if (isNaN(baseLineValue))
            baseLineValue = 0;

        if (isNaN(targetValue))
            targetValue = 0;

        //Calculate the Target Improvement Percent
        if (improvementType == "valDesc") {
            if (baseLineValue > 0) {
                improvementPercent = ((baseLineValue - targetValue) / baseLineValue) * 100;
            }
            else {
                improvementPercent = 100;
            }
        }
        else {
            if (baseLineValue > 0) {
                improvementPercent = ((targetValue - baseLineValue) / baseLineValue) * 100;
            }
            else {
                improvementPercent = 100;
            }
        }
    }
    catch (Error) {
    }

    if (document.getElementById(txtImprovePercent) != null)
        document.getElementById(txtImprovePercent).value = improvementPercent.toFixed(3).toString();
}

function CalculateBPIMemberFinancialBenefit(txtFinanceBaseLineValue, txtFinanceTargetValue, txtBenefitPercent, txtFinanceBenefitValue, cboFinancialImproveType) {
    var baseLineValue = 0;
    var targetValue = 0;
    var improvementType = "";
    var benefitPercent = 0;
    var benefitValue = 0;

    try {
        if (document.getElementById(txtFinanceBaseLineValue) != null)
            baseLineValue = parseFloat(document.getElementById(txtFinanceBaseLineValue).value.replace(/,/g, ''));

        if (document.getElementById(txtFinanceTargetValue) != null)
            targetValue = parseFloat(document.getElementById(txtFinanceTargetValue).value.replace(/,/g, ''));

        if (document.getElementById(cboFinancialImproveType) != null) {
            improvementType = document.getElementById(cboFinancialImproveType).control._value;
        }

        // Check if values are valid
        if (isNaN(baseLineValue))
            baseLineValue = 0;

        if (isNaN(targetValue))
            targetValue = 0;

        //Calculate the Benefit Percent
        if (improvementType == "valDesc") {
            if (baseLineValue > 0) {
                benefitPercent = ((baseLineValue - targetValue) / baseLineValue) * 100;
            }
            else {
                benefitPercent = 100;
            }
        }
        else {
            if (baseLineValue > 0) {
                benefitPercent = ((targetValue - baseLineValue) / baseLineValue) * 100;
            }
            else {
                benefitPercent = 100;
            }
        }

        if (isNaN(benefitPercent))
            benefitPercent = 0;

        //Calculate the Benefit Value
        if (improvementType == "valDesc") {
            benefitValue = baseLineValue - targetValue;
        }
        else {
            benefitValue = targetValue - baseLineValue;
        }

        if (isNaN(benefitValue))
            benefitValue = 0;
    }
    catch (Error) {
    }

    if (document.getElementById(txtBenefitPercent) != null)
        document.getElementById(txtBenefitPercent).value = benefitPercent.toFixed(3).toString();

    if (document.getElementById(txtFinanceBenefitValue) != null)
        document.getElementById(txtFinanceBenefitValue).value = benefitValue.toFixed(3).toString();
}

function PerformClientSidePostback(btnPostBack) {
    if (document.getElementById(btnPostBack) != null)
        document.getElementById(btnPostBack).click();
}

function EnableCheckboxThreeStates(chk)
{
    var checkBox = document.getElementById(chk);
    if (checkBox != null)
        checkBox.indeterminate = true;
}
