// #region Variables
const CONST_ALL = "valAll";
const CONST_EMPTY = "valEmpty";
const CONST_SUCCESS = "SUCCESS";
const CONST_FAILED = "FAILED";
const CONST_CARD_EXIST = "CARDEXIST";
const CONST_EMPPHOTO_FOLDER = "/Images/EmployeePhoto/";
const CONST_DEFAULT_PHOTO = "/Images/no_photo_big.png";
const CONST_BASE64_URI = "data:image;base64,";
const CONST_BASE64_KEY = "base64,";

const _licenseArray = [];
const _cardHistoryArray = [];
const formTypes = {
    NotSet: 0,
    ManageContractor: 1,
    ManageEmployee: 2
};

var _callerForm;
var queryStrContractorNo;
var _currentFormType;
var _currentFormMode;
var _currentReportType;
var _modalFormType;
var _modalFormLoadType;
var _modalResponse;
var _bypassSecurity = false;
// #endregion

// #region Declare Override Global Functions
$.fn.hasAttr = function (name) {
    return this.attr(name) !== undefined;
}

$.fn.getDateValue = function (dateString) {
    var year = dateString.substring(dateString.lastIndexOf("/") + 1);
    var month = dateString.substring(dateString.indexOf("/") + 1, dateString.indexOf("/") + 4);
    var day = dateString.substring(0, dateString.indexOf("/"))

    return new Date(year + "-" + month + "-" + day);
}

$.fn.getISODate = function (dateInput) {
    try {
        if (dateInput.length > 0)
            return new Date(dateInput).toISOString()
        else
            return "";
    }
    catch (err) {
        throw err;
    }

}
// #endregion

$(function () {
    try {

        // Initialize variables
        gContainer = $('.formWrapper');        
        _currentFormType = formTypes.ManageContractor;

        // Get query string values
        _callerForm = GetQueryStringValue("callerForm");
        queryStrContractorNo = GetQueryStringValue("contractorNo");

        var isBack = Boolean(GetQueryStringValue("isback"));
        if (isBack) {
            _callerForm = GetDataFromSession("cardGeneratorCF");
            DeleteDataFromSession("cardGeneratorCF");
        }
        
        // Show loading panel
        ShowLoadingPanel(gContainer, 1, 'Initalizing form, please wait...');
       
        //#region Show tooltips       
        $("#btnSearch").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to open the Contractor Inquiry form.</span>",
            placement: "right",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnBrowse").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to upload photo from the file system.</span>",
            placement: "left",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnRemovePhoto").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to remove the person\'s photo.</span>",
            placement: "right",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnCreateNew").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to create new ID card.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnSave").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this here to save new record.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnUpdate").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to update changes in the record.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnDelete").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to delete the record.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnReset").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to clear the form.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnPrint").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to view and print the ID card report.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnBack").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to go back to previous page.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $(".pictureFrame img").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>This is the person\'s photo which will be printed in the ID card. To add or change the picture, click the \"Browse\" button below. </span>",
            placement: "right",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#conEmpSwitch").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to switch managing ID card information between Contractor and Employee. </span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#showCardPanel .linkTitle").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to edit the contractor information.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        //#endregion

        // #region Initialize input controls
        $("#txtIssuedDate").datepicker({
            dateFormat: "dd/mm/yy",
            altField: "#hdnIssuedDate",
            altFormat: "yy-mm-dd",
            duration: "slow",
            prevText: "Click for previous months",
            nextText: "Click for next months",
            showOtherMonths: true,
            selectOtherMonths: true,
            changeMonth: true,
            changeYear: true,
            numberOfMonths: [1, 1],
            showWeek: false,
            showAnim: "slideDown"
        });
        $("#txtExpiryDate").datepicker({
            dateFormat: "dd/mm/yy",
            altField: "#hdnExpiryDate",
            altFormat: "yy-mm-dd",
            duration: "slow",
            prevText: "Click for previous months",
            nextText: "Click for next months",
            showOtherMonths: true,
            selectOtherMonths: true,
            changeMonth: true,
            changeYear: true,
            numberOfMonths: [1, 1],
            showWeek: false,
            showAnim: "slideDown"
        });
        $("#txtContractorNo").attr("onkeypress", "return OnlyNumberKey(event)");    // Allow numbers only
        $("#txtEmpNo").attr("onkeypress", "return OnlyNumberKey(event)");    // Allow numbers only
        // #endregion

        // #region Initialize event handlers
        $("input:required").on({
            focus: function () {
                $(this).css("border", "2px solid red");
                $(this).css("border-radius", "5px");
            },
            blur: function () {
                $(this).css("border", $("input:optional").css("border"));
                $(this).css("border-radius", $("input:optional").css("border"));
            }
        });

        // Show red border to all required input fields in the modal forms
        $("modal input:required").on({
            focus: function () {
                $(this).css("border", "2px solid red");
                $(this).css("border-radius", "5px");
            },
            blur: function () {
                $(this).css("border", $("input:optional").css("border"));
                $(this).css("border-radius", $("input:optional").css("border"));
            }
        });

        $("#txtContractorNo").on("keypress", function(event) {
                var ASCIICode = event.which || event.keyCode
                if (ASCIICode == 13) {      // Enter key 
                    $("#btnFind").click();
                }
           }
        );

        $("#txtEmpNo").on("keypress", function (event) {
                var ASCIICode = event.which || event.keyCode
                if (ASCIICode == 13) {      // Enter key 
                    $("#btnFindEmp").click();
                }
            }
        );

        $(".actionButton").on("click", handleActionButtonClick);
        $(".formButton").on("click", handleActionButtonClick);
        $(".pictureButton").on("click", handleActionButtonClick);
        $(".dropdown-menu .link-basic").on("click", handleActionButtonClick);
        $("#collapseLicense .linkTitle").on("click", handleAddNewLicenseLink);      // Add New License link
        $("#collapseCardInfo .linkTitle").on("click", handleAddNewCardLink);        // Add New Card link
        $("#showCardPanel .linkTitle").click(function () {
            ShowLoadingPanel(gContainer, 1, 'Redirecting to Contractor Registration page, please wait...');
            SaveDataToSession("contractorNo", $("#txtContractorNo").val());
            location.href = formURLs.ContractorRegistration.concat("?callerForm=").concat(formURLs.IDCardGenerator);
        });

        $("#conEmpSwitch").click(function() {
            if ($(this).is(":checked"))
                handleToggleContractorClick();
            else
                handleToggleEmployeeClick();
        });

        $("#cardInfoSwitch").click(function () {
            if ($(this).is(":checked")) {
                // Show the Card Information panel
                $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").text("Hide Card Information");
                $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").removeClass("text-primary");
                $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").addClass("text-secondary");
                $("#collapseCardInfo").collapse("show");

                populateCardHistoryTable();
            }
            else {
                // Show the Employee panel
                $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").text("Show Card Information");
                $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").removeClass("text-secondary");
                $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").addClass("text-primary");
                $("#collapseCardInfo").collapse("hide");
            }
        });

        $("#licenseInfoSwitch").click(function () {
            if ($(this).is(":checked")) {
                // Show the Card Information panel
                $(".custom-switch label[data-switch-value$='ShowHideLicenseInfo']").text("Hide License Information");
                $("#collapseLicense").collapse("show");
            }
            else {
                // Show the Employee panel
                $(".custom-switch label[data-switch-value$='ShowHideLicenseInfo']").text("Show License Information");
                $("#collapseLicense").collapse("hide");
            }
        });
        //$("#uploadPhoto").on("change", copyPhoto);
        
        // Modal form events
        $(".modal-footer > button").on("click", handleModalButtonClick);
        $(".modal").on("show.bs.modal", handleShowModalForm);
        $(".modal").on("hide.bs.modal", handleHideModalForm);

        // #endregion

        // Initialize user form access
        GetUserFormAccess($("#hidFormCode").val().trim(), $("#hidCostCenter").val().trim(), GetIntValue($("#hidCurrentUserEmpNo").val()));

        getLookupTable();
        resetForm();                

    } catch (err) {
        ShowErrorMessage("The following exception has occured while loading the page: " + err);
    }    
});

// #region Event Handlers
function handleCardInfoChecked() {
    // Show the Card Information panel
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").text("Hide Card Information");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").removeClass("text-primary");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").addClass("text-secondary");
    $("#collapseCardInfo").collapse("show");
        
    populateCardHistoryTable();
}

function handleToggleContractorClick() {
    // Show the Contractor panel
    $(".custom-switch label[data-switch-value$='ShowHideContractEmp']").text("Manage Contractor ID Card");
    $("#collapseContractor").collapse("show");
    $("#collapseEmployee").collapse("hide");
    $("#collapseLicense .linkTitle").attr("hidden", "hidden");
    $("#txtContractorNo").focus();

    // Reset Card Information
    $("#cardInfoSwitch").prop("checked", false);        
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").text("Show Card Information");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").removeClass("text-secondary");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").addClass("text-primary");
    $("#collapseCardInfo").collapse("hide");
    $("#showCardPanel").attr("hidden", "hidden");

    //#region Clear license grid
    if (_licenseArray.length > 0)
        _licenseArray.splice(0, _licenseArray.length);

    var table = $("#licenseTable").dataTable().api();
    table.clear().draw();
    //#endregion

    //#region Clear card history grid
    if (_cardHistoryArray.length > 0)
        _cardHistoryArray.splice(0, _cardHistoryArray.length);

    var table = $("#cardHistoryTable").dataTable().api();
    table.clear().draw();
    //#endregion

    // #region Reset input controls
    if (_currentFormType == formTypes.ManageContractor) {
        $("#collapseContractor input").val("");
        $("#collapseContractor select").val("");

        // Show "Edit Details" link
        $("#showCardPanel .linkTitle").prop("hidden", false);
    }
    else {
        $("#collapseEmployee input").val("");
        $("#collapseEmployee select").val("");
        $("#collapseEmployee input[data-entry='yes']").prop("readonly", true);
        $("#collapseEmployee select").prop("disabled", true);

        // Hide "Edit Details" link
        $("#showCardPanel .linkTitle").prop("hidden", true);
    }
    $("#hidCardRegistryID").val("");
    $("#hidRegistryID").val("");

    // Reset employee photo panel
    initializeEmpPhoto(false, true);
    //#endregion

    // Set the flag
    _currentFormType = formTypes.ManageContractor;
}

function handleToggleEmployeeClick() {
    // Show the Employee panel
    $(".custom-switch label[data-switch-value$='ShowHideContractEmp']").text("Manage Employee ID Card");
    $("#collapseContractor").collapse("hide");
    $("#collapseEmployee").collapse("show");
    $("#collapseLicense .linkTitle").removeAttr("hidden");
    $("#txtEmpNo").focus();

    // Reset Card Information
    $("#cardInfoSwitch").prop("checked", false);
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").text("Show Card Information");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").removeClass("text-secondary");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").addClass("text-primary");
    $("#collapseCardInfo").collapse("hide");
    $("#showCardPanel").attr("hidden", "hidden");

    //#region Clear license grid
    if (_licenseArray.length > 0)
        _licenseArray.splice(0, _licenseArray.length);

    var table = $("#licenseTable").dataTable().api();
    table.clear().draw();
    //#endregion

    //#region Clear card history grid
    if (_cardHistoryArray.length > 0)
        _cardHistoryArray.splice(0, _cardHistoryArray.length);

    var table = $("#cardHistoryTable").dataTable().api();
    table.clear().draw();
    //#endregion

    // #region Reset input controls
    if (_currentFormType == formTypes.ManageContractor) {
        $("#collapseContractor input").val("");
        $("#collapseContractor select").val("");

        // Show "Edit Details" link
        $("#showCardPanel .linkTitle").prop("hidden", false);
    }
    else {
        $("#collapseEmployee input").val("");
        $("#collapseEmployee select").val("");
        $("#collapseEmployee input[data-entry='yes']").prop("readonly", true);
        $("#collapseEmployee select").prop("disabled", true);

        // Hide "Edit Details" link
        $("#showCardPanel .linkTitle").prop("hidden", true);
    }
    $("#hidCardRegistryID").val("");
    $("#hidRegistryID").val("");

    // Reset employee photo panel
    initializeEmpPhoto(false, true);
    //#endregion

    // Set the flag
    _currentFormType = formTypes.ManageEmployee;
}

