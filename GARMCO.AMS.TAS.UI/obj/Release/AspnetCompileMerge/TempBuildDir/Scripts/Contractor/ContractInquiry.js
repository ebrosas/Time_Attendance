// #region Declare Constants
const CONST_ALL = "valAll";
const CONST_EMPTY = "valEmpty";
const CONST_SUCCESS = "SUCCESS";
const CONST_FAILED = "FAILED";

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

const licenseArray = [];

// #endregion

// #region Declare Variables
var _modalFormType;
var _modalFormLoadType;
var _modalResponse;
var _callerForm;
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

        HideLoadingPanel(gContainer);
        
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
        //#endregion

        //#region Show tooltips
        $("#btnCreateNew").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to register new contractor.</span>",
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
        $("#btnBack").popover({
            title: "<span class='text-info font-weight-bold'>Tips!</span>",
            content: "<span class='text-dark'>Click this button to go back to previous page.</span>",
            placement: "top",
            trigger: "hover",
            animation: true,
            html: true
        });
        //#endregion

        // #region Initialize event handlers
        $(".actionButton").on("click", handleActionButtonClick);
        $(".formButton").on("click", handleActionButtonClick);

        $(".groupTitle .linkTitle").click(function () {
            if (_callerForm != undefined && _callerForm.length > 0)
                location.href = _callerForm;
            else
                location.href = formURLs.ContractorRegistration.concat("?callerForm=").concat(formURLs.ContractorInquiry);
        });
        $("#txtContractorNo, #txtContractorName, #txtJobTitle, #txtIDNo, #txtCompanyName, #txtSupervisor").on(
            {
                keypress : function (event) {
                    var ASCIICode = event.which || event.keyCode
                    if (ASCIICode == 13) {      // Enter key 
                        $("#btnSearch").click();
                    }
                }
            }
        );

        $("#mainForm").keydown(function (event) {
            var ASCIICode = event.keyCode
            if (ASCIICode == 113) {     //F2 key
                alert("This application was developed by Ervin Brosas");
            }
        });

        // Modal form events
        $(".modal-footer > button").on("click", handleModalButtonClick);
        $(".modal").on("show.bs.modal", handleShowModalForm);
        $(".modal").on("hide.bs.modal", handleHideModalForm);
        //#endregion
                
        // #region Initialize controls
        $("#txtContractorNo").attr("onkeypress", "return OnlyNumberKey(event)");    // Allow numbers only
        // #endregion

        ShowLoadingPanel(gContainer, 1, 'Initalizing form, please wait...');        
        
        // Initialize user form access
        GetUserFormAccess($("#hidFormCode").val().trim(), $("#hidCostCenter").val().trim(), GetIntValue($("#hidCurrentUserEmpNo").val()));

        getLookupTable();
        resetForm();

        // Check if there was saved search criteria
        var filterData = GetDataFromSession("searchCriteria");
        if (!CheckIfNoValue(filterData)) {
            var filterArray = JSON.parse(filterData);
            var contractorNo = parseInt(filterArray.contractorNo);
            if (isNaN(contractorNo) || contractorNo == 0)
                $("#txtContractorNo").val("");
            else
                $("#txtContractorNo").val(contractorNo);

            $("#txtIDNo").val(GetStringValue(filterArray.idNumber));
            $("#txtContractorName").val(filterArray.contractorName);
            $("#txtCompanyName").val(filterArray.companyName);
            $("#cboCostCenter").val(filterArray.costCenter);
            $("#txtJobTitle").val(filterArray.jobTitle);
            $("#txtSupervisor").val(filterArray.supervisorName);

            if (IsValidDate(filterArray.contractStartDateStr))
                $("#txtContractStartDate").val(new Date($.fn.getISODate(filterArray.contractStartDateStr)).toLocaleDateString());
            else
                $("#txtContractStartDate").val("");

            if (IsValidDate(filterArray.contractEndDateStr))
                $("#txtContractEndDate").val(new Date($.fn.getISODate(filterArray.contractEndDateStr)).toLocaleDateString());
            else
                $("#txtContractEndDate").val("");

            _bypassSecurity = true;
            $("#btnSearch").click();
        }
    }
    catch (err) {
        ShowErrorMessage("The following exception has occured while loading the page: " + err)
        //HideLoadingPanel(gContainer);
    }
});

