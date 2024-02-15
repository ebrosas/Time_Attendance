<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterContractor.aspx.cs" Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.RegisterContractor" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <title>Contractor Registration Form</title>

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />

    <%--CSS file references--%>
    <link rel="stylesheet" href="../../Content/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="../../Content/lib/jqueryui/jquery-ui.min.css" />
    <link rel="stylesheet" href="../../Content/lib/datatables/datatables.min.css" />
    <link rel="stylesheet" href="../../Content/lib/fontawesome/all.min.css" />
    <link rel="stylesheet" href="../../Content/lib/loader/waitMe.min.css" />
    <link rel="stylesheet" href="../../Content/lib/toastr/toastr.min.css" />
    <link rel="stylesheet" href="../../Styles/common.css" />

    <%--JavaScript file references--%>
    <script src="../../Content/lib/jquery/jquery-3.6.0.min.js"></script>
    <script src="../../Content/lib/bootstrap/js/bootstrap.bundle.min.js"></script>    
    <script src="../../Content/lib/jqueryui/jquery-ui.min.js"></script>
    <script src="../../Content/lib/datatables/datatables.min.js"></script>
    <script src="../../Content/lib/fontawesome/all.min.js"></script>
    <script src="../../Content/lib/loader/waitMe.min.js"></script>
    <script src="../../Content/lib/toastr/toastr.min.js"></script>
    <script src="../../Content/lib/moment/moment.js"></script>
    <script src="../../Content/lib/moment/ellipsis.js"></script>
    <script src="../../Content/lib/sweetalert/sweetalert.min.js"></script>
    <%--<script src="https://unpkg.com/sweetalert/dist/sweetalert.min.js"></script>--%>

    <script type="text/javascript">
        $(function () {
            var test = '<%= Session["ShowLoadingPanel"] %>'; 
            if (test === true) {
                alert('Hello');
            }
        });
    </script>
