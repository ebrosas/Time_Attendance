using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using GARMCO.AMS.TAS.BL.Entities;
//using GARMCO.AMS.TAS.UI.TASWCFProxy;
using GARMCO.Common.DAL.Employee;
using GARMCO.Common.Object;
using Telerik.Web.UI;
using System.Text;

namespace GARMCO.AMS.TAS.UI.Helpers
{
    public static class UIHelper
    {
        #region Constants

        #region Dynamic Control Constants
        public const string CONST_TABLECELL_DEFAULT_WIDTH = "170px";
        public const string CONST_TABLEROW_DEFAULT_HEIGHT = "18px";
        public const string CONST_CONTROL_DEFAULT_WIDTH = "300px";
        public const string CONST_VALIDATION_DEFAULT_WIDTH = "10px";
        public const string CONST_COMBOBOX_DEFAULT_WIDTH = "300";
        public const int CONST_DYNAMIC_CONTROL_DEFAULT_WIDTH = 300;
        public const string CONST_NO_GROUP = "NoGroup";
        public const string CONST_USER_CONTROL_CODE = "UIUC";
        public const string CONST_DEFAULT_COMBO_WATERMARK = "Select from the list";
        public const string CONST_COMBO_EMTYITEM_ID = "ItemEmpty";
        public const string CONST_COMBO_OTHERSITEM_ID = "ItemOthers";
        public const string CONST_COMBO_ALLSITEM_ID = "ItemAll";
        #endregion

        #region Page Navigation Constants
        public const string PAGE_FILE_HANDLER = @"~/Views/Shared/FileHandler.ashx";
        public const string PAGE_ERROR = @"~/Views/Shared/ErrorMessage.aspx";
        public const string PAGE_HOME = @"~/Views/UserFunctions/EmployeeSelfService.aspx";
        public const string PAGE_UNDER_MAINTENANCE = @"~/Views/Shared/UnderMaintenance.aspx";
        public const string PAGE_UNDER_CONSTRUCTION = @"~/Views/Shared/UnderConstruction.aspx";
        public const string PAGE_SESSION_TIMEOUT_PAGE = @"~/Views/Shared/SessionTimeoutPage.aspx";
        public const string PAGE_REPORT_VIEWER = @"~/Views/Reports/ReportViewer.aspx";
        public const string PAGE_EMPLOYEE_SEARCH = @"~/Views/Shared/EmployeeLookup.aspx";
        public const string PAGE_LOGIN = @"~/Views/Shared/Login.aspx";
        public const string PAGE_VISITOR_PASS_ENTRY = @"~/Views/UserFunctions/VisitorPassEntry.aspx";
        public const string PAGE_VISITOR_PASS_INQUIRY = @"~/Views/UserFunctions/VisitorPassInquiry.aspx";
        public const string PAGE_SHIFT_PATTERN_CHANGES_INQ = @"~/Views/HRFunctions/ShiftPatternChanges.aspx";
        public const string PAGE_SHIFT_PATTERN_CHANGE_ENTRY = @"~/Views/HRFunctions/ShiftPatternChangeEntry.aspx";
        public const string PAGE_WORKING_COSTCENTER_INQ = @"~/Views/HRFunctions/AssignWorkingCostCenterInq.aspx";
        public const string PAGE_WORKING_COSTCENTER_ENTRY = @"~/Views/HRFunctions/AssignWorkingCostCenterEntry.aspx";
        public const string PAGE_EMPLOYEE_EXCEPTIONAL_INQUIRY = @"~/Views/HRFunctions/EmployeeExceptionalInq.aspx";
        public const string PAGE_ONLEAVE_BUTSWIPED_INQUIRY = @"~/Views/HRFunctions/OnLeaveButSwipedInq.aspx";
        public const string PAGE_RESIGNED_BUTSWIPED_INQUIRY = @"~/Views/HRFunctions/ResignedButSwipedInq.aspx";
        public const string PAGE_LONG_ABSENCES_INQUIRY = @"~/Views/HRFunctions/LongAbsenceInq.aspx";
        public const string PAGE_TIMESHEET_INTEGRITY = @"~/Views/HRFunctions/TimesheetIntegrity.aspx";
        public const string PAGE_MANUAL_TIMESHEET_INQ = @"~/Views/HRFunctions/ManualTimesheetInq.aspx";
        public const string PAGE_MANUAL_TIMESHEET_ENTRY = @"~/Views/HRFunctions/ManualTimesheetEntry.aspx";
        public const string PAGE_CONTRACTOR_SEARCH = @"~/Views/Shared/ContractorLookup.aspx";
        public const string PAGE_REASON_ABSENCE_INQ = @"~/Views/HRFunctions/ReasonOfAbsenceInq.aspx";
        public const string PAGE_REASON_ABSENCE_ENTRY = @"~/Views/HRFunctions/ReasonOfAbsenceEntry.aspx";
        public const string PAGE_TIMESHEET_EXCEPTIONAL = @"~/Views/HRFunctions/TimesheetByPeriodInq.aspx";
        public const string PAGE_TIMESHEET_CORRECTION_HISTORY = @"~/Views/HRFunctions/TimesheetCorrectionHistory.aspx";
        public const string PAGE_TIMESHEET_CORRECTION_INQUIRY = @"~/Views/HRFunctions/TimesheetCorrectionInq.aspx";
        public const string PAGE_TIMESHEET_CORRECTION_ENTRY = @"~/Views/HRFunctions/TimesheetCorrectionEntry.aspx";
        public const string PAGE_EMERGENCY_RESPONSE_TEAM = @"~/Views/SecurityModule/EmergencyResponseTeam.aspx";
        public const string PAGE_MANUAL_ATTENDANCE = @"~/Views/SecurityModule/ManualAttendance.aspx";
        public const string PAGE_EMPLOYEE_ATTENDANCE_DASHBOARD = @"~/Views/UserFunctions/AttendanceDashboard.aspx"; 
        public const string PAGE_CONTRACTOR_ATTENDANCE_INQUIRY = @"~/Views/Reports/ContractorAttendanceInq.aspx";
        public const string PAGE_OT_MEALVOUCHER_APPROVAL = @"~/Views/HRFunctions/OvertimeApproval.aspx";
        public const string PAGE_DUTY_ROTA_INQ = @"~/Views/UserFunctions/DutyROTAInq.aspx";
        public const string PAGE_DUTY_ROTA_ENTRY = @"~/Views/UserFunctions/DutyROTAEntry.aspx";
        public const string PAGE_VIEW_SHIFT_PATTERN = @"~/Views/UserFunctions/EmpShiftPatternInq.aspx";
        public const string PAGE_CURRENT_SHIFT_PATTERN_ENTRY = @"~/Views/UserFunctions/EmpShiftPatternEntry.aspx";
        public const string PAGE_SHIFT_PROJECTION_REPORT = @"~/Views/Reports/ShiftProjection.aspx";
        public const string PAGE_EMPLOYEE_ATTENDANCE_HISTORY_REPORT = @"~/Views/Reports/EmpAttendanceHistory.aspx";
        public const string PAGE_GARMCO_CALENDAR = @"~/Views/HRFunctions/GARMCOCalendar.aspx";
        public const string PAGE_COST_CENTER_MANAGERS = @"~/Views/HRFunctions/ManagerList.aspx";
        public const string PAGE_DUTY_ROTA_REPORT = @"~/Views/Reports/DutyROTAReportFilter.aspx";
        public const string PAGE_DAILY_ATTENDANCE_REPORT = @"~/Views/Reports/DailyAttendanceReportFilter.aspx";
        public const string PAGE_VIEW_ATTENDANCE_HISTORY = @"~/Views/Reports/ViewAttendanceHistory.aspx";
        public const string PAGE_ABSENCE_REASON_REPORT = @"~/Views/Reports/AbsenceReasonReportFilter.aspx";
        public const string PAGE_LATE_DUTY_ROTA_REPORT = @"~/Views/Reports/LateEntryDutyROTAReportFilter.aspx";
        public const string PAGE_DAYINLIEU_REPORT = @"~/Views/Reports/DILReportFilter.aspx";
        public const string PAGE_VIEW_DIL_HISTORY = @"~/Views/Reports/ViewDILHistory.aspx";
        public const string PAGE_WEEKLY_OVERTIME_REPORT = @"~/Views/Reports/WeeklyOvertimeReportFilter.aspx";
        public const string PAGE_EMPLOYEE_DIRECTORY = @"~/Views/HRFunctions/EmployeeDirectoryInq.aspx";
        public const string PAGE_ASPIRE_PAYROLL_REPORT = @"~/Views/HRFunctions/AspirePayrollInq.aspx";
        public const string PAGE_PUNCTUALITY_REPORT = @"~/Views/Reports/PunctualityReportFilter.aspx";
        public const string PAGE_TAS_JDE_COMPARISON_REPORT = @"~/Views/HRFunctions/TASJDEComparisonReport.aspx";
        public const string PAGE_CONTRACTOR_SHIFT_PATTERN_INQ = @"~/Views/HRFunctions/ContractorShiftPatternInq.aspx";
        public const string PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY = @"~/Views/HRFunctions/ContractorShiftPatternEntry.aspx";
        public const string PAGE_DAILY_ATTENDANCE_SALARY_STAFF = @"~/Views/Reports/DailyAttendanceReportSL.aspx";
        public const string PAGE_COST_CENTER_ACCESS_INQ = @"~/Views/AdminFunctions/CostCenterPermission.aspx";
        public const string PAGE_COST_CENTER_ACCESS_ENTRY = @"~/Views/AdminFunctions/CostCenterPermissionEntry.aspx";
        public const string PAGE_WEEKLY_PUNCTUALITY_REPORT = @"~/Views/Reports/WeeklyEmpPunctualityFilter.aspx";
        public const string PAGE_OVERTIME_ENTRY = @"~/Views/UserFunctions/OvertimeMealVoucherEntry.aspx";
        public const string PAGE_OVERTIME_APPROVAL = @"~/Views/HRFunctions/OTMealVoucherApproval.aspx";
        public const string PAGE_OVERTIME_REQUISITION_INQUIRY = @"~/Views/HRFunctions/OTRequisitionInquiry.aspx";
        public const string PAGE_OVERTIME_APPROVAL_HISTORY = @"~/Views/UserFunctions/OvertimeApprovalHistory.aspx";
        public const string PAGE_REASSIGNMENT_FORM = @"~/Views/Shared/ReassignmentForm.aspx";
        public const string PAGE_UNPUNCTUAL_EMP_SUMMARY = @"~/Views/Reports/EmpPunctualityByPeriod.aspx";
        public const string PAGE_WORKING_COSTCENTER_HISTORY = @"~/Views/HRFunctions/AssignWorkingCostCenterHistory.aspx";
        public const string PAGE_MASTER_SHIFT_PATTERN_SETUP = @"~/Views/AdminFunctions/MasterShiftPatternSetup.aspx";
        public const string PAGE_USER_FORM_ACCESS = @"~/Views/AdminFunctions/UserFormAccessInq.aspx";
        public const string PAGE_TIMESHEET_PROCESS_SPU_ANALYSIS = @"~/Views/AdminFunctions/TimesheetProcessAnalysis.aspx";
        public const string PAGE_EMPLOYEE_ABSENCES_REPORT_FILTER = @"~/Views/Reports/EmpAbsencesReportFilter.aspx";
        #endregion

