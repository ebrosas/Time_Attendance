// #region Declare Constants
const CONST_ALL = "valAll";
const CONST_EMPTY = "valEmpty";
const CONST_SUCCESS = "SUCCESS";
const CONST_FAILED = "FAILED";

const idType = {
    CPR: 0,
    Passport: 1
};

const formMode = {
    ClearForm: 0,
    LoadExistingRecord: 1,
    CreateNewRecord: 2,
    UpdateRecord: 3,
    DeleteRecord: 4
}

const modalFormTypes = {
    DeleteConfirmation: "delete",
    RegisterLicense: "license"
};

const modalFormLoadTypes = {
    OpenExistingRecord: 0,
    AddNewRecord: 1,
    UpdateRecord: 2,
    DeleteRecord: 3
};

const modalResponseTypes = {
    ModalYes: "modalYes",
    ModalNo: "modalNo",
    ModalCancel: "modalCancel",
    ModalSave: "modalSave",
    ModalDelete: "modalDelete"
};

const purchaseOrderData = {
    PONumber: 0,
    HasError: false,
    ErrorDescription: ""
};

const licenseArray = [];

//const swal = window.require('sweetalert');
// #endregion

// #region Declare Variables
var gCurrentFormMode;
var gModalFormType;
var gModalFormLoadType;
var gModalResponse;
var _contractorNo;
var _callerForm;
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
    try
    {
        if (dateInput.length > 0) 
            return new Date(dateInput).toISOString()
        else
            return "";
    }
    catch (err) {
        throw err;
    }
    
}

$.fn.getIntValue = function (inputString) {
    return isNaN(parseInt(inputString)) ? 0 : parseInt(inputString);
}
// #endregion

$(function () {
    try
    {
        // Set the current container
        gContainer = $('.formWrapper');

        // Get query string values
        _callerForm = GetQueryStringValue("callerForm");

        //#region Initialize input controls
        $("#txtContractStartDate").datepicker({
            dateFormat: "dd/mm/yy",
            altField: "#hdnStartDate",
            altFormat: "yy-mm-dd",
            //defaultDate: "01/01/2021",
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

        $("#txtContractEndDate").datepicker({
            dateFormat: "dd/mm/yy",
            altField: "#hdnEndDate",
            altFormat: "yy-mm-dd",
            //defaultDate: "01/01/2021",
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

        $("#txtRegistrationDate").datepicker({
            dateFormat: "dd/mm/yy",
            altField: "#hdnRegisterDate",
            altFormat: "yy-mm-dd",
            //defaultDate: "01/01/2021",
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
        $("#txtIssuedDate").datepicker({
            dateFormat: "dd/mm/yy",
            altField: "#hdnIssuedDate",
            altFormat: "yy-mm-dd",
            //defaultDate: "01/01/2021",
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
            //defaultDate: "01/01/2021",
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
        
        //#endregion

        //#region Show tooltips       
        $("#btnSearch").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to open the Contractor Inquiry form.</span>",
            placement: "right",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnIDType").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click here to swithc between CPR or Passport ID type.</span>",
            placement: "right",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnCreateNew").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to register new contractor.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnSave").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to save contractor details.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnUpdate").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to save contractor details.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnDelete").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to delete contractor record.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnReset").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to clear the form.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnPrint").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to view and print the contractor details report.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnGenerateCard").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to open the ID Card Generator form.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#btnBack").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to go back to previous page.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        $("#cboPurchaseOrder").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>The Purchase Order list is populated based on the specified Company Name.</span>",
            placement: "left",
            trigger: "hover",
            animation: true,
            html: true
        });
        //#endregion

        // #region Initialize event handlers
        $(".actionButton").on("click", handleActionButtonClick);
        $(".formButton").on("click", handleActionButtonClick);
        $(".legendTitle .linkTitle").on("click", showAddLicenseForm);       
        $("#txtIDNo").on("keyup", handleIDNumberEntry);
        $("#lnkCPR").on("click", handleCPRLinkClick);
        $("#lnkPassport").on("click", handlePassportLinkClick);
        $("input:required").on({
            focus: function () {
                $(this).css("border", "2px solid red");
                $(this).css("border-radius", "5px");
            },
            blur: function(){
                $(this).css("border", $("input:optional").css("border"));
                $(this).css("border-radius", $("input:optional").css("border"));
            }             
        });

        // Restrict input in Contractor No. field to numbers only
        $("#txtContractorNo").on(
            {
                keypress : function (event) {
                    var ASCIICode = event.which || event.keyCode
                    if (ASCIICode == 13) {      // Enter key 
                        $("#btnFind").click();
                    }
                }
                //keydown : function(event){
                //    var ASCIICode = event.keyCode
                //    if (ASCIICode == 27) {
                //        alert("Esc key is pressed");
                //    }
                //}
            }
        );

        // Add data validation event for Purchase Order No. entry
        //$("#txtPONumber").on({
        //    blur: function () {
        //        var oldPONumber = GetFloatValue($("#hidPONumber").val());
        //        var poNumber = GetFloatValue($(this).val());
        //        if (oldPONumber > 0 && poNumber > 0 && oldPONumber != poNumber) {
        //            validatePONumber(poNumber);
        //        }
        //    },
        //    keypress: function (event) {
        //        return OnlyNumberKey(event)
        //    }
        //})

        $("#cboPurchaseOrder").focus(function () {
            $("#cboPurchaseOrder").popover("hide");
        });

        $("#mainForm").keydown(function (event) {
            var ASCIICode = event.keyCode
            if (ASCIICode == 113) {     //F2 key
                ShowToastMessage(toastTypes.info, "This web application was developed by Ervin Olinas Brosas. For any technical support, please call ext. #3152.", "System Information");
            }
        })

        $("#licenseInfoSwitch").click(function () {
            if ($(this).is(":checked")) {
                // Show License Information panel
                $(".custom-switch label").text("Hide License Information");
                $("#collapseLicenseInfo").collapse("show");
                populateLicenseTable();
            }
            else {
                // Hide License Information panel
                $(".custom-switch label").text("Show License Information");
                $("#collapseLicenseInfo").collapse("hide");
            }
        });

        // Restrict entry to numbers only for Work Duration fields
        $("#txtWorkHours").attr("onkeypress", "return OnlyNumberKey(event)");
        $("#txtWorkMins").attr("onkeypress", "return OnlyNumberKey(event)");

        // Modal form events
        $(".modal-footer > button").on("click", handleModalButtonClick);
        $(".modal").on("show.bs.modal", handleShowModalForm);
        $(".modal").on("hide.bs.modal", handleHideModalForm);
        //#endregion

        ShowLoadingPanel(gContainer, 1, 'Initalizing form, please wait...');        

        // Initialize user form access
        GetUserFormAccess($("#hidFormCode").val().trim(), $("#hidCostCenter").val().trim(), GetIntValue($("#hidCurrentUserEmpNo").val()));

        // Populate comboboxes
        getLookupTable();

        var isBackClicked = Boolean(GetQueryStringValue("isback"));
        if (isBackClicked) {
            _contractorNo = parseInt(GetDataFromSession("contractorNo"));

            // Replace caller form with one saved in session storage
            _callerForm = GetDataFromSession("contractRegistrationCF");
            
            // Remove data from the session storage
            DeleteDataFromSession("contractorNo");
            DeleteDataFromSession("contractRegistrationCF");

            //#region Initialize form when called by another form
            if (_callerForm != "undefined" && _callerForm != null) {
                // Show "Go Back" button
                $("#btnBack").removeAttr("hidden");

                // Hide "Generate Card", "Go" and search buttons
                $("#btnFind").prop("hidden", true);
                $("#btnSearch").prop("hidden", true);

                if (_callerForm != formURLs.ContractorInquiry)
                    $("#btnGenerateCard").prop("hidden", true);
            }
            //#endregion
        }
        else {
            _contractorNo = parseInt(GetDataFromSession("contractorNo"));            
            resetForm();
        }

        if (Boolean(GetDataFromSession("showLicense"))) {
            DeleteDataFromSession("showLicense");
            $("#licenseInfoSwitch").prop("checked", true);
            $("#licenseInfoSwitch").click();
        }

        // Move to the top of the page
        window.scrollTo(0, 0);
    }
    catch (err) {
        ShowErrorMessage("The following exception has occured while loading the page: " + err)
        HideLoadingPanel(gContainer);
    }
});

// #region Event Handlers
function handleActionButtonClick() {
    var btn = $(this);

    // Hide all error messages
    HideErrorMessage();
    HideToastMessage();

    switch ($(btn)[0].id) {
        case "btnCreateNew":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Create)) {
                createNewRegistration(this);
                setContractorNo();
            }
            else 
                ShowToastMessage(toastTypes.error, CONST_CREATE_DENIED, "Access Denied");
            break;

        case "btnSave":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Create))
                beginAddContractor();
            else
                ShowToastMessage(toastTypes.error, CONST_CREATE_DENIED, "Access Denied");
            break;

        case "btnUpdate":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Update)) 
                beginUpdateContractor();
            else
                ShowToastMessage(toastTypes.error, CONST_UPDATE_DENIED, "Access Denied");
            break;

        case "btnDelete":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Delete))
                beginDeleteContractor();
            else
                ShowToastMessage(toastTypes.error, CONST_DELETE_DENIED, "Access Denied");
            break;

        case "btnReset":
            // Set Form Load flag
            gCurrentFormMode = formMode.ClearForm;
            resetForm();
            break;

        case "btnPrint":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Print))
                beginPrintReport();
            else
                ShowToastMessage(toastTypes.error, CONST_PRINT_DENIED, "Access Denied");
            break;

        case "btnGenerateCard":
            if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Print))
                beginGenerateCard();
            else
                ShowToastMessage(toastTypes.error, CONST_PRINT_DENIED, "Access Denied");
            break;

        case "btnBack":
            ShowLoadingPanel(gContainer, 1, 'Going back to previous page, please...');
            if (_callerForm != "undefined" && _callerForm != null) {
                if (_callerForm == formURLs.IDCardGenerator) {
                    location.href = _callerForm.concat("?contractorNo=").concat($("#txtContractorNo").val());
                }
                else 
                    location.href = _callerForm;
            }                
            break;

        case "btnFind":
            beginFindContractor();
            break;

        case "btnSearch":
            ShowLoadingPanel(gContainer, 1, 'Please wait...');
            location.href = formURLs.ContractorInquiry.concat("?callerForm=RegisterContractor.aspx")
            break;
    }
}

