// #region Constants
var toastTypes = {
    info: 0,
    success: 1,
    warning: 2,
    error: 3
}
var formURLs = {
    ContractorInquiry: "ContractorInquiry.aspx",
    ContractorRegistration: "RegisterContractor.aspx",
    IDCardGenerator: "IDCardGenerator.aspx",
    ReportViewer: "ContractorReportViewer.aspx"
}

var ModalTypes = {
    DeleteConfirmation: "delete",
    ContractorIDConfirmation: "newcontractorid",
    EmployeeIDConfirmation: "newemployeeid",
    RegisterLicense: "license",
    IDCardExist: "cardexist",
    ManageCardHistory: "cardhistory"
};

var ModalFormLoadType = {
    OpenExistingRecord: 0,
    AddNewRecord: 1,
    UpdateRecord: 2,
    DeleteRecord: 3
};

var ModalResponseType = {
    ModalYes: "modalYes",
    ModalNo: "modalNo",
    ModalCancel: "modalCancel",
    ModalSave: "modalSave",
    ModalDelete: "modalDelete"
};

var FormModes = {
    ClearForm: 0,
    LoadExistingRecord: 1,
    CreateNewRecord: 2,
    UpdateRecord: 3,
    DeleteRecord: 4
}

var ReportTypes = {
    IDCardLicenseReport: 0,
    LicenseOnlyReport: 1,
    IDCardOnlyReport: 2,
    ContractorDetailsReport: 3
}

const FormAccessIndex = {
    Create: 0,
    Retrieve: 1,
    Update: 2,
    Delete: 3,
    Print: 4
};

const CONST_RETRIEVE_DENIED = "Sorry, you don\'t have access to retrieve data. Please contact ICT or create a Helpdesk request!";
const CONST_CREATE_DENIED = "Sorry, you don\'t have access to create new record. Please contact ICT or create a Helpdesk request!";
const CONST_UPDATE_DENIED = "Sorry, you don\'t have access to update record. Please contact ICT or create a Helpdesk request!";
const CONST_DELETE_DENIED = "Sorry, you don\'t have access to delete record. Please contact ICT or create a Helpdesk request!";
const CONST_PRINT_DENIED = "Sorry, you don\'t have access to print a report. Please contact ICT or create a Helpdesk request!";
// #endregion

// #region Global variables
var gContainer;
var gUserFormAccess = {};
// #endregion

$(function () {

});

//#region Loading Panel Methods
function ShowLoadingPanel(container, num, text) {
    ShowWaitMe(container, num, text);
}

function HideLoadingPanel(container) {
    if (container != null && container != undefined)
        HideWaitMe(container);
}

function ShowWaitMe(container, num, text) {
    var effect = '',
        maxSize = '',
        fontSize = '',
        color = '',             // Color for background animation and text (string).
        textPos = ''            // Options: 'vertical' | 'horizontal'

    switch (num) {
        case 1: // Entire form
            effect = 'win8',
            maxSize = '200';
            fontSize = '20px';
            color = '#1E4A6D';
            textPos = 'vertical';
            break;

        case 2: // Selected section
            effect = 'stretch';
            maxSize = '200';
            fontSize = '14px';
            color = '#3F92B7', //'#000',
            textPos = 'horizontal';
            break;
    }

    if (container != null || container != undefined) {
        container.waitMe({
            effect: effect,
            text: text,
            bg: 'rgba(255,255,255,0.8)',
            color: color,
            maxSize: maxSize,
            source: '',
            textPos: textPos,
            fontSize: fontSize,
            waitTime: -1,
            onClose: function () { }
        });
    }
}

function HideWaitMe(container) {
    if (container != null || container != undefined) {
        container.waitMe('hide');
    }
}
//#endregion

//#region Show Error/Message Methods
function ShowMessageBox(message) {
    //$('label[id$=lblAlertTitle]')[0].innerText = title;
    $('label[id$=lblAlertMsg]')[0].innerText = message;
    $('#messageBox').modal('show');
}

function ShowErrorMessage(message) {
    $(".errorMsg").html(message);
    $(".errorMsgBox").removeAttr("hidden");
}

function HideErrorMessage() {
    $(".errorMsg").html("");
    $(".errorMsgBox").attr("hidden", true);
}


function ShowSuccessMessage(message) {
    $(".successMsg").html(message);
    $(".successMsgBox").removeAttr("hidden");
}

