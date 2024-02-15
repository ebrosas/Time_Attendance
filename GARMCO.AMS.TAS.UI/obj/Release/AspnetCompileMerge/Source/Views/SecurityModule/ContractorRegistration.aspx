<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ContractorRegistration.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.ContractorRegistration" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Contractor Registration</title>

    <link rel="stylesheet" href="../../Content/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="../../Content/lib/jqueryui/jquery-ui.min.css" />
    <link rel="stylesheet" href="../../Content/lib/datatables/datatables.min.css" />
    <link rel="stylesheet" href="../../Content/lib/fontawesome/all.min.css" />
    <link rel="stylesheet" href="../../Content/lib/loader/waitMe.min.css" />
    <link rel="stylesheet" href="../../Styles/common.css" />
    
    <script src="../../Content/lib/jquery/jquery-3.6.0.min.js"></script>
    <script src="../../Content/lib/bootstrap/js/bootstrap.bundle.min.js"></script>    
    <script src="../../Content/lib/jqueryui/jquery-ui.min.js"></script>
    <script src="../../Content/lib/datatables/datatables.min.js"></script>
    <script src="../../Content/lib/fontawesome/all.min.js"></script>
    <script src="../../Content/lib/loader/waitMe.min.js"></script>
    <script src="../../Scripts/Contractor/contractregister.js?v=<%=JSVersion%>"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <div class="alert alert-danger alert-dismissible fade show errorPanel" hidden>
        <button type="button" class="close" data-dismiss="alert">&times;</button>
        <strong>Error! </strong>&nbsp;<span class="errorText"></span>
    </div>
    <div class="alert alert-success alert-dismissible fade show successPanel" hidden>
        <button type="button" class="close" data-dismiss="alert">&times;</button>
        <strong>Success!</strong>&nbsp;<span class="successText">@Model.NotificationMessage</span>
    </div>

    <div class="container-fluid mt-3 mb-4 px-5 formWrapper">
        <div class="row no-gutters">
            <div class="col-sm-12">
                <div class="formHeader">
                    Contractor Registration Form
                </div>
            </div>
        </div>

        <div class="requestHeader">
            <div class="container-fluid pl-5 pr-0 py-3">
                <div class="form-row pt-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtContractorNo" class="col-form-label">Contractor No.</label>                        
                    </div>
                    <div class="col-sm-4">
                        <div class="form-row">
                            <div class="col-sm-3">
                                <input type="text" class="form-control form-control-sm fieldValue" id="txtContractorNo" name="contractorNo" placeholder="6xxxx" style="font-size: 14px;" readonly />
                            </div>
                            <div class="col">
                                <button type="button" id="btnFind" class="btn btn-sm btn-secondary" title="Tips:" data-toggle="popover" data-content="Click here to search for specific Contractor." data-trigger="hover" data-placement="top">
                                    ...
                                    <%--<span><i class="fas fa-search fa-fw"></i></span>--%>
                                </button>
                            </div>
                        </div>                        
                        
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label class="col-form-label">Identification No.</label>
                    </div>
                    <div class="col-sm-4">
                        <div class="form-row">
                            <div class="col-sm-4">
                                <input type="text" id="txtIDNo" name="idNo" class="form-control form-control-sm fieldValue" style="font-size: 14px;" />
                            </div>
                            <div class="col-sm-7">
                                <div class="custom-control custom-radio custom-control-inline ml-2 fieldValue">
                                    <input type="radio" class="custom-control-input pt-1" id="chkCPR" name="idNumber" />
                                    <label for="chkCPR" class="custom-control-label pt-1">CPR</label>
                                </div>
                                <div class="custom-control custom-radio custom-control-inline fieldValue">
                                    <input type="radio" class="custom-control-input pt-1" id="chkPassport" name="idNumber" />
                                    <label for="chkPassport" class="custom-control-label pt-1">Passport</label>
                                </div>
                            </div>
                            <div class="col"></div>
                        </div>
                    </div>
                </div>
                <div class="form-row my-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtFirstName" class="col-form-label">First Name</label>
                    </div>
                    <div class="col-sm-4">
                        <input type="text" id="txtFirstName" name="firstName" class="form-control form-control-sm fieldValue" style="font-size: 14px;" />
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="cboCompanyAuto" class="col-form-label">Company Name</label>
                    </div>
                    <div class="col-sm-3">
                        <input id="cboCompanyAuto" class="form-control form-control-sm fieldValue" type="text" 
                            title="Tips:" data-toggle="popover" data-content="Type the company name to filter the list" data-trigger="hover" data-placement="top"
                            name="companyName" value="" placeholder="company name..." style="font-size: 14px;" />
                    </div>
                    <div class="col-1"></div>
                </div>
                <div class="form-row my-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtLastName" class="col-form-label">Last Name</label>
                    </div>
                    <div class="col-sm-4">
                        <input type="text" id="txtLastName" name="lastName" class="form-control form-control-sm fieldValue" style="font-size: 14px;" />
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtCompanyCR" class="col-form-label">Company CR No.</label>
                    </div>
                    <div class="col-sm-3">
                        <input type="text" id="txtCompanyCR" name="companyCR" class="form-control form-control-sm fieldValue" style="font-size: 14px;" />
                    </div>
                    <div class="col-1"></div>
                </div>
                <div class="form-row my-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtJobTitle" class="col-form-label">Job Title</label>
                    </div>
                    <div class="col-sm-4">
                        <input type="text" id="txtJobTitle" name="jobTitle" class="form-control form-control-sm fieldValue" style="font-size: 14px;" />
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="cboCostCenterAuto" class="col-form-label">Department to Visit</label>
                    </div>
                    <div class="col-sm-3">
                        <select id="cboCostCenter" class="form-control custom-select custom-select-sm fieldValue" name="costCenter">
                        </select>
                        <%--<input id="cboCostCenterAuto" class="form-control form-control-sm fieldValue" type="text" 
                            title="Tips:" data-toggle="popover" data-content="Type the cost center name or code to filter the list" data-trigger="hover" data-placement="top"
                            name="costCenter" value="" placeholder="cost center..." style="font-size: 14px;" />--%>
                    </div>
                    <div class="col-1"></div>
                </div>
                <div class="form-row my-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtMobileNo" class="col-form-label">Mobile No.</label>
                    </div>
                    <div class="col-sm-4">
                        <input type="text" id="txtMobileNo" name="mobileNo" class="form-control form-control-sm fieldValue" style="font-size: 14px;" />
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtPurpose" class="col-form-label">Purpose of Visit</label>
                    </div>
                    <div class="col-sm-3">
                        <textarea id="txtPurpose" rows="3" class="form-control form-control-sm fieldValue"></textarea>
                    </div>
                    <div class="col-1"></div>
                </div>
                <div class="form-row my-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label class="col-form-label">Contract Duration</label>
                    </div>
                    <div class="col-sm-4">
                        <input class="borderLess fieldValue form-control-sm" type="text" id="dtpStartDate" style="width: 130px; font-size: 14px; text-align: center;" readonly />&nbsp; ~ &nbsp;
                        <input class="borderLess fieldValue form-control-sm" type="text" id="dtpEndDate" style="width: 130px; font-size: 14px; text-align: center;" readonly />
                        <input type="hidden" id="hdnStartDate" />
                        <input type="hidden" id="hdnEndDate" />
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtRegistrationDate" class="col-form-label">Registration Date</label>
                    </div>
                    <div class="col-sm-3">
                        <input type="text" id="txtRegistrationDate"  class="form-control form-control-plaintext fieldValue" style="font-size: 14px;" value="20-Dec-2021 12:00 AM" />
                        <%--<input class="borderLess fieldValue form-control-sm" type="text" id="dtpRegisterDate" style="width: 130px; font-size: 14px; text-align: center;" readonly />
                        <input type="hidden" id="hdndtpRegisterDate" />--%>
                    </div>
                    <div class="col-1"></div>
                </div>
                <div class="form-row my-1">
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label class="col-form-label">Active Licenses</label>
                    </div>
                    <div class="col-sm-4" id="divLicense">
                    </div>
                    <div class="col-sm-2 fieldLabel pr-2">
                        <label for="txtRemarks" class="col-form-label">Remarks</label>
                    </div>
                    <div class="col-sm-3 pt-1">
                        <textarea id="txtRemarks" rows="5" class="form-control form-control-sm fieldValue"></textarea>
                    </div>
                    <div class="col-1"></div>
                </div>
                <div class="form-row mt-3">
                    <div class="col-sm-12 pl-2">
                        <button type="button" id="btnCreateNew" class="form-control btn btn-sm btn-primary float-sm-left border-0 actionButton">
                            <span><i class="fas fa-edit fa-fw fa-1x"></i></span>&nbsp;
                            Create New
                        </button>
                        <button type="button" id="btnSave" class="form-control btn btn-sm btn-success border-0 actionButton" hidden>
                            <span><i class="fas fa-archive fa-fw fa-1x"></i></span>&nbsp;
                            Save
                        </button>
                        <button type="button" id="btnDelete" class="form-control btn btn-sm btn-danger border-0 actionButton" hidden>
                            <span><i class="far fa-trash-alt fa-fw fa-1x"></i></span>&nbsp;
                            Delete
                        </button>
                        <%--<input type="reset" id="btnReset" class="form-control btn btn-sm btn-warning text-white border-0 actionButton" value="Reset" />--%>
                        <button type="button" id="btnReset" class="form-control btn btn-sm btn-warning text-white border-0 actionButton" hidden>
                            <span><i class="fas fa-sync fa-fw fa-1x"></i></span>&nbsp;
                            Reset
                        </button>
                        <button type="button" id="btnPrint" class="form-control btn btn-sm btn-outline-secondary actionButton">
                            <span><i class="fas fa-print fa-fw fa-1x"></i></span>&nbsp;
                            Print Report
                        </button>
                        <button type="button" id="btnGenerateCard" class="form-control btn btn-sm btn-outline-secondary actionButton">
                            <span><i class="fas fa-share fa-fw fa-1x"></i></span>&nbsp; 
                            Generate Card
                        </button>
                        <button type="button" id="btnBack" class="form-control btn btn-sm btn-outline-secondary actionButton">
                            <span><i class="fas fa-angle-double-left fa-fw fa-1x"></i></span>&nbsp; 
                            Back
                        </button>                       
                    </div>
                </div>
            </div>            
        </div>
    </div>
</asp:Content>