// #region Event Handlers
function handleActionButtonClick() {
    var btn = $(this);

    // Hide all error messages
    HideErrorMessage();
    HideToastMessage();

    switch ($(btn)[0].id) {
        case "btnSearch":
            if (_bypassSecurity) {
                _bypassSecurity = false;
                beginSearchContractor();
            }
            else {
                if (gUserFormAccess.UserFrmCRUDP != undefined) {
                    if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Retrieve))
                        beginSearchContractor();
                    else
                        ShowToastMessage(toastTypes.error, CONST_RETRIEVE_DENIED, "Access Denied");
                }
            }
            break;

        case "btnReset":
            // Reset session
            DeleteDataFromSession("searchCriteria");
            resetForm();
            break;

        case "btnPrint":
            if (gUserFormAccess.UserFrmCRUDP != undefined) {
                if (HasAccess(gUserFormAccess.UserFrmCRUDP, FormAccessIndex.Retrieve))
                    beginPrintReport();
                else
                    ShowToastMessage(toastTypes.error, CONST_PRINT_DENIED, "Access Denied");
            }
            break;

        case "btnBack":
            ShowLoadingPanel(gContainer, 1, 'Going back to previous page, please...');
            if (_callerForm != undefined && _callerForm.length > 0)
                location.href = _callerForm;
            else
                location.href = formURLs.ContractorRegistration.concat("?isback=true").concat("&callerForm=").concat(formURLs.ContractorInquiry);
            break;
    }
}

function handleHideModalForm() {
    if (_modalResponse != undefined && _modalFormType != undefined) {
        switch (_modalFormType) {
            case modalFormTypes.DeleteConfirmation:
                if (_modalResponse == modalResponseTypes.ModalYes) {
                    deleteContractor();
                }
                break;

            case modalFormTypes.RegisterLicense:
                if (_modalResponse == modalResponseTypes.ModalSave ||
                    _modalResponse == modalResponseTypes.ModalDelete) {
                    resetModalForm();
                }
                break;
        }
    }
}