        #region User Controls
        public const string CONST_UC_AFFECTED_ENTITIES_PATH = @"~/Views/CommonControls/AffectedEntity.ascx";
        #endregion

        #region Page Title Constants
        public const string MASTER_PAGE_TITLE = @"Time & Attendance System";
        public const string TRAINING_MASTER_PAGE_TITLE = "Training Management System";
        public const string PAGE_ERROR_TITLE = "Application Error";
        public const string PAGE_SECURITY_ERROR_TITLE = "Security Access Violation Error";
        public const string PAGE_EMPLOYEE_LOOKUP_TITLE = "Employee Lookup";
        public const string PAGE_REPORT_VIEWER_TITLE = "Report Viewer Page";
        public const string PAGE_LOGIN_PAGE_TITLE = "System Login Page";
        public const string PAGE_EMPLOYEE_SELF_SERVICE_TITLE = "Employee Self Service";
        public const string PAGE_VISITOR_PASS_ENTRY_TITLE = @"Visitor's Pass Entry";
        public const string PAGE_VISITOR_PASS_INQUIRY_TITLE = @"Visitor's Pass Inquiry";
        public const string PAGE_GARMCO_CALENDAR_TITLE = "GARMCO Calendar";
        public const string PAGE_COSTCENTER_MANAGER_TITLE = "Cost Center Managers";
        public const string PAGE_SHIFT_PATTERN_CHANGE_INQUIRY_TITLE = "Shift Pattern Change (Employee)";
        public const string PAGE_SHIFT_PATTERN_CHANGE_FIRETEAM_INQUIRY_TITLE = "Shift Pattern Change (Fire Team)";
        public const string PAGE_SHIFT_PATTERN_CHANGE_ENTRY_TITLE = "Shift Pattern Change Entry (Employees)";
        public const string PAGE_WORKING_COSTCENTER_INQUIRY_TITLE = @"Assign Temporary Working Cost Center & Special Job Catalog (Inquiry)";
        public const string PAGE_WORKING_COSTCENTER_ENTRY_TITLE = @"Assign Temporary Working Cost Center & Special Job Catalog (Data Entry)";
        public const string PAGE_EMPLOYEE_EXCEPTIONAL_INQUIRY_TITLE = "Employee Exceptional Inquiry";
        public const string PAGE_ONLEAVE_BUTSWIPED_INQUIRY_TITLE = "On Annual Leave But Swiped Inquiry";
        public const string PAGE_RESIGNED_BUTSWIPED_INQUIRY_TITLE = "Resigned But Swiped Inquiry";
        public const string PAGE_LONG_ABSENCES_INQUIRY_TITLE = "Long Absences Inquiry";
        public const string PAGE_TIMESHEET_INTEGRITY_TITLE = "Timesheet Integrity by Correction Code";
        public const string PAGE_MANUAL_TIMESHEET_INQUIRY_TITLE = "Manual Timesheet Inquiry";
        public const string PAGE_MANUAL_TIMESHEET_ENTRY_TITLE = "Manual Timesheet Entry";
        public const string PAGE_CONTRACT_EMPLOYEE_SEARCH_TITLE = "Contract Employee Search";
        public const string PAGE_REASON_OF_ABSENCE_INQUIRY_TITLE = "Reason of Absence Inquiry";
        public const string PAGE_REASON_OF_ABSENCE_ENTRY_TITLE = "Reason of Absence Entry";
        public const string PAGE_TIMESHEET_EXCEPTIONAL_TITLE = "Timesheet by Pay Period";
        public const string PAGE_TIMESHEET_CORRECTION_HISTORY_TITLE = "Attendance History";
        public const string PAGE_TIMESHEET_CORRECTION_TITLE = "Timesheet Correction";
        public const string PAGE_EMERGENCY_RESPONSE_TEAM_TITLE = "Emergency Response Team";
        public const string PAGE_MANUAL_ATTENDANCE_TITLE = "Manual Attendance";
        public const string PAGE_SYSTEM_LOGIN_TITLE = "System Login";
        public const string PAGE_CONTRACTOR_ATTENDANCE_INQUIRY_TITLE = "Contractor Attendance Report";
        public const string PAGE_ATTENDANCE_DASHBOARD_TITLE = "Employee Attendance Dashboard";
        public const string PAGE_OVERTIME_APPROVAL_TITLE = "Overtime and Meal Voucher Approval";
        public const string PAGE_DUTY_ROTA_INQUIRY_TITLE = "Duty ROTA Inquiry";
        public const string PAGE_DUTY_ROTA_ENTRY_TITLE = "Duty ROTA Entry";
        public const string PAGE_VIEW_CURRENT_SHIFT_PATTERN_TITLE = "View Current Shift Pattern (Employee)";
        public const string PAGE_VIEW_CURRENT_SHIFT_PATTERN_ENTRY_TITLE = "Employee Current Shift Pattern Entry";
        public const string PAGE_SHIFT_SCHEDULE_REPORT_TITLE = "Shift Schedule Report";
        public const string PAGE_ATTENDANCE_HISTORY_REPORT_TITLE = "Employee Attendance History Report";
        public const string PAGE_DUTY_ROTA_REPORT_TITLE = "Duty ROTA Report";
        public const string PAGE_DAILY_ATTENDANCE_REPORT_TITLE = "Daily Attendance Report";
        public const string PAGE_DAILY_ATTENDANCE_SALARY_STAFF_TITLE = "Daily Attendance for Salary Staff";
        public const string PAGE_VIEW_EMPLOYEE_ATTENDANCE_HISTORY_TITLE = "View Employee Attendance History";
        public const string PAGE_ABSENCE_REASON_REPORT_TITLE = "Absence Reason Report";
        public const string PAGE_LATE_DUTY_ROTA_REPORT_TITLE = "DIL Due to Late Entry of Duty Rota Report";
        public const string PAGE_DIL_REPORT_TITLE = "Day In Lieu Report";
        public const string PAGE_VIEW_DIL_HISTORY_HISTORY_TITLE = "View Employee DIL History";
        public const string PAGE_WEEKLY_OVERTIME_REPORT_TITLE = "Weekly Overtime Report";
        public const string PAGE_EMPLOYEE_DIRECTORY_TITLE = "Employee Directory";
        public const string PAGE_ASPIRE_EMPLOYEES_PAYROLL_REPORT_TITLE = "Aspire Employees Payroll Report";
        public const string PAGE_PUNCTUALITY_REPORT_TITLE = "Punctuality Statistics Report";
        public const string PAGE_TASJDECOMPARISON_TITLE = "TAS and JDE Comparison Report";
        public const string PAGE_CONTRACTOR_SHIFT_PATTERN_INQUIRY_TITLE = "Contractors Shift Pattern Inquiry";
        public const string PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY_TITLE = "Contractors Shift Pattern Entry";
        public const string PAGE_COST_CENTER_SECURITY_SETUP_TITLE = "Cost Center Security";
        public const string PAGE_USER_FORM_ACCESS_TITLE = "User Form Access";
        public const string PAGE_WEEKLY_PUNCTUALITY_REPORT_TITLE = "Weekly Employee Punctuality Report";
        public const string PAGE_EMPLOYEE_OVERTIME_ENTRY_TITLE = "Employee Overtime Entry";
        public const string PAGE_EMPLOYEE_OVERTIME_APPROVAL_TITLE = @"Overtime & Meal Voucher Approval";
        public const string PAGE_OVERTIME_APPROVAL_HISTORY_TITLE = @"Overtime & Meal Voucher Approval History";
        public const string PAGE_APPROVAL_REASSIGNMENT_FORM_TITLE = "Overtime Approval Reassignment";
        public const string PAGE_OVERTIME_REQUISITION_INQ_TITLE = "Overtime Requisition Inquiry";
        public const string PAGE_UNPUNCTUAL_EMP_SUMMARY_REPORT_TITLE = "Unpunctual Employees Summary";
        public const string PAGE_WORKING_COSTCENTER_HISTORY_TITLE = @"Assign Temporary Working Cost Center & Special Job Catalog (History)";
        public const string PAGE_MASTER_SHIFT_PATTERN_SETUP_TITLE = "Master Shift Pattern Setup";
        public const string PAGE_SERVICE_LOG_DETAIL_TITLE = @"Timesheet Process / SPU Service Log Analysis";
        public const string PAGE_EMPLOYEE_ABSENCES_SUMMARY_REPORT_TITLE = "Employee Absences Summary Report";
        public const string PAGE_CONTRACTOR_REGISTRATION_TITLE = "Contractor Registration";
        public const string PAGE_CONTRACTOR_INQUIRY_TITLE = "Contractor Inquiry";
        public const string PAGE_ID_CARD_GENERATOR_TITLE = "ID Card Generator";
        #endregion