function handleIDNumberEntry() {
    if ($("#optCPR").hasAttr("hidden") && $("#optPassport").hasAttr("hidden"))
        $("#optCPR").removeAttr("hidden");
}

function handleCPRLinkClick() {
    if ($("#optCPR").hasAttr("hidden"))
        $("#optCPR").removeAttr("hidden");

    if ($("#optPassport").hasAttr("hidden") == false)
        $("#optPassport").attr("hidden", "hidden");

    // Set the ID No. text input to 8 chars
    $("#txtIDNo").attr("maxlength", "9");

    // Set the tooltip
    $("#txtIDNo").attr("title", "Notes: Valid input are numbers only (0-9).");

    // Force input to numbers only
    $("#txtIDNo").attr("onkeypress", "return OnlyNumberKey(event)");
    $("#txtContractorNo").attr("onkeypress", "return OnlyNumberKey(event)");
    //$("#txtPONumber").attr("onkeypress", "return OnlyNumberKey(event)");

    // Force text input to uppercase
    $("#txtIDNo").css("text-transform", "lowercase");

    // Set the ID type hidden field
    $("input[id$='hidIDType'").val(idType.CPR);

    // Initialize the ID type button
    $("#btnIDType").text("CPR");
    $("#btnIDType").removeClass("btn-danger").addClass("btn-success");

    // Clear the ID No. text and set the focus
    if (gCurrentFormMode == formMode.CreateNewRecord ||
        gCurrentFormMode == formMode.LoadExistingRecord)
        $("#txtIDNo").val("").focus();
}

function handlePassportLinkClick() {
    if ($("#optPassport").hasAttr("hidden"))
        $("#optPassport").removeAttr("hidden");

    if ($("#optCPR").hasAttr("hidden") == false)
        $("#optCPR").attr("hidden", "hidden");

    // Set the ID No. text input to 20 chars
    $("#txtIDNo").attr("maxlength", "20");

    // Set the tooltip
    $("#txtIDNo").attr("title", "Notes: Valid input are alpha-numeric characters.");

    if ($("#txtIDNo").hasAttr("onkeypress"))
        $("#txtIDNo").removeAttr("onkeypress");

    // Force text input to uppercase
    $("#txtIDNo").css("text-transform", "uppercase");

    // Set the ID type hidden field
    $("input[id$='hidIDType'").val(idType.Passport);

    // Initialize the ID type button
    $("#btnIDType").text("Passport");
    $("#btnIDType").removeClass("btn-success").addClass("btn-danger");

    // Clear the ID No. text and set the focus
    $("#txtIDNo").val("").focus();
}

function handleHideModalForm() {
    if (gModalResponse != undefined && gModalFormType != undefined) {
        switch (gModalFormType) {
            case modalFormTypes.DeleteConfirmation:
                if (gModalResponse == modalResponseTypes.ModalYes) {
                    deleteContractor();
                }
                break;

            case modalFormTypes.RegisterLicense:
                if (gModalResponse == modalResponseTypes.ModalSave ||
                    gModalResponse == modalResponseTypes.ModalDelete) {
                    resetModalForm();
                }
                break;
        }
    }
}