function ShowToastMessage(type, msgText, msgTitle) {
    switch (type) {
        case toastTypes.info:
            toastr.info(msgText, msgTitle,
               {
                   "closeButton": true,
                   "progressBar": true,
                   "newestOnTop": true,
                   "preventDuplicates": true
               });
            break;

        case toastTypes.success:
            toastr.success(msgText, msgTitle,
               {
                   "closeButton": true,
                   "progressBar": true,
                   "newestOnTop": true,
                   "preventDuplicates": true
               });
            break;

        case toastTypes.warning:
            toastr.warning(msgText, msgTitle,
               {
                   "closeButton": true,
                   "progressBar": true,
                   "newestOnTop": true,
                   "preventDuplicates": true
               });
            break;

        case toastTypes.error:
            toastr.error(msgText, msgTitle,
               {
                   "closeButton": true,
                   "progressBar": true,
                   "newestOnTop": true,
                   "preventDuplicates": true
               });
            break;
    }
}

function HideToastMessage() {
    toastr.clear();
}
//#endregion

//#region Helper Methods
function IsValidDate(dateInput) {
    try {
        var date = moment(dateInput, ["DD-MM-YYYY", "MM-DD-YYYY", "YYYY-MM-DD"]);
        return date.isValid();
    }
    catch (err) {
        return false;
    }
}

function CheckIfNoValue(obj) {
    return obj == undefined || obj == null || obj == "";
}

function ConvertToISODate(inputString) {
    try {
        var date = moment(inputString, ["DD-MM-YYYY", "MM-DD-YYYY", "YYYY-MM-DD"]);
        return date.format("YYYY-MM-DD");
    }
    catch (err) {
        return null;
    }
}

function GetDateValue(dtPicker) {
    var result = "";

    if (dtPicker == null || dtPicker == undefined || dtPicker.val().length == 0)
        return;

    var d = new Date(dtPicker.val());
    if (Object.prototype.toString.call(d) === "[object Date]") {
        if (!isNaN(d.getTime())) {
            result = d.getFullYear() + "-" + d.getMonth() + 1 + "-" + d.getDate();
        }
    }
    return result;
}

function GetIntValue(inputString) {
    try {
        let value = parseInt(inputString);
        return isNaN(value) ? 0 : value;
    } catch (e) {
        return 0;
    }
}

function GetFloatValue(inputString) {
    try {
        let value = parseFloat(inputString);
        return isNaN(value) ? 0 : value;
    } catch (e) {
        return 0;
    }
}

function GetStringValue(input) {
    if (input != undefined && input != null) {
        return input.toString();
    }
    else
        return "";
}

function CreateGUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function OnlyNumberKey(evt) {
    // Only ASCII character in that range allowed
    var ASCIICode = (evt.which) ? evt.which : evt.keyCode
    if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
        return false;
    return true;
}

function GetQueryStringValue(key) {
    key = key.replace(/[*+?^$.\[\]{}()|\\\/]/g, "\\$&"); // escape RegEx control chars
    var match = location.search.match(new RegExp("[?&]" + key + "=([^&]+)(&|$)"));
    return match && decodeURIComponent(match[1].replace(/\+/g, " "));
}

function SaveDataToSession(key, value) {
    sessionStorage.setItem(key, value);
}

function GetDataFromSession(key) {
    return sessionStorage.getItem(key);
}

function DeleteDataFromSession(key) {
    sessionStorage.removeItem(key);
}
//#endregion

// #region User Form Access Methods
function HasAccess(userAccess, formAccessIndex) {
    if (CheckFormAccess(userAccess, formAccessIndex))
        return true;
    else
        return false;
}

function CheckFormAccess(access, formAccess) {
    var hasAccess = false;

    try {
        var formAccessIndex = Number(formAccess);
        if (access.length > formAccessIndex && String(access).substr(formAccessIndex, 1) == "1") {
            hasAccess = true;
        }

        return hasAccess;
    } catch (e) {
        return false;
    }

}

function GetUserFormAccess(formCode, costCenter, empNo) {
    try {
        var userAcessParam = {
            mode: 1,
            userFrmFormAppID: 0,
            userFrmFormCode: formCode,
            userFrmCostCenter: costCenter,
            userFrmEmpNo: empNo
        };

        // Convert object to JSON
        var jsonData = JSON.stringify({
            userAcessParam: userAcessParam
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "/WebService/ContractorWS.asmx/GetUserFormAccess",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d.toString();
                if (!result.includes(CONST_FAILED)) {
                    var objList = JSON.parse(result);

                    gUserFormAccess.EmpNo = objList.EmpNo;
                    gUserFormAccess.EmpName = objList.EmpName;
                    gUserFormAccess.CostCenter = objList.CostCenter;
                    gUserFormAccess.FormCode = objList.FormCode;
                    gUserFormAccess.FormName = objList.FormName;
                    gUserFormAccess.FormPublic = objList.FormPublic;
                    gUserFormAccess.UserFrmCRUDP = objList.UserFrmCRUDP;
                }
            },
            error: function (err) {
                throw err;
            }
        });
    }
    catch (err) {
        throw err;
    }
}
// #endregion