        #region Query String Constants
        public const string QUERY_STRING_EMPLOYEE_STATUS_KEY = "EmpStatusKey";
        public const string QUERY_STRING_EMPNO_KEY = "EmpNoKey";
        public const string QUERY_STRING_EMPNAME_KEY = "EmpNameKey";
        public const string QUERY_STRING_PAY_GRADE_KEY = "PayGradeKey";
        public const string QUERY_STRING_EMP_EMAIL_KEY = "EmpEmailKey";
        public const string QUERY_STRING_COSTCENTER_KEY = "CostCenterKey";
        public const string QUERY_STRING_COSTCENTER_NAME_KEY = "CostCenterNameKey";
        public const string QUERY_STRING_WORKINGCOSTCENTER_KEY = "WorkingCostCenterKey";
        public const string QUERY_STRING_WORKINGCOSTCENTER_NAME_KEY = "WorkingCostCenterNameKey";
        public const string QUERY_STRING_DEPARTMENT_KEY = "DeptKey";
        public const string QUERY_STRING_COSTCENTER_MANAGER_KEY = "ManagerKey";
        public const string QUERY_STRING_SUPERVISOR_KEY = "SupervisorKey";
        public const string QUERY_STRING_SUPERVISOR_NO_KEY = "SupervisorNoKey";
        public const string QUERY_STRING_SUPERVISOR_NAME_KEY = "SuprevisorNameKey";
        public const string QUERY_STRING_POSITION_KEY = "PositionKey";
        public const string QUERY_STRING_EXTENSION_KEY = "ExtensionKey";
        public const string QUERY_STRING_LOGIN_EMPNO_KEY = "LoginEmpNoKey";
        public const string QUERY_STRING_CALLER_FORM_KEY = "CallerFormKey";
        public const string QUERY_STRING_REPORT_TYPE_KEY = "ReportTypeKey";
        public const string QUERY_STRING_SWIPEDATE_KEY = "SwipeDateKey";
        public const string QUERY_STRING_SHIFTCODE_KEY = "ShiftCodeKey";
        public const string QUERY_STRING_STARTDATE_KEY = "StartDateKey";
        public const string QUERY_STRING_ENDDATE_KEY = "EndDateKey";
        public const string QUERY_STRING_STATUS_CODE_KEY = "StatusCodeKey";
        public const string QUERY_STRING_ASSIGNED_TO_KEY = "AssignedToKey";
        public const string QUERY_STRING_RELOAD_DATA_KEY = "ReloadDataKey";
        public const string QUERY_STRING_REQUISITION_NO_KEY = "RequisitionNoKey";
        public const string QUERY_STRING_IS_ASSIGNED_KEY = "IsAssignedKey";
        public const string QUERY_STRING_FORM_LOAD_TYPE = "FormLoadTypeKey";
        public const string QUERY_STRING_OPEN_REQUEST_SOURCE_KEY = "OpenReqSourceKey";
        public const string QUERY_STRING_IDENTITY_FIELD_KEY = "IdentityFieldKey";
        public const string QUERY_STRING_REPORT_TITLE_KEY = "ReportTitleKey";
        public const string QUERY_STRING_DASHBOARD_LOAD_TYPE = "DashboardKey";
        #endregion

        #region Notification Constants
        public const string CONST_DELETE_CONFIRMATION = @"Are you sure you want to delete the selected record(s)? \nPlease click Ok if yes, otherwise Cancel if no.";
        public const string CONST_DELETE_SINGLE_RECORD_CONFIRMATION = @"Are you sure you want to delete this record? \nPlease click Ok if yes, otherwise Cancel if no.";
        public const string CONST_REASSIGN_CONFIRMATION = @"Are you sure you want to reassign the selected records to other approver? \n\nPlease click Ok if yes, otherwise Cancel if no.";
        public const string CONST_ASSIGN_TOME_CONFIRMATION = @"Are you sure you want to reassign the selected records to yourself? \n\nPlease click Ok if yes, otherwise Cancel if no.";
        public const string CONST_SHOW_VISITOR_OFFENSE = @"The specified visitor has been block by Security Department. Do you want to view the previous visit record? \n\nPlease click Ok if yes, otherwise Cancel if no.";
        public const string CONST_CANCEL_OVERTIME_CONFIRMATION = @"Are you sure you want to cancel this overtime request? \nPlease click Ok if yes, otherwise Cancel if no.";
        public const string CONST_DELETE_SHIFTPATTERN_CONFIRMATION = @"Are you sure you want to delete the selected shift pattern and all associated records? Please click Ok if yes, otherwise Cancel if no.";
        public const string CONST_CANCEL_OT_REQUISITION_CONFIRMATION = @"Are you sure you want to cancel the selected overtime request(s)? \nPlease click Ok if yes, otherwise Cancel if no.";
        #endregion

        #region Regex Constants
        public const string CONST_EMAIL_FORMAT = @"^(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.]([ ]*)?(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$";
        public const string CONST_24HOUR_TIME_FORMAT = @"^(?:0?[0-9]|1[0-2]):[0-5][0-9] [ap]m$";
        public const string CONST_12HOUR_TIME_FORMAT = @"^(?:[01][0-9]|2[0-3]):[0-5][0-9]$";
        public const string CONST_24AND12HOUR_TIME_FORMAT = @"^(?:(?:0?[0-9]|1[0-2]):[0-5][0-9] [ap]m|(?:[01][0-9]|2[0-3]):[0-5][0-9])$";
        #endregion

        #region Workflow Constants
        public const int CONST_SMS_APPLICATION_ID = 1;
        public const string CONST_SMS_APPLICATION_NAME = @"GARMCO Time & Attendance System";
        public const string CONST_NOT_PERSISTED_ERROR_KEY = "not yet been persisted to the instance";
        public const string CONST_NOT_INSTANTIATED_ERROR_KEY = "not associated to an instance";
        public const string CONST_CONDITION_TYPE_IF = "CONDIF";
        public const string CONST_CONDITION_TYPE_AND = "CONDAND";
        public const string CONST_CONDITION_TYPE_OR = "CONDOR";
        public const string CONST_CONDITION_OPERATOR_EQUAL = "EQUAL";
        public const string CONST_STATUS_CODE_ALL = "All Status";
        public const string STATUS_HANDLING_CODE_APPROVED = "Approved";
        public const string STATUS_HANDLING_CODE_CANCELLED = "Cancelled";
        public const string STATUS_HANDLING_CODE_CLOSED = "Closed";
        public const string STATUS_HANDLING_CODE_OPEN = "Open";
        public const string STATUS_HANDLING_CODE_REJECTED = "Rejected";
        public const string STATUS_HANDLING_CODE_VALIDATED = "Validated";
        #endregion

        #region GARMCO Constants
        public const string GARMCO_EMP_INFO = "GARMCO_EmpInfo";
        public const string GARMCO_FULLNAME = "GARMCO_Fullname";
        public const string GARMCO_SMTP_SERVER = "GARMCO_SMTP";
        public const string GARMCO_USER_COMPANY = "GARMCO_UserCompany";
        public const string GARMCO_USER_COST_CENTER = "GARMCO_UserCostCenter";
        public const string GARMCO_USER_COST_CENTER_NAME = "GARMCO_UserCostCenterName";
        public const string GARMCO_USER_DESTINATION = "GARMCO_UserDestination";
        public const string GARMCO_USER_DOB = "GARMCO_UserDateOfBirth";
        public const string GARMCO_USER_EMAIL = "GARMCO_UserEmail";
        public const string GARMCO_USER_EMP_CLASS = "GARMCO_UserEmpClass";
        public const string GARMCO_USER_EXT = "GARMCO_UserExt";
        public const string GARMCO_USER_GENDER = "GARMCO_UserGender";
        public const string GARMCO_USER_PAY_GRADE = "GARMCO_UserPayGrade";
        public const string GARMCO_USER_POSITION_DESC = "GARMCO_UserPositionDesc";
        public const string GARMCO_USER_POSITION_ID = "GARMCO_UserPositionID";
        public const string GARMCO_USER_SERVICE_CENTER = "GARMCO_UserServiceCenter";
        public const string GARMCO_USER_SUPERVISOR_LEAVEREASON = "GARMCO_UserSupervisorLeaveReason";
        public const string GARMCO_USER_SUPERVISOR_NAME = "GARMCO_UserSupervisorName";
        public const string GARMCO_USER_SUPERVISOR_NO = "GARMCO_UserSupervisorNo";
        public const string GARMCO_USER_TICKET_CLASS = "GARMCO_UserTicketClass";
        public const string GARMCO_USERID = "GARMCO_UserID";
        public const string GARMCO_USERNAME = "GARMCO_Username";
        public const string GARMCO_WF_RUNTIME = "GRMWFRuntime";
        public const string GARMCO_USER_IS_ADMIN = "IsUserAdminKey";
        public const string GARMCO_USER_IS_SPECIAL = "IsSpecialUserKey";
        public const string GARMCO_USER_IS_GROUP_ACCOUNT = "IsGroupAccount";
        #endregion