function handleShowModalForm() {
    switch (gModalFormType) {
        case modalFormTypes.RegisterLicense:
            if (gModalFormLoadType == modalFormLoadTypes.AddNewRecord) {
                if (!$(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").attr("hidden", "hidden");
            }
            else if (gModalFormLoadType == modalFormLoadTypes.OpenExistingRecord || gModalFormLoadType == modalFormLoadTypes.UpdateRecord) {
                if ($(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").removeAttr("hidden");
            }
            break;
    }
}

function handleModalButtonClick() {
    var btnAttrib = $(this).attr("data-button-value");

    if (btnAttrib == modalResponseTypes.ModalYes)
        gModalResponse = modalResponseTypes.ModalYes;

    else if (btnAttrib == modalResponseTypes.ModalNo)
        gModalResponse = modalResponseTypes.ModalNo;

    else if (btnAttrib == modalResponseTypes.ModalCancel)
        gModalResponse = modalResponseTypes.ModalCancel;

    else if (btnAttrib == modalResponseTypes.ModalDelete) {
        gModalResponse = modalResponseTypes.ModalDelete;
        if (beginDeleteLicense()) {
            $("#modLicenseRegistration").modal("hide");
            gContainer = $('.formWrapper');

            // Show success message
            ShowToastMessage(toastTypes.success, "License record has been deleted successfully!", "Delete License Notification");
        }
    }

    else if (btnAttrib == modalResponseTypes.ModalSave) {
        gModalResponse = modalResponseTypes.ModalSave;
        if (beginSaveLicense()) {
            $("#modLicenseRegistration").modal("hide");
            gContainer = $('.formWrapper');

            // Show success message
            ShowToastMessage(toastTypes.success, "License details have been saved successfully!", "Save License Notification");
        }
    }
}
// #endregion

// #region Private Functions
function beginSaveLicense() {
    try {
        var hasError = false;

        //#region Validate data entry
        // Check the License Type
        if ($('#cboLicenseType').val() == null || $('#cboLicenseType').val() == CONST_EMPTY) {
            DisplayAlert($('#licenseTypeValid'), "<b>" + $(".modalFieldTitle label[data-field='LicenseType']").text() + "</b> is a required field.", $('#cboLicenseType'));
            hasError = true;
        }
        else {
            if ($('#licenseTypeValid').attr("hidden") == undefined)
                $('#licenseTypeValid').attr("hidden", "hidden");
        }

        // Check License No.
        if ($('#txtLicenseNo').val().trim().length == 0) {
            DisplayAlert($('#licenseNoValid'), "<b>" + $(".modalFieldTitle label[data-field='LicenseNo']").text() + "</b> is a required field.", $('#txtLicenseNo'));
            hasError = true;
        }
        else {
            if ($('#licenseNoValid').attr("hidden") == undefined)
                $('#licenseNoValid').attr("hidden", "hidden");
        }

        // Check Issued Date
        if ($('#txtIssuedDate').val().trim().length == 0) {
            DisplayAlert($('#issuedDateValid'), "<b>" + $(".modalFieldTitle label[data-field='IssuedDate']").text() + "</b> is a required field.");
            hasError = true;
        }
        else {
            if (!IsValidDate($('#txtIssuedDate').val().trim())) {
                DisplayAlert($('#issuedDateValid'), "<b>" + $(".modalFieldTitle label[data-field='IssuedDate']").text() + "</b> is invalid!", $('#txtIssuedDate'));
                hasError = true;
            }
            else {
                if ($('#issuedDateValid').attr("hidden") == undefined)
                    $('#issuedDateValid').attr("hidden", "hidden");
            }
        }

        // Check Expiry Date
        if ($('#txtExpiryDate').val().trim().length == 0) {
            DisplayAlert($('#expiryDateValid'), "<b>" + $(".modalFieldTitle label[data-field='ExpiryDate']").text() + "</b> is a required field.");
            hasError = true;
        }
        else {
            if (!IsValidDate($('#txtExpiryDate').val().trim())) {
                DisplayAlert($('#expiryDateValid'), "<b>" + $(".modalFieldTitle label[data-field='ExpiryDate']").text() + "</b> is invalid!", $('#txtExpiryDate'));
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
                    DisplayAlert($('#issuedDateValid'), "<b>" + $(".modalFieldTitle label[data-field='IssuedDate']").text() + "</b> should be less than Expiry Date!");
                    hasError = true;
                }
            }}
        //#endregion

        if (!hasError) {
            // Display loading panel
            gContainer = $('#modLicenseRegistration');
            ShowLoadingPanel(gContainer, 2, 'Saving license information, please wait...');

            switch (gModalFormLoadType) {
                case modalFormLoadTypes.AddNewRecord:
                    addLicense();
                    break;

                case modalFormLoadTypes.UpdateRecord:
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
    try{
        var hasError = false;

        // Check License No.
        if ($('#txtLicenseNo').val().trim().length == 0) {
            DisplayAlert($('#licenseNoValid'), $(".modalFieldTitle label[data-field='LicenseNo']").text() + " is not defined.", $('#txtLicenseNo'));
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

function beginPrintReport() {
    try {
        var hasError = false;

        // Validate Contractor No.
        if ($('#txtContractorNo').val().length == 0) {
            DisplayAlert($('#contractorNoValid'), $(".fieldLabel label[data-field='ContractorNo']").text() + " is required and cannot be left blank.", $('#txtContractorNo'));
            hasError = true;
        }
        else {
            if ($('#contractorNoValid').attr("hidden") == undefined)
                $('#contractorNoValid').attr("hidden", "hidden");
        }

        if (!hasError) {
            if ($("#licenseInfoSwitch").is(":checked"))
                SaveDataToSession("showLicense", "true");
            else
                SaveDataToSession("showLicense", "");

            // Save caller form value to session storage
            if (_callerForm != "undefined" && _callerForm != null) {
                SaveDataToSession("contractRegistrationCF", _callerForm);
            }

            location.href = formURLs.ReportViewer.concat("?callerForm=").concat(formURLs.ContractorRegistration).concat("&empNo=").concat($('#txtContractorNo').val()).concat("&reporttype=").concat(ReportTypes.ContractorDetailsReport);
        }
    }
    catch (err) {
        throw err;
    }
}

function beginGenerateCard() {
    var hasError = false;

    // Validate Contractor No.
    if ($('#txtContractorNo').val().trim().length == 0) {
        DisplayAlert($('#contractorNoValid'), $(".fieldLabel label[data-field='ContractorNo']").text() + " is required and cannot be left blank.", $('#txtContractorNo'));
        hasError = true;
    }
    else {
        if ($('#contractorNoValid').attr("hidden") == undefined)
            $('#contractorNoValid').attr("hidden", "hidden");
    }

    if (!hasError) {

        ShowLoadingPanel(gContainer, 1, 'Opening ID Card Generator form, please wait...');

        // Save caller form value to session storage
        if (_callerForm != "undefined" && _callerForm != null) {
            SaveDataToSession("contractRegistrationCF", _callerForm);
        }

        // Open the ID Card Generator form
        var contractorNo = GetIntValue($("#txtContractorNo").val());
        location.href = formURLs.IDCardGenerator.concat("?callerForm=RegisterContractor.aspx").concat("&contractorNo=" + contractorNo);        
    }
}

function beginDeleteContractor() {
    $("#btnDelete").popover("hide");

    // Set the modal header title
    gModalFormType = modalFormTypes.DeleteConfirmation;
    setModalTitle();

    $("#modalConfirmation").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
    
    //swal({
    //    title: "Are you sure you want to delete the selected contractor?",
    //    text: "Once deleted, you will not be able to see the contractor details or restore back the data!",
    //    icon: "success",
    //    buttons: true,
    //    dangerMode: true
    //})
    //.then((willDelete) => {
    //    if (willDelete) {
    //        alert('Delete was selected');
    //        //deleteContractor();
    //    }
    //});
}

function setModalTitle() {
    if (gModalFormType == undefined)
        return;

    switch (gModalFormType) {
        case modalFormTypes.DeleteConfirmation:
            // Set the title of the modal form
            $("#modalConfirmation .modalHeader").html("&nbsp;Warning");

            // Set the icon of the modal form
            $("#modalConfirmationIcon").removeClass("fa-times-circle").addClass("fa-exclamation-triangle");

            $(".modal-body > p").html("Deleting a contractor will delete all associated licenses and ID card record in the database. Are you sure you want to <span class='text-info font-weight-bold'>DELETE</span> this record?");
            break;
    }
}

function resetForm() {
    HideErrorMessage();

    // Hide all error alerts
    $('.errorPanel').attr("hidden", "hidden");

    // Enable input controls
    if ($("#txtContractorNo").hasAttr("readonly"))
        $("#txtContractorNo").removeAttr("readonly");

    // Hide all popovers
    $("#txtContractorNo").popover("hide");
    $("#btnFind").popover("hide");
    $("#btnIDType").popover("hide");
    $("#btnCreateNew").popover("hide");
    $("#btnSave").popover("hide");
    $("#btnUpdate").popover("hide");
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnGenerateCard").popover("hide");
    $("#btnBack").popover("hide");
    $("#cboPurchaseOrder").popover("hide");

    // Enable "Create New" button
    if ($("#btnCreateNew").hasClass("btn-outline-secondary"))
        $("#btnCreateNew").removeClass("btn-outline-secondary")

    $("#btnCreateNew").addClass("btn-primary").addClass("border-0");
    $("#btnCreateNew").removeAttr("disabled");

    // Enable/Disable buttons
    $("#btnPrint").removeAttr("disabled");
    $("#btnGenerateCard").removeAttr("disabled");
    $("#btnBack").removeAttr("disabled");
    $("#btnFind").removeAttr("disabled");
    $(".legendTitle .linkTitle").attr("disabled", "disabled");      // Add New License link

    // Show/Hide buttons
    $("#btnSave").attr("hidden", "hidden");
    $("#btnUpdate").attr("hidden", "hidden");
    $("#btnDelete").attr("hidden", "hidden");

    //#region Initialize form when called by another form
    if (_callerForm != "undefined" && _callerForm != null) {
        // Show "Go Back" button
        $("#btnBack").removeAttr("hidden");

        // Hide "Generate Card", "Go" and search buttons
        $("#btnFind").prop("hidden", true);
        $("#btnSearch").prop("hidden", true);

        if (_callerForm != formURLs.ContractorInquiry)
            $("#btnGenerateCard").prop("hidden", true);
    }
    //#endregion

    $("#optPassport").attr("hidden", "hidden");
    $("#btnHiddenReset").click();
    $("#lnkCPR").click();

    // Refresh DataTable
    if (licenseArray.length > 0) {
        licenseArray.splice(0, licenseArray.length);
    }
    populateLicenseTable();

    // Reset controls
    $("#collapseLicenseInfo").collapse("hide");
    $("#licenseInfoSwitch").prop("disabled", true);
    $("#licenseInfoSwitch").prop("checked", false);
    $("#licenseInfoSwitch").click();

    // Remove all PO items
    if ($("#cboPurchaseOrder").children().length > 0) {
        $("#cboPurchaseOrder").children().remove().end();
    }

    // Delete session objects
    DeleteDataFromSession("contractorNo");

    // Move to the top of the page
    window.scrollTo(0, 0);

    // Set focus to Contractor No
    $("#txtContractorNo").focus();
}

function setFormDataLoaded() {
    // Enable input controls
    if ($("#txtContractorNo").hasAttr("readonly"))
        $("#txtContractorNo").removeAttr("readonly");

    // Hide all popovers
    $("#txtContractorNo").popover("hide");
    $("#btnFind").popover("hide");
    $("#btnIDType").popover("hide");
    $("#btnCreateNew").popover("hide");
    $("#btnSave").popover("hide");
    $("#btnUpdate").popover("hide");
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnGenerateCard").popover("hide");
    $("#btnBack").popover("hide");
    $("#cboPurchaseOrder").popover("hide");
      
    // Enable "Create New" button
    if ($("#btnCreateNew").hasClass("btn-outline-secondary"))
        $("#btnCreateNew").removeClass("btn-outline-secondary")

    $("#btnCreateNew").addClass("btn-primary").addClass("border-0");
    $("#btnCreateNew").removeAttr("disabled");

    // Enable/Disable buttons
    $("#btnPrint").removeAttr("disabled");
    $("#btnGenerateCard").removeAttr("disabled");
    $("#btnBack").removeAttr("disabled");
    $("#btnFind").removeAttr("disabled");
    $(".legendTitle .linkTitle").removeAttr("disabled");

    // Show/Hide buttons and controls
    $("#btnSave").attr("hidden", "hidden");
    $("#btnUpdate").removeAttr("hidden");
    $("#btnDelete").removeAttr("hidden");
    $("#optPassport").attr("hidden", "hidden");
    $("#licenseInfoSwitch").prop("disabled", false);
}

function createNewRegistration(btn) {
    // Hide error message
    HideErrorMessage();

    // Hide all error alerts
    $('.errorPanel').attr("hidden", "hidden");

    // Disable input controls
    $("#txtContractorNo").attr("readonly", "readonly");

    // Hide all popovers
    $("#txtContractorNo").popover("hide");
    $("#btnFind").popover("hide");
    $("#btnIDType").popover("hide");
    $("#btnCreateNew").popover("hide");
    $("#btnSave").popover("hide");
    $("#btnUpdate").popover("hide");
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnGenerateCard").popover("hide");
    $("#btnBack").popover("hide");
    $("#cboPurchaseOrder").popover("hide");
       
    // Disable Create New button
    if ($(btn).hasClass("btn-primary"))
        $(btn).removeClass("btn-primary");

    if ($(btn).hasClass("border-0"))
        $(btn).removeClass("border-0");

    $(btn).addClass("btn-outline-secondary");

    //Disable buttons
    $(btn).attr("disabled", "disabled");
    $("#btnPrint").attr("disabled", "disabled");
    $("#btnGenerateCard").attr("disabled", "disabled");
    $("#btnBack").attr("disabled", "disabled");
    $("#btnFind").attr("disabled", "disabled");

    // Show/Hide buttons and links
    $("#btnSave").removeAttr("hidden");
    $("#btnUpdate").attr("hidden", "hidden");
    $("#btnDelete").attr("hidden", "hidden");
    $("#btnHiddenReset").click();
    $("#lnkCPR").click();
    $(".legendTitle .linkTitle").removeAttr("disabled");    // Add New License link
    $("#licenseInfoSwitch").prop("disabled", false);        // Enable Show License Information switch

    // Set the Registration Date to current date
    var currentDate = new Date();
    $("#txtRegistrationDate").val(currentDate.toLocaleDateString());
    $("#hdnRegisterDate").val(currentDate.toISOString());

    // Clear License grid
    if (licenseArray.length > 0) {
        //licenseArray.length = 0;
        licenseArray.splice(0, licenseArray.length);
    }
    populateLicenseTable();

    // Set Form Load flag
    gCurrentFormMode = formMode.CreateNewRecord;

    // Set focus to ID No
    $("#txtIDNo").focus();
}

function beginAddContractor() {
    var hasError = false;
    
    // #region Validate data input

    // Validate ID Number
    if ($('#txtIDNo').val().trim().length == 0) {
        DisplayAlert($('#idNoValid'), "<b>" + $(".fieldLabel label[data-field='IDNo']").text() + "</b> is required and cannot be left blank.", $('#txtIDNo'));
        hasError = true;
    }
    else {
        if ($('#idNoValid').attr("hidden") == undefined)
            $('#idNoValid').attr("hidden", "hidden");
    }

    // Validate First Name
    if ($('#txtFirstName').val().trim().length == 0) {
        DisplayAlert($('#firstNameValid'), "<b>" + $(".fieldLabel label[data-field='FirstName']").text() + "</b> is required and cannot be left blank.", $('#txtFirstName'));
        hasError = true;
    }
    else {
        if ($('#firstNameValid').attr("hidden") == undefined)
            $('#firstNameValid').attr("hidden", "hidden");
    }

    // Validate Last Name
    if ($('#txtLastName').val().trim().length == 0) {
        DisplayAlert($('#lastNameValid'), "<b>" + $(".fieldLabel label[data-field='LastName']").text() + "</b> is required and cannot be left blank.", $('#txtLastName'));
        hasError = true;
    }
    else {
        if ($('#lastNameValid').attr("hidden") == undefined)
            $('#lastNameValid').attr("hidden", "hidden");
    }

    // Validate Company Name
    if ($('#txtCompanyName').val().trim().length == 0) {
        DisplayAlert($('#companyNameValid'), "<b>" + $(".fieldLabel label[data-field='CompanyName']").text() + "</b> is required and cannot be left blank.", $('#txtCompanyName'));
        hasError = true;
    }
    else if ($("#hidCompanyID").val().length == 0) {
        DisplayAlert($('#companyNameValid'), "The specified company name is not yet registered in the system.", $('#txtCompanyName'));
        hasError = true;
    }
    else {
        if ($('#companyNameValid').attr("hidden") == undefined)
            $('#companyNameValid').attr("hidden", "hidden");
    }

    // Validate Job Title
    if ($('#cboJobTitle').val().trim() == CONST_EMPTY) {
        DisplayAlert($('#jobTitleValid'), "<b>" + $(".fieldLabel label[data-field='JobTitle']").text() + "</b> is required and cannot be left blank.", $('#cboJobTitle'));
        hasError = true;
    }
    else {
        if ($('#jobTitleValid').attr("hidden") == undefined)
            $('#jobTitleValid').attr("hidden", "hidden");
    }
   
    //if ($('#cboPurchaseOrder').val() == null || $('#cboPurchaseOrder').val() == undefined || $('#cboPurchaseOrder').val().trim() == CONST_EMPTY) {
    //    DisplayAlert($('#poNumberValid'), "<b>" + $(".fieldLabel label[data-field='PONumber']").text() + "</b> is required and cannot be left blank.", $('#cboPurchaseOrder'));
    //    hasError = true;
    //}
    //else {
    //    if ($('#poNumberValid').attr("hidden") == undefined)
    //        $('#poNumberValid').attr("hidden", "hidden");
    //}

    // Validate Cost Center
    if ($('#cboCostCenter').val().trim() == CONST_EMPTY) {
        DisplayAlert($('#costCenterValid'), "<b>" + $(".fieldLabel label[data-field='CostCenter']").text() + "</b> is required and cannot be left blank.", $('#cboCostCenter'));
        hasError = true;
    }
    else {
        if ($('#costCenterValid').attr("hidden") == undefined)
            $('#costCenterValid').attr("hidden", "hidden");
    }

    // Validate Supervisor In-charge
    if ($('#txtSupervisor').val().trim().length == 0) {
        DisplayAlert($('#supervisorValid'), "<b>" + $(".fieldLabel label[data-field='Supervisor']").text() + "</b> is required and cannot be left blank.", $('#txtSupervisor'));
        hasError = true;
    }
    else if ($("#hidSupervisorNo").val().length == 0) {
        DisplayAlert($('#supervisorValid'), "The specified supervisor does not exist.", $('#txtSupervisor'));
        hasError = true;
    }
    else {
        if ($('#supervisorValid').attr("hidden") == undefined)
            $('#supervisorValid').attr("hidden", "hidden");
    }

    // Validate Contract Duration
    if ($('#txtContractStartDate').val().trim().length == 0 && $('#txtContractEndDate').val().trim().length == 0) {
        DisplayAlert($('#durationValid'), "<b>" + $(".fieldLabel label[data-field='Duration']").text() + "</b> is required and cannot be left blank.");
        hasError = true;
    }
    else if ($('#txtContractStartDate').val().trim().length == 0 && $('#txtContractEndDate').val().trim().length > 0) {
        DisplayAlert($('#durationValid'), "Must provide the contract start date.", $('#txtContractStartDate'));
        hasError = true;
    }
    else if ($('#txtContractStartDate').val().trim().length > 0 && $('#txtContractEndDate').val().trim().length == 0) {
        DisplayAlert($('#durationValid'), "Must provide the contract end date.", $('#txtContractEndDate'));
        hasError = true;
    }
    else {
        if (IsValidDate($('#txtContractStartDate').val().trim()) == false) {
            DisplayAlert($('#durationValid'), "Start date is invalid!", $('#txtContractStartDate'));
            hasError = true;
        }
        else if (IsValidDate($('#txtContractEndDate').val().trim()) == false) {
            DisplayAlert($('#durationValid'), "End date is invalid!", $('#txtContractEndDate'));
            hasError = true;
        }
        else {
            if ($('#durationValid').attr("hidden") == undefined)
                $('#durationValid').attr("hidden", "hidden");
        }
    }

    // Validate Work Duration
    var hourMax = GetIntValue($('#txtWorkHours').attr("max"));
    var minMax = GetIntValue($('#txtWorkMins').attr("max"));

    if (hourMax > 0 && GetIntValue($('#txtWorkHours').val()) > hourMax &&
        minMax > 0 && GetIntValue($('#txtWorkMins').val()) > minMax) {
        DisplayAlert($('#workDurationValid'), "The value entered in the work duration (hours & minutes) exceeded the maximum limit.", $('#txtWorkHours'));
        hasError = true;
    }
    else if (hourMax > 0 && GetIntValue($('#txtWorkHours').val()) <= hourMax &&
        minMax > 0 && GetIntValue($('#txtWorkMins').val()) > minMax) {
        DisplayAlert($('#workDurationValid'), "The value entered in the work duration (minutes) exceeded the maximum limit.", $('#txtWorkMins'));
        hasError = true;
    }
    else if (hourMax > 0 && GetIntValue($('#txtWorkHours').val()) > hourMax &&
        minMax > 0 && GetIntValue($('#txtWorkMins').val()) <= minMax) {
        DisplayAlert($('#workDurationValid'), "The value entered in the work duration (hours) exceeded the maximum limit.", $('#txtWorkHours'));
        hasError = true;
    }
    else {
        if ($('#workDurationValid').attr("hidden") == undefined)
            $('#workDurationValid').attr("hidden", "hidden");
    }
    // #endregion

    if (!hasError) {
        // Show the loading panel
        ShowLoadingPanel(gContainer, 1, 'Registering new contractor, please wait...');

        insertContractor();
    }        
}

function beginUpdateContractor() {
    var hasError = false;

    // #region Validate data input

    // Validate Contractor No.
    if ($('#txtContractorNo').val().trim().length == 0) {
        DisplayAlert($('#contractorNoValid'), "<b>" + $(".fieldLabel label[data-field='ContractorNo']").text() + "</b> is required and cannot be left blank.", $('#txtContractorNo'));
        hasError = true;
    }
    else {
        if ($('#contractorNoValid').attr("hidden") == undefined)
            $('#contractorNoValid').attr("hidden", "hidden");
    }

    // Validate ID Number
    if ($('#txtIDNo').val().trim().length == 0) {
        DisplayAlert($('#idNoValid'), "<b>" + $(".fieldLabel label[data-field='IDNo']").text() + "</b> is required and cannot be left blank.", $('#txtIDNo'));
        hasError = true;
    }
    else {
        if ($('#idNoValid').attr("hidden") == undefined)
            $('#idNoValid').attr("hidden", "hidden");
    }

    // Validate First Name
    if ($('#txtFirstName').val().trim().length == 0) {
        DisplayAlert($('#firstNameValid'), "<b>" + $(".fieldLabel label[data-field='FirstName']").text() + "</b> is required and cannot be left blank.", $('#txtFirstName'));
        hasError = true;
    }
    else {
        if ($('#firstNameValid').attr("hidden") == undefined)
            $('#firstNameValid').attr("hidden", "hidden");
    }

    // Validate Last Name
    if ($('#txtLastName').val().trim().length == 0) {
        DisplayAlert($('#lastNameValid'), "<b>" + $(".fieldLabel label[data-field='LastName']").text() + "</b> is required and cannot be left blank.", $('#txtLastName'));
        hasError = true;
    }
    else {
        if ($('#lastNameValid').attr("hidden") == undefined)
            $('#lastNameValid').attr("hidden", "hidden");
    }

    // Validate Company Name
    if ($('#txtCompanyName').val().trim().length == 0) {
        DisplayAlert($('#companyNameValid'), "<b>" + $(".fieldLabel label[data-field='CompanyName']").text() + "</b> is required and cannot be left blank.", $('#txtCompanyName'));
        hasError = true;
    }
    else if ($("#hidCompanyID").val().length == 0) {
        DisplayAlert($('#companyNameValid'), "The specified company name is not yet registered in the system.", $('#txtCompanyName'));
        hasError = true;
    }
    else {
        if ($('#companyNameValid').attr("hidden") == undefined)
            $('#companyNameValid').attr("hidden", "hidden");
    }

    // Validate Job Title
    if ($('#cboJobTitle').val().trim() == CONST_EMPTY) {
        DisplayAlert($('#jobTitleValid'), "<b>" + $(".fieldLabel label[data-field='JobTitle']").text() + "</b> is required and cannot be left blank.", $('#cboJobTitle'));
        hasError = true;
    }
    else {
        if ($('#jobTitleValid').attr("hidden") == undefined)
            $('#jobTitleValid').attr("hidden", "hidden");
    }

    // Validate Purchase Order No.
    //if ($('#cboPurchaseOrder').val() == null || $('#cboPurchaseOrder').val() == undefined || $('#cboPurchaseOrder').val().trim() == CONST_EMPTY) {
    //    DisplayAlert($('#poNumberValid'), "<b>" + $(".fieldLabel label[data-field='PONumber']").text() + "</b> is required and cannot be left blank.", $('#cboPurchaseOrder'));
    //    hasError = true;
    //}
    //else {
    //    if ($('#poNumberValid').attr("hidden") == undefined)
    //        $('#poNumberValid').attr("hidden", "hidden");
    //}

    // Validate Cost Center
    if ($('#cboCostCenter').val().trim() == CONST_EMPTY) {
        DisplayAlert($('#costCenterValid'), "<b>" + $(".fieldLabel label[data-field='CostCenter']").text() + "</b> is required and cannot be left blank.", $('#cboCostCenter'));
        hasError = true;
    }
    else {
        if ($('#costCenterValid').attr("hidden") == undefined)
            $('#costCenterValid').attr("hidden", "hidden");
    }

    // Validate Supervisor In-charge
    if ($('#txtSupervisor').val().trim().length == 0) {
        DisplayAlert($('#supervisorValid'), "<b>" + $(".fieldLabel label[data-field='Supervisor']").text() + "</b> is required and cannot be left blank.", $('#txtSupervisor'));
        hasError = true;
    }
    else if ($("#hidSupervisorNo").val().length == 0) {
        DisplayAlert($('#supervisorValid'), "The specified supervisor does not exist.", $('#txtSupervisor'));
        hasError = true;
    }
    else {
        if ($('#supervisorValid').attr("hidden") == undefined)
            $('#supervisorValid').attr("hidden", "hidden");
    }

    // Validate Contract Duration
    if ($('#hdnStartDate').val().trim().length == 0 && $('#hdnEndDate').val().trim().length == 0) {
        DisplayAlert($('#durationValid'), "<b>" + $(".fieldLabel label[data-field='Duration']").text() + "</b> is required and cannot be left blank.");
        hasError = true;
    }
    else if ($('#hdnStartDate').val().trim().length == 0 && $('#hdnEndDate').val().trim().length > 0) {
        DisplayAlert($('#durationValid'), "Must provide the contract start date.", $('#hdnStartDate'));
        hasError = true;
    }
    else if ($('#hdnStartDate').val().trim().length > 0 && $('#hdnEndDate').val().trim().length == 0) {
        DisplayAlert($('#durationValid'), "Must provide the contract end date.", $('#hdnStartDate'));
        hasError = true;
    }
    else {
        if (IsValidDate($('#hdnStartDate').val().trim()) == false) {
            DisplayAlert($('#durationValid'), "Start date is invalid!", $('#hdnStartDate'));
            hasError = true;
        }
        else if (IsValidDate($('#hdnEndDate').val().trim()) == false) {
            DisplayAlert($('#durationValid'), "End date is invalid!", $('#hdnEndDate'));
            hasError = true;
        }
        else {
            if ($('#durationValid').attr("hidden") == undefined)
                $('#durationValid').attr("hidden", "hidden");
        }
    }

    // Validate Work Duration
    var hourMax = GetIntValue($('#txtWorkHours').attr("max"));
    var minMax = GetIntValue($('#txtWorkMins').attr("max"));

    if (hourMax > 0 && GetIntValue($('#txtWorkHours').val()) > hourMax &&
       minMax > 0 && GetIntValue($('#txtWorkMins').val()) > minMax) {
        DisplayAlert($('#workDurationValid'), "The value entered in the work duration (hours & minutes) exceeded the maximum limit.", $('#txtWorkHours'));
        hasError = true;
    }
    else if (hourMax > 0 && GetIntValue($('#txtWorkHours').val()) <= hourMax &&
        minMax > 0 && GetIntValue($('#txtWorkMins').val()) > minMax) {
        DisplayAlert($('#workDurationValid'), "The value entered in the work duration (minutes) exceeded the maximum limit.", $('#txtWorkMins'));
        hasError = true;
    }
    else if (hourMax > 0 && GetIntValue($('#txtWorkHours').val()) > hourMax &&
        minMax > 0 && GetIntValue($('#txtWorkMins').val()) <= minMax) {
        DisplayAlert($('#workDurationValid'), "The value entered in the work duration (hours) exceeded the maximum limit.", $('#txtWorkHours'));
        hasError = true;
    }
    else {
        if ($('#workDurationValid').attr("hidden") == undefined)
            $('#workDurationValid').attr("hidden", "hidden");
    }
    // #endregion

    if (!hasError) {
        // Show loading panel
        ShowLoadingPanel(gContainer, 1, 'Updating data, please wait...');

        updateContractor();
    }
}

function DisplayAlert(obj, errText, focusObj) {
    var alert = $(obj).find(".alert");

    if ($(alert).find(".errorText") != undefined)
        $(alert).find(".errorText").html(errText);

    if (obj != undefined)
        $(obj).removeAttr("hidden");

    //if ($(alert).attr("hidden") == "hidden")
    //    $(alert).removeAttr("hidden");
    $(alert).show();

    if (focusObj != undefined)
        $(focusObj).focus();
}

function loadDataToControls(data) {
    if (data != null && data != undefined) {
        var objList = JSON.parse(data);
        var item;

        //#region Populate data to cost center autocomplete control
        var costCenterData = objList[0];
        if (costCenterData != undefined) {
            var cbo = $("#cboCostCenter");
            var optionValue = "";
            var optionText = "";
            
            // Add empty item
            optionValue = CONST_EMPTY;
            optionText = "";
            cbo.append(new Option(optionText, optionValue, true));

            for (var i = 0; i < costCenterData.length; i++) {
                optionValue = costCenterData[i].CostCenter;
                optionText = costCenterData[i].CostCenterFullName;
                cbo.append(new Option(optionText, optionValue));
            }
        }
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

            /*  Old code commented for reference
            var cont = document.getElementById("divLicense");

            // Resize the Remarks field based on the number of licenses
            //$("#txtRemarks").attr("rows", (licenseData.length + 2).toString())

            // Create ul element and set the attributes.
            var ul = document.createElement('ul');
            ul.setAttribute('style', 'padding: 0; margin: 0;');
            ul.setAttribute('id', 'licenseList');

            for (var i = 0; i < licenseData.length; i++) {
                var li = document.createElement('li');     // create li element.

                li.innerHTML = ProcessLicenseItem(licenseData[i].UDCDesc1, i);      // assigning text to li using array value.
                li.setAttribute('style', 'display: block;');    // remove the bullets.
                ul.appendChild(li);     // append li to ul.
            }

            cont.appendChild(ul);       // add list to the container.
            */
        }
        // #endregion

        //#region Populate data to employee autocomplete control
        var employeeData = objList[2];
        if (employeeData != null && employeeData != undefined) {
            var empArray = [];
            var empItem;

            for (var i = 0; i < employeeData.length - 1; i++) {
                item = employeeData[i];

                empItem = {
                    label: item.EmpNo + " - " + item.EmpName,
                    value: item.EmpNo,
                };

                // Add object to array
                empArray.push(empItem);
            }

            $("#txtSupervisor").autocomplete({
                source: empArray,           // Source should be Javascript array or object
                autoFocus: true,            // Set first item of the menu to be automatically focused when the menu is shown
                minLength: 2,               // The number of characters that must be entered before trying to obtain the matching values. By default its value is 1.
                delay: 300,                 // This option is an Integer representing number of milliseconds to wait before trying to obtain the matching values. By default its value is 300.
                select: function (event, ui) {
                    if (ui.item != null && ui.item != undefined) {
                        // Save the employee details to the hidden fields
                        $("#hidSupervisorNo").val(ui.item.value);
                        //$("#txtSupervisor").val(ui.item.label);

                        // Get the supervisor name
                        if (ui.item.label.length > 0) {
                            var supervisorArray = ui.item.label.trim().split("-");
                            if (supervisorArray != undefined) {
                                if (supervisorArray.length == 1) {
                                    $("#txtSupervisor").val(supervisorArray[0].trim());
                                }
                                else {
                                    $("#txtSupervisor").val(supervisorArray[1].trim());
                                }
                            }
                        }
                        return false;
                    }                                       
                },
                change: function (event, ui) {
                    if (ui.item == undefined || ui.item == null) {
                        $("#hidSupervisorNo").val("");
                        DisplayAlert($('#supervisorValid'), "The specified supervisor does not exist.", $('#txtSupervisor'));
                        $("#txtSupervisor").focus();
                    }
                    else
                        $('#supervisorValid').attr("hidden", "hidden");

                    return false;
                }
            });
            $("#txtSupervisor").autocomplete("enable");
        }
        //#endregion

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

        //#region Populate suplier list
        var supplierData = objList[4];
        if (supplierData != null && supplierData != undefined) {
            var supplierArray = [];
            var supplierItem;

            for (var i = 0; i < supplierData.length - 1; i++) {
                item = supplierData[i];

                supplierItem = {
                    label: item.SupplierName + " (Code: " + item.SupplierCode + ")",
                    value: item.SupplierCode
                };

                // Add object to array
                supplierArray.push(supplierItem);
            }

            $("#txtCompanyName").autocomplete({
                source: supplierArray,       // Source should be Javascript array or object
                autoFocus: true,        // Set first item of the menu to be automatically focused when the menu is shown
                minLength: 1,           // The number of characters that must be entered before trying to obtain the matching values. By default its value is 1.
                delay: 300,              // This option is an Integer representing number of milliseconds to wait before trying to obtain the matching values. By default its value is 300.
                select: function (event, ui) {
                    if (ui.item != null && ui.item != undefined) {
                        $("#hidCompanyID").val(ui.item.value);

                        // Get the Contractor company details
                        if (ui.item.label.length > 0) {
                            var idx = ui.item.label.trim().indexOf("(Code:");
                            if (idx > 0) {
                                var companyArray = ui.item.label.replace("(Code:", "|").split("|");
                                if (companyArray != undefined) {
                                    if (companyArray.length == 1) {
                                        $("#hidCompanyName").val(companyArray[0].trim());
                                        //contractorData.companyID = 0;
                                        //contractorData.companyName = companyArray[0].trim();
                                    }
                                    else {
                                        var supplierID = companyArray[1].trim().slice(0, companyArray[1].length - 2);
                                        //contractorData.companyID = isNaN(parseInt(supplierID)) ? 0 : parseInt(supplierID);
                                        //contractorData.companyName = companyArray[0].trim();
                                        $("#hidCompanyName").val(companyArray[0].trim());
                                    }
                                }
                            }
                            else {
                                //contractorData.companyID = null;
                                //contractorData.companyName = $("#hidCompanyName").val().trim();
                                $("#hidCompanyName").val("");
                            }
                        }

                        $("#txtCompanyName").val(ui.item.label);

                        // #region Populate the purchase order list
                        var supplierNo = GetFloatValue(ui.item.value);
                        if (supplierNo > 0) {
                            getPurchaseOrderList(supplierNo);
                        }
                        // #endregion
                        
                        return false;
                    }
                },
                change: function (event, ui) {
                    if (ui.item == undefined || ui.item == null) {
                        $("#hidCompanyID").val("");
                        $("#hidCompanyName").val("");

                        // Remove PO items
                        if ($("#cboPurchaseOrder").children().length > 0) {
                            $("#cboPurchaseOrder").children().remove().end();
                        }

                        if ($(this).val() != null && $(this).val() != "") {
                            DisplayAlert($('#companyNameValid'), "The specified company name is not yet registered in the system.", $(this));
                            $(this).focus();
                        }
                        else 
                            $('#companyNameValid').attr("hidden", "hidden");
                    }
                    else {
                        // Hide error message
                        $('#companyNameValid').attr("hidden", "hidden");
                    }

                    return false;
                }
            });
            $("#txtCompanyName").autocomplete("enable");
        }
        //#endregion

        //#region Populate Job Title master list
        var jobTitleList = objList[5];
        if (jobTitleList != undefined) {
            var cbo = $("#cboJobTitle");
            var optionValue = "";
            var optionText = "";

            // Add empty item
            optionValue = CONST_EMPTY;
            optionText = "";
            cbo.append(new Option(optionText, optionValue, true));

            for (var i = 0; i < jobTitleList.length; i++) {
                optionValue = jobTitleList[i].UDCCode;
                optionText = jobTitleList[i].UDCDesc1;
                cbo.append(new Option(optionText, optionValue));
            }
        }
        //#endregion

        HideLoadingPanel(gContainer);
    }
}

function ProcessLicenseItem(value, idx) {
    var itemtext = "";
    var itemID = "chkLicense" + idx;

    itemtext += "<div class='custom-control custom-checkbox pt-1'>";
    itemtext += "<input type='checkbox' class='custom-control-input' id='" + itemID + "' name='licenseType' />";
    itemtext += "<label class='custom-control-label fieldValue' for='" + itemID + "'>" + value + "</label>";
    itemtext += "</div>";

    return itemtext;
}

function loadContractorDetails(data) {
    HideLoadingPanel();

    var contractorData = JSON.parse(data);
    if (contractorData != null) {
        // Set Form Load flag
        gCurrentFormMode = formMode.LoadExistingRecord;

        $("#hidRegistryID").val(contractorData.registryID);

        if (contractorData.idType == 0)
            $("#lnkCPR").click();
        else
            $("#lnkPassport").click();
        $("#txtIDNo").val(contractorData.idNumber);
        $("#txtFirstName").val(contractorData.firstName);
        $("#txtLastName").val(contractorData.lastName);
        $("#txtCompanyCR").val(contractorData.companyCRNo);
        $("#txtCompanyName").val(contractorData.CompanyFullName);
        $("#hidCompanyName").val(contractorData.companyName);
        $("#hidCompanyID").val(contractorData.companyID);
        
        // #region Populate purchase order combobox items based on selected Company
        var supplierNo = GetFloatValue(contractorData.companyID);
        if (supplierNo > 0) {
            getPurchaseOrderList(supplierNo, contractorData.purchaseOrderNo);
        }
        // #endregion

        //$("#hidPONumber").val(contractorData.purchaseOrderNo);
        //$("#cboPurchaseOrder").val(contractorData.purchaseOrderNo);

        $("#cboJobTitle").val(contractorData.jobCode);
        $("#txtMobileNo").val(contractorData.mobileNo);
        $("#cboCostCenter").val(contractorData.visitedCostCenter);
        $("#cboBloodGroup").val(contractorData.bloodGroup);
        $("#txtSupervisor").val(contractorData.supervisorEmpName);
        $("#hidSupervisorNo").val(contractorData.supervisorEmpNo);
        $("#txtPurpose").val(contractorData.purposeOfVisit);
        $("#txtRemarks").val(contractorData.remarks);
        $("#txtContractStartDate").val(new Date(contractorData.contractStartDate).toLocaleDateString());
        $("#hdnStartDate").val($.fn.getISODate(contractorData.contractStartDate));
        $("#txtContractEndDate").val(new Date(contractorData.contractEndDate).toLocaleDateString());
        $("#hdnEndDate").val($.fn.getISODate(contractorData.contractEndDate));
        $("#txtRegistrationDate").val(new Date(contractorData.registrationDate).toLocaleDateString());
        $("#hdnRegisterDate").val($.fn.getISODate(contractorData.registrationDate));
        $("#txtWorkHours").val(contractorData.workDurationHours);
        $("#txtWorkMins").val(contractorData.workDurationMins);
        $("#txtContactNo").val(contractorData.companyContactNo);

        // #region Load license grid
        if (contractorData.licenseList != null && contractorData.licenseList != undefined) {
            for (var i = 0; i < contractorData.licenseList.length; i++) {
                licenseArray.push(contractorData.licenseList[i]);
            }
        }
        else {
            if (licenseArray.length > 0) 
                licenseArray.splice(0, licenseArray.length);
        }

        populateLicenseTable();
        // #endregion

        // Store data to session storage
        SaveDataToSession("contractorNo", contractorData.contractorNo);

        // Remove the focus to all input elements
        $(':focus').blur();
    }
}

function resetModalForm() {
    switch (gModalFormType) {
        case modalFormTypes.RegisterLicense:
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
    }
}

function addLicense() {
    try
    {
        var licenseItem = {
            licenseGUID: CreateGUID(),
            registryID: GetIntValue($("#hidRegistryID").val()),
            empNo: GetIntValue($("#txtContractorNo").val()),
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
            licenseArray.push(licenseItem);
                        
            // Refresh the frid
            populateLicenseTable();
        }
        else {
            ShowToastMessage(toastTypes.error, "The specified license information already exists or the period duration overlaps with existing record!", "Error Notification");
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
        var selectedLicense = licenseArray.find(function (value, index, array) {
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
                ShowToastMessage(toastTypes.error, "The specified license information already exists or the period duration overlaps with existing record!", "Error Notification");
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while saving the contractor license." + "\n\nError: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
}

function deleteLicense() {
    try {
        var selectedLicense = licenseArray.find(function (value, index, array) {
            return value.licenseGUID == $("#hidLicenseGUID").val().trim();
        });

        if (selectedLicense != undefined) {
            var position = licenseArray.indexOf(selectedLicense);

            if (position >= 0) {
                // Remove the item in the array
                licenseArray.splice(position, 1);

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
    try{
        // Check for duplicate record
        if (licenseArray != undefined && licenseArray.length > 0) {
            var duplicateLicense = licenseArray.find(function (value, index, array) {
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

function populateLicenseTable() {
    try
    {
        if (licenseArray == undefined || licenseArray.length == 0) {
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
                    data: licenseArray,
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
                                return '<a href="javascript:void(0)" class="lnklicenseNo gridLink" data-licenseguid=' + row.licenseGUID + '> ' + data + '</a>';
                            }
                            //createdCell: function (td, cellData, rowData, row, col) {
                            //    $(td).css('color', 'red');
                            //}
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
    gModalFormType = modalFormTypes.RegisterLicense;
    gModalFormLoadType = modalFormLoadTypes.UpdateRecord;

    // Get the selected license 
    var selectedLicense = licenseArray.find(function (value, index, array) {
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

function showAddLicenseForm() {
    // Set the flags
    gModalFormType = modalFormTypes.RegisterLicense;
    gModalFormLoadType = modalFormLoadTypes.AddNewRecord;

    //Reset the form
    resetModalForm();

    $("#modLicenseRegistration").modal({
        backdrop: "static",     // Cannot close the modal when clicking outside of it
        keyboard: true          // The modal can be closed with Esc
    });
}
// #endregion

// #region Database Methods
function getLookupTable() {
    $.ajax({
        type: "POST",
        url: "RegisterContractor.aspx/GetRegistrationLookup",
        //url: '<%=ResolveUrl("RegisterContractor.aspx/GetRegistrationLookup") %>',
        //url: "/WebService/ContractorWS.asmx/GetRegistrationLookup",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: "true",
        cache: "false",
        success: function (result) {
            if (result.d != null) {
                loadDataToControls(result.d);

                if (!isNaN(_contractorNo)) {
                    $("#txtContractorNo").val(_contractorNo);
                    $("#btnFind").click();
                }
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

function setContractorNo() {
    try {
        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "RegisterContractor.aspx/GetMaxContractorNo",
            //url: "/WebService/ContractorWS.asmx/GetMaxContractorNo",
            async: "true",
            cache: "false",
            success: function (result) {
                if (result.d > 0) {
                    $("#txtContractorNo").val(result.d.toString());
                }
                else {
                    $("#txtContractorNo").val("");
                }
            },
            error: function (err) {
                ShowErrorMessage("An error encountered while fetching the maximum contractor number." + "\n\nError: " + err.responseText);
            }
        });
    }
    catch (err) {
        throw err;
    }
}

function beginFindContractor() {
    try {
        var hasError = false;

        // Validate Contractor No.
        if ($('#txtContractorNo').val().trim().length == 0) {
            DisplayAlert($('#contractorNoValid'), $(".fieldLabel label[data-field='ContractorNo']").text() + " is required and cannot be left blank.", $('#txtContractorNo'));
            hasError = true;
        }
        else {
            if ($('#contractorNoValid').attr("hidden") == undefined)
                $('#contractorNoValid').attr("hidden", "hidden");
        }

        if (!hasError) {
            ShowLoadingPanel(gContainer, 1, 'Loading data, please wait...');

            // Clear license array
            if (licenseArray.length > 0)
                licenseArray.splice(0, licenseArray.length);

            // Reset the license grid
            var table = $("#licenseTable").dataTable().api();
            table.clear().draw();

            var contractorNo = isNaN(parseInt($("#txtContractorNo").val())) ? 0 : parseInt($("#txtContractorNo").val());

            // Call Web Service method using AJAX
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: "RegisterContractor.aspx/GetContractorDetails",
                data: JSON.stringify({ contractorNo: contractorNo }),
                async: "true",
                cache: "false",
                success: function (result) {
                    if (result.d != null) {
                        setFormDataLoaded();
                        loadContractorDetails(result.d);
                        HideLoadingPanel(gContainer);
                    }
                    else {
                        HideLoadingPanel(gContainer);
                        ShowToastMessage(toastTypes.error, "Unable to find a matching record for the contractor number you've specified. Please enter another one then try again!", "No Matching Record")
                    }
                },
                error: function (err) {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("An error encountered while fetching the contractor data." +
                        "\n\nError: " + err.responseText);                    
                }
            });
        }
    }
    catch (err) {
        throw err;
    }
}

function insertContractor() {
    try 
    {        
        var contractorData = {};
        
        // #region Populate the Contractor object
        contractorData.contractorNo = GetIntValue($("#txtContractorNo").val());
        contractorData.idNumber = $("#txtIDNo").val();
        contractorData.idType = $("input[id$='hidIDType'").val();
        contractorData.firstName = $("#txtFirstName").val();
        contractorData.lastName = $("#txtLastName").val();
        contractorData.companyCRNo = $("#txtCompanyCR").val();
        contractorData.purchaseOrderNo = GetFloatValue($("#cboPurchaseOrder").val()); //$("#txtPONumber").val().length > 0 ? GetFloatValue($("#txtPONumber").val()) : null;
        contractorData.jobTitle = $("#cboJobTitle").val();
        contractorData.mobileNo = $("#txtMobileNo").val();
        contractorData.visitedCostCenter = $("#cboCostCenter").val();
        contractorData.purposeOfVisit = $("#txtPurpose").val();
        contractorData.remarks = $("#txtRemarks").val();
        contractorData.bloodGroup = $("#cboBloodGroup").val();
        contractorData.createdByEmpNo = GetIntValue($("input[id$='hidCurrentUserEmpNo'").val());
        contractorData.createdByUser = $("input[id$='hidCurrentUserID'").val();                
        contractorData.registrationDateStr = ConvertToISODate($("#txtRegistrationDate").val());
        contractorData.contractStartDateStr = ConvertToISODate($("#txtContractStartDate").val());
        contractorData.contractEndDateStr = ConvertToISODate($("#txtContractEndDate").val());
        contractorData.workDurationHours = GetIntValue($("#txtWorkHours").val());
        contractorData.workDurationMins = GetIntValue($("#txtWorkMins").val());
        contractorData.companyContactNo = $("#txtContactNo").val();

        // Get the Contractor company details
        //if ($("#hidCompanyName").val().trim().length > 0) {
        //    var idx = $("#hidCompanyName").val().trim().indexOf("(Code:");
        //    if (idx > 0) {
        //        var companyArray = $("#hidCompanyName").val().trim().replace("(Code:", "|").split("|");
        //        if (companyArray != undefined) {
        //            if (companyArray.length == 1) {
        //                contractorData.companyID = 0;
        //                contractorData.companyName = companyArray[0].trim();
        //            }
        //            else {
        //                var supplierID = companyArray[1].trim().slice(0, companyArray[1].length - 2);
        //                contractorData.companyID = isNaN(parseInt(supplierID)) ? 0 : parseInt(supplierID);
        //                contractorData.companyName = companyArray[0].trim();
        //            }
        //        }
        //    }
        //    else {
        //        contractorData.companyID = null;
        //        contractorData.companyName = $("#hidCompanyName").val().trim();
        //    }
        //}
        contractorData.companyID = GetIntValue($("#hidCompanyID").val());
        contractorData.companyName = $("#hidCompanyName").val();

        // Get the Supervisor details
        //if ($("#txtSupervisor").val().trim().length > 0) {
        //    var supervisorArray = $("#txtSupervisor").val().trim().split("-");
        //    if (supervisorArray != undefined) {
        //        if (supervisorArray.length == 1) {
        //            contractorData.supervisorEmpNo = 0;
        //            contractorData.supervisorEmpName = supervisorArray[0].trim();
        //        }
        //        else {
        //            contractorData.supervisorEmpNo = isNaN(parseInt(supervisorArray[0])) ? 0 : parseInt(supervisorArray[0]);
        //            contractorData.supervisorEmpName = supervisorArray[1].trim();
        //        }
        //    }
        //}
        contractorData.supervisorEmpNo = GetIntValue($("#hidSupervisorNo").val());
        contractorData.supervisorEmpName = $("#txtSupervisor").val();

        // Save the licenses
        if (licenseArray != undefined && licenseArray.length > 0)
            contractorData.licenseList = licenseArray;
        // #endregion

        // Convert object to JSON
        var jsonData = JSON.stringify({
            contractorData: contractorData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "RegisterContractor.aspx/AddNewContractor",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result == CONST_SUCCESS) {
                    setFormDataLoaded();
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "The specfied contractor has been added successfully!", "Add Record Notification");

                    // Set Form Load flag
                    gCurrentFormMode = formMode.LoadExistingRecord;
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to save new contractor record due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while adding new contractor." +
                   "\n\nError: " + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while adding new contractor record.\n\n" + err);
    }
}

function updateContractor() {
    try {
        var contractorData = {};

        // #region Populate the Contractor object
        contractorData.registryID = GetIntValue($("#hidRegistryID").val());
        contractorData.contractorNo = GetIntValue($("#txtContractorNo").val());
        contractorData.idNumber = $("#txtIDNo").val();
        contractorData.idType = $("input[id$='hidIDType'").val();
        contractorData.firstName = $("#txtFirstName").val();
        contractorData.lastName = $("#txtLastName").val();        
        contractorData.companyCRNo = $("#txtCompanyCR").val();
        contractorData.purchaseOrderNo = GetFloatValue($("#cboPurchaseOrder").val()); //$("#txtPONumber").val().length > 0 ? GetFloatValue($("#txtPONumber").val()) : null;
        contractorData.jobTitle = $("#cboJobTitle").val();
        contractorData.mobileNo = $("#txtMobileNo").val();
        contractorData.visitedCostCenter = $("#cboCostCenter").val();
        contractorData.purposeOfVisit = $("#txtPurpose").val();
        contractorData.registrationDateStr = ConvertToISODate($("#txtRegistrationDate").val());
        contractorData.contractStartDateStr = ConvertToISODate($("#txtContractStartDate").val());
        contractorData.contractEndDateStr = ConvertToISODate($("#txtContractEndDate").val());
        contractorData.remarks = $("#txtRemarks").val();
        contractorData.bloodGroup = $("#cboBloodGroup").val();
        contractorData.lastUpdatedByEmpNo = GetIntValue($("input[id$='hidCurrentUserEmpNo'").val());
        contractorData.lastUpdatedByUser = $("input[id$='hidCurrentUserID'").val();
        contractorData.workDurationHours = GetIntValue($("#txtWorkHours").val());
        contractorData.workDurationMins = GetIntValue($("#txtWorkMins").val());
        contractorData.companyContactNo = $("#txtContactNo").val();

        // Get the Contractor company details
        //if ($("#hidCompanyName").val().trim().length > 0) {
        //    var idx = $("#hidCompanyName").val().trim().indexOf("(Code:");
        //    if (idx > 0) {
        //        var companyArray = $("#hidCompanyName").val().trim().replace("(Code:", "|").split("|");
        //        if (companyArray != undefined) {
        //            if (companyArray.length == 1) {
        //                contractorData.companyID = 0;
        //                contractorData.companyName = companyArray[0].trim();
        //            }
        //            else {
        //                var supplierID = companyArray[1].trim().slice(0, companyArray[1].length - 2);
        //                contractorData.companyID = isNaN(parseInt(supplierID)) ? 0 : parseInt(supplierID);
        //                contractorData.companyName = companyArray[0].trim();
        //            }
        //        }
        //    }
        //    else {
        //        contractorData.companyID = null;
        //        contractorData.companyName = $("#hidCompanyName").val().trim();
        //    }
        //}

        contractorData.companyID = GetIntValue($("#hidCompanyID").val());
        contractorData.companyName = $("#hidCompanyName").val();

        // Get the Supervisor details
        //if ($("#txtSupervisor").val().trim().length > 0) {
        //    var supervisorArray = $("#txtSupervisor").val().trim().split("-");
        //    if (supervisorArray != undefined) {
        //        if (supervisorArray.length == 1) {
        //            contractorData.supervisorEmpNo = 0;
        //            contractorData.supervisorEmpName = supervisorArray[0].trim();
        //        }
        //        else {
        //            contractorData.supervisorEmpNo = isNaN(parseInt(supervisorArray[0])) ? 0 : parseInt(supervisorArray[0]);
        //            contractorData.supervisorEmpName = supervisorArray[1].trim();
        //        }
        //    }
        //}
        contractorData.supervisorEmpNo = GetIntValue($("#hidSupervisorNo").val());
        contractorData.supervisorEmpName = $("#txtSupervisor").val();

        // Save the licenses
        if (licenseArray != undefined && licenseArray.length > 0)
            contractorData.licenseList = licenseArray;
        // #endregion

        // Convert object to JSON
        var jsonData = JSON.stringify({
            contractorData: contractorData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "RegisterContractor.aspx/UpdateContractor",
            //url: "/WebService/ContractorWS.asmx/UpdateContractor",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result == CONST_SUCCESS) {
                    setFormDataLoaded();
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "Contractor record has been updated successfully!", "Update Notification");

                    // Set Form Load flag
                    gCurrentFormMode = formMode.LoadExistingRecord;
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to update the contractor record due to the following error:\n\n" + result);
                    //ShowToastMessage(toastTypes.error, "An unknown error has occured while updating the contractor record. Please check with ICT!", "Update Contractor Failure");
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while updating the contractor record.\n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while updating the contractor record.\n\n" + err);
    }
}

function deleteContractor() {
    try {
        var contractorData = {};

        // #region Populate the Contractor object
        contractorData.registryID = $.fn.getIntValue($("#hidRegistryID").val());
        contractorData.contractorNo = $.fn.getIntValue($("#txtContractorNo").val());        
        contractorData.idNumber = $("#txtIDNo").val().trim();
        contractorData.idType = $("input[id$='hidIDType'").val().trim();
        contractorData.firstName = $("#txtFirstName").val().trim();
        contractorData.lastName = $("#txtLastName").val().trim();
        contractorData.companyCRNo = $("#txtCompanyCR").val().trim();
        contractorData.purchaseOrderNo = GetFloatValue($("#cboPurchaseOrder").val()); //$("#txtPONumber").val().length > 0 ? GetFloatValue($("#txtPONumber").val()) : null;
        contractorData.jobTitle = $("#cboJobTitle").val();
        contractorData.mobileNo = $("#txtMobileNo").val().trim();
        contractorData.visitedCostCenter = $("#cboCostCenter").val().trim();
        contractorData.purposeOfVisit = $("#txtPurpose").val().trim();        
        contractorData.remarks = $("#txtRemarks").val().trim();
        contractorData.bloodGroup = $("#cboBloodGroup").val().trim();
        contractorData.lastUpdatedByEmpNo = $("input[id$='hidCurrentUserEmpNo'").val().trim();
        contractorData.lastUpdatedByUser = $("input[id$='hidCurrentUserID'").val().trim();
        contractorData.registrationDateStr = ConvertToISODate($("#txtRegistrationDate").val());
        contractorData.contractStartDateStr = ConvertToISODate($("#txtContractStartDate").val());
        contractorData.contractEndDateStr = ConvertToISODate($("#txtContractEndDate").val());
        contractorData.workDurationHours = GetIntValue($("#txtWorkHours").val());
        contractorData.workDurationMins = GetIntValue($("#txtWorkMins").val());
        contractorData.companyContactNo = $("#txtContactNo").val();

        // Get the Contractor company details
        //if ($("#hidCompanyName").val().trim().length > 0) {
        //    var idx = $("#hidCompanyName").val().trim().indexOf("(Code:");
        //    if (idx > 0) {
        //        var companyArray = $("#hidCompanyName").val().trim().replace("(Code:", "|").split("|");
        //        if (companyArray != undefined) {
        //            if (companyArray.length == 1) {
        //                contractorData.companyID = 0;
        //                contractorData.companyName = companyArray[0].trim();
        //            }
        //            else {
        //                var supplierID = companyArray[1].trim().slice(0, companyArray[1].length - 2);
        //                contractorData.companyID = isNaN(parseInt(supplierID)) ? 0 : parseInt(supplierID);
        //                contractorData.companyName = companyArray[0].trim();
        //            }
        //        }
        //    }
        //    else {
        //        contractorData.companyID = null;
        //        contractorData.companyName = $("#hidCompanyName").val().trim();
        //    }
        //}
        contractorData.companyID = GetIntValue($("#hidCompanyID").val());
        contractorData.companyName = $("#hidCompanyName").val();

        // Get the Supervisor details
        //if ($("#txtSupervisor").val().trim().length > 0) {
        //    var supervisorArray = $("#txtSupervisor").val().trim().split("-");
        //    if (supervisorArray != undefined) {
        //        if (supervisorArray.length == 1) {
        //            contractorData.supervisorEmpNo = 0;
        //            contractorData.supervisorEmpName = supervisorArray[0].trim();
        //        }
        //        else {
        //            contractorData.supervisorEmpNo = isNaN(parseInt(supervisorArray[0])) ? 0 : parseInt(supervisorArray[0]);
        //            contractorData.supervisorEmpName = supervisorArray[1].trim();
        //        }
        //    }
        //}
        contractorData.supervisorEmpNo = GetIntValue($("#hidSupervisorNo").val());
        contractorData.supervisorEmpName = $("#txtSupervisor").val();
        // #endregion

        // Convert object to JSON
        var jsonData = JSON.stringify({
            contractorData: contractorData
        });

        // Call Web Service method using AJAX
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "RegisterContractor.aspx/DeleteContractor",
            //url: "/WebService/ContractorWS.asmx/DeleteContractor",
            data: jsonData,
            async: "true",
            cache: "false",
            success: function (response) {
                var result = response.d;
                if (result == CONST_SUCCESS) {
                    resetForm();
                    HideLoadingPanel(gContainer);
                    ShowToastMessage(toastTypes.success, "The selected contractor record has been deleted successfully!", "Delete Notification");
                    
                    // Set Form Load flag
                    gCurrentFormMode = formMode.ClearForm;
                }
                else {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("Unable to delete the selected contractor record due to the following error:\n\n" + result);
                }
            },
            error: function (err) {
                HideLoadingPanel(gContainer);
                ShowErrorMessage("The following error has occured while deleting the contractor record." + "\n\nError: " + err.responseText);
            }
        });
    }
    catch (err) {
        HideLoadingPanel(gContainer);
        ShowErrorMessage("The following error has occured while deleting the contractor record.\n\n" + err);
    }
}

function validatePONumber(poNumber) {
    try {

        // Initialize object
        purchaseOrderData.PONumber =  0;
        purchaseOrderData.HasError = false;
        purchaseOrderData.ErrorDescription = "";

        // Hide error message
        $('#poNumberValid').attr("hidden", "hidden");

        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "RegisterContractor.aspx/GetPurchaseOrderDetails",
            data: JSON.stringify({ poNumber: poNumber }),
            async: "true",
            cache: "false",
            success: function (result) {
                if (result.d != null) {
                    var poData = JSON.parse(result.d);

                    // Save PO number to the hidden field
                    $("#hidPONumber").val(poNumber);

                    if (poData.StatusHandlingCode == "Cancelled" || poData.StatusHandlingCode == "Rejected") {
                        purchaseOrderData.HasError = true;
                        purchaseOrderData.ErrorDescription = "Cannot use a <b>Rejected</b> or <b>Cancelled</b> purchase order.",
                        purchaseOrderData.PONumber = poNumber
                    }
                    else if (poData.SupplierNo != $("#hidCompanyID").val()) {
                        purchaseOrderData.HasError = true;
                        purchaseOrderData.ErrorDescription = "The specified PO number is not associated with the contractor\'s Company Name.",
                        purchaseOrderData.PONumber = poNumber
                    }
                }
                else {
                    // Save PO number to the hidden field
                    $("#hidPONumber").val(poNumber);

                    purchaseOrderData.HasError = true;
                    purchaseOrderData.ErrorDescription = "Could not find a matching record for the specified PO number.",
                    purchaseOrderData.PONumber = poNumber
                }
            },
            error: function (err) {
                ShowErrorMessage("The following error has occured while fetching the purchase order data. \n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        throw err;
    }
}

function getPurchaseOrderList(supplierNo, purchaseOrderNo) {
    try {
        // Remove all items
        if ($("#cboPurchaseOrder").children().length > 0) {
            $("#cboPurchaseOrder").children().remove().end();
        }

        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            url: "RegisterContractor.aspx/GetPurchaseOrderList",
            data: JSON.stringify({ supplierNo: supplierNo }),
            async: "true",
            cache: "false",
            success: function (result) {
                if (result.d != null) {
                    var poList = JSON.parse(result.d);
                    if (poList.length > 0) {
                        var cbo = $("#cboPurchaseOrder");
                        var optionValue = "";
                        var optionText = "";

                        // Add empty item
                        optionValue = CONST_EMPTY;
                        optionText = "";
                        cbo.append(new Option(optionText, optionValue, true));

                        for (var i = 0; i < poList.length; i++) {
                            optionValue = poList[i].PONumber;
                            optionText = poList[i].PurchaseOrderDetails;
                            cbo.append(new Option(optionText, optionValue));
                        }

                        if (purchaseOrderNo != undefined && purchaseOrderNo != null)
                            $("#cboPurchaseOrder").val(purchaseOrderNo);
                    }
                }
            },
            error: function (err) {
                ShowErrorMessage("The following error has occured while fetching the purchase order list. \n\n" + err.responseText);
            }
        });
    }
    catch (err) {
        throw err;
    }
}
// #endregion