</head>
<body>    
    <form id="mainForm" class="formWrapper">      
        <%--License Registration Form--%> 
        <div class="modal fade" id="modLicenseRegistration">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <%--Modal Header--%>
                    <div class="modal-header bg-info">
                        &nbsp;&nbsp;
                        <span class="text-white"><i class='fas fa-id-card fa-3x fa-fw'></i></span>
                        <span class="modal-title text-white modalHeader" style="line-height: 2.6;">&nbsp;&nbsp;License Registration</span>
                        <button type="button" class="close" data-dismiss="modal">
                            <span class="text-danger"><i class="fas fa-window-close"></i></span>
                        </button>
                    </div>

                    <%--Modal Body--%>
                    <div class="modal-body">
                        <div class="form-row my-1">
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="cboLicenseType" class="col-form-label" data-field="LicenseType">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    License Type
                                </label>   
                            </div>
                            <div class="col-sm-4">
                                <select id="cboLicenseType" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" data-primarykey="yes" tabindex="26">
                                </select>
                                <div class="form-row errorPanel" id="licenseTypeValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div>    
                            </div>
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtLicenseNo" class="col-form-label" data-field="LicenseNo">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    License No.
                                </label>   
                            </div>
                            <div class="col-sm-4 pr-3">
                                <input type="text" id="txtLicenseNo" class="form-control form-control-sm fieldValue" maxlength="20" tabindex="27" data-primarykey="yes" style="font-size: 14px;" />
                                <div class="form-row errorPanel" id="licenseNoValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div>    
                            </div>
                        </div>
                        <div class="form-row my-1">
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtIssuedDate" class="col-form-label" data-field="IssuedDate">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    Issued Date
                                </label>   
                            </div>
                            <div class="col-sm-4">
                                <div class="input-group input-group-sm mt-1">
                                    <div class="input-group-prepend">
                                        <span class="input-group-text inputGroupTitle">
                                            <i class="far fa-calendar-alt fa-1x fa-fw"></i>
                                        </span>
                                    </div>
                                    <input class="borderLess fieldValue form-control-sm" type="text" id="txtIssuedDate" maxlength="10" tabindex="28" readonly 
                                        style="width: 120px; font-size: 14px; text-align: center;" />
                                    <input type="hidden" id="hdnIssuedDate" />
                                </div> 
                                <div class="form-row errorPanel" id="issuedDateValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div> 
                            </div>
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtExpiryDate" class="col-form-label" data-field="ExpiryDate">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    Expiry Date
                                </label>   
                            </div>
                            <div class="col-sm-4">
                                <div class="input-group input-group-sm mt-1">
                                    <div class="input-group-prepend">
                                        <span class="input-group-text inputGroupTitle">
                                            <i class="far fa-calendar-alt fa-1x fa-fw"></i>
                                        </span>
                                    </div>
                                    <input class="borderLess fieldValue form-control-sm" type="text" id="txtExpiryDate" maxlength="10" tabindex="29" readonly 
                                        style="width: 120px; font-size: 14px; text-align: center;" />
                                    <input type="hidden" id="hdnExpiryDate" />
                                </div> 
                                <div class="form-row errorPanel" id="expiryDateValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div> 
                            </div>
                        </div>
                        <div class="form-row mt-2">
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtIssuingAuthority" data-field="IssueAuthority">Issuing Authority</label>   
                            </div>
                            <div class="col-sm-4">
                                <textarea id="txtIssuingAuthority" rows="4" class="form-control form-control-sm fieldValue" maxlength="200" tabindex="30"></textarea>
                            </div>
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtNotes" data-field="Notes">Notes</label>   
                            </div>
                            <div class="col-sm-4">
                                <textarea id="txtNotes" rows="4" class="form-control form-control-sm fieldValue" maxlength="300" tabindex="31"></textarea>
                            </div>
                        </div>
                        <div class="form-row pl-1 mt-2">
                            <div class="col-sm-12 text-danger clearfix">
                                <span class="float-left ml-1"><i class="fas fa-asterisk fa-fw fa-xs"></i></span>                            
                                <span class="font-italic float-left">- indicates a required field</span>
                            </div>
                        </div>   
                        <input type="hidden" id="hidLicenseGUID" />
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-link font-weight-bold modalButton" data-button-value="modalCancel" data-dismiss="modal" tabindex="32" style="width: 90px;">
                            <%--<span><i class="fas fa-sign-in-alt fa-fw fa-1x"></i></span>&nbsp;--%>
                            Cancel
                        </button>
                        <button type="button" class="btn btn-danger modalButton" data-button-value="modalDelete" tabindex="33" style="width: 110px;">
                            <span><i class="fas fa-trash fa-fw fa-1x"></i></span>&nbsp;Delete
                        </button>
                        <button type="button" class="btn btn-primary modalButton" data-button-value="modalSave" tabindex="34" style="width: 110px;">
                            <span><i class="fas fa-thumbs-up fa-fw fa-1x"></i></span>&nbsp;Save
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <%--Pop-up Confirmation Form--%>
        <div class="modal fade" id="modalConfirmation">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <%--Modal Header--%>
                    <div class="modal-header">
                        <span class="text-danger"><i id="modalConfirmationIcon" class='fas fa-times-circle fa-2x fa-fw'></i></span>
                        <span class="modal-title text-danger modalHeader">&nbsp;Confirmation</span>
                        <button type="button" class="close" data-dismiss="modal">
                            <span class="text-danger"><i class="fas fa-window-close"></i></span>
                        </button>
                    </div>

                    <%--Modal Body--%>
                    <div class="modal-body">
                        <p></p>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-danger modalButton" data-button-value="modalNo" data-dismiss="modal">
                            <span><i class="fas fa-thumbs-down fa-fw fa-1x"></i></span>&nbsp;No
                        </button>
                        <button type="button" class="btn btn-success modalButton" data-button-value="modalYes" data-dismiss="modal">
                            <span><i class="fas fa-thumbs-up fa-fw fa-1x"></i></span>&nbsp;Yes
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div class="container-fluid mt-3 mb-4 px-4">
            <div class="row no-gutters">
                <div class="col-sm-12">
                    <div class="formHeader">
                        <span><i class="fas fa-edit fa-lg fa-fw"></i></span>
                        Contractor  Registration 
                    </div>
                </div>
            </div>

            <div class="requestHeader">
                <div class="form-row">
                    <div class="col-12 mx-auto px-5">
                        <div class="alert alert-danger alert-dismissible fade show errorMsgBox" hidden>
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Error! </strong>&nbsp;<span class="errorMsg">Test error!</span>
                        </div>
                        <div class="alert alert-success alert-dismissible fade show successMsgBox" hidden>
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Success!</strong>&nbsp;<span class="successMsg">Test success!</span>
                        </div>
                    </div>
                </div>
                <div class="container-fluid pl-5 pr-0 pt-1 pb-3">        
                    <div class="form-row pr-4">
                        <div class="col-sm-12 text-danger clearfix">
                            <span class="font-italic float-right">- indicates a required field</span>
                            <span class="float-right mr-1"><i class="fas fa-asterisk fa-fw fa-xs"></i></span>                            
                        </div>
                    </div>            
                    <div class="form-row pt-0">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <label for="txtContractorNo" class="col-form-label" data-field="ContractorNo">Contractor No.</label>                        
                        </div>
                        <div class="col-sm-4">         
                            <div class="form-row">
                                <div class="col-sm-5">
                                    <div class="input-group input-group-sm">
                                        <input type="text" class="form-control form-control-sm fieldValue" id="txtContractorNo" name="contractorNo" placeholder="6xxxx" tabindex="1" maxlength="5" autofocus                                            
                                            style="font-size: 14px; font-weight: 800;" />
                                        <div class="input-group-append pl-1">
                                            <button type="button" id="btnFind" class="btn btn-sm btn-success formButton"> 
                                                Go 
                                            </button>                                        
                                        </div>                                        
                                        <div class="input-group-append pl-0">
                                            <button type="button" id="btnSearch" class="btn btn-sm btn-success formButton"> 
                                                <span><i class="fas fa-search fa-fw"></i></span>
                                            </button>                                        
                                        </div>    
                                    </div>
                                </div>
                            </div>   
                            <div class="form-row errorPanel" id="contractorNoValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div>                                                                  
                        </div>
                        <div class="col-sm-2 fieldLabel pr-2">
                            <label class="col-form-label" data-field="IDNo">
                                <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                ID Number
                            </label>
                        </div>
                        <div class="col-sm-4">                            
                            <div class="form-row">
                                <div class="col-sm-6">        
                                    <div id="idNoInput" class="input-group input-group-sm">
                                        <input type="text" id="txtIDNo" name="idNo" class="form-control form-control-sm fieldValue" style="font-size: 14px;" maxlength="9" tabindex="2" placeholder="cpr or passport no..." 
                                            title="Notes: Valid input are numbers from (0-9)." data-readonly-field="false" required />         
                                        <div class="input-group-append pl-1">
                                            <button type="button" id="btnIDType" class="btn btn-group-sm btn-success dropdown-toggle" data-toggle="dropdown">
                                                Type
                                            </button>
                                            <div class="dropdown-menu">
                                                <a id="lnkCPR" class="dropdown-item fieldValue" href="#">                                                    
                                                    CPR
                                                    <span><i id="optCPR" class="fas fa-check-circle fa-fw fa-2x text-success" data-idtype="cpr"></i></span>
                                                </a>
                                                <a id="lnkPassport" class="dropdown-item fieldValue" href="#">                                                    
                                                    Passport
                                                    <span><i id="optPassport" class="fas fa-check-circle fa-fw fa-2x text-success" data-idtype="passport" hidden></i></span>
                                                </a>
                                            </div>
                                        </div>             
                                    </div>                                                                
                                </div>
                                <div class="col"></div>
                            </div>     
                            <div class="form-row errorPanel" id="idNoValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <%--<button type="button" class="close" data-dismiss="alert">&times;</button>--%>
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span id="idError" class="errorText"></span>
                                    </div>
                                </div>
                            </div>                       
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <label for="txtFirstName" class="col-form-label" data-field="FirstName">
                                <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                First Name
                            </label>
                        </div>
                        <div class="col-sm-4">
                            <input type="text" id="txtFirstName" name="firstName" class="form-control form-control-sm fieldValue" maxlength="30" tabindex="3" data-readonly-field="false" autocomplete="off" required 
                                style="font-size: 14px; text-transform: uppercase;" />                            
                            <div class="form-row errorPanel" id="firstNameValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div>     
                        </div>
                        <div class="col-sm-2 fieldLabel pr-2">
                            <label for="txtCompanyName" class="col-form-label" data-field="CompanyName">
                                <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                Company Name
                            </label>
                        </div>
                        <div class="col-sm-4">
                            <input id="txtCompanyName" class="form-control form-control-sm fieldValue" type="text"  maxlength="50" tabindex="4" data-readonly-field="false"
                                title="Tips:" data-toggle="popover" data-content="Type the company name to filter the list" data-trigger="hover" data-placement="top"
                                name="companyName" value="" placeholder="registered company name..." required style="font-size: 14px;" />
                            <input type="hidden" id="hidCompanyID" /> 
                            <input type="hidden" id="hidCompanyName" /> 
                            <div class="form-row errorPanel" id="companyNameValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <label for="txtLastName" class="col-form-label" data-field="LastName">
                                <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                Last Name
                            </label>
                        </div>
                        <div class="col-sm-4">
                            <input type="text" id="txtLastName" name="lastName" class="form-control form-control-sm fieldValue" maxlength="30" tabindex="5" data-readonly-field="false" autocomplete="off" required 
                                style="font-size: 14px; text-transform: uppercase;" />
                            <div class="form-row errorPanel" id="lastNameValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-2 fieldLabel pr-2">
                            <label for="txtContactNo" class="col-form-label">Company Contact No.</label>
                        </div>
                        <div class="col-sm-4">
                            <input type="text" id="txtContactNo" class="form-control form-control-sm fieldValue" maxlength="30" tabindex="6" data-readonly-field="false" title="Max. input is 30 chars." style="font-size: 14px;"  />
                            <input type="text" id="txtCompanyCR" name="companyCR" class="form-control form-control-sm fieldValue" maxlength="20" tabindex="66" data-readonly-field="false" hidden style="font-size: 14px;"  />
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <label for="cboJobTitle" class="col-form-label" data-field="JobTitle">
                                <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                Job Title
                            </label>
                        </div>
                        <div class="col-sm-4">
                            <select id="cboJobTitle" class="form-control custom-select custom-select-sm fieldValue" tabindex="7" required>
                            </select>
                            <%--<input type="text" id="txtJobTitle" name="jobTitle" class="form-control form-control-sm fieldValue" maxlength="50" tabindex="7" data-readonly-field="false" placeholder="position..." required style="font-size: 14px;" />--%>
                            <div class="form-row errorPanel" id="jobTitleValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-2 fieldLabel pr-2">
                            <label for="cboPurchaseOrder" class="col-form-label" data-field="PONumber">
                                <%--<span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>--%>
                                Purchase Order No.
                            </label>
                        </div>
                        <div class="col-sm-4">
                            <select id="cboPurchaseOrder" class="form-control custom-select custom-select-sm fieldValue" tabindex="8">
                            </select>
                            <%--<input type="text" id="txtPONumber" name="poNumber" class="form-control form-control-sm fieldValue" maxlength="12" tabindex="8" data-readonly-field="false" autocomplete="off" placeholder="po number..." required style="font-size: 14px;" />  --%>
                            <input type="hidden" id="hidPONumber" />
                            <div class="form-row errorPanel" id="poNumberValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <label for="txtMobileNo" class="col-form-label">Mobile No.</label>
                        </div>
                        <div class="col-sm-4">
                            <input type="text" id="txtMobileNo" name="mobileNo" class="form-control form-control-sm fieldValue" maxlength="20" tabindex="9" data-readonly-field="false" style="font-size: 14px;" />
                        </div>
                        <div class="col-sm-2 fieldLabel pr-2">
                            <label for="cboCostCenter" class="col-form-label" data-field="CostCenter">
                                <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                Department to Visit
                            </label>
                        </div>
                        <div class="col-sm-4">
                            <select id="cboCostCenter" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" tabindex="10" required>
                            </select>
                            <div class="form-row errorPanel" id="costCenterValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div>  
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                            <label class="col-form-label" data-field="Duration">Duration</label>
                        </div>
                        <div class="col-sm-4">
                            <div class="input-group input-group-sm mt-1">
                                <div class="input-group-prepend">
                                    <span class="input-group-text inputGroupTitle">
                                        <i class="far fa-calendar-alt fa-1x fa-fw"></i>
                                        From                                        
                                    </span>
                                </div>
                                <input class="borderLess fieldValue form-control-sm" type="text" id="txtContractStartDate" maxlength="10" tabindex="11" data-readonly-field="true" readonly 
                                    style="width: 120px; font-size: 14px; text-align: center;" />
                                <input type="hidden" id="hdnStartDate" />
                                <div class="input-group-prepend ml-2">
                                    <span class="input-group-text inputGroupTitle">
                                        <i class="far fa-calendar-alt fa-fw"></i>
                                        To                                        
                                    </span>
                                </div>
                                <input class="borderLess fieldValue form-control-sm" type="text" id="txtContractEndDate" maxlength="10" tabindex="12" data-readonly-field="true" readonly 
                                    style="width: 120px; font-size: 14px; text-align: center;" />
                                <input type="hidden" id="hdnEndDate" />
                            </div> 
                            <div class="form-row errorPanel" id="durationValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                        <span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-2 fieldLabel pr-2 mt-1">
                            <label for="txtPurpose" class="col-form-label">Purpose of Visit</label>
                        </div>
                        <div class="col-sm-4">
                            <textarea id="txtPurpose" rows="1" maxlength="300" tabindex="13" class="form-control form-control-sm fieldValue"></textarea>            
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row mt-1 mb-0">
                        <div class="col-sm-1 fieldLabel pr-2">
                            <label for="txtRemarks" class="col-form-label">Remarks</label>
                        </div>
                        <div class="col-sm-4 pt-1">
                            <textarea id="txtRemarks" rows="6" class="form-control form-control-sm fieldValue" maxlength="500" tabindex="14"></textarea>
                        </div>
                        <div class="col-sm-6">
                            <div class="form-row my-1">
                                <div class="col-sm-4 fieldLabel pr-2">
                                    <label for="txtSupervisor" class="col-form-label" data-field="Supervisor">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        Supervisor In-charge
                                    </label>
                                </div>
                                <div class="col-sm-8">
                                    <input id="txtSupervisor" class="form-control form-control-sm fieldValue" type="text" maxlength="100" tabindex="15" data-readonly-field="false" 
                                        title="Notes:" data-toggle="popover" data-content="Please enter the employee no. or type the name to filter the list." data-trigger="hover" data-placement="top"
                                        name="supervisor" value="" placeholder="employee name..." required style="font-size: 14px;" />
                                    <input type="hidden" id="hidSupervisorNo" />
                                    <div class="form-row errorPanel" id="supervisorValid" hidden>
                                        <div class="col-sm-12">
                                            <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                <span class="errorText"></span>
                                            </div>
                                        </div>
                                    </div>   
                                </div>
                            </div>
                            <div class="form-row my-1">
                                <div class="col-sm-4 fieldLabel pr-2">
                                    <label for="txtWorkHours" class="col-form-label">Work Duration</label>
                                </div>
                                <div class="col">
                                    <div class="input-group input-group-sm mt-1">
                                        <div class="input-group-prepend">
                                            <span class="input-group-text inputGroupTitle">
                                                <i class="far fa-clock fa-1x fa-fw"></i>
                                                Hours                                        
                                            </span>
                                        </div>
                                        <input type="number" id="txtWorkHours" class="borderLess fieldValue form-control-sm" min="0" max="24" value="0" autocomplete="off" tabindex="11" title="Accepts numeric values from 0 to 24" 
                                            style="width: 70px; font-size: 14px; text-align: center;" />
                                        <div class="input-group-prepend ml-2">
                                            <span class="input-group-text inputGroupTitle">
                                                <i class="far fa-clock fa-fw"></i>
                                                Mins                                        
                                            </span>
                                        </div>
                                        <input type="number" id="txtWorkMins" class="borderLess fieldValue form-control-sm" min="0" max="59" value="0" autocomplete="off" tabindex="12" title="Accepts numeric values from 0 to 59" 
                                            style="width: 70px; font-size: 14px; text-align: center;" />
                                    </div> 
                                    <div class="form-row errorPanel" id="workDurationValid" hidden>
                                        <div class="col-sm-12">
                                            <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                <span class="errorText"></span>
                                            </div>
                                        </div>
                                    </div> 
                                </div>
                            </div>
                            <div class="form-row mb-1 mt-2">
                                <div class="col-sm-4 fieldLabel pr-2">
                                    <label for="cboBloodGroup" class="col-form-label">Blood Type</label>
                                </div>
                                <div class="col-sm-3">
                                    <select id="cboBloodGroup" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" tabindex="16" style="width: 165px;">
                                    </select>
                                </div>
                                <div class="col"></div>
                            </div>
                            <div class="form-row my-1">
                                <div class="col-sm-4 fieldLabel pr-2">
                                    <label for="txtRegistrationDate" class="col-form-label">Registration Date</label>
                                </div>
                                <div class="col">
                                    <div class="input-group input-group-sm">
                                        <div class="input-group-prepend">
                                            <span class="input-group-text"><i class="far fa-calendar-alt fa-fw"></i></span>
                                        </div>
                                        <input class="borderLess fieldValue form-control-sm" type="text" id="txtRegistrationDate" maxlength="10" tabindex="17" data-readonly-field="false" 
                                            style="width: 130px; font-size: 14px; text-align: center;" readonly />
                                        <input type="hidden" id="hdnRegisterDate" />
                                    </div>  
                                </div>
                            </div>                            
                        </div>
                       
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row mt-0 mb-1">
                        <div class="col-sm-12">
                            <div id="showLicensePanel" class="form-row mb-2 mt-0 pt-0 pl-0 ml-0">
                                <div class="col-sm-11">
                                    <div class="custom-control custom-switch">
                                        <input type="checkbox" class="custom-control-input" id="licenseInfoSwitch" disabled>
                                        <label class="custom-control-label groupTitle text-primary" for="licenseInfoSwitch">Show License Information</label>
                                    </div>
                                </div>
                                <div class="col-1"></div>
                            </div>  
                        </div>                        
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-12">
                            <div id="collapseLicenseInfo" class="collapse">
                                <div class="form-row mt-0 mb-0 legendTitle">
                                    <div class="col-sm-12">
                                        <span>License Information:</span> &nbsp;
                                        <span class="float-sm-right">
                                            <button type="button" class="btn btn-link pr-4 mr-4 linkTitle" disabled>Add New License</button>
                                        </span>
                                    </div>
                                </div>
                                <div class="form-row mt-0 pt-0 mb-4">
                                    <div class="col-sm-12 pr-5 pl-0 ml-0">
                                        <div class="container-fluid tableWrapper">
                                            <div class="table-responsive py-3" style="-ms-overflow-style: auto;">
                                                <table id="licenseTable" class="generalTable display nowrap stripe row-border table-bordered" style="width: 100%;">
                                                    <thead>
                                                        <tr>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                Registry ID
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                Contractor No.
                                                            </th>
                                                            <th class="centeredColumn" style="width: 120px;">
                                                                License Type
                                                            </th>
                                                            <th class="centeredColumn" style="width: 100px;">
                                                                License No.
                                                            </th>
                                                            <th class="centeredColumn" style="width: 100px;">
                                                                Issued Date
                                                            </th>
                                                            <th class="centeredColumn" style="width: 100px;">
                                                                Expiry Date
                                                            </th>
                                                            <th class="centeredColumn" style="width: 250px;">
                                                                Issuing Authority
                                                            </th>
                                                            <th class="centeredColumn" style="width: 300px;">
                                                                Notes
                                                            </th>
                                                            <th class="centeredColumn" style="width: 100px;">
                                                                Created Date
                                                            </th>
                                                            <th class="centeredColumn" style="width: 300px;">
                                                                Created By
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                Last Update Date
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 200px; display: none;">
                                                                Last Updated By
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                License GUID
                                                            </th>
                                                        </tr>
                                                    </thead>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="form-row mt-2">
                        <div class="col-sm-12 pl-2">
                            <button type="button" id="btnCreateNew" class="form-control btn btn-sm btn-primary float-sm-left border-0 actionButton" tabindex="18">
                                <span><i class="fas fa-edit fa-fw fa-lg"></i></span>&nbsp;
                                Create New
                            </button>
                            <button type="button" id="btnSave" class="form-control btn btn-sm btn-success border-0 actionButton" tabindex="19" data-form-mode="2" hidden>
                                <span><i class="fas fa-archive fa-fw fa-lg"></i></span>&nbsp;
                                Save
                            </button>
                            <button type="button" id="btnUpdate" class="form-control btn btn-sm btn-success border-0 actionButton" tabindex="20" hidden>
                                <span><i class="fas fa-archive fa-fw fa-lg"></i></span>&nbsp;
                                Update
                            </button>
                            <button type="button" id="btnDelete" class="form-control btn btn-sm btn-danger border-0 actionButton" tabindex="21" hidden>
                                <span><i class="far fa-trash-alt fa-fw fa-lg"></i></span>&nbsp;
                                Delete
                            </button>                            
                            <button type="button" id="btnReset" class="form-control btn btn-sm btn-warning text-white border-0 actionButton" tabindex="22">
                                <span><i class="fas fa-sync fa-fw fa-lg"></i></span>&nbsp;
                                Reset
                            </button>                            
                            <button type="button" id="btnGenerateCard" class="form-control btn btn-sm btn-outline-secondary actionButton" tabindex="23" 
                                style="width: 175px;">
                                <span><i class="fas fa-id-card-alt fa-fw fa-lg"></i></span>&nbsp;
                                Generate Card
                            </button>
                            <button type="button" id="btnPrint" class="form-control btn btn-sm btn-outline-secondary actionButton" tabindex="24">
                                <span><i class="fas fa-print fa-fw fa-lg"></i></span>&nbsp;
                                Print Report
                            </button>
                            <button type="button" id="btnBack" class="form-control btn btn-sm btn-outline-secondary actionButton" tabindex="25" hidden>
                                <span><i class="fas fa-arrow-circle-left fa-fw fa-lg"></i></span>&nbsp; 
                                Go Back
                            </button>
                            <input type="reset" id="btnHiddenReset" class="btn btn-sm btn-warning text-white border-0" value="Reset" hidden />                                 
                        </div>
                    </div>
                </div>            
            </div>
        </div>

        <input type="hidden" id="hidRegistryID" /> 
        <input type="hidden" id="hidIDType" /> 
        <input type="hidden" id="hidMasterFrameClientID" />
        <input type="hidden" id="hidFormCode" value="CONTREGSTR" />
        <input type="hidden" id="hidCurrentUserID" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpNo" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpName" runat="server" /> 
        <input type="hidden" id="hidCostCenter" runat="server" />         
    </form>

    <%--Local JS file reference--%>
    <script src="../../Scripts/Contractor/common.js?v=<%=JSVersion%>"></script>
    <script src="../../Scripts/Contractor/ContractRegister.js?v=<%=JSVersion%>"></script>
</body>
</html>