        #region Other Constants
        public const string INCIDENT_SERVICE_PATH = @"/GARMCO.AMS.IMS.WCF/IncidentService.svc";
        public const string EXCEPTION_ERROR = "ExceptionError";
        public const string FORM_ACCESS_DEFAULT = "0000000000";
        public const string PAGE_ERROR_SESSION_END = "PageSession";
        public const string PAGE_WCF_SESSION = "WCFSession";
        public const string PAGE_DAL_SESSION = "DALSession";
        public const string PAGE_WORKFLOW_SESSION = "WorkflowSession";
        public const string PAGE_COMMON_WORKFLOW_SESSION = "CommonWorkflowSession";
        public const string PAGE_RECREATE_WORKFLOW_SESSION = "RecreateWorkflowSession";
        public const string CONST_ALLOWED_COSTCENTER = "AllowedCostCenter";
        public const string CONST_HAS_COSTCENTER = "HasCostCenter";
        public const string CONST_EMPLOYEE_SEARCH_FLAG = "EmpSearchFlag";
        public const string CONST_DEFAULT_EMPTY_TEXT = "";
        public const string CONST_OTHERS_KEY = "other";
        public const string CONST_OTHERS_LABEL = "Others";
        public const string CONST_CONTROL_AUTOSIZE = "Auto";
        public const string CONST_GROUP_ALL = "All";
        public const string CONST_DOCTYPE = @"<!DOCTYPE";
        public const string CONST_FETCH_EMPLOYEE_MASTER = "FetchEmpMaster";
        public const string CONST_WORKPLACE_COST_CENTER = "WorkplaceCostCenter";
        public const string GARMCO_USER_IS_AUTHENTICATED = "GARMCO_UserIsAuthenticated";
        public const string CONST_BUTTON_SAVE = "Save";
        public const string CONST_BUTTON_UPDATE = "Update";
        public const string CONST_DEFAULT_EMP_PHOTO = "~/Images/no_picture_icon.png";
        public const string CONST_NOT_DEFINED = "Not defined";
        public const string CONST_EXCEL_FILE_TYPE = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string CONST_PUBLIC_HOLIDAY_CODE = "PH";        
        public const string CONST_PUBLIC_HOLIDAY_DESC = "Public Holiday";
        public const string CONST_RAMADAN_OT_REASON_CODE = "ROT";   
        public const string CONST_RAMADAN_OT_REASON_DESC = "OT for Ramadan";        
        public const string CONST_OT_DURATION_RAMADAN = "Overtime duration exceeds the normal limit of 2 hours during Ramadan!";
        public const string CONST_NOT_DEFINED_TEXT = "- Not defined - ";
        public const string CONST_JS_VERSION = "JSVersion";
        #endregion

        #region Email Message Constants
        public const string CONST_MEMO_TEMPLATE = @"~/Messages/MemoTemplate.xml";
        public const string CONST_APPROVER_EMAIL_TEMPLATE = @"~/Messages/ApproverEmailTemplate.xml";
        public const string CONST_REASSIGN_EMAIL_TEMPLATE = @"~/Messages/ReassignEmailTemplate.xml";
        public const string CONST_REJECTION_EMAIL_TEMPLATE = @"~/Messages/RejectionEmailTemplate.xml";
        public const string CONST_MODIFIED_OTDETAILS_EMAIL_TEMPLATE = @"~/Messages/OvertimeChangesNotification.xml";
        public const string CONST_WFCOMPLETION_EMAIL_TEMPLATE = @"~/Messages/GenericMessageTemplate.xml";
        #endregion

        #region User Guide Constants
        public const string CONST_USER_GUIDE_TYPE = "UserGuide";
        public const string CONST_USER_GUIDE_OVERTIME = @"~/UserManual/OvertimeApproval_UserManual.pdf";
        public const string CONST_USER_GUIDE_FIRE_TEAM = @"~/UserManual/EmergencyResponse_UserManual.pdf";
        #endregion

        #region UDC Keys
        public const string CONST_SPECIAL_JOB_CATALOG = "55-SJ";
        #endregion

        #region Attendance Status Constants
        public const string CONST_ARRIVAL_NORMAL = "i";
        public const string CONST_ARRIVAL_LATE = "l";
        public const string CONST_LEFT_NORMAL = "o";
        public const string CONST_LEFT_EARLY = "e";
        public const string CONST_NOT_COME_YET = "x";
        public const string CONST_MANUAL_IN = "im";
        public const string CONST_MANUAL_OUT = "om";

        public const string CONST_ARRIVAL_NORMAL_ICON = "~/Images/ArrivalNormal.ICO";
        public const string CONST_ARRIVAL_LATE_ICON = "~/Images/ArrivalLate.png";
        public const string CONST_LEFT_NORMAL_ICON = "~/Images/LeftNormal.ico";
        public const string CONST_LEFT_EARLY_ICON = "~/Images/LeftEarly.png";
        public const string CONST_NOT_COME_YET_ICON = "~/Images/NotComeYet.ICO";
        public const string CONST_MANUAL_IN_ICON = "~/Images/InManual.ICO";
        public const string CONST_MANUAL_OUT_ICON = "~/Images/OutManual.png";
        public const string CONST_NO_EMPLOYEE_PHOTO = "~/Images/no_photo_icon.png";
        public const string CONST_NO_PHOTO_MESSAGE = "Unable to view the employee photo due to access restriction or the image file was not found.";

        public const string CONST_ARRIVAL_NORMAL_NOTES = "Arrived on-time";
        public const string CONST_ARRIVAL_LATE_NOTES = "Arrived late";
        public const string CONST_LEFT_NORMAL_NOTES = "Left on-time";
        public const string CONST_LEFT_EARLY_NOTES = "Left early";
        public const string CONST_NOT_COME_YET_NOTES = "Not attended to work";
        public const string CONST_MANUAL_IN_NOTES = "Manually logged the time-in";
        public const string CONST_MANUAL_OUT_NOTES = "Manually logged the time-out";
        #endregion

        #region Search Box Navigation Pages
        public const string CONST_EMPLOYEE_SELF_SERVICE = "Employee Self Service";
        public const string CONST_EMPLOYEE_ATTENDANCE_DASHBOARD = "Employee Attendance Dashboard";
        public const string CONST_VIEW_ATTENDANCE_HISTORY = "View Attendance History";
        public const string CONST_DUTY_ROTA_ENTRY = "Duty ROTA Entry";
        public const string CONST_EMPLOYEE_OVERTIME_ENTRY = "Employee Overtime Entry";
        public const string CONST_VIEW_CURRENT_SHIFT_PATTERN = "View Current Shift Pattern (Employee)";
        public const string CONST_TIMESHEET_EXCEPTIONAL = "Timesheet Exceptional (By Pay Period)";
        public const string CONST_TIMESHEET_CORRECTION = "Timesheet Correction";
        public const string CONST_REASON_OF_ABSENCE_ENTRY = "Reason of Absence Entry";
        public const string CONST_MANUAL_TIMESHEET_ENTRY = "Manual Timesheet Entry";
        public const string CONST_OTMEAL_VOUCHER_APPROVAL = "Overtime & Meal Voucher Approval";
        public const string CONST_SHIFT_PATTERN_CHANGES_EMPLOYEE = "Shift Pattern Changes (Employee)";
        public const string CONST_SHIFT_PATTERN_CHANGES_FIRETEAM = "Shift Pattern Changes (Fire Team)";
        public const string CONST_SHIFT_PATTERN_CHANGES_CONTRACTOR = "Shift Pattern Changes (Contractors)";
        public const string CONST_ASSIGN_CONTRACTOR_SHIFT_PATTERN = "Assign Contractor Shift Pattern";
        public const string CONST_ASSIGN_TEMP_COST_CENTER = "Assign Temporary Cost Center and Special Job Catalog";
        public const string CONST_EMPLOYEE_EXCEPTIONAL_INQUIRY = "Employee Exceptional Inquiry";
        public const string CONST_LONG_ABSENCES_INQUIRY = "Long Absences Inquiry";
        public const string CONST_REASSIGNED_BUT_SWIPED = "Reasigned But Swiped";
        public const string CONST_TIMESHEET_INTEGRITY = "Timesheet Integrity by Correction Code";
        public const string CONST_EMPLOYEE_DIRECTORY = "Employee Directory";
        public const string CONST_GARMCO_CALENDAR = "GARMCO Calendar";
        public const string CONST_COST_CENTER_MANAGERS = "Cost Center Managers";
        public const string CONST_MANUAL_ATTENDANCE = "Manual Attendance";
        public const string CONST_EMERGENCY_RESPONSE_TEAM = "Emergency Response Team";
        public const string CONST_VISITOR_PASS_INQUIRY = "Visitor Pass Inquiry";
        public const string CONST_VISITOR_PASS_ENTRY = "Visitor Pass Entry";
        public const string CONST_COST_CENTER_SECURITY_SETUP = "Cost Center Security Setup";
        public const string CONST_FORM_SECURITY_SETUP = "Form Security Setup";
        public const string CONST_MASTER_SHIFT_PATTERN_SETUP = "Master Shift Pattern Setup";
        public const string CONST_MASTER_TABLE_SETUP = "Master Table Setup";
        public const string CONST_TIMESHEET_VALIDATION_SETUP = "Timesheet Validations Setup";
        public const string CONST_SITE_VISITOR_LOG = "Site Visitor Log Setup";
        public const string CONST_SHIFT_PATTERN_UPDATE_LOG = "Shift Pattern Update Service Log";
        public const string CONST_TIMESHEET_PROCESSING_LOG = "Timesheet Processing Service Log";
        public const string CONST_ABSENCE_REASON_REPORT = "Absence Reason Report";
        public const string CONST_SHIFT_PROJECTION_REPORT = "Shift Projection Report";
        public const string CONST_CONTRACTOR_ATTENDANCE_REPORT = "Contractor Attendance Report";
        public const string CONST_EMPLOYEE_ATTENDANCE_HISTORY_REPORT = "Employee Attendance History";
        public const string CONST_EMPLOYEE_DAILY_ATTENDANCE_REPORT= "Employee Daily Attendance";
        public const string CONST_DAILY_ATTENDANCE_FOR_SALARY_STAFF= "Daily Attendance for Salary Staff";
        public const string CONST_PUNCTUALITY_REPORT = "Punctuality Statistics Report";
        public const string CONST_WEEKLY_PUNCTUALITY_REPORT = "Weekly Employee Punctuality Report";
        public const string CONST_DAY_IN_LIEU_REPORT = "Day In Lieu Report";
        public const string CONST_DUTY_ROTA_REPORT = "Duty ROTA Report";
        public const string CONST_DIL_LATE_ENTRY_REPORT = "DIL Due to Late Entry of Duty ROTA Report";
        public const string CONST_ASPIRE_PAYROLL_REPORT_REPORT = "Aspire Employees Payroll Report";
        public const string CONST_WEEKLY_OVERTIME_REPORT = "Weekly Overtime Report";
        public const string CONST_TAS_JDE_COMPARISON_REPORT = "TAS and JDE Comparison Report";
        public const string CONST_OT_REQUISITION_INQUIRY = "Overtime Requisition Inquiry";
        public const string CONST_USER_FORM_ACCESS = "User Form Access";
        public const string CONST_TIMESHEET_PROCESS_SPU_ANALYSIS = "Timesheet Process and SPU Service Analysis";
        #endregion