function handleActionButtonClick() {
    var btn = $(this);
    var hasError = false;
    var empNo = 0;

    // Hide all error messages
    HideErrorMessage();
    HideToastMessage();

    switch ($(btn)[0].id) {
        case "btnFind":
            if (_currentFormMode == FormModes.CreateNewRecord)
                beginFindContractor();
            else {
                // Check Contractor No.
                if ($('#txtContractorNo').val().trim().length == 0) {
                    displayAlert($('#contractorNoValid'), "<b>" + $("#collapseContractor label[data-field='ContractorNo']").text() + "</b> is required and cannot be left blank.", $('#txtContractorNo'));
                    hasError = true;
                }
                else {
                    if ($('#contractorNoValid').attr("hidden") == undefined)
                        $('#contractorNoValid').attr("hidden", "hidden");

                    empNo = GetIntValue($("#txtContractorNo").val());
                }

                if (!hasError)
                    searchIDCard(empNo, true);
            }
            break;

        case "btnFindEmp":
            if (_currentFormMode == FormModes.CreateNewRecord)
                beginFindEmployee();
            else {
                // Check Emp. No.
                if ($('#txtEmpNo').val().trim().length == 0) {
                    displayAlert($('#empNoValid'), "<b>" + $("#collapseContractor label[data-field='EmpNo']").text() + "</b> is required and cannot be left blank.", $('#txtEmpNo'));
                    hasError = true;
                }
                else {
                    if ($('#empNoValid').attr("hidden") == undefined)
                        $('#empNoValid').attr("hidden", "hidden");

                    empNo = GetIntValue($("#txtEmpNo").val());
                    if (isNaN(empNo))
                        empNo = 0;
                    else {
                        if (empNo.toString().length == 4) {
                            empNo += 10000000;
                            $("#txtEmpNo").val(empNo);
                        }
                    }
                }

                if (!hasError)
                    searchIDCard(empNo, true);
            }
            break;

        case "btnCreateNew":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Create)) 
                initializeNewIDCard(this);
            else
                ShowToastMessage(toastTypes.error, CONST_CREATE_DENIED, "Access Denied");
            break;

        case "btnSave":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Create))
                beginAddIDCard();
            else
                ShowToastMessage(toastTypes.error, CONST_CREATE_DENIED, "Access Denied");
            break;

        case "btnUpdate":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Update))
                beginUpdateIDCard();
            else
                ShowToastMessage(toastTypes.error, CONST_UPDATE_DENIED, "Access Denied");
            break;

        case "btnDelete":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Delete))
                beginDeleteIDCard();
            else
                ShowToastMessage(toastTypes.error, CONST_DELETE_DENIED, "Access Denied");
            break;

        case "btnReset":
            var regID = GetIntValue($("#hidCardRegistryID").val());
            resetForm();

            // Reload data in the form if caller form is not null
            if (_callerForm != "undefined" && _callerForm != null) {
                if (queryStrContractorNo > 0 && regID > 0) {
                    $("#txtContractorNo").val(queryStrContractorNo);
                    $("#btnFind").click();
                }
            }
            break;

        case "btnPrint":
        case "btnPrintLicenseOnly":
        case "btnPrintIDOnly":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Print)) {
                if ($(btn)[0].id == "btnPrintLicenseOnly")
                    _currentReportType = ReportTypes.LicenseOnlyReport;
                else if ($(btn)[0].id == "btnPrintIDOnly")
                    _currentReportType = ReportTypes.IDCardOnlyReport;
                else
                    _currentReportType = ReportTypes.IDCardLicenseReport;

                beginPrintIDCard();
            }
            else
                ShowToastMessage(toastTypes.error, CONST_PRINT_DENIED, "Access Denied");
            break;

        case "btnBack":
            ShowLoadingPanel(gContainer, 1, 'Going back to previous page, please...');
            if (_callerForm != "undefined" && _callerForm != null)
                location.href = _callerForm.concat("?isback=true");
            else
                location.href = _callerForm;
            break;

        case "btnBrowse":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Update)) {
                $(this).popover("hide");
                $("#uploadPhoto").trigger("click");
            }
            else
                ShowToastMessage(toastTypes.error, CONST_UPDATE_DENIED, "Access Denied");
            break;

        case "btnRemovePhoto":
            //if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Update)) {
                $(this).popover("hide");
                $(".pictureFrame img").attr("src", CONST_DEFAULT_PHOTO);
                $("#hidImageFileName").val("");
                $("#hidBase64Photo").val("");
            //}
            //else
            //    ShowToastMessage(toastTypes.error, CONST_UPDATE_DENIED, "Access Denied");
            break;

        case "btnSearch":
            ShowLoadingPanel(gContainer, 1, 'Please wait...');
            location.href = formURLs.ContractorInquiry.concat("?callerForm=").concat(formURLs.IDCardGenerator);
            break;
    }
}

function handleHideModalForm() {
    if (_modalResponse != undefined && _modalFormType != undefined) {
        var empConNo;

        switch (_modalFormType) {
            case ModalTypes.DeleteConfirmation:
                if (_modalResponse == ModalResponseType.ModalYes) {                    
                    deleteIDCard();
                }
                break;

            case ModalTypes.RegisterLicense:
                if (_modalResponse == ModalResponseType.ModalSave ||
                    _modalResponse == ModalResponseType.ModalDelete) {
                    resetModalForm();
                }
                break;

            case ModalTypes.ManageCardHistory:
                if (_modalResponse == ModalResponseType.ModalSave ||
                    _modalResponse == ModalResponseType.ModalDelete) {
                    resetModalForm();
                }
                break;

            case ModalTypes.ContractorIDConfirmation:
                if (_modalResponse == ModalResponseType.ModalYes) {
                    empConNo = GetIntValue($("#txtContractorNo").val());
                    $("#btnCreateNew").click();
                    $("#txtContractorNo").val(empConNo);
                    $("#btnFind").click();
                }
                break;

            case ModalTypes.EmployeeIDConfirmation:
                if (_modalResponse == ModalResponseType.ModalYes) {
                    empConNo = GetIntValue($("#txtEmpNo").val());
                    $("#btnCreateNew").click();
                    $("#txtEmpNo").val(empConNo);
                    $("#btnFindEmp").click();
                }
                break;

            case ModalTypes.IDCardExist:
                if (_modalResponse == ModalResponseType.ModalYes) {
                    _currentFormMode = FormModes.LoadExistingRecord;
                    empConNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());

                    // Clear the form
                    $("#btnReset").click();

                    if (_currentFormType == formTypes.ManageContractor) {
                        $("#txtContractorNo").val(empConNo);
                        $("#btnFind").click();
                    }
                    else {
                        $("#txtEmpNo").val(empConNo);
                        $("#btnFindEmp").click();
                    }
                }
                break;
        }
    }
}