function handleShowModalForm() {
    switch (_modalFormType) {
        case modalFormTypes.RegisterLicense:
            if (_modalFormLoadType == modalFormLoadTypes.AddNewRecord) {
                if (!$(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").attr("hidden", "hidden");
            }
            else if (_modalFormLoadType == modalFormLoadTypes.OpenExistingRecord || _modalFormLoadType == modalFormLoadTypes.UpdateRecord) {
                if ($(".modal-footer button[data-button-value$='modalDelete'").hasAttr("hidden"))
                    $(".modal-footer button[data-button-value$='modalDelete'").removeAttr("hidden");
            }
            break;
    }
}

function handleModalButtonClick() {
    var btnAttrib = $(this).attr("data-button-value");

    if (btnAttrib == modalResponseTypes.ModalYes)
        _modalResponse = modalResponseTypes.ModalYes;

    else if (btnAttrib == modalResponseTypes.ModalYes)
        _modalResponse = modalResponseTypes.ModalNo;

    else if (btnAttrib == modalResponseTypes.ModalCancel)
        _modalResponse = modalResponseTypes.ModalCancel;

    else if (btnAttrib == modalResponseTypes.ModalDelete) {
        _modalResponse = modalResponseTypes.ModalDelete;
        if (beginDeleteLicense()) {
            $("#modLicenseRegistration").modal("hide");
            gContainer = $('.formWrapper');

            // Show success message
            ShowToastMessage(toastTypes.success, "License record has been deleted successfully!", "Delete License Notification");
        }
    }

    else if (btnAttrib == modalResponseTypes.ModalSave) {
        _modalResponse = modalResponseTypes.ModalSave;
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
function beginPrintReport() {
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
            //swal("Click on either the button or outside the modal.")
            //.then((value) => {
            //    swal(`The returned value is: ${value}`);
            //});

            
            //swal(
            //    {
            //        title: "Alert",
            //        text: "Hello world!",
            //        icon: "warning"
            //    });
            
            //swal({
            //    title: "Are you sure?",
            //    text: "Once deleted, you will not be able to recover this imaginary file!",
            //    icon: "warning",
            //    buttons: true,
            //    dangerMode: true,
            //})
            //.then((willDelete) => {
            //    if (willDelete) {
            //        swal("Poof! Your imaginary file has been deleted!", {
            //            icon: "success",
            //        });
            //    } else {
            //        swal("Your imaginary file is safe!");
            //    }
            //});

            //alert("Printing report is under construction");

            //ShowLoadingPanel(gContainer, 1, 'Previewing contractor report, please wait...');

            //var contractorNo = isNaN(parseInt($("#txtContractorNo").val())) ? 0 : parseInt($("#txtContractorNo").val());

            //// Call Web Service method using AJAX
            //$.ajax({
            //    type: "POST",
            //    dataType: "json",
            //    contentType: "application/json; charset=utf-8",
            //    url: "/WebService/ContractorWS.asmx/GetContractorDetails",
            //    data: JSON.stringify({ contractorNo: contractorNo }),
            //    async: "true",
            //    cache: "false",
            //    success: function (result) {
            //        if (result.d != null) {
            //            setFormDataLoaded();
            //            loadContractorDetails(result.d);
            //            HideLoadingPanel(gContainer);
            //        }
            //        else {
            //            ShowErrorMessage("Unable to find a matching record with the Contractor No. you have specified. Please enter another number then try again!");
            //            HideLoadingPanel(gContainer);
            //        }
            //    },
            //    error: function (err) {
            //        ShowErrorMessage("An error encountered while fetching the contractor data." +
            //            "\n\nError: " + err.responseText);
            //        HideLoadingPanel(gContainer);
            //    }
            //});
        }
    }
    catch (err) {
        throw err;
    }
}

function beginGenerateCard() {
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
           

            //alert("Generate ID Card is under construction");

            //ShowLoadingPanel(gContainer, 1, 'Previewing contractor report, please wait...');

            //var contractorNo = isNaN(parseInt($("#txtContractorNo").val())) ? 0 : parseInt($("#txtContractorNo").val());

            //// Call Web Service method using AJAX
            //$.ajax({
            //    type: "POST",
            //    dataType: "json",
            //    contentType: "application/json; charset=utf-8",
            //    url: "/WebService/ContractorWS.asmx/GetContractorDetails",
            //    data: JSON.stringify({ contractorNo: contractorNo }),
            //    async: "true",
            //    cache: "false",
            //    success: function (result) {
            //        if (result.d != null) {
            //            setFormDataLoaded();
            //            loadContractorDetails(result.d);
            //            HideLoadingPanel(gContainer);
            //        }
            //        else {
            //            ShowErrorMessage("Unable to find a matching record with the Contractor No. you have specified. Please enter another number then try again!");
            //            HideLoadingPanel(gContainer);
            //        }
            //    },
            //    error: function (err) {
            //        ShowErrorMessage("An error encountered while fetching the contractor data." +
            //            "\n\nError: " + err.responseText);
            //        HideLoadingPanel(gContainer);
            //    }
            //});
        }
    }
    catch (err) {
        throw err;
    }
}