        #endregion

        #region Enumerations
        public enum EmployeeInfoSearchType
        {
            SearchByEmpNo,
            SearchByUserID
        }

        public enum PageErrorCodes
        {
            NoAccessOnPage = 1,
            SessionExpired,
            NotAllowedToCreate
        }

        public enum FormAccessCodes
        {
            EMPSELFSVC,         // EMPLOYEE SELF SERVICE
            VPASSENTRY,         // Visitor Pass Entry
            VPASSINQ,           // Visitor Pass Inquiry
            GARMCOCAL,          // GARMCO Calendar
            MANGRLIST,          // Cost Center Managers' List
            SHFTPATINQ,         // Shift Pattern Change (Employee)            
            SHFTPATENT,         // Shift Pattern Changes Entry (For Employees)
            WORKCCINQ,          // Assign Temporary Working Cost Center & Special Job Catalog (Inquiry)
            WORKCCENTY,         // Assign Temporary Working Cost Center & Special Job Catalog (Data Entry)
            EMPEXCPINQ,         // Employee Exceptional Inquiry
            LEAVESWIPE,         // On Annual leave But Swiped
            RESIGNSWPE,         // Resigned But Swiped
            LONGABSENT,         // Long Absences Inquiry
            TSINTEGRTY,         // Timesheet Integrity by Correction Code
            MANUALTS,           // Manual Timesheet Inquiry
            MANLTSENTY,         // Manual Timesheet Entry
            CONTCRLKUP,         // Contract Employee Search
            ABSENCEINQ,         // Reason of Absence (Inquiry)
            ABSENTENTR,         // Reason of Absence (Entry)
            TSEXCEPTNL,         // Timesheet Exceptional by Pay Period
            TSCORRECTN,         // Timesheet Correction History
            TSCOREKINQ,         // Timesheet Correction Inquiry
            TSCOREKENT,         // Timesheet Correction Entry
            TASREPORTS,         // Report Viewer Page
            FIRETEAM,           // Emergency Response Team
            MANUALSWIP,         // Manual Attendance
            SYSLOGIN,           // System Login Page
            CONTRATEND,         // Contractor Attendance Inquiry
            ATTENDDASH,         // Employee Attendance Dashboard
            OTAPPROVAL,         // Overtime & Meal Voucher Approval
            DROTAINQ,           // Duty ROTA Inquiry
            DROTAENTRY,         // Duty ROTA Entry
            VWSHIFTPAT,         // View Current Shift Pattern (Employee)
            SHFTPATDET,         // View Current Shift Pattern (Entry)
            SHIFTPROJN,         // Shift Projection Report
            RATENDHIST,         // Employee Attendance History Report
            ROTAREPORT,         // Duty ROTA Report
            DAILYREPRT,         // Daily Attendance Report
            DAILYRPTSL,         // Daily Attendance for Salary Staff
            VIEWATTEND,         // View Attendance History
            ABSENCERPT,         // Absence Reason Report
            LATEDUROTA,         // DIL Due to Late Entry of Duty Rota Report
            DILREPORT,          // Day In Lieu Report
            VIEWDILHIS,         // View Employee DIL History
            WEEKOTRPT,          // Weekly Overtime Report
            EMPDIRECT,          // Employee Directory
            ASPIREREPT,         // Aspire Employees Payroll Report
            PUNCTLYRPT,         // Punctuality Report
            TASJDECOMP,         // TAS and JDE Comparison Report
            CONTSHFINQ,         // Contractor Shift Pattern Inquiry
            CONTSHFENT,         // Contractor Shift Pattern Entry
            CCSECSETUP,         // Cost Center Security Setup
            CCSETUPENT,         // Cost Center Security Setup (Data Entry)
            SHFPATFIRE,         // Shift Pattern Change (Fire Team)
            OTENTRY,            // Overtime Entry Form
            OTAPPROVE,          // Overtime Approval Form
            OTWFHISTRY,         // Overtime Approval History
            OTINQUIRY,          // Overtime Requisition Inquiry
            REASIGNFRM,         // Overtime Approval Reassignment 
            UNPUNCTUAL,         // Unpunctual Employees Statistics Report
            FORMACCESS,         // User Form Access
            SHIFTPATRN,         // Shift Pattern Setup 
            SVCLOGDETL,         // Service Log Detail
            ABSSUMYRPT,         // Employee Absences Summary Report
            EMPCONTACT,         // Employee Emergency Contact
            CONTREGSTR,         // Contractor Registration
            CONTRCTINQ,         // Contractor Inquiry
            CONTIDCARD          // ID Card Generator
        }

        public enum PagePostBackFlags
        {
            GetEmployeeInfo,
            RedirectToOtherPage,
            ShowReport,
            ShowWorkplaceManualAttendance,
            ShowReassignmentForm,
            GetContractorInfo
        }

        public enum ReportTypes
        {
            NotDefined,
            VisitorLogReport,
            VisitorPassSummaryReport,
            ContractorAttendanceReport,
            ShiftProjectionReport,
            EmployeeAttendanceHistoryReport,
            DutyROTAReport,
            DailyAttendanceReportSalaryStaff,
            DailyAttendanceReportNonSalaryStaff,
            DailyAttendanceReportSalaryStaffOnly,
            DailyAttendanceReportAll,
            AbsenceReasonReport,
            LateDutyRotaReport,
            DayInLieuReport,
            WeeklyOvertimeReport,
            AspirePayrollReport,
            PunctualityReport,
            EmployeeWorkplaceAttendanceReport,
            PunctualitySummaryReport,
            UnpuntualEmployeeSummary,
            EmployeeAbsencesSummaryReport
        }

        public enum UserLoginOption
        {
            LoginByUsername,
            LoginByEmpNo
        }

        public enum FireTeamLoadTypes
        {
            AllFireTeamMembers,
            AvailableFireTeamMembers
        }

        public enum AttendanceCorrectionType
        {
            NoCorrectionRequired,
            CorrectWorkplaceTimeIn,
            CorrectWorkplaceTimeOut,
            CorrectWorkplaceTimeInOut
        }

        public enum UDCGroupCodes
        {
            TSINTEGRTY,         // Search Criteria used in "Timesheet Integrity by Correction Code" form
            TSOTFILTER,         // Overtime Filter Options
            SWIPSTATUS          // Overtime Requisition Status
        }

        public enum TimesheetUDCCode
        {
            ABSENCE_REASON_CODE = 1,
            CORRECTION_CODE,
            OVERTIME_TYPE,
            DIL_TYPES,
            LEAVE_TYPES,
            RELIGION_CODES,
            SUPPLIER_LIST,
            GROUP_CODES,
            COMPANY_CODES,
            SHIFT_CODES
        }

        public enum UDCSorterColumn
        {
            UDCID,
            UDCDesc1,
            UDCDesc2,
            UDCSequenceNo,
            UDCSpecialHandlingCode,
            UDCCode
        }

        public enum MissingSwipeDisplayOption
        {
            SWPALL,	        // Show missing swipes (all)
            SWPNOCHK,	    // Show missing swipes (no correction)
            SWPWITHCHK,	    // Show corrected swipes for submission
            SWPVALSWIP,     // Show valid swipes
            SWPVALMISS,     // Show valid and missing swipes
            SWIPECCC,       // Return the cost centers with missing swipes
            SWPATENHIS,     // Employee Attendance History Report
            SWPFORAPV       // Show corrected swipes for approval
        }

        public enum SaveType
        {
            NotDefined,
            Insert,
            Update,
            Delete,
            Others
        }

        public enum EmployeeInfoSearchMethod
        {
            SearchUsingWebService,
            SearchUsingCommonLibrary,
            SearchUsingEmployeeMaster
        }

        public enum OpenRequisitionSource
        {
            InquiryPage,
            EmailLink
        }

        public enum ApprovalStatus
        {
            STATALL,        // All Status
            STATOPEN,	    // Open
            STATAPPRVE,	    // Approved
            STATREJECT,	    // Rejected
            STATCANCEL	    // Cancelled
        }

        public enum WorkflowActionTypes
        {
            CreateWorkflow = 1,
            GetNextWFActivity,
            UndoRequestSubmission,
            ReassignToOtherApprover
        }

        public enum EmailRecipientType
        {
            BuiltinGroup = 1,
            IndividualEmployee,
            DistributionList
        }

        public enum FormLoadTypes
        {
            InternalPage,
            ExternalEmailLink
        }

        public enum DataLoadTypes
        {
            CreateNewRecord,
            EditExistingRecord,
            OpenReadonlyRecord
        }

        public enum ApplicationCodes
        {
            TASNEW,     // Old TAS
            TAS3,       // Time & Attendance System Ver. 3.0
            GRMTRAIN    // Training Management System
        }

        public enum DistributionGroupCodes
        {
            VISITADMIN,     // Training Administrators
            TASADMIN,       // System Administrators of TAS
            OTHRVALIDR,     // HR Validator for OT and Meal Voucher Request
            OTHRAPROVE      // HR Final Approver for OT and Meal Voucher Request
        }

        public enum UserManualType
        {
            OvertimeOnlineApproval = 1,
            EmergencyResponseTeam
        }

        public enum DashboardLoadType
        {
            Draft,
            Open,
            Assigned,
            ClosedPastWeek
        }

        public enum FormDataLoadType
        {
            OpenSpecificTimesheetRecord,
            SearchTimesheetRecord
        }

        public enum SwipeCode
        {
            MANUAL,
            MAINGATE,
            WORKPLACE
        }