function handleShowModalForm() {
    switch (_modalFormType) {
        case ModalTypes.RegisterLicense:
            if (_modalFormLoadType == ModalFormLoadType.AddNewRecord) {
                if (!$(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").attr("hidden", "hidden");
            }
            else if (_modalFormLoadType == ModalFormLoadType.OpenExistingRecord || _modalFormLoadType == ModalFormLoadType.UpdateRecord) {
                if ($(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").removeAttr("hidden");
            }
            break;

        case ModalTypes.ManageCardHistory:
            if (_modalFormLoadType == ModalFormLoadType.AddNewRecord) {
                if (!$(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").attr("hidden", "hidden");
            }
            else if (_modalFormLoadType == ModalFormLoadType.OpenExistingRecord || _modalFormLoadType == ModalFormLoadType.UpdateRecord) {
                if ($(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").removeAttr("hidden");
            }
            break;
    }
}

function handleModalButtonClick() {
    var btnAttrib = $(this).attr("data-button-value");

    if (btnAttrib == ModalResponseType.ModalYes)
        _modalResponse = ModalResponseType.ModalYes;

    else if (btnAttrib == ModalResponseType.ModalNo)
        _modalResponse = ModalResponseType.ModalNo;

    else if (btnAttrib == ModalResponseType.ModalCancel)
        _modalResponse = ModalResponseType.ModalCancel;

    else if (btnAttrib == ModalResponseType.ModalDelete) {
        _modalResponse = ModalResponseType.ModalDelete;

        switch (_modalFormType) {
            case ModalTypes.RegisterLicense:
                if (beginDeleteLicense()) {
                    $("#modLicenseRegistration").modal("hide");
                    gContainer = $('.formWrapper');

                    // Show success message
                    ShowToastMessage(toastTypes.success, "License record has been deleted successfully!", "Delete License Notification");
                }
                break;

            case ModalTypes.ManageCardHistory:
                if (beginDeleteCardHistory()) {
                    $("#modCardInfo").modal("hide");
                    gContainer = $('.formWrapper');

                    // Show success message
                    ShowToastMessage(toastTypes.success, "Card history information has been deleted successfully!", "Delete Card Notification");
                }
                break;
        }
    }

    else if (btnAttrib == ModalResponseType.ModalSave) {
        _modalResponse = ModalResponseType.ModalSave;

        switch(_modalFormType)
        {
            case ModalTypes.RegisterLicense:
                if (beginSaveLicense()) {
                    $("#modLicenseRegistration").modal("hide");
                    gContainer = $('.formWrapper');

                    // Show success message
                    //ShowToastMessage(toastTypes.success, "License details have been saved successfully!", "Save License Notification");
                }
                break;

            case ModalTypes.ManageCardHistory:
                if (beginSaveCardHistory()) {
                    $("#modCardInfo").modal("hide");
                    gContainer = $('.formWrapper');

                    // Show success message
                    //ShowToastMessage(toastTypes.success, "Card history information has been saved successfully!", "Save Card Notification");
                }
                break;
        }
    }
}
// #endregion

// #region Private Functions
function displayAlert(obj, errText, focusObj) {
    var alert = $(obj).find(".alert");

    if ($(alert).find(".errorText") != undefined)
        $(alert).find(".errorText").html(errText);

    if (obj != undefined)
        $(obj).removeAttr("hidden");

    $(alert).show();

    if (focusObj != undefined)
        $(focusObj).focus();
}

function showModalConfirmation(modType) {
    // Set the modal header title
    switch (modType) {
        case ModalTypes.DeleteConfirmation:
            _modalFormType = ModalTypes.DeleteConfirmation;

            // Set the title of the modal form
            $("#modalConfirmation .modalHeader").html("&nbsp;Warning");

            // Set the icon of the modal form
            if ($("#modalConfirmationIcon").hasClass("fa-info-circle"))
                $("#modalConfirmationIcon").removeClass("fa-info-circle").addClass("fa-exclamation-triangle");

            // Set the message contents
            $(".modal-body > p").html("Deleting the ID card will <span class='text-info font-weight-bold'>DELETE</span> all associated records in the database. Are you sure you want to proceed?");
            break;

        case ModalTypes.ContractorIDConfirmation:
            _modalFormType = ModalTypes.ContractorIDConfirmation;

            // Set the title of the modal form
            $("#modalConfirmation .modalHeader").html("&nbsp;Confirmation");

            // Set the icon of the modal form
            if ($("#modalConfirmationIcon").hasClass("fa-exclamation-triangle"))
                $("#modalConfirmationIcon").removeClass("fa-exclamation-triangle").addClass("fa-info-circle");

            // Set the modal message contents
            $(".modal-body > p").html("No record was found for the specified contractor. Do you want to <span class='text-info font-weight-bold'>CREATE</span> new ID card?");
            break;

        case ModalTypes.EmployeeIDConfirmation:
            _modalFormType = ModalTypes.EmployeeIDConfirmation;

            // Set the title of the modal form
            $("#modalConfirmation .modalHeader").html("&nbsp;Confirmation");

            // Set the icon of the modal form
            if ($("#modalConfirmationIcon").hasClass("fa-exclamation-triangle"))
                $("#modalConfirmationIcon").removeClass("fa-exclamation-triangle").addClass("fa-info-circle");

            // Set the message contents
            $(".modal-body > p").html("No record was found for the specified employee. Do you want to <span class='text-info font-weight-bold'>CREATE</span> new ID card?");
            break;

        case ModalTypes.IDCardExist:
            _modalFormType = ModalTypes.IDCardExist;

            // Set the title of the modal form
            $("#modalConfirmation .modalHeader").html("&nbsp;Confirmation");

            // Set the icon of the modal form
            if ($("#modalConfirmationIcon").hasClass("fa-exclamation-triangle"))
                $("#modalConfirmationIcon").removeClass("fa-exclamation-triangle").addClass("fa-info-circle");

            if (_currentFormType == formTypes.ManageContractor) 
                $(".modal-body > p").html("Unable to save data because there is an existing ID Card record for the specified contractor. Do you want to <span class='text-info font-weight-bold'>VIEW</span> the details?");
            else
                $(".modal-body > p").html("Unable to save data because there is an existing ID Card record for the specified employee. Do you want to <span class='text-info font-weight-bold'>VIEW</span> the details?");
            break;
    }

    // Show modal form
    $("#modalConfirmation").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
}

function resetForm() {
    HideErrorMessage();

    // Hide all error alerts
    $('.errorPanel').attr("hidden", "hidden");

    //#region Hide all tooltips
    $("#btnSearch").popover("hide");
    $("#btnBrowse").popover("hide");
    $("#btnRemovePhoto").popover("hide");
    $("#btnCreateNew").popover("hide");
    $("#btnSave").popover("hide");
    $("#btnUpdate").popover("hide");
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnBack").popover("hide");
    $(".pictureFrame img").popover("hide");
    $("#conEmpSwitch").popover("hide");
    $("#showCardPanel .linkTitle").popover("hide");
    //#endregion
    
    //#region Enable/Disable buttons
    if ($("#btnCreateNew").hasClass("btn-outline-secondary"))
        $("#btnCreateNew").removeClass("btn-outline-secondary")

    $("#btnCreateNew").addClass("btn-primary").addClass("border-0");
    $("#btnCreateNew").removeAttr("disabled");
    $("#btnPrint").removeAttr("disabled");
    $("#btnBack").removeAttr("disabled");
    //#endregion

    //#region Show/Hide buttons
    $("#btnSave").attr("hidden", "hidden");
    $("#btnUpdate").attr("hidden", "hidden");
    $("#btnDelete").attr("hidden", "hidden");
    $("#printBtnGroup").attr("hidden", "hidden");        
    //#endregion

    //#region Initialize form when called by another form
    if (_callerForm != "undefined" && _callerForm != null) {
        // Show "Go Back" button
        $("#btnBack").prop("hidden", false);

        // Hide "Go" and Search buttons
        $("#btnFind").prop("hidden", true);
        $("#btnSearch").prop("hidden", true);

        // Hide "Edit Details" panel
        //$("#showCardPanel .linkTitle").prop("disabled", true);
        $("#panEditDetails").prop("hidden", true);

        // Disable the Employee/Contractor switch button
        $("#conEmpSwitch").prop("disabled", true);
    }
    else {
        // Enable the Employee/Contractor switch button
        $("#conEmpSwitch").prop("disabled", false);
    }
    //#endregion

    // Refresh DataTables
    if (_licenseArray.length > 0) {
        _licenseArray.splice(0, _licenseArray.length);
    }
    populateLicenseTable();

    if (_cardHistoryArray.length > 0) {
        _cardHistoryArray.splice(0, _cardHistoryArray.length);
    }
    populateCardHistoryTable();

    // Reset variables
    _currentFormMode = FormModes.ClearForm;

    // #region Reset input controls
    if (_currentFormType == formTypes.ManageContractor) {
        $("#collapseContractor input").val("");
        $("#collapseContractor select").val("");        
        $("#txtContractorNo").focus();
    }
    else {
        $("#collapseEmployee input").val("");
        $("#collapseEmployee select").val("");
        $("#collapseEmployee input[data-entry='yes']").prop("readonly", true);
        $("#collapseEmployee select").prop("disabled", true);
        $("#txtEmpNo").focus();
    }

    // Reset hidden fields
    $("#hidCardRegistryID").val("");
    $("#hidCardHistoryID").val("");
    $("#hidRegistryID").val("");
    $("#hidImageFileName").val("");
    $("#hidBase64Photo").val("");

    //$("#txtContractorNo").attr("disabled", "disabled");
    //$("#txtEmpNo").attr("disabled", "disabled");
    //$("#conEmpSwitch").removeAttr("disabled");
    $("#conEmpSwitch").attr("checked", "checked");    
    $("#collapseLicense .linkTitle").attr("disabled", "disabled");
    $("#cboBloodGroup").attr("disabled", "disabled");

    // Reset employee photo panel
    initializeEmpPhoto(false, true);

    // Reset Card Information
    //$("#cardInfoSwitch").removeAttr("checked");       // Use this to set check property applicble to jQuery < 1.6
    $("#cardInfoSwitch").prop("checked", false);        // Use this to set check property applicble to jQuery > 1.6
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").text("Show Card Information");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").removeClass("text-secondary");
    $(".custom-switch label[data-switch-value$='ShowHideCardInfo']").addClass("text-primary");
    $("#collapseCardInfo").collapse("hide");
    $("#showCardPanel").attr("hidden", "hidden");

    // Move to the top of the page
    window.scrollTo(0, 0);

    // Remove focus of all input controls
    //$(":focus").blur();
    // #endregion
}

function setFormDataLoaded() {
    // Hide all popovers
    $("#btnFind").popover("hide");
    $("#btnCreateNew").popover("hide");
    $("#btnSave").popover("hide");
    $("#btnUpdate").popover("hide");
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnBack").popover("hide");
    $(".pictureFrame img").popover("hide");
    $("#conEmpSwitch").popover("hide");
    $("#showCardPanel .linkTitle").popover("hide");

    // Enable "Create New" button
    if ($("#btnCreateNew").hasClass("btn-outline-secondary"))
        $("#btnCreateNew").removeClass("btn-outline-secondary")

    $("#btnCreateNew").addClass("btn-primary").addClass("border-0");
    $("#btnCreateNew").removeAttr("disabled");

    // Enable/Disable buttons
    $("#btnPrint").removeAttr("disabled");
    $("#btnBack").removeAttr("disabled");
    $("#btnFind").removeAttr("disabled");

    // Show and Hide buttons
    $("#btnSave").attr("hidden", "hidden");
    $("#btnUpdate").removeAttr("hidden");
    $("#btnDelete").removeAttr("hidden");
    $("#printBtnGroup").removeAttr("hidden");

    // Display "Show Card Information" panel
    if ($("#showCardPanel").hasAttr("hidden"))
        $("#showCardPanel").removeAttr("hidden");

    if (_currentFormType == formTypes.ManageContractor) {
        // Show "Edit Details" link
        $("#showCardPanel .linkTitle").prop("hidden", false);
    }
    else {
        // Hide "Edit Details" link
        $("#showCardPanel .linkTitle").prop("hidden", true);
    }

    // Enable "Add New License" link
    $("#collapseLicense .linkTitle").removeAttr("disabled");
}

function populateLicenseTable() {
    try {
        if (_licenseArray == undefined || _licenseArray.length == 0) {
            // Get DataTable API instance
            var table = $("#licenseTable").dataTable().api();
            table.clear().draw();
        }
        else {
            $("#licenseTable")
                .on('init.dt', function () {    // This event will fire after loading the data in the table
                    HideLoadingPanel(gContainer);
                })
                .DataTable({
                    data: _licenseArray,
                    processing: true,           // To show progress bar 
                    serverSide: false,          // To enable processing server side processing (e.g. sorting, pagination, and filtering)
                    filter: true,               // To enable/disable filter (search box)
                    orderMulti: false,          // To disable mutiple column sorting
                    destroy: true,              // To destroy an old instance of the table and to initialise a new one
                    scrollX: true,              // To enable horizontal scrolling
                    sScrollX: "100%",
                    sScrollXInner: "110%",      // This property can be used to force a DataTable to use more width than it might otherwise do when x-scrolling is enabled.
                    language: {
                        emptyTable: "No data available in table"
                    },
                    width: "100%",
                    lengthMenu: [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]],
                    iDisplayLength: 10,         // Number of rows to display on a single page when using pagination.
                    order: [[1, 'asc']],
                    fnDrawCallback: function () {
                        $('.lnklicenseNo').on('click', openLicenseDetails);
                    },
                    columns: [
                        {
                            "data": "registryID"
                        },
                        {
                            "data": "empNo"
                        },
                        {
                            "data": "licenseTypeDesc",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            "data": "licenseNo"
                        },
                        {
                            data: "issuedDate",
                            render: function (data, type, row) {
                                return moment(data).format('DD-MMM-YYYY');
                            }
                        },
                        {
                            data: "expiryDate",
                            render: function (data, type, row) {
                                return moment(data).format('DD-MMM-YYYY');
                            }
                        },
                        {
                            "data": "issuingAuthority",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            "data": "remarks",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            data: "createdDate",
                            render: function (data, type, row) {
                                return moment(data).isValid() ? moment(data).format('DD-MMM-YYYY') : "";
                            }
                        },
                        {
                            data: "createdByEmpName"
                        },
                        {
                            data: "lastUpdatedDate",
                            render: function (data, type, row) {
                                return moment(data).isValid() ? moment(data).format('DD-MMM-YYYY') : "";
                            }
                        },
                        {
                            data: "lastUpdatedByEmpName"
                        },
                        {
                            data: "licenseGUID"
                        }
                    ],
                    columnDefs: [
                        {
                            targets: "centeredColumn",
                            className: 'dt-body-center'
                        },
                        {
                            targets: 3,
                            render: function (data, type, row) {
                                if (_currentFormType == formTypes.ManageEmployee)
                                    return '<a href="javascript:void(0)" class="lnklicenseNo gridLink" data-licenseguid=' + row.licenseGUID + '> ' + data + '</a>';
                                else
                                    return data;
                            }
                        },
                        {
                            targets: "hiddenColumn",
                            visible: false
                        },
                        {
                            targets: 9,
                            render: function (data, type, row) {
                                if (row.createdByEmpNo > 0)
                                    return row.createdByEmpNo + ' - ' + row.createdByEmpName;
                                else
                                    return "";
                            }
                        },
                        {
                            targets: 11,
                            render: function (data, type, row) {
                                if (row.lastUpdatedByEmpNo > 0)
                                    return row.lastUpdatedByEmpNo + ' - ' + row.lastUpdatedByEmpName;
                                else
                                    return "";
                            }
                        },
                        {
                            targets: "doNotOrder",
                            orderable: false
                        }
                    ]
                });
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while loading the license in the grid." + "\n\nError: " + err);
    }
}

function openLicenseDetails() {
    var licenseNo = parseInt($(this).text());
    var licenseGUID = $(this).attr("data-licenseguid").trim();

    if (licenseNo == 0 || isNaN(licenseNo)) {
        ShowErrorMessage("Unable to load the details of the selected license record!");
        return;
    }

    // Set the flags
    _modalFormType = ModalTypes.RegisterLicense;
    _modalFormLoadType = ModalFormLoadType.UpdateRecord;

    // Get the selected license 
    var selectedLicense = _licenseArray.find(function (value, index, array) {
        return value.licenseNo == licenseNo &&
            value.licenseGUID.trim() == licenseGUID;
    });

    if (selectedLicense != undefined) {
        $("#hidRegistryID").val(selectedLicense.registryID);
        $("#txtContractorNo").val(selectedLicense.empNo);
        $("#txtLicenseNo").val(selectedLicense.licenseNo);
        $("#cboLicenseType").val(selectedLicense.licenseTypeCode);
        $("#txtIssuedDate").val(ConvertToISODate(selectedLicense.issuedDate));
        $("#txtExpiryDate").val(ConvertToISODate(selectedLicense.expiryDate));
        $("#txtIssuingAuthority").val(selectedLicense.issuingAuthority);
        $("#txtNotes").val(selectedLicense.remarks);
        $("#hidLicenseGUID").val(selectedLicense.licenseGUID);

        // Disable License Type and License No. fields
        $("#cboLicenseType").attr("disabled", "disabled");
        $("#txtLicenseNo").attr("disabled", "disabled");
    }

    $("#modLicenseRegistration").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
}

function initializeNewIDCard(btn) {
    // Hide error message
    HideErrorMessage();

    // Hide all error alerts
    $('.errorPanel').attr("hidden", "hidden");

    //#region Hide all tooltips
    $("#btnSearch").popover("hide");
    $("#btnBrowse").popover("hide");
    $("#btnRemovePhoto").popover("hide");
    $("#btnCreateNew").popover("hide");
    $("#btnSave").popover("hide");
    $("#btnUpdate").popover("hide");
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnBack").popover("hide");
    $(".pictureFrame img").popover("hide");
    $("#conEmpSwitch").popover("hide");
    $("#showCardPanel .linkTitle").popover("hide");
    //#endregion

    //#region Disable Create New button
    if ($(btn).hasClass("btn-primary"))
        $(btn).removeClass("btn-primary");

    if ($(btn).hasClass("border-0"))
        $(btn).removeClass("border-0");

    $(btn).addClass("btn-outline-secondary");
    $(btn).attr("disabled", "disabled");
    //#endregion

    //#region Disable buttons
    $("#btnPrint").attr("disabled", "disabled");
    $("#btnBack").attr("disabled", "disabled");

    // Show/Hide buttons and links
    $("#btnSave").removeAttr("hidden");
    $("#btnUpdate").attr("hidden", "hidden");
    $("#btnDelete").attr("hidden", "hidden");
    $("#printBtnGroup").attr("hidden", "hidden");
    //#endregion

    // Clear License grid
    if (_licenseArray.length > 0) {
        _licenseArray.splice(0, _licenseArray.length);
    }
    populateLicenseTable();

    // Clear Card History grid
    if (_cardHistoryArray.length > 0) {
        _cardHistoryArray.splice(0, _cardHistoryArray.length);
    }
    populateCardHistoryTable

    // Set Form Load flag
    _currentFormMode = FormModes.CreateNewRecord;

    // #region Initialize input controls
    if (_currentFormType == formTypes.ManageContractor) {
        $("#collapseContractor input").val("");
        $("#collapseContractor select").val("");
        $("#txtContractorNo").focus();
    }
    else {
        $("#collapseEmployee input").val("");
        $("#collapseEmployee select").val("");
        $("#txtEmpNo").focus();
    }
    $("#hidCardRegistryID").val("");
    $("#hidRegistryID").val("");

    // Disable the Employee/Contractor switch button
    $("#conEmpSwitch").prop("disabled", true);
    // #endregion
}

function beginAddIDCard() {
    var hasError = false;

    // #region Validate data input
    if (_currentFormType == formTypes.ManageContractor) {
        // Check Contractor No.
        if ($('#txtContractorNo').val().trim().length == 0) {
            displayAlert($('#contractorNoValid'),"<b>" + $("#collapseContractor label[data-field='ContractorNo']").text() + "</b> is required and cannot be left blank.", $('#txtContractorNo'));
            hasError = true;
        }
        else {
            if ($('#contractorNoValid').attr("hidden") == undefined)
                $('#contractorNoValid').attr("hidden", "hidden");
        }

        var registryID = GetIntValue($("#hidRegistryID").val());
        if (registryID == undefined || isNaN(registryID) || registryID == 0) {
            displayAlert($('#contractorNoValid'), "The specified Contractor No. does not exist in the database.", $('#txtContractorNo'));
            hasError = true;
        }
    }
    else {
        // Check Employee No.
        if ($('#txtEmpNo').val().trim().length == 0) {
            displayAlert($('#empNoValid'), "<b>" + $("#collapseEmployee label[data-field='EmpNo']").text() + "</b> is required and cannot be left blank.", $('#txtEmpNo'));
            hasError = true;
        }
        else {
            if ($('#empNoValid').attr("hidden") == undefined)
                $('#empNoValid').attr("hidden", "hidden");
        }

        // Check Employee Name
        if ($('#txtEmpName').val().trim().length == 0) {
            displayAlert($('#empNameValid'), "<b>" + $("#collapseEmployee label[data-field='EmpName']").text() + "</b> is required and cannot be left blank.", $('#txtEmpName'));
            hasError = true;
        }
        else {
            if ($('#empNameValid').attr("hidden") == undefined)
                $('#empNameValid').attr("hidden", "hidden");
        }
        
        // Check Position
        if ($('#txtPosition').val().trim().length == 0) {
            displayAlert($('#positionValid'), "<b>" + $("#collapseEmployee label[data-field='JobTitle']").text() + "</b> is required and cannot be left blank.", $('#txtPosition'));
            hasError = true;
        }
        else {
            if ($('#positionValid').attr("hidden") == undefined)
                $('#positionValid').attr("hidden", "hidden");
        }

        // Check Cost Center
        if ($('#txtCostCenter').val().trim().length == 0) {
            displayAlert($('#costCenterValid'), "<b>" + $("#collapseEmployee label[data-field='CostCenter']").text() + "</b> is required and cannot be left blank.", $('#txtCostCenter'));
            hasError = true;
        }
        else {
            if ($('#costCenterValid').attr("hidden") == undefined)
                $('#costCenterValid').attr("hidden", "hidden");
        }

        // Check CPR No.
        if ($('#txtCPRNo').val().trim().length == 0) {
            displayAlert($('#cprValid'), "<b>" + $("#collapseEmployee label[data-field='CPRNo']").text() + "</b> is required and cannot be left blank.", $('#txtCPRNo'));
            hasError = true;
        }
        else {
            if ($('#cprValid').attr("hidden") == undefined)
                $('#cprValid').attr("hidden", "hidden");
        }
    }
    
    // #endregion

    if (!hasError) {
        // Show the loading panel
        ShowLoadingPanel(gContainer, 1, 'Saving information to database, please wait...');

        //insertIDCard();
        insertIDCardBase64();
    }
}

function beginUpdateIDCard() {
    var hasError = false;

    // #region Validate data input
    if (_currentFormType == formTypes.ManageContractor) {
        // Check Contractor No.
        if ($('#txtContractorNo').val().trim().length == 0) {
            displayAlert($('#contractorNoValid'), "<b>" + $("#collapseContractor label[data-field='ContractorNo']").text() + "</b> is required and cannot be left blank.", $('#txtContractorNo'));
            hasError = true;
        }
        else {
            if ($('#contractorNoValid').attr("hidden") == undefined)
                $('#contractorNoValid').attr("hidden", "hidden");
        }
    }
    else {
        // Check Employee No.
        if ($('#txtEmpNo').val().trim().length == 0) {
            displayAlert($('#empNoValid'), "<b>" + $("#collapseEmployee label[data-field='EmpNo']").text() + "</b> is required and cannot be left blank.", $('#txtEmpNo'));
            hasError = true;
        }
        else {
            if ($('#empNoValid').attr("hidden") == undefined)
                $('#empNoValid').attr("hidden", "hidden");
        }

        // Check Employee Name
        if ($('#txtEmpName').val().trim().length == 0) {
            displayAlert($('#empNameValid'), "<b>" + $("#collapseEmployee label[data-field='EmpName']").text() + "</b> is required and cannot be left blank.", $('#txtEmpName'));
            hasError = true;
        }
        else {
            if ($('#empNameValid').attr("hidden") == undefined)
                $('#empNameValid').attr("hidden", "hidden");
        }

        // Check Position
        if ($('#txtPosition').val().trim().length == 0) {
            displayAlert($('#positionValid'), "<b>" + $("#collapseEmployee label[data-field='JobTitle']").text() + "</b> is required and cannot be left blank.", $('#txtPosition'));
            hasError = true;
        }
        else {
            if ($('#positionValid').attr("hidden") == undefined)
                $('#positionValid').attr("hidden", "hidden");
        }

        // Check Cost Center
        if ($('#txtCostCenter').val().trim().length == 0) {
            displayAlert($('#costCenterValid'), "<b>" + $("#collapseEmployee label[data-field='CostCenter']").text() + "</b> is required and cannot be left blank.", $('#txtCostCenter'));
            hasError = true;
        }
        else {
            if ($('#costCenterValid').attr("hidden") == undefined)
                $('#costCenterValid').attr("hidden", "hidden");
        }

        // Check CPR No.
        if ($('#txtCPRNo').val().trim().length == 0) {
            displayAlert($('#cprValid'), "<b>" + $("#collapseEmployee label[data-field='CPRNo']").text() + "</b> is required and cannot be left blank.", $('#txtCPRNo'));
            hasError = true;
        }
        else {
            if ($('#cprValid').attr("hidden") == undefined)
                $('#cprValid').attr("hidden", "hidden");
        }
    }

    // #endregion

    if (!hasError) {
        // Show the loading panel
        ShowLoadingPanel(gContainer, 1, 'Updating card information, please wait...');

        //updateIDCard();
        updateIDCardBase64();
    }
}

function beginDeleteIDCard() {
    $("#btnDelete").popover("hide");

    SaveDataToSession("currentFormType", _currentFormType);
    showModalConfirmation(ModalTypes.DeleteConfirmation);
}

function loadContractorDetails(data) {
    HideLoadingPanel();

    var contractorData = JSON.parse(data);
    if (contractorData != null) {
        // Set the flags 
        _currentFormMode = FormModes.LoadExistingRecord;        

        $("#hidRegistryID").val(contractorData.registryID);
        $("#txtContractorName").val(contractorData.firstName.concat(" ").concat(contractorData.lastName));
        $("#txtIDNumber").val(contractorData.idNumber);
        $("#txtJobTitle").val(contractorData.jobTitle);
        $("#txtCompanyName").val(contractorData.companyName);        
        $("#txtVisitedDept").val(contractorData.VisitedDepartment);

        // #region Load license grid
        if (contractorData.licenseList != null && contractorData.licenseList != undefined) {
            for (var i = 0; i < contractorData.licenseList.length; i++) {
                _licenseArray.push(contractorData.licenseList[i]);
            }
        }
        else {
            if (_licenseArray.length > 0)
                _licenseArray.splice(0, _licenseArray.length);
        }
        populateLicenseTable();
        // #endregion

        // Display "Show Card Information" panel
        if ($("#showCardPanel").hasAttr("hidden"))
            $("#showCardPanel").removeAttr("hidden");

        // Enable employee photo
        initializeEmpPhoto(true, true);

        // Store data to session storage
        SaveDataToSession("contractorNo", contractorData.contractorNo);

        // Remove the focus to all input elements
        $("input:focus").blur();
        $("#txtContractorNo").blur();        
    }
}

function loadContractorIDCard(data) {
    HideLoadingPanel();

    var contractorData = JSON.parse(data);
    if (contractorData != null) {

        // Set the flag and hidden fields
        _currentFormMode = FormModes.LoadExistingRecord;

        // Store the base64 image string to the hidden field
        if (contractorData.ImageURLBase64 != null && contractorData.ImageURLBase64 != undefined && contractorData.ImageURLBase64.includes(CONST_BASE64_KEY)) {
            var base64Idx = contractorData.ImageURLBase64.indexOf(CONST_BASE64_KEY);
            var base64Image = contractorData.ImageURLBase64.slice(base64Idx + CONST_BASE64_KEY.length);
            $("#hidBase64Photo").val(base64Image);
        }
        
        $("#hidImageFileName").val(contractorData.ImageFileName);
        $("#hidCardRegistryID").val(contractorData.RegistryID);
        $("#txtContractorName").val(contractorData.EmpName);
        $("#txtIDNumber").val(contractorData.IDNumber);
        $("#txtJobTitle").val(contractorData.Position);
        $("#txtCompanyName").val(contractorData.CompanyName);
        $("#txtVisitedDept").val(contractorData.CostCenterFullName);

        // Use the byte array image 
        //if (contractorData.ImageURL != null && contractorData.ImageURL.length > 0) {
        //    $(".pictureFrame img").prop("src", contractorData.ImageURL);
        //}
        //else
        //    $(".pictureFrame img").prop("src", CONST_DEFAULT_PHOTO);

        // Use the Base64 string image 
        if (contractorData.ImageURLBase64 != null && contractorData.ImageURLBase64.length > 0) {
            $(".pictureFrame img").attr("src", contractorData.ImageURLBase64);
            $(".pictureFrame img").prop("data-filename", contractorData.ImageFileName);
        }
        else {
            $(".pictureFrame img").attr("src", CONST_DEFAULT_PHOTO);
            $(".pictureFrame img").prop("data-filename", "");
        }

        // #region Load license grid
        if (contractorData.LicenseList != null && contractorData.LicenseList != undefined) {
            for (var i = 0; i < contractorData.LicenseList.length; i++) {
                _licenseArray.push(contractorData.LicenseList[i]);
            }
        }
        else {
            if (_licenseArray.length > 0)
                _licenseArray.splice(0, _licenseArray.length);
        }
        populateLicenseTable();
        // #endregion

        // #region Load card history grid
        if (contractorData.CardHistoryList != null && contractorData.CardHistoryList != undefined) {
            for (var i = 0; i < contractorData.CardHistoryList.length; i++) {
                _cardHistoryArray.push(contractorData.CardHistoryList[i]);
            }
        }
        else {
            if (_cardHistoryArray.length > 0)
                _cardHistoryArray.splice(0, _cardHistoryArray.length);
        }

        if ($("#cardInfoSwitch").is(":checked")) {
            populateCardHistoryTable();
        }            
        // #endregion

        // Enable employee photo
        initializeEmpPhoto(true);

        // Remove the focus to all input elements        
        $(':focus').blur();
        $("#txtContractorNo").blur();
    }
}

function loadEmployeeDetails(data) {
    HideLoadingPanel();

    var employeeData = JSON.parse(data);
    if (employeeData != null) {
        // Set the flag
        _currentFormMode = FormModes.LoadExistingRecord;
        
        $("#txtEmpName").val(employeeData.EmpName);
        $("#txtPosition").val(employeeData.Position);
        $("#txtCostCenter").val(employeeData.CostCenterName);
        $("#txtSupervisor").val(employeeData.SupervisorName);
        $("#txtManager").val(employeeData.ManagerName);
        $("#txtCPRNo").val(employeeData.CPRNo);
        $("#cboBloodGroup").val(employeeData.BloodGroup);

        // Display "Show Card Information" panel
        if ($("#showCardPanel").hasAttr("hidden"))
            $("#showCardPanel").removeAttr("hidden");

        // Enable "Add New License" link
        $("#collapseLicense .linkTitle").prop("disabled", false);

        // Enable inut controls
        //$("#collapseEmployee input[data-entry='yes']").prop("readonly", false);
        $("#collapseEmployee select").prop("disabled", false);

        // Enable employee photo
        initializeEmpPhoto(true, true);

        // Store data to session storage
        SaveDataToSession("empNo", employeeData.empNo);

        // Remove the focus of all input elements        
        $(':focus').blur();
        $("#txtEmpNo").blur();
    }
}

function loadEmployeeIDCard(data) {
    HideLoadingPanel();

    var employeeData = JSON.parse(data);
    if (employeeData != null) {

        // Set the flag and hidden fields
        _currentFormMode = FormModes.LoadExistingRecord;

        // Store the base64 image string to the hidden field
        if (employeeData.ImageURLBase64 != null && employeeData.ImageURLBase64 != undefined && employeeData.ImageURLBase64.includes(CONST_BASE64_KEY)) {
            var base64Idx = employeeData.ImageURLBase64.indexOf(CONST_BASE64_KEY);
            var base64Image = employeeData.ImageURLBase64.slice(base64Idx + CONST_BASE64_KEY.length);
            $("#hidBase64Photo").val(base64Image);
        }

        $("#hidImageFileName").val(employeeData.ImageFileName);
        $("#hidCardRegistryID").val(employeeData.RegistryID);
        $("#txtEmpName").val(employeeData.EmpName);
        $("#txtPosition").val(employeeData.Position);
        //$("#txtCostCenter").val(decodeURIComponent(employeeData.CustomCostCenter));
        $("#txtCostCenter").val(employeeData.CustomCostCenter);
        $("#txtSupervisor").val(employeeData.SupervisorName);
        $("#txtManager").val(employeeData.ManagerName);
        $("#txtCPRNo").val(employeeData.CPRNo);
        $("#cboBloodGroup").val(employeeData.BloodGroup);

        // Use the byte array image 
        //if (employeeData.ImageURL != null && employeeData.ImageURL.length > 0) {
        //    $(".pictureFrame img").prop("src", employeeData.ImageURL);
        //}
        //else
        //    $(".pictureFrame img").prop("src", CONST_DEFAULT_PHOTO);

        // Use the Base64 string image 
        if (employeeData.ImageURLBase64 != null && employeeData.ImageURLBase64.length > 0) {
            $(".pictureFrame img").attr("src", employeeData.ImageURLBase64);
            $(".pictureFrame img").prop("data-filename", employeeData.ImageFileName);            
        }
        else {
            $(".pictureFrame img").attr("src", CONST_DEFAULT_PHOTO);
            $(".pictureFrame img").prop("data-filename", "");
        }

        // #region Load license grid
        if (employeeData.LicenseList != null && employeeData.LicenseList != undefined) {
            for (var i = 0; i < employeeData.LicenseList.length; i++) {
                _licenseArray.push(employeeData.LicenseList[i]);
            }
        }
        else {
            if (_licenseArray.length > 0)
                _licenseArray.splice(0, _licenseArray.length);
        }
        populateLicenseTable();
        // #endregion

        // #region Load card history grid
        if (employeeData.CardHistoryList != null && employeeData.CardHistoryList != undefined) {
            for (var i = 0; i < employeeData.CardHistoryList.length; i++) {
                _cardHistoryArray.push(employeeData.CardHistoryList[i]);
            }
        }
        else {
            if (_cardHistoryArray.length > 0)
                _cardHistoryArray.splice(0, _cardHistoryArray.length);
        }
        populateCardHistoryTable();
        // #endregion

        // Enable inut controls
        //$("#collapseEmployee input[data-entry='yes']").prop("readonly", false);
        $("#collapseEmployee select").prop("disabled", false);

        // Enable employee photo
        initializeEmpPhoto(true);

        // Store data to session storage
        SaveDataToSession("empNo", employeeData.empNo);

        // Remove the focus of all input elements        
        $(':focus').blur();
        $("#txtEmpNo").blur();
    }
}

function loadDataToControls(data) {
    if (data != null && data != "undefined") {
        var objList = JSON.parse(data);
        var item;

        //#region Populate data to cost center autocomplete control
        //var costCenterData = objList[0];
        //if (costCenterData != "undefined") {
        //    var cbo = $("#cboCostCenter");
        //    var optionValue = "";
        //    var optionText = "";

        //    // Add empty item
        //    optionValue = CONST_EMPTY;
        //    optionText = "";
        //    cbo.append(new Option(optionText, optionValue, true));

        //    for (var i = 0; i < costCenterData.length; i++) {
        //        optionValue = costCenterData[i].CostCenter;
        //        optionText = costCenterData[i].CostCenterFullName;
        //        cbo.append(new Option(optionText, optionValue));
        //    }
        //}
        //#endregion

        // #region Populate license types
        var licenseData = objList[1];
        if (licenseData != undefined) {
            var cbo = $("#cboLicenseType");
            var optionValue = "";
            var optionText = "";

            // Add empty item
            optionValue = CONST_EMPTY;
            optionText = "";
            cbo.append(new Option(optionText, optionValue, true));

            for (var i = 0; i < licenseData.length; i++) {
                optionValue = licenseData[i].UDCCode;
                optionText = licenseData[i].UDCDesc1;
                cbo.append(new Option(optionText, optionValue));
            }
        }
        // #endregion

        //#region Populate data to Blood Group drop-down list control
        var bloodGroupData = objList[3];
        if (bloodGroupData != undefined) {
            var cbo = $("#cboBloodGroup");
            var optionValue = "";
            var optionText = "";

            // Add empty item
            optionValue = CONST_EMPTY;
            optionText = "";
            cbo.append(new Option(optionText, optionValue, true));

            for (var i = 0; i < bloodGroupData.length; i++) {
                optionValue = bloodGroupData[i].UDCCode;
                optionText = bloodGroupData[i].UDCDesc1;
                cbo.append(new Option(optionText, optionValue));
            }
        }
        //#endregion

        HideLoadingPanel(gContainer);
    }
}

function encodeImageFileAsURL(element) {
    try {
        var file = element.files[0];

        // Save the filename to the image attribute
        $(".pictureFrame img").prop("data-filename", file.name);

        var reader = new FileReader();
        reader.onloadend = function () {
            var base64String = reader.result;
            if (base64String.toString().length > 0)
                $(".pictureFrame img").attr("src", base64String);
        }
        reader.readAsDataURL(file);
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while executing encodeImageFileAsURL() method.\n\n" + err);
    }
}

function toDataURL(src, callback, outputFormat) {
    try {

        var img = new Image();
        img.crossOrigin = 'Anonymous';
        img.onload = function () {
            var canvas = document.createElement('CANVAS');
            var ctx = canvas.getContext('2d');
            var dataURL;
            canvas.height = this.naturalHeight;
            canvas.width = this.naturalWidth;
            ctx.drawImage(this, 0, 0);
            dataURL = canvas.toDataURL(outputFormat);
            callback(dataURL);
        };
        img.src = src;
        if (img.complete || img.complete === undefined) {
            img.src = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==";
            img.src = src;
        }

    } 
    catch (err) {
        ShowErrorMessage("The following error has occured while uploading the image.\n\n" + err);
    }
}

function copyPhoto() {
    try {
        
        var imagePath = $(this).val().trim();
        if (imagePath.length == 0) {
            ShowToastMessage(toastTypes.error, "No valid image file was selected!", "Error Notification");
            return;
        }

        //toDataURL(
        //  'https://www.gravatar.com/avatar/d50c83cc0c6523b4d3f6085295c953e0',
        //  function (dataUrl) {
        //      console.log('RESULT:', dataUrl)
        //  }
        //)
        
        const extensionArray = [".JPG", ".JPEG", ".PNG", ".GIF", ".BMP"]
        var fileExtension = "";
        var idx = imagePath.lastIndexOf(".");
        if (idx > 0) {
            fileExtension = imagePath.slice(idx);
        }
        
        if (fileExtension.length == 0 || !extensionArray.includes(fileExtension.toUpperCase())) {
            ShowToastMessage(toastTypes.error, "The selected image is invalid. The system accepts the following file types only: PNG, GIF, BMP, and JPEG.", "Error Notification");
            return;
        }


        var empNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());

        // Convert object to JSON
        var jsonData = JSON.stringify({
            imagePath: imagePath,
            empNo: empNo
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/ReplicatePhoto",
            //url: "IDCardGenerator.aspx/CopyPhoto",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result == CONST_SUCCESS) {
                    var fileName = CONST_EMPPHOTO_FOLDER.concat(empNo).concat(fileExtension) + "?timestamp=" + new Date().getTime();
                    $(".pictureFrame img").attr("src", fileName);

                    HideLoadingPanel(gContainer);                    
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to upload the selected image due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while uploading the image.\n\n" + err.responseText);
            }
        });

       

    } catch (err) {
        ShowErrorMessage("The following error has occured while uploading the image.\n\n" + err);
    }    
}

function handleAddNewLicenseLink() {
    // Set the flags
    _modalFormType = ModalTypes.RegisterLicense;
    _modalFormLoadType = ModalFormLoadType.AddNewRecord;

    //Reset the form
    resetModalForm();

    $("#modLicenseRegistration").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
}

function handleAddNewCardLink() {
    // Set the flags
    _modalFormType = ModalTypes.ManageCardHistory;
    _modalFormLoadType = ModalFormLoadType.AddNewRecord;

    //Reset the form
    resetModalForm();

    $("#modCardInfo").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
}

function resetModalForm() {
    switch (_modalFormType) {
        case ModalTypes.RegisterLicense:
            //#region Reset License Registration form
            // Hide all validation panels
            if (!$('#licenseTypeValid').hasAttr("hidden"))
                $('#licenseTypeValid').attr("hidden", "hidden");

            if (!$('#licenseNoValid').hasAttr("hidden"))
                $('#licenseNoValid').attr("hidden", "hidden");

            if (!$('#issuedDateValid').hasAttr("hidden"))
                $('#issuedDateValid').attr("hidden", "hidden");

            if (!$('#expiryDateValid').hasAttr("hidden"))
                $('#expiryDateValid').attr("hidden", "hidden");

            // Clear all controls
            $("#modLicenseRegistration input").val("");
            $("#modLicenseRegistration select").val("");
            $("#modLicenseRegistration textarea").val("");

            if ($("#cboLicenseType").hasAttr("disabled"))
                $("#cboLicenseType").removeAttr("disabled");

            if ($("#txtLicenseNo").hasAttr("disabled"))
                $("#txtLicenseNo").removeAttr("disabled");

            break;
            //#endregion

        case ModalTypes.ManageCardHistory:
            // #region Reset Manage Card Information form
            // Hide all validation panels
            if (!$('#cardRefNoValid').hasAttr("hidden"))
                $('#cardRefNoValid').attr("hidden", "hidden");

            // Clear all controls
            $("#modCardInfo input").val("");
            $("#modCardInfo textarea").val("");

            if ($("#txtCardRefNo").hasAttr("disabled"))
                $("#txtCardRefNo").removeAttr("disabled");

            // Set focus to Card Reference No. 
            $("#txtCardRefNo").focus();
            break;
            // #endregion
    }
}

function beginSaveLicense() {
    try {
        var hasError = false;

        //#region Validate data entry
        // Check the License Type
        if ($('#cboLicenseType').val() == null || $('#cboLicenseType').val() == CONST_EMPTY) {
            displayAlert($('#licenseTypeValid'), "<b>" + $(".modalFieldTitle label[data-field='LicenseType']").text() + "</b> is a required field.", $('#cboLicenseType'));
            hasError = true;
        }
        else {
            if ($('#licenseTypeValid').attr("hidden") == undefined)
                $('#licenseTypeValid').attr("hidden", "hidden");
        }

        // Check License No.
        if ($('#txtLicenseNo').val().trim().length == 0) {
            displayAlert($('#licenseNoValid'), "<b>" + $(".modalFieldTitle label[data-field='LicenseNo']").text() + "</b> is a required field.", $('#txtLicenseNo'));
            hasError = true;
        }
        else {
            if ($('#licenseNoValid').attr("hidden") == undefined)
                $('#licenseNoValid').attr("hidden", "hidden");
        }

        // Check Issued Date
        if ($('#txtIssuedDate').val().trim().length == 0) {
            displayAlert($('#issuedDateValid'), "<b>" + $(".modalFieldTitle label[data-field='IssuedDate']").text() + "</b> is a required field.");
            hasError = true;
        }
        else {
            if (!IsValidDate($('#txtIssuedDate').val().trim())) {
                displayAlert($('#issuedDateValid'), "<b>" + $(".modalFieldTitle label[data-field='IssuedDate']").text() + "</b> is invalid!", $('#txtIssuedDate'));
                hasError = true;
            }
            else {
                if ($('#issuedDateValid').attr("hidden") == undefined)
                    $('#issuedDateValid').attr("hidden", "hidden");
            }
        }

        // Check Expiry Date
        if ($('#txtExpiryDate').val().trim().length == 0) {
            displayAlert($('#expiryDateValid'), "<b>" + $(".modalFieldTitle label[data-field='ExpiryDate']").text() + "</b> is a required field.");
            hasError = true;
        }
        else {
            if (!IsValidDate($('#txtExpiryDate').val().trim())) {
                displayAlert($('#expiryDateValid'), "<b>" + $(".modalFieldTitle label[data-field='ExpiryDate']").text() + "</b> is invalid!", $('#txtExpiryDate'));
                hasError = true;
            }
            else {
                if ($('#expiryDateValid').attr("hidden") == undefined)
                    $('#expiryDateValid').attr("hidden", "hidden");
            }
        }

        if ($('#txtIssuedDate').val().trim().length > 0 && $('#txtExpiryDate').val().trim().length > 0) {
            if (IsValidDate($('#txtIssuedDate').val().trim()) && IsValidDate($('#txtExpiryDate').val().trim())) {
                var startDate = Date.parse(ConvertToISODate($('#txtIssuedDate').val().trim()));
                var endDate = Date.parse(ConvertToISODate($('#txtExpiryDate').val().trim()));

                if (startDate > endDate) {
                    displayAlert($('#issuedDateValid'), "<b>" + $(".modalFieldTitle label[data-field='IssuedDate']").text() + "</b> should be less than Expiry Date!");
                    hasError = true;
                }
            }
        }
        //#endregion

        if (!hasError) {
            // Display loading panel
            gContainer = $('#modLicenseRegistration');
            ShowLoadingPanel(gContainer, 2, 'Saving license information, please wait...');

            switch (_modalFormLoadType) {
                case ModalFormLoadType.AddNewRecord:
                    addLicense();
                    break;

                case ModalFormLoadType.UpdateRecord:
                    updateLicense();
                    break;
            }

            return true;
        }

        return false;

    } catch (err) {
        return false;
    }
}

function beginDeleteLicense() {
    try {
        var hasError = false;

        // Check License No.
        if ($('#txtLicenseNo').val().trim().length == 0) {
            displayAlert($('#licenseNoValid'), $(".modalFieldTitle label[data-field='LicenseNo']").text() + " is not defined.", $('#txtLicenseNo'));
            hasError = true;
        }
        else {
            if ($('#licenseNoValid').attr("hidden") == undefined)
                $('#licenseNoValid').attr("hidden", "hidden");
        }

        if (!hasError) {
            // Display loading panel
            gContainer = $('#modLicenseRegistration');
            ShowLoadingPanel(gContainer, 2, 'Deleting license record, please wait...');

            deleteLicense();

            return true;
        }

        return false;

    } catch (err) {
        return false;
    }
}

function addLicense() {
    try {

        var licenseItem = {
            licenseGUID: CreateGUID(),
            registryID: GetIntValue($("#hidRegistryID").val()),
            empNo: GetIntValue($("#txtEmpNo").val()),
            licenseNo: $("#txtLicenseNo").val(),
            licenseTypeCode: $("#cboLicenseType").val(),
            licenseTypeDesc: $("#cboLicenseType option:selected").text(),
            issuedDate: ConvertToISODate($("#txtIssuedDate").val()),
            expiryDate: ConvertToISODate($("#txtExpiryDate").val()),
            issuingAuthority: $("#txtIssuingAuthority").val(),
            remarks: $("#txtNotes").val(),
            createdDate: ConvertToISODate((new Date()).toLocaleDateString()),
            createdByEmpNo: GetIntValue($("#hidCurrentUserEmpNo").val()),
            createdByEmpName: $("#hidCurrentUserEmpName").val(),
            createdByUser: $("#hidCurrentUserID").val(),
            lastUpdatedDate: null,
            lastUpdatedByEmpNo: 0,
            lastUpdatedByEmpName: null,
            lastUpdatedByUser: null
        };

        if (!isDuplicateLicense(licenseItem.empNo, licenseItem.licenseTypeCode, licenseItem.licenseNo, licenseItem.issuedDate, licenseItem.expiryDate, licenseItem.licenseGUID)) {
            // Add item to the array
            _licenseArray.push(licenseItem);

            // Refresh the frid
            populateLicenseTable();
        }
        else {
            ShowToastMessage(toastTypes.error, "The specified license already exists or the period duration overlaps with existing record!", "Error Notification");
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the contractor license." + "\n\nError: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function updateLicense() {
    try {
        // Get the selected license item
        var selectedLicense = _licenseArray.find(function (value, index, array) {
            return value.licenseGUID == $("#hidLicenseGUID").val().trim();
        });

        if (selectedLicense != undefined) {
            if (!isDuplicateLicense(selectedLicense.empNo, selectedLicense.licenseTypeCode, selectedLicense.licenseNo, selectedLicense.issuedDate, selectedLicense.expiryDate, selectedLicense.licenseGUID)) {
                selectedLicense.licenseNo = $("#txtLicenseNo").val();
                selectedLicense.licenseTypeCode = $("#cboLicenseType").val();
                selectedLicense.licenseTypeDesc = $("#cboLicenseType option:selected").text();
                selectedLicense.issuedDate = ConvertToISODate($("#txtIssuedDate").val());
                selectedLicense.expiryDate = ConvertToISODate($("#txtExpiryDate").val());
                selectedLicense.issuingAuthority = $("#txtIssuingAuthority").val();
                selectedLicense.remarks = $("#txtNotes").val();
                selectedLicense.lastUpdatedDate = ConvertToISODate((new Date()).toLocaleDateString());
                selectedLicense.lastUpdatedByEmpNo = GetIntValue($("#hidCurrentUserEmpNo").val());
                selectedLicense.lastUpdatedByEmpName = $("#hidCurrentUserEmpName").val();
                selectedLicense.lastUpdatedByUser = $("#hidCurrentUserID").val();

                // Refresh the frid
                populateLicenseTable();
            }
            else
                ShowToastMessage(toastTypes.error, "The specified license already exists or the period duration overlaps with an existing record!", "Error Notification");
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the license details." + "\n\nError: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function deleteLicense() {
    try {
        let selectedLicense = _licenseArray.find(function (value, index, array) {
            return value.licenseGUID == $("#hidLicenseGUID").val().trim();
        });

        if (selectedLicense != undefined) {
            var position = _licenseArray.indexOf(selectedLicense);

            if (position >= 0) {
                // Remove the item in the array
                _licenseArray.splice(position, 1);

                // Refresh the frid
                populateLicenseTable();
            }
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while deleting the license record." + "\n\nError: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function isDuplicateLicense(empNo, licenseTypeCode, licenseNo, issuedDate, expiryDate, licenseGUID) {
    try {
        // Check for duplicate record
        if (_licenseArray != undefined && _licenseArray.length > 0) {
            var duplicateLicense = _licenseArray.find(function (value, index, array) {
                return value.empNo == empNo &&
                    value.licenseTypeCode == licenseTypeCode &&
                    value.licenseNo == licenseNo &&
                    ((issuedDate >= value.issuedDate && issuedDate <= value.expiryDate) || (expiryDate >= value.issuedDate && expiryDate <= value.expiryDate)) &&
                    value.licenseGUID != licenseGUID;
            });

            return duplicateLicense != undefined;
        }

        return false;
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the contractor license." + "\n\nError: " + err);
        return false;
    }
}

function populateCardHistoryTable() {
    try {
        if (_cardHistoryArray == undefined || _cardHistoryArray.length == 0) {
            // Get DataTable API instance
            var table = $("#cardHistoryTable").dataTable().api();
            table.clear().draw();
        }
        else {
            $("#cardHistoryTable")
                .on('init.dt', function () {    // This event will fire after loading the data in the table
                    HideLoadingPanel(gContainer);
                })
                .DataTable({
                    data: _cardHistoryArray,
                    processing: true,           // To show progress bar 
                    serverSide: false,          // To enable processing server side processing (e.g. sorting, pagination, and filtering)
                    filter: true,               // To enable/disable filter (search box)
                    orderMulti: false,          // To disable mutiple column sorting
                    destroy: true,              // To destroy an old instance of the table and to initialise a new one
                    scrollX: true,              // To enable horizontal scrolling
                    sScrollX: "100%",
                    sScrollXInner: "110%",      // This property can be used to force a DataTable to use more width than it might otherwise do when x-scrolling is enabled.
                    language: {
                        emptyTable: "No data available in table"
                    },
                    width: "100%",
                    lengthMenu: [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]],
                    iDisplayLength: 10,         // Number of rows to display on a single page when using pagination.
                    order: [[1, 'asc']],
                    fnDrawCallback: function () {
                        $('.lnkCardRefNo').on('click', openCardHistoryDetails);
                    },
                    columns: [
                        {
                            "data": "cardRefNo"
                        },
                        {
                            "data": "remarks",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            data: "createdDate",
                            render: function (data, type, row) {
                                return moment(data).isValid() ? moment(data).format('DD-MMM-YYYY') : "";
                            }
                        },
                        {
                            data: "createdByEmpName"
                        },
                        //{
                        //    data: "lastUpdatedDate"
                        //},
                        //{
                        //    data: "lastUpdatedByEmpName"
                        //},
                        {
                            "data": "empNo"
                        },
                        {
                            "data": "historyID"
                        },
                        {
                            data: "cardGUID"
                        }
                    ],
                    columnDefs: [
                        {
                            targets: "centeredColumn",
                            className: 'dt-body-center'
                        },
                        {
                            targets: 0,
                            render: function (data, type, row) {
                                return '<a href="javascript:void(0)" class="lnkCardRefNo gridLink" data-cardguid=' + row.cardGUID + '> ' + data + '</a>';
                            }
                        },
                        {
                            targets: "hiddenColumn",
                            visible: false
                        },
                        {
                            targets: 3,
                            render: function (data, type, row) {
                                if (row.createdByEmpNo > 0)
                                    return row.createdByEmpNo + ' - ' + row.createdByEmpName;
                                else
                                    return "";
                            }
                        },
                        {
                            targets: "doNotOrder",
                            orderable: false
                        }
                    ]
                });
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while loading the license in the grid." + "\n\nError: " + err);
    }
}

function openCardHistoryDetails() {
    var cardRefNo = $(this).text().trim();
    var cardGUID = $(this).attr("data-cardguid").trim();

    if (cardRefNo.length == 0) {
        ShowErrorMessage("Unable to load details of the selected card history record!");
        return;
    }

    // Set the flags
    _modalFormType = ModalTypes.ManageCardHistory;
    _modalFormLoadType = ModalFormLoadType.UpdateRecord;

    // Get the selected card details 
    var selectedCard = _cardHistoryArray.find(function (value, index, array) {
        return value.cardRefNo == cardRefNo &&
            value.cardGUID.trim() == cardGUID;
    });

    if (selectedCard != undefined) {
        $("#hidCardHistoryID").val(selectedCard.historyID);
        $("#hidCardGUID").val(selectedCard.cardGUID);
        $("#txtCardRefNo").val(selectedCard.cardRefNo);
        $("#txtCardRemarks").val(selectedCard.remarks);

        // Disable Card Ref. No. fields
        $("#txtCardRefNo").attr("disabled", "disabled");
    }

    $("#modCardInfo").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
}

function beginSaveCardHistory() {
    try {
        var hasError = false;

        //#region Validate data entry
        // Check Card Reference No.
        if ($('#txtCardRefNo').val().trim().length == 0) {
            displayAlert($('#cardRefNoValid'), "<b>Card Ref. No.</b> is a required field.", $('#txtCardRefNo'));
            hasError = true;
        }
        else {
            if ($('#cardRefNoValid').attr("hidden") == undefined)
                $('#cardRefNoValid').attr("hidden", "hidden");
        }
        //#endregion

        if (!hasError) {
            // Display loading panel
            gContainer = $('#modCardInfo');
            ShowLoadingPanel(gContainer, 2, 'Saving card history information, please wait...');

            switch (_modalFormLoadType) {
                case ModalFormLoadType.AddNewRecord:
                    addCardHistory();
                    break;

                case ModalFormLoadType.UpdateRecord:
                    updateCardHistory();
                    break;
            }

            return true;
        }

        return false;

    } catch (err) {
        return false;
    }
}

function beginDeleteCardHistory() {
    try {
        var hasError = false;

        // Check Card Reference No.
        if ($('#txtCardRefNo').val().trim().length == 0) {
            displayAlert($('#cardRefNoValid'), "<b>" + $(".modalFieldTitle label[data-field='cardrefno']").text() + "</b> is a required field.", $('#txtCardRefNo'));
            hasError = true;
        }
        else {
            if ($('#cardRefNoValid').attr("hidden") == undefined)
                $('#cardRefNoValid').attr("hidden", "hidden");
        }

        if (!hasError) {
            // Display loading panel
            gContainer = $('#modCardInfo');
            ShowLoadingPanel(gContainer, 2, 'Deleting card history record, please wait...');

            deleteCardHistory();

            return true;
        }

        return false;

    } catch (err) {
        return false;
    }
}

function addCardHistory() {
    try {
        var cardHistItem = {
            cardGUID: CreateGUID(),
            historyID: GetIntValue($("#hidCardHistoryID").val()),
            empNo: _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val()),
            isContractor: _currentFormType == formTypes.ManageContractor ? true : false,
            cardRefNo: $("#txtCardRefNo").val().trim(),
            remarks: $("#txtCardRemarks").val().trim(),
            createdDate: ConvertToISODate((new Date()).toLocaleDateString()),
            createdByEmpNo: GetIntValue($("#hidCurrentUserEmpNo").val()),
            createdByEmpName: $("#hidCurrentUserEmpName").val().trim(),
            createdByUser: $("#hidCurrentUserID").val().trim(),
            lastUpdatedDate: null,
            lastUpdatedByEmpNo: 0,
            lastUpdatedByEmpName: null,
            lastUpdatedByUser: null
        };

        if (!isDuplicateCardHistory(cardHistItem.empNo, cardHistItem.cardRefNo, cardHistItem.cardGUID)) {
            // Add item to the array
            _cardHistoryArray.push(cardHistItem);

            // Refresh the frid
            populateCardHistoryTable();
        }
        else {
            ShowToastMessage(toastTypes.error, "The specified card reference number already exists. Please enter a unique one!", "Duplicate Record Notification");
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the card history information." + "\n\nError: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function updateCardHistory() {
    try {
        // Get the selected license item
        var selectedCardItem = _cardHistoryArray.find(function (value, index, array) {
            return value.cardGUID == $("#hidCardGUID").val().trim();
        });

        if (selectedCardItem != undefined) {
            if (!isDuplicateCardHistory(selectedCardItem.empNo, selectedCardItem.cardRefNo, selectedCardItem.cardGUID)) {
                selectedCardItem.remarks = $("#txtCardRemarks").val().trim(),
                selectedCardItem.lastUpdatedDate = ConvertToISODate((new Date()).toLocaleDateString());
                selectedCardItem.lastUpdatedByEmpNo = $("#hidCurrentUserEmpNo").val().trim();
                selectedCardItem.lastUpdatedByEmpName = $("#hidCurrentUserEmpName").val().trim();
                selectedCardItem.lastUpdatedByUser = $("#hidCurrentUserID").val().trim();

                // Refresh the frid
                populateCardHistoryTable();
            }
            else
                ShowToastMessage(toastTypes.error, "The specified card reference number already exists. Please enter a unique one!", "Duplicate Record Notification");
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the card history information." + "\n\nError: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function deleteCardHistory() {
    try {
        var selectedCardItem = _cardHistoryArray.find(function (value, index, array) {
            return value.cardGUID == $("#hidCardGUID").val().trim();
        });

        if (selectedCardItem != undefined) {
            var position = _cardHistoryArray.indexOf(selectedCardItem);

            if (position >= 0) {
                // Remove the item in the array
                _cardHistoryArray.splice(position, 1);

                // Refresh the frid
                populateCardHistoryTable();
            }
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while deleting the card history record.\n\n" + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function isDuplicateCardHistory(empNo, cardRefNo, cardGUID) {
    try {
        // Check for duplicate record
        if (_cardHistoryArray != undefined && _cardHistoryArray.length > 0) {
            var duplicateItem = _cardHistoryArray.find(function (value, index, array) {
                return value.empNo == empNo &&
                    value.cardRefNo == cardRefNo &&
                    value.cardGUID != cardGUID;
            });

            return duplicateItem != undefined;
        }

        return false;
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the contractor license." + "\n\nError: " + err);
        return false;
    }
}

function initializeEmpPhoto(allowPhoto, clearPhoto) {
    if (allowPhoto) {
        $(".pictureFrame button").prop("disabled", false);

        if ($(".pictureFrame button").hasClass("btn-outline-secondary"))
            $(".pictureFrame button").removeClass("btn-outline-secondary");

        if (!$("#btnBrowse").hasClass("btn-success"))
            $("#btnBrowse").addClass("btn-success");

        if (!$("#btnRemovePhoto").hasClass("btn-danger"))
            $("#btnRemovePhoto").addClass("btn-danger");                
    }
    else {
        $(".pictureFrame button").prop("disabled", true);
        
        if ($("#btnBrowse").hasClass("btn-success"))
            $("#btnBrowse").removeClass("btn-success");

        if ($("#btnRemovePhoto").hasClass("btn-danger"))
            $("#btnRemovePhoto").removeClass("btn-danger");

        $(".pictureFrame button").addClass("btn-outline-secondary");                
    }

    if (clearPhoto)
        $("#btnRemovePhoto").click();
}

function beginPrintIDCard() {
    ShowLoadingPanel(gContainer, 1, 'Please wait...');
        
    // Save employee/contractor no. to session
    SaveDataToSession("empConNo", _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val()));
    SaveDataToSession("isContractor", _currentFormType == formTypes.ManageContractor ? "true" : "");

    // Save caller form value to session storage
    if (_callerForm != "undefined" && _callerForm != null) {
        SaveDataToSession("cardGeneratorCF", _callerForm);
    }

    // Generate employee photo
    generatePhoto();
}
// #endregion

// #region Database Methods
function getLookupTable() {
    $.ajax({
        type: "POST",
        url: "IDCardGenerator.aspx/GetRegistrationLookup",
        //url: "/WebService/ContractorWS.asmx/GetRegistrationLookup",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: "true",
        cache: "false",
        success: function (result) {
            if (result.d != null) {
                loadDataToControls(result.d);

                // #region Load card details
                // Check if "Back" button was clicked from the previous page
                var isBackClicked = Boolean(GetQueryStringValue("isback"));
                if (isBackClicked) {
                    queryStrContractorNo = parseInt(GetDataFromSession("empConNo"));
                    var isContractor = Boolean(GetDataFromSession("isContractor"));
                    if (isContractor) {
                        $("#conEmpSwitch").prop("checked", true);
                        //$("#conEmpSwitch").click();
                        handleToggleContractorClick();
                        $("#txtContractorNo").val(queryStrContractorNo);
                        $("#btnFind").click();
                    }
                    else {
                        _currentFormType = formTypes.ManageEmployee;
                        $("#conEmpSwitch").prop("checked", false);
                        //$("#conEmpSwitch").click();
                        handleToggleEmployeeClick();
                        $("#txtEmpNo").val(queryStrContractorNo);
                        $("#btnFindEmp").click();
                    }

                    // Show the Back button
                    //if (_callerForm != "undefined" && _callerForm != null) {
                    //    $("#btnBack").prop("hidden", false);
                    //}
                }
                else {
                    if (queryStrContractorNo > 0) {
                        $("#txtContractorNo").val(queryStrContractorNo);
                        $("#btnFind").click();
                    }
                }
                // #endregion

                HideLoadingPanel(gContainer);
            }
            else {
                $("#txtSupervisor").autocomplete("disable");
                $("#txtCompanyName").autocomplete("disable");
                HideLoadingPanel(gContainer);
                ShowMessageBox("Unable to retrieve cost center and employee data, please contact ICT for technical support.");
            }
        },
        error: function (error) {
            HideLoadingPanel(gContainer);
            ShowErrorMessage("The following exception has occured while fetching the cost center and employee list from the database: " + error.responseText);
        }
    });
}

function beginFindContractor() {
    try {
        var hasError = false;

        // Check Contractor No.
        if ($('#txtContractorNo').val().trim().length == 0) {
            displayAlert($('#contractorNoValid'), "<b>" + $("#collapseContractor label[data-field='ContractorNo']").text() + "</b> is required and cannot be left blank.", $('#txtContractorNo'));
            hasError = true;
        }
        else {
            if ($('#contractorNoValid').attr("hidden") == undefined)
                $('#contractorNoValid').attr("hidden", "hidden");
        }

        if (!hasError) {
            ShowLoadingPanel(gContainer, 1, 'Searching contractor record, please wait...');

            //#region Clear license grid
            if (_licenseArray.length > 0)
                _licenseArray.splice(0, _licenseArray.length);

            var table = $("#licenseTable").dataTable().api();
            table.clear().draw();
            //#endregion

            //#region Clear card history grid
            if (_cardHistoryArray.length > 0)
                _cardHistoryArray.splice(0, _cardHistoryArray.length);

            var table = $("#cardHistoryTable").dataTable().api();
            table.clear().draw();
            //#endregion

            var contractorNo = isNaN(parseInt($("#txtContractorNo").val())) ? 0 : parseInt($("#txtContractorNo").val());

            // Call Web Service method using AJAX
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: "IDCardGenerator.aspx/GetContractorDetails",
                //url: "/WebService/ContractorWS.asmx/GetContractorDetails",
                data: JSON.stringify({ contractorNo: contractorNo }),
                async: "true",
                cache: "false",
                success: function (result) {
                    if (result.d != null) {
                        loadContractorDetails(result.d);
                        HideLoadingPanel(gContainer);
                    }
                    else {
                        HideLoadingPanel(gContainer);
                        ShowToastMessage(toastTypes.error, "The specified contractor does not exist. Please ensure that the contractor number you've entered is registered in the system!", "No Record Found")
                    }
                },
                error: function (err) {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("An error encountered while fetching the contractor data.\n\n" + err.responseText);
                }
            });
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while fetching the contractor record.\n\n" + err);
    }
}

function beginFindEmployee() {
    try {
        var hasError = false;

        // Check Emp. No.
        if ($('#txtEmpNo').val().trim().length == 0) {
            displayAlert($('#empNoValid'), "<b>" + $("#collapseContractor label[data-field='EmpNo']").text() + "</b> is required and cannot be left blank.", $('#txtEmpNo'));
            hasError = true;
        }
        else {
            if ($('#empNoValid').attr("hidden") == undefined)
                $('#empNoValid').attr("hidden", "hidden");
        }

        if (!hasError) {
            ShowLoadingPanel(gContainer, 1, 'Searching employee record, please wait...');

            //#region Clear license grid
            if (_licenseArray.length > 0)
                _licenseArray.splice(0, _licenseArray.length);

            var table = $("#licenseTable").dataTable().api();
            table.clear().draw();
            //#endregion

            //#region Clear card history grid
            if (_cardHistoryArray.length > 0)
                _cardHistoryArray.splice(0, _cardHistoryArray.length);

            var table = $("#cardHistoryTable").dataTable().api();
            table.clear().draw();
            //#endregion

            var empNo = GetIntValue($("#txtEmpNo").val());
            if (isNaN(empNo))
                empNo = 0;
            else {
                if (empNo.toString().length == 4) {
                    empNo += 10000000;
                    $("#txtEmpNo").val(empNo);
                }
            }

            // Call Web Service method using AJAX
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: "IDCardGenerator.aspx/SearchEmployee",
                //url: "/WebService/ContractorWS.asmx/SearchEmployee",
                data: JSON.stringify({ empNo: empNo }),
                async: "true",
                cache: "false",
                success: function (result) {
                    if (result.d != null) {
                        loadEmployeeDetails(result.d);
                        HideLoadingPanel(gContainer);
                    }
                    else {
                        HideLoadingPanel(gContainer);
                        ShowToastMessage(toastTypes.error, "Unable to find a matching record for the employee no. you've specified. Please enter another one then try again!", "No Matching Record")
                    }
                },
                error: function (err) {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("The following error has occured while fetching the employee data.\n\n" + err.responseText);
                }
            });
        }
    }
    catch (err) {
        throw err;
    }
}

function searchIDCard(empNo, showLoadingPanel) {
    try {

        if (empNo > 0) {
            if (showLoadingPanel)
                ShowLoadingPanel(gContainer, 1, 'Loading data, please wait...');

            //#region Clear license grid
            if (_licenseArray.length > 0)
                _licenseArray.splice(0, _licenseArray.length);

            var table = $("#licenseTable").dataTable().api();
            table.clear().draw();
            //#endregion

            //#region Clear card history grid
            if (_cardHistoryArray.length > 0)
                _cardHistoryArray.splice(0, _cardHistoryArray.length);

            var table = $("#cardHistoryTable").dataTable().api();
            table.clear().draw();
            //#endregion

            // Call Web Service method using AJAX
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: "IDCardGenerator.aspx/SearchIDCard",
                //url: "/WebService/ContractorWS.asmx/SearchIDCard",
                data: JSON.stringify({ empNo: empNo }),
                async: "true",
                cache: "false",
                success: function (result) {
                    if (result.d != null && result.d != "") {
                        if (_currentFormType == formTypes.ManageContractor)
                            loadContractorIDCard(result.d);
                        else
                            loadEmployeeIDCard(result.d);

                        setFormDataLoaded();

                        if (showLoadingPanel)
                            HideLoadingPanel(gContainer);
                    }
                    else {
                        if (_currentFormType == formTypes.ManageContractor)
                            showModalConfirmation(ModalTypes.ContractorIDConfirmation);
                        else
                            showModalConfirmation(ModalTypes.EmployeeIDConfirmation);

                        if (showLoadingPanel)
                            HideLoadingPanel(gContainer);
                    }
                },
                error: function (err) {
                    if (showLoadingPanel)
                        HideLoadingPanel(gContainer);

                    ShowErrorMessage("An error encountered while fetching the contractor data.\n\n" + err.responseText);
                }
            });
        }
        else
            ShowToastMessage(toastTypes.warning, "Unable to find a matching record because Employee No. is not defined!", "Missing Parameter")
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while fetching the contractor record.\n\n" + err);
    }
}

function insertIDCard() {
    try {
        var idx;
        var employeeData = {};
        var imagePath = $(".pictureFrame img").attr("src").trim();
        var empNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());

        // #region Populate the object
        employeeData.EmpNo = empNo;
        employeeData.IsContractor = _currentFormType == formTypes.ManageContractor ? true : false;        
        employeeData.UserEmpNo = $("input[id$='hidCurrentUserEmpNo'").val().trim();
        employeeData.UserID = $("input[id$='hidCurrentUserID'").val().trim();

        if (_currentFormType == formTypes.ManageEmployee) {
            employeeData.EmpName = $("#txtEmpName").val().trim();
            employeeData.Position = $("#txtPosition").val().trim();
            //employeeData.CustomCostCenter = encodeURIComponent($("#txtCostCenter").val().trim());
            employeeData.CustomCostCenter = $("#txtCostCenter").val().trim();
            employeeData.CPRNo = $("#txtCPRNo").val().trim();
            if ($("#cboBloodGroup").val() != CONST_EMPTY)
                employeeData.bloodGroup = $("#cboBloodGroup").val();
        }
        
        if (imagePath != CONST_DEFAULT_PHOTO) {
            idx = imagePath.indexOf("?");

            if (idx > 0) {
                employeeData.ImagePath = imagePath.slice(0, idx);
            }
            else {
                employeeData.ImagePath = imagePath;
            }
        }

        // Save the licenses
        if (_currentFormType == formTypes.ManageEmployee) {
            if (_licenseArray != undefined && _licenseArray.length > 0)
                employeeData.LicenseList = _licenseArray;
        }

        // Save Card History information
        if (_cardHistoryArray != undefined && _cardHistoryArray.length > 0)
            employeeData.CardHistoryList = _cardHistoryArray;
        // #endregion

        // Convert object to JSON
        var jsonData = JSON.stringify({
            employeeData: employeeData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/AddIDCard",
            //url: "/WebService/ContractorWS.asmx/AddIDCard",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result != null && result.includes(CONST_SUCCESS)) {
                    // Get the Registry ID
                    idx = result.indexOf("|");
                    if (idx > 0) {
                        var registryID = GetIntValue(result.slice(idx + 1));
                        if (registryID > 0)
                            $("#hidCardRegistryID").val(registryID);
                    }

                    setFormDataLoaded();

                    // Load ID card details to get the base64 photo
                    if (imagePath != CONST_DEFAULT_PHOTO) {
                        searchIDCard(empNo);
                    }
                    
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "The new ID card information has been saved successfully!", "Add Record Notification");

                    // Set Form Load flag
                    _currentFormMode = FormModes.LoadExistingRecord;
                }
                else if (result == CONST_CARD_EXIST) {
                    HideLoadingPanel(gContainer);
                    showModalConfirmation(ModalTypes.IDCardExist);
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to save information to the database due to the following error.\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while adding new ID card.\n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while adding new ID card.\n\n" + err);
    }
}

function updateIDCard() {
    try {
        var employeeData = {};
        var imagePath = $(".pictureFrame img").attr("src").trim();

        // #region Populate the object
        employeeData.RegistryID = GetIntValue($("#hidCardRegistryID").val());
        employeeData.EmpNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());        
        employeeData.UserEmpNo = GetIntValue($("input[id$='hidCurrentUserEmpNo'").val());
        employeeData.UserID = $("input[id$='hidCurrentUserID'").val().trim();

        if (_currentFormType == formTypes.ManageEmployee) {
            employeeData.EmpName = $("#txtEmpName").val().trim();
            employeeData.Position = $("#txtPosition").val().trim();
            //employeeData.CustomCostCenter = encodeURIComponent($("#txtCostCenter").val().trim());
            employeeData.CustomCostCenter = $("#txtCostCenter").val().trim();
            employeeData.CPRNo = $("#txtCPRNo").val().trim();
            
            if ($("#cboBloodGroup").val() != CONST_EMPTY)
                employeeData.bloodGroup = $("#cboBloodGroup").val();
        }

        if (imagePath != CONST_DEFAULT_PHOTO) {
            var idx = imagePath.indexOf("?");

            if (idx > 0) {
                if (imagePath.includes(CONST_BASE64_URI)) {
                    //var base64Idx = imagePath.indexOf(CONST_BASE64_URI);
                    //employeeData.ImagePath = imagePath.slice(base64Idx + CONST_BASE64_URI.length, idx);

                    // Employee photo was not changed, so set the image path to null
                    employeeData.ImagePath = null;
                    employeeData.ExcludePhoto = true;   // Note: Set this flag to true to disable updating the base64 and byte array value of the employee photo
                }
                else
                    employeeData.ImagePath = imagePath.slice(0, idx);
            }
            else {
                if (imagePath.includes(CONST_BASE64_URI)) {
                    //var base64Idx = imagePath.indexOf(CONST_BASE64_URI);
                    //employeeData.ImagePath = imagePath.slice(base64Idx + CONST_BASE64_URI.length);

                    // Employee photo was not changed, so set the image path to null
                    employeeData.ImagePath = null;
                    employeeData.ExcludePhoto = true;   // Note: Set this flag to true to disable updating the base64 and byte array value of the employee photo
                }
                else
                    employeeData.ImagePath = imagePath;
            }
        }

        // Save Licenses information
        if (_currentFormType == formTypes.ManageEmployee) {
            if (_licenseArray != undefined && _licenseArray.length > 0)
                employeeData.LicenseList = _licenseArray;
        }

        // Save Card History information
        if (_cardHistoryArray != undefined && _cardHistoryArray.length > 0)
            employeeData.CardHistoryList = _cardHistoryArray;
        // #endregion                

        // Convert object to JSON
        var jsonData = JSON.stringify({
            employeeData: employeeData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/UpdateIDCard",
            //url: "/WebService/ContractorWS.asmx/UpdateIDCard",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d.toString();                
                if (result.startsWith(CONST_SUCCESS)) {
                    setFormDataLoaded();
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "The ID card information has been updated successfully!", "Update Notification");

                    // Set Form Load flag
                    _currentFormMode = FormModes.LoadExistingRecord;

                    // Set the image filename
                    var idx = result.indexOf("|");
                    if (idx > 0) {
                        var fileName = result.substring(idx + 1);
                        $("#hidImageFileName").val(fileName);
                    }
                    //else
                    //    $("#hidImageFileName").val("");
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to update the ID card information due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while updating the ID card record.\n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while updating the ID card record.\n\n" + err);
    }
}

function updateIDCardBase64() {
    try {
        var employeeData = {};
        var imagePath = $(".pictureFrame img").attr("src");

        // #region Populate the object
        employeeData.ImageURLBase64 = imagePath != CONST_DEFAULT_PHOTO ? imagePath : "";
        employeeData.ImageFileName = $(".pictureFrame img").prop("data-filename");

        // Get the base64 image string to the hidden field
        if (employeeData.ImageURLBase64 != null && employeeData.ImageURLBase64 != undefined && employeeData.ImageURLBase64.includes(CONST_BASE64_KEY)) {
            var base64Idx = employeeData.ImageURLBase64.indexOf(CONST_BASE64_KEY);
            var base64Image = employeeData.ImageURLBase64.slice(base64Idx + CONST_BASE64_KEY.length);
            employeeData.ImagePath = base64Image;
        }

        employeeData.RegistryID = GetIntValue($("#hidCardRegistryID").val());
        employeeData.EmpNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());
        employeeData.UserEmpNo = GetIntValue($("input[id$='hidCurrentUserEmpNo'").val());
        employeeData.UserID = $("input[id$='hidCurrentUserID'").val().trim();

        if (_currentFormType == formTypes.ManageEmployee) {
            employeeData.EmpName = $("#txtEmpName").val().trim();
            employeeData.Position = $("#txtPosition").val().trim();
            employeeData.CustomCostCenter = $("#txtCostCenter").val().trim();
            employeeData.CPRNo = $("#txtCPRNo").val().trim();

            if ($("#cboBloodGroup").val() != CONST_EMPTY)
                employeeData.bloodGroup = $("#cboBloodGroup").val();
        }

        // Save Licenses information
        if (_currentFormType == formTypes.ManageEmployee) {
            if (_licenseArray != undefined && _licenseArray.length > 0)
                employeeData.LicenseList = _licenseArray;
        }

        // Save Card History information
        if (_cardHistoryArray != undefined && _cardHistoryArray.length > 0)
            employeeData.CardHistoryList = _cardHistoryArray;
        // #endregion                

        // Convert object to JSON
        var jsonData = JSON.stringify({
            employeeData: employeeData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/UpdateIDCardBase64",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d.toString();
                if (result.startsWith(CONST_SUCCESS)) {
                    setFormDataLoaded();
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "The ID card information has been updated successfully!", "Update Notification");

                    // Set Form Load flag
                    _currentFormMode = FormModes.LoadExistingRecord;

                    // Set the image filename
                    var idx = result.indexOf("|");
                    if (idx > 0) {
                        var fileName = result.substring(idx + 1);
                        $("#hidImageFileName").val(fileName);
                    }
                    
                    // Update the value of the hidden field that stores the base64 image
                    if (imagePath.includes(CONST_BASE64_KEY)) {
                        var base64Idx = imagePath.indexOf(CONST_BASE64_KEY);
                        var base64Image = imagePath.slice(base64Idx + CONST_BASE64_KEY.length);
                        $("#hidBase64Photo").val(base64Image);
                    }
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to update the ID card information due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while updating the ID card record.\n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while updating the ID card record.\n\n" + err);
    }
}

function insertIDCardBase64() {
    try {
        var idx;
        var employeeData = {};
        var imagePath = $(".pictureFrame img").attr("src");
        var empNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());

        // #region Populate the object

        // Initialize employee photo
        employeeData.ImageURLBase64 = imagePath != CONST_DEFAULT_PHOTO ? imagePath : "";
        employeeData.ImageFileName = $(".pictureFrame img").prop("data-filename");

        employeeData.EmpNo = empNo;
        employeeData.IsContractor = _currentFormType == formTypes.ManageContractor ? true : false;
        employeeData.UserEmpNo = $("input[id$='hidCurrentUserEmpNo'").val().trim();
        employeeData.UserID = $("input[id$='hidCurrentUserID'").val().trim();

        if (_currentFormType == formTypes.ManageEmployee) {
            employeeData.EmpName = $("#txtEmpName").val().trim();
            employeeData.Position = $("#txtPosition").val().trim();
            //employeeData.CustomCostCenter = encodeURIComponent($("#txtCostCenter").val().trim());
            employeeData.CustomCostCenter = $("#txtCostCenter").val().trim();
            employeeData.CPRNo = $("#txtCPRNo").val().trim();
            if ($("#cboBloodGroup").val() != CONST_EMPTY)
                employeeData.bloodGroup = $("#cboBloodGroup").val();
        }

        // Get the base64 image string to the hidden field
        if (employeeData.ImageURLBase64 != null && employeeData.ImageURLBase64 != undefined && employeeData.ImageURLBase64.includes(CONST_BASE64_KEY)) {
            var base64Idx = employeeData.ImageURLBase64.indexOf(CONST_BASE64_KEY);
            var base64Image = employeeData.ImageURLBase64.slice(base64Idx + CONST_BASE64_KEY.length);
            employeeData.ImagePath = base64Image;
        }

        // Save the licenses
        if (_currentFormType == formTypes.ManageEmployee) {
            if (_licenseArray != undefined && _licenseArray.length > 0)
                employeeData.LicenseList = _licenseArray;
        }

        // Save Card History information
        if (_cardHistoryArray != undefined && _cardHistoryArray.length > 0)
            employeeData.CardHistoryList = _cardHistoryArray;
        // #endregion

        // Convert object to JSON
        var jsonData = JSON.stringify({
            employeeData: employeeData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/InserIDCardBase64",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result != null && result.includes(CONST_SUCCESS)) {
                    // Get the Registry ID
                    idx = result.indexOf("|");
                    if (idx > 0) {
                        var registryID = GetIntValue(result.slice(idx + 1));
                        if (registryID > 0)
                            $("#hidCardRegistryID").val(registryID);
                    }

                    setFormDataLoaded();

                    // Load ID card details to get the base64 photo
                    if (imagePath != CONST_DEFAULT_PHOTO) {
                        searchIDCard(empNo);
                    }

                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "The new ID card information has been saved successfully!", "Add Record Notification");

                    // Set Form Load flag
                    _currentFormMode = FormModes.LoadExistingRecord;
                }
                else if (result == CONST_CARD_EXIST) {
                    HideLoadingPanel(gContainer);
                    showModalConfirmation(ModalTypes.IDCardExist);
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to save information to the database due to the following error.\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while adding new ID card.\n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while adding new ID card.\n\n" + err);
    }
}

function deleteIDCard() {
    try {
        var empNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());
        var isContractor = _currentFormType = formTypes.ManageContractor ? true : false;

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/DeleteIDCard",
            //url: "/WebService/ContractorWS.asmx/DeleteIDCard",
            data: JSON.stringify({ empNo: empNo, isContractor: isContractor }),
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result == CONST_SUCCESS) {
                    // Retreive the session variable
                    _currentFormType = GetDataFromSession("currentFormType");
                    DeleteDataFromSession("currentFormType");

                    resetForm();
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "ID Card record of employee no. " + empNo + " has been deleted successfully!", "Delete Notification");

                    // Set Form Load flag
                    gCurrentFormMode = FormModes.ClearForm;
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to delete the selected record due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while deleting the selected record." + "\n\nError: " + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while deleting the selected record.\n\n" + err);
    }
}

function generatePhoto() {
    try {
        var employeeData = {};

        // #region Populate the object
        employeeData.RegistryID = GetIntValue($("#hidCardRegistryID").val());
        employeeData.EmpNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());
        employeeData.ImageFileName = $("#hidImageFileName").val();
        employeeData.ImageURLBase64 = $("#hidBase64Photo").val().trim();
        // #endregion                

        // Convert object to JSON
        var jsonData = JSON.stringify({
            employeeData: employeeData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "IDCardGenerator.aspx/GeneratePhoto",
            //url: "/WebService/ContractorWS.asmx/GeneratePhoto",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result == CONST_SUCCESS) {
                    // Open the report viewer form
                    var empNo = _currentFormType == formTypes.ManageContractor ? GetIntValue($("#txtContractorNo").val()) : GetIntValue($("#txtEmpNo").val());
                    var isContractor = _currentFormType == formTypes.ManageContractor ? "true" : "";

                    if (_currentReportType == ReportTypes.LicenseOnlyReport)
                        location.href = formURLs.ReportViewer.concat("?callerForm=").concat(formURLs.IDCardGenerator).concat("&empNo=" + empNo).concat("&isContractor=").concat(isContractor).concat("&fileName=").concat($("#hidImageFileName").val()).concat("&reporttype=").concat(ReportTypes.LicenseOnlyReport);
                    else if (_currentReportType == ReportTypes.IDCardOnlyReport)
                        location.href = formURLs.ReportViewer.concat("?callerForm=").concat(formURLs.IDCardGenerator).concat("&empNo=" + empNo).concat("&isContractor=").concat(isContractor).concat("&fileName=").concat($("#hidImageFileName").val()).concat("&reporttype=").concat(ReportTypes.IDCardOnlyReport);
                    else
                        location.href = formURLs.ReportViewer.concat("?callerForm=").concat(formURLs.IDCardGenerator).concat("&empNo=" + empNo).concat("&isContractor=").concat(isContractor).concat("&fileName=").concat($("#hidImageFileName").val()).concat("&reporttype=").concat(ReportTypes.IDCardLicenseReport);

                    //HideLoadingPanel(gContainer);
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to print the ID card due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while viewing the ID card:\n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while viewing the ID:\n\n" + err);
    }
}
// #endregion