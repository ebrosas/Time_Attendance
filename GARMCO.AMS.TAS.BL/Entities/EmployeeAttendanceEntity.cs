using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class EmployeeAttendanceEntity
    {
        #region Properties
        public DateTime? SwipeDate { get; set; }        
        public DateTime? SwipeTime { get; set; }
        public DateTime? ArrivalFrom { get; set; }
        public DateTime? ArrivalTo { get; set; }
        public DateTime? DepartFrom { get; set; }
        public DateTime? DepartTo { get; set; }
        public string SwipeLocation { get; set; }
        public string SwipeType { get; set; }
        public string SwipeCode { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string EmpFullName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public int? SupervisorEmpNo { get; set; }
        public string SupervisorEmpName { get; set; }
        public string SupervisorFullName { get; set; }
        public int SuperintendentEmpNo { get; set; }
        public string SuperintendentEmpName { get; set; }
        public string SuperintendentFullName { get; set; }
        public string SuperintendentEmail { get; set; }
        public int? ManagerEmpNo { get; set; }
        public string ManagerEmpName { get; set; }
        public string ManagerFullName { get; set; }
        public string ManagerEmail { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public string ActualShiftCode { get; set; }
        public int? ShiftPointer { get; set; }
        public string ShiftDetail { get; set; }
        public string Position { get; set; }
        public int? GradeCode { get; set; }
        public int? EmployeeStatus { get; set; }
        public int DurationRequired { get; set; }
        public string ShiftTiming { get; set; }
        public bool IsContractor { get; set; }
        public bool IsDayShift { get; set; }
        public string DayShiftDesc { get; set; }
        public DateTime? OTStartTime { get; set; }
        public DateTime? OTStartTimeTE { get; set; }
        public DateTime? OTEndTime { get; set; }
        public DateTime? OTEndTimeTE { get; set; }
        public string OTType { get; set; }
        public string OTTypeTE { get; set; }
        public bool OTApproved { get; set; }
        public int OTDuration { get; set; }
        public string OTDurationDesc { get; set; }
        public int? NoPayHours { get; set; }
        public string NoPayHoursDesc { get; set; }
        public DateTime? ShavedTimeIn { get; set; }
        public DateTime? ShavedTimeOut { get; set; }
        public int AutoID { get; set; }        
        public DateTime? dtIN { get; set; }
        public DateTime? dtINLastRow { get; set; }
        public DateTime? dtOUT { get; set; }
        public int? Duration_Worked_Cumulative { get; set; }
        public string Duration_Worked_Cumulative_Desc { get; set; }
        public int? NetMinutes { get; set; }
        public string Reason { get; set; }
        public bool? IsSalaryStaff { get; set; }
        public string DurationHourString { get; set; }
        #endregion

        #region Other Properties Used in Absence History
        public DateTime? DT { get; set; }
        public string RemarkCode { get; set; }
        public string AttendanceRemarks { get; set; }
        public bool IsAttendanceRemarksModified { get; set; }
        #endregion

        #region Other Properties Used in Leave History
        public double LeaveNo { get; set; }
        public DateTime? LeaveStartDate { get; set; }
        public DateTime? LeaveEndDate { get; set; }
        public string LeaveType { get; set; }
        public string LeaveTypeDesc { get; set; }
        public double LeaveDuration { get; set; }
        #endregion

        #region Other Properties Used in Attendance History
        public string CorrectionCode { get; set; }
        public string AbsenceReasonCode { get; set; }
        public string DILEntitlement { get; set; }
        public int? LastUpdateEmpNo { get; set; }
        public string LastUpdateEmpName { get; set; }
        public string LastUpdateFullName { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        #endregion

        #region Other Properties Used in On Leave But Swiped form
        public int? Duration { get; set; }
        public bool HasMultipleSwipe { get; set; }
        #endregion

        #region Other Properties Used in Resigned But Swiped form
        public DateTime? DateResigned { get; set; }
        public string PayStatus{ get; set; }
        public int TotalRecords { get; set; }
        public DateTime? RunDate { get; set; }
        public bool IsOTDueToShiftSpan { get; set; }
        public bool IsArrivedEarly { get; set; }
        public string ArrivalSchedule { get; set; }
        public bool IsOTExceedOrig { get; set; }
        #endregion

        #region Other Properties Used in Long Absence Inquiry Form
        public string ActualCostCenter { get; set; }
        public string AttendanceHistoryValue { get; set; }
        public string AttendanceHistoryTitle { get; set; }
        #endregion

        #region Other Properties Used in Timesheet Integrity by Correction Code Form
        public int? DurationShiftAllowanceEvening { get; set; }
        public int? DurationShiftAllowanceNight { get; set; }
        public string CorrectionDesc { get; set; }
        public bool? Processed { get; set; }
        public bool? ShiftAllowance { get; set; }
        #endregion

        #region Other Properties Used in Manual Timesheet Entry Form
        public string CreatedUser { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public DateTime? SwipeIn { get; set; }
        public DateTime? SwipeOut { get; set; }
        #endregion

        #region Other Properties Used in Timesheet Exceptional by Pay Period Form
        public string ShiftAllowanceDesc { get; set; }
        public bool IsLastRow { get; set; }
        public string CorrectionCodeDesc { get; set; }
        public bool IsExceptional { get; set; }
        #endregion

        #region Other Properties used in Timesheet Correction History form
        public int XID_AutoID { get; set; }
        public DateTime? ActionDate { get; set; }
        public string ActionMachineName { get; set; }
        public string ActionType { get; set; }
        public bool IsOTAlreadyProcessed { get; set; }
        public bool IsDriver { get; set; }
        public bool IsLiasonOfficer { get; set; }
        public bool IsHedger { get; set; }
        #endregion

        #region Other Properties used in Employee Attendance Dashboard form
        public DateTime? InquiryDate { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public string InOutStatus { get; set; }
        public string ExtensionNo { get; set; }
        public DateTime? FirstTimeIn { get; set; }
        public DateTime? LastTimeOut { get; set; }
        public DateTime? RequiredTimeOut { get; set; }
        public string StatusIconPath { get; set; }
        public string StatusIconNotes { get; set; }
        public string EmployeeImagePath { get; set; }
        public string EmployeeImageTooltip { get; set; }
        #endregion

        #region Other Properties used in OT & Meal Voucher Approval form
        public string OTApprovalCode { get; set; }
        public bool IsOTApprovalCodeModified { get; set; }
        public string OTApprovalDesc { get; set; }
        public bool IsOTWFProcessed { get; set; }
        public string OTWFApprovalCode { get; set; }
        public string OTWFApprovalDesc { get; set; }
        public string MealVoucherEligibility { get; set; }
        public bool IsMealVoucherEligibilityModified { get; set; }
        public string MealVoucherEligibilityCode { get; set; }
        public int? OTDurationMinute { get; set; }
        public int? OTDurationMinuteOrig { get; set; }
        public int OTDurationHour { get; set; }
        public double OTDurationHourOrig { get; set; }
        public int OTDurationHourClone { get; set; }
        public string OTDurationText { get; set; }
        public bool IsOTDurationModified { get; set; }
        public bool? Approved { get; set; }
        public string OTReason { get; set; }
        public bool IsOTReasonModified { get; set; }
        public string OTReasonCode { get; set; }
        public bool IsDirty { get; set; }
        public bool AllowOvertime { get; set; }
        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }
        public string StatusHandlingCode { get; set; }
        public string ApprovalRole { get; set; }
        public string ServiceProviderTypeCode { get; set; }
        public string DistListCode { get; set; }
        public string DistListDesc { get; set; }
        public string DistListMembers { get; set; }
        public int? CurrentlyAssignedEmpNo { get; set; }
        public string CurrentlyAssignedEmpName { get; set; }
        public string CurrentlyAssignedFullName { get; set; }
        public string CurrentlyAssignedEmpEmail { get; set; }
        public string CurrentlyAssignedEmpPosition { get; set; }
        public long OTRequestNo { get; set; }
        public int CreatedByEmpNo { get; set; }
        public string CreatedByUserID { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public string CreatedByEmail { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? RequestSubmissionDate { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverRemarks { get; set; }
        public bool IsRemarksRequired { get; set; }
        public bool IsForHRValidation { get; set; }
        public bool IsCallOut { get; set; }
        public string TotalWorkDuration { get; set; }
        public string RequiredWorkDuration { get; set; }
        public int OTShavedTimeDuration { get; set; }
        public string OTDurationTooltip { get; set; }
        public int ExcessWorkDuration { get; set; }
        public string SelectedOTReasonCode { get; set; }
        public DateTime? OTStartTimeOrig { get; set; }
        public DateTime? OTEndTimeOrig { get; set; }
        public bool IsHold { get; set; }
        #endregion

        #region Other Properties used in "Employee Attendance History Report"
        public string ShavedWorkDurationHours { get; set; }
        public int ShavedWorkDurationMinutes { get; set; }
        public string WorkDurationHours { get; set; }
        public int WorkDurationCumulative { get; set; }
        public int WorkDurationMinutes { get; set; }
        public int OTDurationMinutes { get; set; }
        public int DayOffDuration { get; set; }
        public string OvertimeDurationHours { get; set; }
        public bool RequiredToSwipeAtWorkplace { get; set; }
        public DateTime? TimeInWP { get; set; }
        public DateTime? TimeOutWP { get; set; }
        public DateTime? TimeInMG { get; set; }
        public DateTime? TimeOutMG { get; set; }
        public string TimeInWPString { get; set; }
        public string TimeOutWPString { get; set; }
        public string IsCorrected { get; set; }
        public string IsCorrectionApproved { get; set; }
        public bool IsPublicHoliday { get; set; }
        public bool IsOTRamadanExceedLimit { get; set; }
        public bool IsRamadan { get; set; }
        #endregion

        #region Other Properties used in "DIL due to late entry of Duty Rota Report"
        public string DILDescription { get; set; }
        public int? EntryAfterDays { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsModifiedByHR { get; set; }
        #endregion

        #region Other Properties used in "DIL Report"
        public DateTime? DateUsed { get; set; }
        public string Remarks { get; set; }
        #endregion

        #region Other Properties used in "Weekly Overtime Report"
        public int TotalOTMinutes { get; set; }
        #endregion

        #region Other Properties used in "ASPIRE EMPLOYEES PAYROLL REPORT"
        public string PayHour { get; set; }
        public string PayDescription { get; set; }
        public string PaymentStatus { get; set; }
        public string PayGrade { get; set; }
        public int PayMinute { get; set; }
        #endregion

        #region Other Properties for death related timesheet corrections
        public string RelativeTypeName { get; set; }
        public string DeathRemarks { get; set; }
        #endregion
    }
}