        public enum CreatedByOptions
        {
            All,
            Me,
            Others
        }

        public enum SwipeTypes
        {
            IN,
            OUT,
            Unknown
        }

        public enum SwipeStatus
        {
            CheckIN,
            CheckOUT
        }

        public enum CostCenterLoadType
        {
            AllWithManagerDefined,
            WithActiveEmployeesOnly
        }

        public enum OvertimeFilter
        {
            OTUNPROCSD = 1,     // Show unprocessed overtime
            OTSUBMITED,         // Show submitted overtime
            OTAPPROVED,         // Show approved overtime 
            OTREJECTED,         // Show rejected overtime
            OTCANCELED,         // Show cancelled overtime
            OTSHOWALL           // Show all
        }

        public enum SystemErrorCode
        {
            MultithreadingError
        }

        public enum FormAccessIndex
        {
            Create, Retrieve, Update, Delete, Print
        }
        #endregion

        #region Properties
        //private static TASServiceClient WCFProxy
        //{
        //    get
        //    {
        //        string DynamicEndpointAddress = ConfigurationManager.AppSettings["WCFServiceURL"];
        //        BasicHttpBinding customBinding = ServiceHelper.GetCustomBinding();
        //        EndpointAddress endpointAddress = new EndpointAddress(DynamicEndpointAddress);
        //        TASServiceClient proxy = new TASServiceClient(customBinding, endpointAddress);

        //        #region Set the value of MaxItemsInObjectGraph to maximum so that the service can receive large files
        //        try
        //        {
        //            foreach (OperationDescription op in proxy.ChannelFactory.Endpoint.Contract.Operations)
        //            {
        //                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
        //                if (dataContractBehavior != null)
        //                {
        //                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
        //                }
        //            }

        //            return proxy;
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //        #endregion                
        //    }
        //}
        #endregion

        #region Public Methods
        public static string GetUserFirstName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return string.Empty;