function setModalTitle() {
    if (_modalFormType == undefined)
        return;

    switch (_modalFormType) {
        case modalFormTypes.DeleteConfirmation:
            $(".modal-body > p").html("&nbsp;&nbsp;&nbsp;Are you sure you want to <span class='text-info font-weight-bold'>DELETE</span> the selected record?");
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
    $("#btnDelete").popover("hide");
    $("#btnReset").popover("hide");
    $("#btnPrint").popover("hide");
    $("#btnBack").popover("hide");

    // Enable/Disable buttons
    $("#btnPrint").removeAttr("disabled");
    $("#btnGenerateCard").removeAttr("disabled");
    $("#btnBack").removeAttr("disabled");

    // Show/Hide buttons
    $("#btnHiddenReset").click();
    if (_callerForm != "undefined" && _callerForm != null) {
        $(".groupTitle .linkTitle").attr("hidden", "hidden");
        $("#btnBack").removeAttr("hidden");
    }

    // Reset DataTable
    populateDataTable();

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

    // Enable "Create New" button
    //if ($("#btnCreateNew").hasClass("btn-outline-secondary"))
    //    $("#btnCreateNew").removeClass("btn-outline-secondary")

    //$("#btnCreateNew").addClass("btn-primary").addClass("border-0");
    //$("#btnCreateNew").removeAttr("disabled");

    // Enable/Disable buttons
    $("#btnPrint").removeAttr("disabled");
    $("#btnGenerateCard").removeAttr("disabled");
    $("#btnBack").removeAttr("disabled");
    $("#btnFind").removeAttr("disabled");

    // Show and Hide buttons
    $("#btnSave").attr("hidden", "hidden");
    $("#btnUpdate").removeAttr("hidden");
    $("#btnDelete").removeAttr("hidden");

    $("#optPassport").attr("hidden", "hidden");        
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
    if (data != null && data != "undefined") {
        var objList = JSON.parse(data);
        var item;

        //#region Populate data to cost center autocomplete control
        var costCenterData = objList[0];
        if (costCenterData != "undefined") {
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

        //#region Populate data to employee autocomplete control
        var employeeData = objList[2];
        if (employeeData != null && employeeData != "undefined") {
            var empArray = [];
            var empItem;

            for (var i = 0; i < employeeData.length - 1; i++) {
                item = employeeData[i];

                empItem = {
                    label: item.EmpNo + " - " + item.EmpName,
                    value: item.EmpName,
                };

                // Add object to array
                empArray.push(empItem);
            }

            $("#txtSupervisor").autocomplete({
                source: empArray,    // Source should be Javascript array or object
                autoFocus: true,            // Set first item of the menu to be automatically focused when the menu is shown
                minLength: 2,               // The number of characters that must be entered before trying to obtain the matching values. By default its value is 1.
                delay: 300                  // This option is an Integer representing number of milliseconds to wait before trying to obtain the matching values. By default its value is 300.
            });
            $("#txtSupervisor").autocomplete("enable");
        }
        //#endregion

        //#region Populate suplier list
        var supplierData = objList[4];
        if (supplierData != null && supplierData != "undefined") {
            var supplierArray = [];
            var supplierItem;

            for (var i = 0; i < supplierData.length - 1; i++) {
                item = supplierData[i];

                supplierItem = {
                    label: item.SupplierName, //+ " (Code: " + item.SupplierCode + ")",
                    value: item.SupplierName //+ " (Code: " + item.SupplierCode + ")"
                };

                // Add object to array
                supplierArray.push(supplierItem);
            }

            $("#txtCompanyName").autocomplete({
                source: supplierArray,       // Source should be Javascript array or object
                autoFocus: true,        // Set first item of the menu to be automatically focused when the menu is shown
                minLength: 1,           // The number of characters that must be entered before trying to obtain the matching values. By default its value is 1.
                delay: 300              // This option is an Integer representing number of milliseconds to wait before trying to obtain the matching values. By default its value is 300.
            });
            $("#txtCompanyName").autocomplete("enable");
        }
        //#endregion

        HideLoadingPanel(gContainer);
    }
}

function resetModalForm() {
    switch (_modalFormType) {
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

function populateDataTable(data) {
    try
    {
        if (data == "undefined" || data == null || data == "") {
            // Get DataTable API instance
            var table = $("#contractorTable").dataTable().api();
            table.clear().draw();
            HideLoadingPanel(gContainer);
            return;
        }

        var reportTitle = "Contractors Summary Report";
        var dataset = JSON.parse(data);
        if (dataset == "undefined" || dataset == null) {
            // Get DataTable API instance
            var table = $("#contractorTable").dataTable().api();
            table.clear().draw();
            HideLoadingPanel(gContainer);
        }
        else {
            $("#contractorTable")
                .on('init.dt', function () {    // This event will fire after loading the data in the table
                    HideLoadingPanel(gContainer);
                })
                .DataTable({
                    data: dataset,
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
                        $('.lnkContractorNo').on('click', openContractorRecord);
                    },
                    //dom: 'Bfrtip',
                    dom: "<'row' <'col-sm-5'l> <'col-sm-4 text-center'B> <'col-sm-3'f> >" +
                       "<'row'<'col-sm-12 col-md-12'tr>>" +
                       "<'row'<'col-xs-12 col-sm-5 col-md-5'i><'col-xs-12 col-sm-7 col-md-7'p>>",
                    buttons: [
                       {
                           text: '<i class="fas fa-file-excel fa-lg fa-fw"></i>',
                           extend: 'excel',
                           className: 'btn btn-info tableButton',
                           titleAttr: 'Export results to Excel',
                           title: function () {
                               return reportTitle;
                           }
                       },
                        {
                            text: '<i class="fas fa-file-pdf fa-lg fa-fw"></i>',
                            extend: 'pdf',
                            className: 'btn btn-info tableButton',
                            titleAttr: 'Export results to PDF',
                            title: function () {
                                return reportTitle;
                            }
                        },
                        {
                            text: '<i class="fas fa-print fa-lg fa-fw"></i>',
                            extend: 'print',
                            className: 'btn btn-info tableButton',
                            titleAttr: 'Print results',
                            title: function () {
                                return reportTitle;
                            }
                        }
                    ],
                    columns: [
                        {
                            "data": "contractorNo"
                        },
                        {
                            "data": "firstName",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            "data": "lastName",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            "data": "companyName",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            "data": "companyContactNo",
                            render: $.fn.dataTable.render.ellipsis(30, true, true)
                        },
                        {
                            "data": "jobTitle",
                             render: $.fn.dataTable.render.ellipsis(40, true, true)
                        },
                        {
                            data: "contractStartDate",
                            render: function (data, type, row) {
                                return moment(data).format('DD-MMM-YYYY');
                            }
                        },
                        {
                            data: "contractEndDate",
                            render: function (data, type, row) {
                                return moment(data).format('DD-MMM-YYYY');
                            }
                        },
                        {
                            "data": "workDuration"
                        },
                        {
                            "data": "idNumber",
                            render: $.fn.dataTable.render.ellipsis(20, true, true)
                        },
                        {
                            "data": "idTypeDesc",
                            render: $.fn.dataTable.render.ellipsis(20, true, true)
                        },
                        {
                            "data": "mobileNo",
                            render: $.fn.dataTable.render.ellipsis(20, true, true)
                        },
                        {
                            "data": "purchaseOrderNo"
                        },
                        {
                            "data": "visitedCostCenterName"
                        },
                        {
                            "data": "purposeOfVisit",
                            render: $.fn.dataTable.render.ellipsis(50, true, true)
                        },
                         {
                             "data": "supervisorEmpName"
                         },
                        {
                            data: "registrationDate",
                             render: function (data, type, row) {
                                 return moment(data).format('DD-MMM-YYYY');
                             }
                        },
                        {
                            data: "createdDate",
                            render: function (data, type, row) {
                                return moment(data).format('DD-MMM-YYYY');
                            }
                        },
                        {
                            data: "createdByEmpName"
                        },
                        {
                            data: "registryID"
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
                                return '<a href="javascript:void(0)" title="Click here to open the contractor details." class="lnkContractorNo gridLink" data-registryid=' + row.registryID + '> ' + data + '</a>';
                            }
                        },
                        {
                            targets: "hiddenColumn",
                            visible: false
                        },
                        {
                            targets: 11,    // Visited Cost Center
                            render: function (data, type, row) {
                                if (row.visitedCostCenter.length > 0)
                                    return row.visitedCostCenter + ' - ' + row.visitedCostCenterName;
                                else
                                    return ""; 
                            }
                        },
                        {
                            targets: 13,    // Supervisor In-charge
                            render: function (data, type, row) {
                                if (row.supervisorEmpNo > 0)
                                    return row.supervisorEmpNo + ' - ' + row.supervisorEmpName;
                                else
                                    return row.supervisorEmpName;
                            }
                        },
                        {
                            targets: 16,    // Created By
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
                    ],
                    order: []
                });
        }
    }
    catch (err) {
        ShowErrorMessage("The following error has occured while populating the data into the grid.\n\n" + err);
    }
}

function openContractorRecord() {
    var contractorNo = parseInt($(this).text());
    var registryID = $(this).attr("data-registryid").trim();

    if (contractorNo == 0 || isNaN(contractorNo)) {
        ShowErrorMessage("Unable to load the details of the selected contractor record!");
        return;
    }

    ShowLoadingPanel(gContainer, 1, 'Opening Contractor Registration form, please wait...');

    // Save contractor no. to sessopn storage
    SaveDataToSession("contractorNo", contractorNo);

    if (_callerForm != undefined && _callerForm.length > 0)
        location.href = _callerForm.concat("?contractorNo=" + contractorNo);    
    else
        location.href = formURLs.ContractorRegistration.concat("?contractorNo=").concat(contractorNo).concat("&callerForm=").concat(formURLs.ContractorInquiry);
}
// #endregion

// #region Database Methods
function getLookupTable() {
    $.ajax({
        type: "POST",
        url: "ContractorInquiry.aspx/GetRegistrationLookup",
        //url: "/WebService/ContractorWS.asmx/GetRegistrationLookup",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: "true",
        cache: "false",
        success: function (result) {
            if (result.d != null) {
                loadDataToControls(result.d);
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

function beginSearchContractor() {
    try {
        var hasError = false;

        // Hide all error alerts
        $('.errorPanel').attr("hidden", "hidden");
        HideErrorMessage();

        // Validate Contract Duration
        if ($('#txtContractStartDate').val().trim().length > 0 || $('#txtContractEndDate').val().trim().length > 0) {
            if ($('#txtContractStartDate').val().trim().length == 0 && $('#txtContractEndDate').val().trim().length > 0) {
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
        }

        if (!hasError) {
            ShowLoadingPanel(gContainer, 1, 'Searching contractor record, please wait...');

            // Reset the grid
            var table = $("#contractorTable").dataTable().api();
            table.clear().draw();

            // #region Initialize paramter object
            var filterData = {};

            filterData.contractorNo = $.fn.getIntValue($("#txtContractorNo").val());
            filterData.idNumber = $("#txtIDNo").val().trim();
            filterData.contractorName = $("#txtContractorName").val().trim();
            filterData.companyName = $("#txtCompanyName").val().trim();

            if ($('#cboCostCenter').val() != null && $('#cboCostCenter').val() != undefined) {
                if ($('#cboCostCenter').val().trim() == CONST_EMPTY)
                    filterData.costCenter = "";
                else
                    filterData.costCenter = $("#cboCostCenter").val().trim();
            }
            else
                filterData.costCenter = "";

            filterData.jobTitle = $("#txtJobTitle").val().trim();
            filterData.supervisorName = $("#txtSupervisor").val().trim();
            filterData.contractStartDateStr = ConvertToISODate($("#txtContractStartDate").val());
            filterData.contractEndDateStr = ConvertToISODate($("#txtContractEndDate").val());

            // Save filter criteria to session storage
            SaveDataToSession("searchCriteria", JSON.stringify(filterData));

            // Convert object to JSON
            var jsonData = JSON.stringify({
                filterData: filterData
            });
            // #endregion

            // Call Web Service method using AJAX
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: "ContractorInquiry.aspx/SearchContractor",
                //url: "/WebService/ContractorWS.asmx/SearchContractor",
                data: jsonData,
                async: "true",
                cache: "false",
                success: function (result) {
                    if (result.d != null) {
                        //setFormDataLoaded();
                        populateDataTable(result.d);
                        HideLoadingPanel(gContainer);
                    }
                    else {
                        HideLoadingPanel(gContainer);
                        ShowToastMessage(toastTypes.error, "Unable to find any matching record from the database. Please specify a new filter criteria then try again!", "No Record Found")
                    }
                },
                error: function (err) {
                    HideLoadingPanel(gContainer);
                    ShowErrorMessage("The following error has occured while searching for contractor records from the database.\n\n" + err.responseText);
                }
            });
        }
    }
    catch (err) {
        throw err;
    }
}
// #endregion