            try
            {
                if (userName.ToUpper().Trim() == "WATER TREATMENT TERMINAL")
                {
                    return userName;
                }

                string result = string.Empty;
                Match m = Regex.Match(userName, @"(\w*) (\w.*)");
                string firstName = m.Groups[1].ToString();

                if (!string.IsNullOrEmpty(firstName))
                {
                    System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                    System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
                    result = textInfo.ToTitleCase(firstName.ToLower().Trim());
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetUserFirstName(string userName, bool isSpecialUser)
        {
            if (string.IsNullOrEmpty(userName))
                return string.Empty;

            if (isSpecialUser)
                return userName;

            try
            {
                string result = string.Empty;
                Match m = Regex.Match(userName, @"(\w*) (\w.*)");
                string firstName = m.Groups[1].ToString();

                if (!string.IsNullOrEmpty(firstName))
                {
                    System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                    System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
                    result = textInfo.ToTitleCase(firstName.ToLower().Trim());
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertStringToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(input.ToLower().Trim());
        }

        public static int ConvertObjectToInt(object value)
        {
            int result;
            if (value != null && int.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static byte ConvertObjectToByte(object value)
        {
            byte result;

            try
            {
                if (value != null && byte.TryParse(value.ToString(), out result))
                    return result;
                else
                    return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static long ConvertObjectToLong(object value)
        {
            long result;
            if (value != null && long.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static double ConvertObjectToDouble(object value)
        {
            double result;
            if (value != null && double.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static decimal ConvertObjectToDecimal(object value)
        {
            decimal result;
            if (value != null && decimal.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static bool ConvertObjectToBolean(object value)
        {
            bool result;
            if (value != null && bool.TryParse(value.ToString(), out result))
                return result;
            else
                return false;
        }

        public static bool ConvertNumberToBolean(object value)
        {
            if (value != null && Convert.ToInt32(value) == 1)
                return true;
            else
                return false;
        }

        public static DateTime? ConvertObjectToDate(object value)
        {
            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Trim() != "en-GB")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                }

                DateTime result;
                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DateTime? ConvertObjectToDate(object value, CultureInfo ci)
        {
            try
            {
                if (ci.Name.Trim() != "en-GB")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                }

                DateTime result;
                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ConvertObjectToString(object value)
        {
            return value != null ? value.ToString().Trim() : string.Empty;
        }

        public static string ConvertObjectToDateString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Trim() != "en-GB")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                }

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("dd-MMM-yyyy");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertObjectToDateTimeString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Trim() != "en-GB")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                }

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("dd-MMM-yyyy hh:mm tt");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertObjectToTimeString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Trim() != "en-GB")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                }

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("HH:mm:ss");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertDoubleToString(object value)
        {
            string result = string.Empty;

            try
            {
                double tempValue;
                if (value != null && double.TryParse(value.ToString(), out tempValue))
                {
                    result = string.Format("{0:N3}", tempValue);
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static Dictionary<int, string> GetSQLDataTypes()
        {
            Dictionary<int, string> codes = new Dictionary<int, string>();
            codes.Add(0, "BigInt");
            codes.Add(1, "Binary");
            codes.Add(2, "Bit");
            codes.Add(3, "Char");
            codes.Add(4, "DateTime");
            codes.Add(5, "Decimal");
            codes.Add(6, "Float");
            codes.Add(7, "Image");
            codes.Add(8, "Int");
            codes.Add(9, "Money");
            codes.Add(10, "NChar");
            codes.Add(11, "NText");
            codes.Add(12, "NVarChar");
            codes.Add(13, "Real");
            codes.Add(14, "UniqueIdentifier");
            codes.Add(15, "SmallDateTime");
            codes.Add(16, "SmallInt");
            codes.Add(17, "SmallMoney");
            codes.Add(18, "Text");
            codes.Add(19, "Timestamp");
            codes.Add(20, "TinyInt");
            codes.Add(21, "VarBinary");
            codes.Add(22, "VarChar");
            codes.Add(23, "Variant");
            codes.Add(25, "Xml");
            codes.Add(29, "Udt");
            codes.Add(30, "Structured");
            codes.Add(31, "Date");
            codes.Add(32, "Time");
            codes.Add(33, "DateTime2");
            codes.Add(34, "DateTimeOffset");
            return codes;
        }

        public static string GetDictionaryValue(Dictionary<string, string> source, string key)
        {
            if (source == null || source.Count == 0)
                return string.Empty;

            if (source.ContainsKey(key))
            {
                string result;
                if (source.TryGetValue(key, out result))
                    return result;
            }
            return string.Empty;
        }

        public static string GetDictionaryKey(Dictionary<string, string> source, string value)
        {
            if (source == null || source.Count == 0)
                return string.Empty;

            if (source.ContainsValue(value))
            {
                string result = string.Empty;
                foreach (var item in source)
                {
                    if (item.Value.Trim() == value.Trim())
                    {
                        result = item.Key;
                        break;
                    }
                }
                return result;
            }
            return string.Empty;
        }

        public static string GetEmailAddress(int empNo)
        {
            string result = string.Empty;

            try
            {
                if (empNo > 0)
                {
                    EmployeeWebService.Employee empWebSrv = new EmployeeWebService.Employee();
                    empWebSrv.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    empWebSrv.Url = ConfigurationManager.AppSettings["GARMCOWebServicesEmployeeService"].ToString();

                    EmployeeWebService.EmployeeInfo empInfo = empWebSrv.GetEmployeeByEmpNo(empNo.ToString());
                    if (empInfo != null)
                        result = empInfo.Email;
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool ValidateEmployeeInfo(string loginName, string loginPassword)
        {
            bool isValidEmployee = true;
            int retError = 0;

            try
            {

                EmployeeWebService.Employee empWebSrv = new EmployeeWebService.Employee();
                empWebSrv.Credentials = System.Net.CredentialCache.DefaultCredentials;
                empWebSrv.Url = ConfigurationManager.AppSettings["GARMCOWebServicesEmployeeService"].ToString();

                EmployeeWebService.EmployeeInfo empInfo = empWebSrv.Login(loginName, loginPassword, ref retError);
                if (empInfo == null || retError > 0)
                {
                    isValidEmployee = false;
                }

                return isValidEmployee;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string RemoveHTMLSpaceInText(string htmlText)
        {
            if (!htmlText.EndsWith("&nbsp;"))
                return htmlText;

            try
            {
                int pos = 0;
                while (htmlText.EndsWith("&nbsp;"))
                {
                    pos = htmlText.LastIndexOf("&nbsp;");
                    if (pos > 0)
                        htmlText += htmlText.Remove(pos);
                }
                return htmlText;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static void DisplayJavaScriptMessage(Control sender, string msg)
        {
            string promptTitle = "Information";
            string script = string.Format("DisplayAlert('{0}');", msg.Trim());
            ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), promptTitle, script, true);
        }

        public static void DisplayJSMessageWithPostback(Control sender, string msg, RadButton btnPostback, HiddenField hdnPostback, string actionCode)
        {
            string promptTitle = "Information";
            string script = string.Format("DisplayAlertWithPostback('{0}','{1}','{2}','{3}');", msg.Trim(), btnPostback.ClientID, hdnPostback.ClientID, actionCode);
            ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), promptTitle, script, true);
        }

        public static byte[] FromHex(string hex)
        {
            try
            {
                //hex = hex.Replace("-", "");
                byte[] raw = new byte[hex.Length / 2];
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return raw;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static DataTable ConvertListToDataTable<T>(IList<T> list)
        {
            DataTable table = null;

            try
            {
                table = CreateTable<T>();

                if (table != null)
                {
                    Type entityType = typeof(T);
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

                    foreach (T item in list)
                    {
                        DataRow row = table.NewRow();

                        foreach (PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                        table.Rows.Add(row);
                    }
                }

                return table;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static DataTable CreateTable<T>()
        {
            try
            {
                Type entityType = typeof(T);
                DataTable table = new DataTable(entityType.Name);
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);


                foreach (PropertyDescriptor prop in properties)
                {
                    // HERE IS WHERE THE ERROR IS THROWN FOR NULLABLE TYPES
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }

                return table;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EmployeeWebService.EmployeeInfo GetEmployeeInfo(int empNo, string userID = "",
            EmployeeInfoSearchType searchType = EmployeeInfoSearchType.SearchByEmpNo)
        {
            EmployeeWebService.EmployeeInfo employeeInfo = null;

            try
            {
                EmployeeWebService.Employee empWebSrv = new EmployeeWebService.Employee();
                empWebSrv.Credentials = System.Net.CredentialCache.DefaultCredentials;
                empWebSrv.Url = ConfigurationManager.AppSettings["GARMCOWebServicesEmployeeService"].ToString();

                if (searchType == EmployeeInfoSearchType.SearchByEmpNo)
                {
                    if (empNo > 0)
                        employeeInfo = empWebSrv.GetEmployeeByEmpNo(empNo.ToString());
                }
                else if (searchType == EmployeeInfoSearchType.SearchByUserID)
                {
                    if (!string.IsNullOrEmpty(userID))
                        employeeInfo = empWebSrv.GetEmployeeByDomainName(userID.Trim());
                }

                return employeeInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EmployeeInfo GetEmployeeInfo(int EmpNo)
        {
            try
            {
                int? retError = 0;
                string errorMsg = string.Empty;
                EmployeeBLL empBLL = new EmployeeBLL();
                return empBLL.GetEmployeeInfo(EmpNo, ref retError, ref errorMsg);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EmployeeDetail GetEmployeeEmailInfo(int empNo)
        {
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;
                DALProxy proxy = new DALProxy();

                List<EmployeeDetail> rawData = proxy.GetEmployeeEmailInfo(empNo, string.Empty, ref error, ref innerError);
                if (rawData != null)
                    return rawData.FirstOrDefault();
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EmployeeDetail GetEmployeeInfoAdvanced(int empNo, string userID = "",
            EmployeeInfoSearchType searchType = EmployeeInfoSearchType.SearchByEmpNo,
            EmployeeInfoSearchMethod searchMethod = EmployeeInfoSearchMethod.SearchUsingWebService)
        {
            EmployeeDetail result = null;
            dynamic empInfo = null;
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                if (searchMethod == EmployeeInfoSearchMethod.SearchUsingWebService)
                {
                    #region Get employee info using the web service
                    EmployeeWebService.Employee empWebSrv = new EmployeeWebService.Employee();
                    empWebSrv.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    empWebSrv.Url = ConfigurationManager.AppSettings["GARMCOWebServicesEmployeeService"].ToString();

                    if (searchType == EmployeeInfoSearchType.SearchByEmpNo)
                    {
                        if (empNo > 0)
                            empInfo = empWebSrv.GetEmployeeByEmpNo(empNo.ToString());
                    }
                    else if (searchType == EmployeeInfoSearchType.SearchByUserID)
                    {
                        if (!string.IsNullOrEmpty(userID))
                            empInfo = empWebSrv.GetEmployeeByDomainName(userID.Trim());
                    }

                    if (empInfo != null && ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                    {
                        result = new EmployeeDetail()
                        {
                            EmpNo = ConvertObjectToInt(empInfo.EmployeeNo),
                            EmpName = ConvertObjectToString(empInfo.FullName),
                            EmpEmail = ConvertObjectToString(empInfo.Email),
                            EmpUserID = ConvertObjectToString(empInfo.Username),
                            CostCenter = ConvertObjectToString(empInfo.CostCenter),
                            CostCenterName = ConvertObjectToString(empInfo.CostCenterName),
                            SupervisorEmpNo = ConvertObjectToInt(empInfo.SupervisorEmpNo),
                            SupervisorEmpName = ConvertObjectToString(empInfo.SupervisorEmpName),
                            SuperintendentEmpNo = ConvertObjectToInt(empInfo.SuperintendentEmpNo),
                            SuperintendentEmpName = ConvertObjectToString(empInfo.SuperintendentEmpName),
                            ManagerEmpNo = ConvertObjectToInt(empInfo.ManagerEmpNo),
                            ManagerEmpName = ConvertObjectToString(empInfo.ManagerEmpName),
                            Position = ConvertObjectToString(empInfo.PositionDesc),
                            PayGrade = ConvertObjectToInt(empInfo.PayGrade),
                            Gender = ConvertObjectToString(empInfo.Gender),
                            PositionID = ConvertObjectToString(empInfo.PositionID),
                            EmployeeClass = ConvertObjectToString(empInfo.EmployeeClass),
                            TicketClass = ConvertObjectToString(empInfo.TicketClass),
                            Destination = ConvertObjectToString(empInfo.Destination)
                        };
                    }
                    #endregion
                }
                else if (searchMethod == EmployeeInfoSearchMethod.SearchUsingEmployeeMaster)
                {
                    #region Get employee from the Employee Master                    
                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                    //var rawData = WCFProxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                    if (rawData != null)
                    {
                        result = new EmployeeDetail()
                        {
                            EmpNo = rawData.EmpNo,
                            EmpName = rawData.EmpName,
                            EmpEmail = rawData.EmpEmail,
                            EmpUserID = rawData.EmpUserID,
                            CostCenter = rawData.CostCenter,
                            CostCenterName = rawData.CostCenterName,
                            SupervisorEmpNo = rawData.SupervisorEmpNo,
                            SupervisorEmpName = rawData.SupervisorEmpName,
                            SuperintendentEmpNo = rawData.SuperintendentEmpNo,
                            SuperintendentEmpName = rawData.SuperintendentEmpName,
                            ManagerEmpNo = rawData.ManagerEmpNo,
                            ManagerEmpName = rawData.ManagerEmpName,
                            Position = rawData.Position,
                            PayGrade = rawData.PayGrade,
                            Gender = rawData.Gender,
                            PositionID = rawData.PositionID,
                            EmployeeClass = rawData.EmployeeClass,
                            TicketClass = rawData.TicketClass,
                            Destination = rawData.Destination,
                            EmployeeStatus = rawData.EmployeeStatus
                        };
                    }
                    #endregion
                }
                else
                {
                    #region Get employee info using the GARMCO Common Library
                    int? retError = 0;
                    string errorMsg = String.Empty;
                    EmployeeBLL empBLL = new EmployeeBLL();

                    if (searchType == EmployeeInfoSearchType.SearchByEmpNo)
                    {
                        if (empNo > 0)
                            empInfo = empBLL.GetEmployeeInfo(empNo, ref retError, ref errorMsg);
                    }
                    else if (searchType == EmployeeInfoSearchType.SearchByUserID)
                    {
                        if (!string.IsNullOrEmpty(userID))
                            empInfo = empBLL.GetEmployeeInfo(userID, ref retError, ref errorMsg);
                    }

                    if (empInfo != null && ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                    {
                        result = new EmployeeDetail()
                        {
                            EmpNo = ConvertObjectToInt(empInfo.EmployeeNo),
                            EmpName = ConvertObjectToString(empInfo.FullName),
                            EmpEmail = ConvertObjectToString(empInfo.Email),
                            EmpUserID = ConvertObjectToString(empInfo.Username),
                            CostCenter = ConvertObjectToString(empInfo.CostCenter),
                            CostCenterName = ConvertObjectToString(empInfo.CostCenterName),
                            SupervisorEmpNo = ConvertObjectToInt(empInfo.SupervisorEmpNo),
                            SupervisorEmpName = ConvertObjectToString(empInfo.SupervisorEmpName),
                            SuperintendentEmpNo = ConvertObjectToInt(empInfo.SuperintendentEmpNo),
                            SuperintendentEmpName = ConvertObjectToString(empInfo.SuperintendentEmpName),
                            ManagerEmpNo = ConvertObjectToInt(empInfo.ManagerEmpNo),
                            ManagerEmpName = ConvertObjectToString(empInfo.ManagerEmpName),
                            Position = ConvertObjectToString(empInfo.PositionDesc),
                            PayGrade = ConvertObjectToInt(empInfo.PayGrade),
                            Gender = ConvertObjectToString(empInfo.Gender),
                            PositionID = ConvertObjectToString(empInfo.PositionID),
                            EmployeeClass = ConvertObjectToString(empInfo.EmployeeClass),
                            TicketClass = ConvertObjectToString(empInfo.TicketClass),
                            Destination = ConvertObjectToString(empInfo.Destination)
                        };
                    }
                    #endregion
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<string> GetAllowedCostCenter(int EmpNo)
        {
            string error = string.Empty;
            string innerError = string.Empty;
            List<string> result = null;

            try
            {
                DALProxy proxy = new DALProxy();
                var source = proxy.GetPermittedCostCenter(EmpNo, ref error, ref innerError);
                if (source != null)
                {
                    result = new List<string>();
                    var costCenterList = source.OrderBy(a => a.PermitCostCenter).Select(a => a.PermitCostCenter).Distinct();

                    // Add to collection    
                    result.AddRange(costCenterList.ToList());
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<string> GetAllowedCostCenterByApp(string appCode, int empNo)
        {
            string error = string.Empty;
            string innerError = string.Empty;
            List<string> result = null;

            try
            {
                DALProxy proxy = new DALProxy();
                var source = proxy.GetPermittedCostCenterByApplication(appCode, empNo, ref error, ref innerError);
                if (source != null)
                {
                    result = new List<string>();
                    var costCenterList = source.OrderBy(a => a.PermitCostCenter).Select(a => a.PermitCostCenter).Distinct();

                    // Add to collection
                    result.AddRange(costCenterList.ToList());
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool HasAllowedCostCenter(int empNo)
        {
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var source = proxy.GetPermittedCostCenter(empNo, ref error, ref innerError);
                return source != null && source.Count() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static EmployeeWebService.EmployeeInfo ValidateEmployeeInfo(int empNo, string loginName, string loginPassword,
            UserLoginOption loginOption = UserLoginOption.LoginByUsername)
        {
            int retError = 0;

            try
            {

                EmployeeWebService.Employee empWebSrv = new EmployeeWebService.Employee();
                empWebSrv.Credentials = System.Net.CredentialCache.DefaultCredentials;
                empWebSrv.Url = ConfigurationManager.AppSettings["GARMCOWebServicesEmployeeService"].ToString();
                EmployeeWebService.EmployeeInfo empInfo = null;

                if (loginOption == UserLoginOption.LoginByUsername)
                {
                    // By Username
                    empInfo = empWebSrv.Login(loginName, loginPassword, ref retError);
                }
                else
                {
                    #region Validate by Employee No.
                    if (empNo > 0)
                    {
                        // Get the login name
                        EmployeeWebService.EmployeeInfo empInfoTemp = GetEmployeeInfoNew(empNo.ToString());
                        if (empInfoTemp != null)
                        {
                            loginName = ConvertObjectToString(empInfoTemp.Username);
                        }
                    }

                    if (!string.IsNullOrEmpty(loginName))
                        empInfo = empWebSrv.Login(loginName, loginPassword, ref retError);
                    #endregion
                }

                return empInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EmployeeInfo GetEmployeeByDomainName(string loginName)
        {
            // Initialize an instance
            EmployeeInfo empInfo = null;

            // Check the username
            if (loginName.ToLower().IndexOf("garmco\\") == -1)
                loginName = "Garmco\\" + loginName;

            // Set the LDAP Path
            string ldapPath = ConfigurationManager.AppSettings["ldapPath"];

            // Use the login name as the criteria
            string filter = String.Format("(&(objectCategory=user)(sAMAccountName={0}))",
                loginName.Split(new char[] { '\\' })[1]);

            try
            {

                using (DirectoryEntry de = new DirectoryEntry(ldapPath))
                {

                    de.AuthenticationType = AuthenticationTypes.Secure;
                    de.Username = ConfigurationManager.AppSettings["username"];
                    de.Password = ConfigurationManager.AppSettings["password"];

                    // Set the attributes to show
                    string[] attribs = new string[]{"samaccountname", "mail", "displayName", "company",
                        "department", "telephonenumber"};
                    DirectorySearcher ds = new DirectorySearcher(de, filter, attribs);

                    using (SearchResultCollection src = ds.FindAll())
                    {

                        SearchResult sr = null;

                        // Check if found
                        if (src.Count > 0)
                        {

                            sr = src[0];

                            #region Retrieve information
                            if (sr != null)
                            {

                                empInfo = new EmployeeInfo();

                                if (sr.Properties["displayName"] != null && sr.Properties["displayName"].Count > 0)
                                    empInfo.FullName = sr.Properties["displayName"][0].ToString();

                                if (sr.Properties["mail"] != null && sr.Properties["mail"].Count > 0)
                                    empInfo.Email = sr.Properties["mail"][0].ToString();

                                if (sr.Properties["samaccountname"] != null && sr.Properties["samaccountname"].Count > 0)
                                    empInfo.Username = sr.Properties["samaccountname"][0].ToString();

                                if (sr.Properties["company"] != null && sr.Properties["company"].Count > 0)
                                    empInfo.EmployeeNo = sr.Properties["company"][0].ToString();

                                if (sr.Properties["department"] != null && sr.Properties["department"].Count > 0)
                                    empInfo.CostCenter = sr.Properties["department"][0].ToString();

                                if (sr.Properties["telephoneNumber"] != null && sr.Properties["telephoneNumber"].Count > 0)
                                    empInfo.ExtensionNo = sr.Properties["telephoneNumber"][0].ToString();

                            }
                            #endregion
                        }
                    }
                }

                return empInfo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string ConvertMinutesToHour(int minutes)
        {
            string result = string.Empty;

            try
            {
                int hours = Math.DivRem(Convert.ToInt32(minutes), 60, out minutes);

                // Display the time in HH:mm format
                result = string.Format("{0}:{1}",
                    hours.ToString("00"),
                    minutes.ToString("00"));

                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string ConvertMinuteToHourString(dynamic minuteValue)
        {
            string result = string.Empty;

            try
            {
                if (minuteValue != null)
                {
                    int inputValue = Convert.ToInt32(minuteValue);
                    if (inputValue > 0)
                    {
                        int hrs = 0;
                        int min = 0;

                        hrs = Math.DivRem(inputValue, 60, out min);
                        result = string.Format("{0}:{1}",
                            string.Format("{0:00}", hrs),
                            string.Format("{0:00}", min));
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetUserFormAccessSetting(bool create, bool retrieve, bool update, bool delete, bool print)
        {
            try
            {
                StringBuilder access = new StringBuilder();

                access.Append(create ? '1' : '0');
                access.Append(retrieve ? '1' : '0');
                access.Append(update ? '1' : '0');
                access.Append(delete ? '1' : '0');
                access.Append(print ? '1' : '0');

                return access.ToString().PadRight(10, '0');
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetConfigurationValue(string key)
        {
            //Check if the key exists in the configuration file
            if (ConfigurationManager.AppSettings[key] != null)
            {
                //Return the value
                return ConfigurationManager.AppSettings[key].ToString();
            }
            else
            {
                //Key does not exist, throw an exception
                throw new IndexOutOfRangeException("Configuration file does not contain the requested key: " + key);
            }
        }
        #endregion

        #region Private Methods
        public static EmployeeWebService.EmployeeInfo GetEmployeeInfoNew(string EmpNo)
        {
            try
            {

                EmployeeWebService.Employee empWebSrv = new EmployeeWebService.Employee();
                empWebSrv.Credentials = System.Net.CredentialCache.DefaultCredentials;
                empWebSrv.Url = ConfigurationManager.AppSettings["GARMCOWebServicesEmployeeService"].ToString();
                EmployeeWebService.EmployeeInfo empInfo = empWebSrv.GetEmployeeByEmpNo(EmpNo);

                return empInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DataTable ContractorAttendanceToDataTable<T>(this IList<T> data)
        {
            try
            {
                PropertyDescriptorCollection properties =
                    TypeDescriptor.GetProperties(typeof(T));
                DataTable table = new DataTable();

                foreach (PropertyDescriptor prop in properties)
                {
                    DataColumn col = new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

                    // Add the column to the table
                    table.Columns.Add(col);
                }

                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    table.Rows.Add(row);
                }

                #region Re-order the columns
                table.Columns["EmpNo"].SetOrdinal(0);
                table.Columns["EmpName"].SetOrdinal(1);
                table.Columns["CPRNo"].SetOrdinal(2);
                table.Columns["JobTitle"].SetOrdinal(3);
                table.Columns["EmployerName"].SetOrdinal(4);
                table.Columns["CostCenter"].SetOrdinal(5);
                table.Columns["CostCenterName"].SetOrdinal(6);
                table.Columns["ReaderName"].SetOrdinal(7);
                table.Columns["SwipeDate"].SetOrdinal(8);
                table.Columns["SwipeIn"].SetOrdinal(9);
                table.Columns["SwipeOut"].SetOrdinal(10);
                table.Columns["WorkHour"].SetOrdinal(11);
                table.Columns["NetHour"].SetOrdinal(12);
                table.Columns["OvertimeHour"].SetOrdinal(13);
                table.Columns["StatusDesc"].SetOrdinal(14);                
                table.Columns["ContractStartDate"].SetOrdinal(15);
                table.Columns["ContractEndDate"].SetOrdinal(16);
                table.Columns["CreatedDate"].SetOrdinal(17);
                #endregion

                return table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Active Directory Methods
        private static EmployeeDetail GetEmployeeInAD(string employeeNo, string ldapPath, string ldapUsername, string ldapPassword)
        {
            if (string.IsNullOrEmpty(employeeNo) ||
                string.IsNullOrEmpty(ldapPath) ||
                string.IsNullOrEmpty(ldapUsername) ||
                string.IsNullOrEmpty(ldapPassword))
            {
                return null;
            }

            // Create an instance
            EmployeeDetail empInfo = null;

            // Use the login name as the criteria
            string filter = String.Format("(&(objectCategory=user)(company={0}))", employeeNo);

            try
            {

                using (DirectoryEntry de = new DirectoryEntry(ldapPath))
                {

                    de.AuthenticationType = AuthenticationTypes.Secure;
                    de.Username = ldapUsername;
                    de.Password = ldapPassword;

                    // Set the attributes to show
                    string[] attribs = new string[]{"samaccountname", "mail", "displayName", "company",
                        "department", "telephonenumber"};
                    DirectorySearcher ds = new DirectorySearcher(de, filter, attribs);

                    using (SearchResultCollection src = ds.FindAll())
                    {

                        SearchResult sr = null;

                        // Check if found
                        if (src.Count > 0)
                        {

                            sr = src[0];

                            // Retrieve information
                            if (sr != null)
                            {

                                empInfo = new EmployeeDetail();

                                if (sr.Properties["samaccountname"].Count > 0)
                                    empInfo.Username = sr.Properties["samaccountname"][0].ToString();

                                if (sr.Properties["displayName"].Count > 0)
                                    empInfo.EmpName = sr.Properties["displayName"][0].ToString();

                                if (sr.Properties["mail"].Count > 0)
                                    empInfo.EmpEmail = sr.Properties["mail"][0].ToString();

                                if (sr.Properties["company"].Count > 0)
                                    empInfo.EmpNo = UIHelper.ConvertObjectToInt(sr.Properties["company"][0].ToString());

                                if (sr.Properties["department"].Count > 0)
                                    empInfo.CostCenter = sr.Properties["department"][0].ToString();

                                if (sr.Properties["telephoneNumber"].Count > 0)
                                    empInfo.PhoneExtension = sr.Properties["telephoneNumber"][0].ToString();

                            }
                        }
                    }
                }

                return empInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Email
        public static string RetrieveXmlMessage(string xmlFile)
        {
            string message = String.Empty;
            XmlTextReader reader = null;

            try
            {
                // Read the file
                reader = new XmlTextReader(xmlFile);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Text)
                        message = reader.Value;
                }
            }

            catch
            {
            }
            finally
            {
                // Close the file
                if (reader != null)
                    reader.Close();
            }

            return message;
        }
        #endregion
    }
}