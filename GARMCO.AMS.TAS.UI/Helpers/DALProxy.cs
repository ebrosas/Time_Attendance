using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.BL.Helpers;
using System.Configuration;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.UI.Helpers
{
    public class DALProxy
    {
        #region Members
        private DataService _dataService;
        #endregion

        #region Constructors
        public DALProxy()
        {
            string connectStringTAS = GetTASConnectionString();
            string connectStringCommonAdmin = GetCommonAdminConnectionString();
            _dataService = new DataService(connectStringTAS, connectStringCommonAdmin);
        }
        #endregion

        #region Properties
        public DataService TASDataService
        {
            get { return _dataService; }
            set { _dataService = value; }
        }
        #endregion

        #region Private Methods
        public static string GetTASConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString.Trim();
        }

        public static string GetCommonAdminConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["GARMCOCommon"].ConnectionString.Trim();
        }
        #endregion

        #region Public Methods
        public EmployeeDetail GetEmployeeDetail(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeDetail(empNo, ref error, ref innerError);
        }

        public List<AllowedCostCenter> GetPermittedCostCenter(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetPermittedCostCenter(empNo, ref error, ref innerError);
        }

        public List<AllowedCostCenter> GetPermittedCostCenterByApplication(string appCode, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetPermittedCostCenterByApplication(appCode, empNo, ref error, ref innerError);
        }

        public List<VisitorPassEntity> GetVisitorPassLog(long? logID, string visitorName, string idNumber, int visitorCardNo, int? visitEmpNo, string visitCostCenter,
            DateTime? startDate, DateTime? endDate, byte? blockOption, int? createdByEmpNo, ref string error, ref string innerError)
        {
            return TASDataService.GetVisitorPassLog(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, visitCostCenter, startDate, endDate,
                blockOption, createdByEmpNo, ref error, ref innerError);
        }

        public void InsertUpdateDeleteVisitorPassLog(int saveTypeID, VisitorPassEntity visitorPassData, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteVisitorPassLog(saveTypeID, visitorPassData, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetSwipeHistory(DateTime? startDate, DateTime? endDate, int? empNo, string costCenter, string locationName,
            string readerName, ref string error, ref string innerError)
        {
            return TASDataService.GetSwipeHistory(startDate, endDate, empNo, costCenter, locationName, readerName, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetEmployeeInfoFromJDE(int empNo, string costCenter, bool? isActiveOnly, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeInfoFromJDE(empNo, costCenter, isActiveOnly, ref error, ref innerError);
        }

        public List<AccessReaderEntity> GetAccessReaders(int loadType, int locationCode, int readerNo, ref string error, ref string innerError)
        {
            return TASDataService.GetAccessReaders(loadType, locationCode, readerNo, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAbsencesHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetAbsencesHistory(empNo, startDate, endDate, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAbsencesHistoryv2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetAbsencesHistoryv2(empNo, startDate, endDate, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetLeaveHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetLeaveHistory(empNo, startDate, endDate, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetLeaveHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetLeaveHistoryV2(empNo, startDate, endDate, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAttendanceHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetAttendanceHistory(empNo, startDate, endDate, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAttendanceHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetAttendanceHistoryV2(empNo, startDate, endDate, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<LeaveEntity> GetLeaveDetails(int? empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetLeaveDetails(empNo, ref error, ref innerError);
        }

        public List<DILEntity> GetDILEntitlements(byte loadType, int empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetDILEntitlements(loadType, empNo, startDate, endDate, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetShiftPatternInfo(int? empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPatternInfo(empNo, ref error, ref innerError);
        }

        public List<LeaveEntity> GetGARMCOCalendar(int? year, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetGARMCOCalendar(year, startDate, endDate, ref error, ref innerError);
        }

        public List<CostCenterEntity> GetCostCenterManagerInfo(string costCenter, string companyCode, ref string error, ref string innerError)
        {
            return TASDataService.GetCostCenterManagerInfo(costCenter, companyCode, ref error, ref innerError);
        }

        public List<DependentEntity> GetDependentInfo(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetDependentInfo(empNo, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetShiftPatternChanges(int? autoID, byte? loadType, int? empNo, string changeType,
            DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPatternChanges(autoID, loadType, empNo, changeType, startDate, endDate, pageNumber, pageSize, ref error, ref innerError);
        }

        public void InsertUpdateDeleteShiftPattern(int saveTypeNum, List<ShiftPatternEntity> shiftPatternList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteShiftPattern(saveTypeNum, shiftPatternList, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetShiftPatternCodes(string shiftPatCode, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPatternCodes(shiftPatCode, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetShiftPointerCodes(string shiftPatCode, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPointerCodes(shiftPatCode, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetFireTeamMember(ref string error, ref string innerError)
        {
            return TASDataService.GetFireTeamMember(ref error, ref innerError);
        }

        public List<EmployeeDetail> GetWorkingCostCenter(int autoID, int empNo, string costCenter, string specialJobCatg, ref string error, ref string innerError)
        {
            return TASDataService.GetWorkingCostCenter(autoID, empNo, costCenter, specialJobCatg, ref error, ref innerError);
        }

        public List<UDCEntity> GetUDCListItem(string udcKey, ref string error, ref string innerError)
        {
            return TASDataService.GetUDCListItem(udcKey, ref error, ref innerError);
        }

        public void InsertUpdateDeleteWorkingCostCenter(int saveTypeID, List<EmployeeDetail> empDetailList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteWorkingCostCenter(saveTypeID, empDetailList, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetEmployeeExceptional(DateTime? startDate, DateTime? endDate, int empNo,
            bool isAbsence, bool isSickLeave, bool isNPH, bool isInjuryLeave, bool isDIL, bool isOvertime, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeExceptional(startDate, endDate, empNo, isAbsence, isSickLeave,
                isNPH, isInjuryLeave, isDIL, isOvertime, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetOnAnnualLeaveBuSwiped(DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetOnAnnualLeaveBuSwiped(startDate, endDate, empNo, costCenter, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetResignedBuSwiped(DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetResignedBuSwiped(startDate, endDate, empNo, costCenter, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetResignedBuSwipedV2(DateTime? startDate, DateTime? endDate, int empNo, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetResignedBuSwipedV2(startDate, endDate, empNo, costCenter, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetLongAbsences(DateTime? processDate, bool showSLP, bool showUL, bool showAbsent,
            ref DateTime? startDate, ref DateTime? endDate, ref string attendanceHistoryTitle, ref string error, ref string innerError)
        {
            return TASDataService.GetLongAbsences(processDate, showSLP, showUL, showAbsent,
                ref startDate, ref endDate, ref attendanceHistoryTitle, ref error, ref innerError);
        }

        public List<UserDefinedCodes> GetUserDefinedCode(string udcCode, ref string error, ref string innerError)
        {
            return TASDataService.GetUserDefinedCode(udcCode, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetTimesheetIntegrity(string actionCode, DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetTimesheetIntegrity(actionCode, startDate, endDate, empNo, costCenter, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetManualTimesheetEntry(int? autoID, int? empNo, string costCenter, DateTime? dateIN, DateTime? dateOUT,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetManualTimesheetEntry(autoID, empNo, costCenter, dateIN, dateOUT, pageNumber, pageSize, ref error, ref innerError);
        }

        public void InsertUpdateDeleteManualTimesheet(int saveTypeNum, List<EmployeeAttendanceEntity> manualTimesheetList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteManualTimesheet(saveTypeNum, manualTimesheetList, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetContractors(int empNo, string empName, ref string error, ref string innerError)
        {
            return TASDataService.GetContractors(empNo, empName, ref error, ref innerError);
        }

        public List<ReasonOfAbsenceEntity> GetReasonOfAbsenceEntry(int? autoID, int? empNo, string costCenter, DateTime? effectiveDate, DateTime? endingDate,
            string absenceReasonCode, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetReasonOfAbsenceEntry(autoID, empNo, costCenter, effectiveDate, endingDate, absenceReasonCode,
                pageNumber, pageSize, ref error, ref innerError);
        }

        public List<UserDefinedCodes> GetTimesheetUDCCodes(byte actionType, ref string error, ref string innerError)
        {
            return TASDataService.GetTimesheetUDCCodes(actionType, ref error, ref innerError);
        }

        public void InsertUpdateDeleteReasonOfAbsence(int saveTypeNum, List<ReasonOfAbsenceEntity> dataList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteReasonOfAbsence(saveTypeNum, dataList, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetTimesheetExceptional(int empNo, DateTime? startDate, DateTime? endDate, bool withExceptionOnly,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetTimesheetExceptional(empNo, startDate, endDate, withExceptionOnly, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetTimesheetHistory(int autoID, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetTimesheetHistory(autoID, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetShiftPatternChangeHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPatternChangeHistory(empNo, DT, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<ReasonOfAbsenceEntity> GetEmployeeAbsenceHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeAbsenceHistory(empNo, DT, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<LeaveEntity> GetEmployeeLeaveHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeLeaveHistory(empNo, DT, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetTimesheetCorrection(string costCenter, int empNo, DateTime? startDate, DateTime? endDate, int autoID,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetTimesheetCorrection(costCenter, empNo, startDate, endDate, autoID, pageNumber, pageSize, ref error, ref innerError);
        }

        public DatabaseSaveResult InsertUpdateDeleteTimesheet(byte actionType, int autoID, string correctionCode, string otType, DateTime? otStartTime, DateTime? otEndTime,
            int noPayHours, string shiftCode, bool? shiftAllowance, int? durationShiftAllowanceEvening, int? durationShiftAllowanceNight, string dilEntitlement,
            string remarkCode, string userID, ref string error, ref string innerError)
        {
            return TASDataService.InsertUpdateDeleteTimesheet(actionType, autoID, correctionCode, otType, otStartTime, otEndTime,
                noPayHours, shiftCode, shiftAllowance, durationShiftAllowanceEvening, durationShiftAllowanceNight, dilEntitlement, remarkCode, userID, ref error, ref innerError);
        }

        public void InsertUpdateDeleteVisitorSwipeLog(int saveTypeID, List<VisitorSwipeEntity> visitorSwipeList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteVisitorSwipeLog(saveTypeID, visitorSwipeList, ref error, ref innerError);
        }

        public List<VisitorSwipeEntity> GetVisitorSwipeHistory(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetVisitorSwipeHistory(empNo, startDate, endDate, pageNumber, pageSize, ref error, ref innerError);
        }

        public void DeleteVisitorPassMultipleRecord(List<VisitorPassEntity> visitorRecordList, ref string error, ref string innerError)
        {
            TASDataService.DeleteVisitorPassMultipleRecord(visitorRecordList, ref error, ref innerError);
        }

        public List<VisitorPassEntity> GetVisitorPassLogV2(long? logID, string visitorName, string idNumber, int visitorCardNo, int? visitEmpNo, string visitCostCenter,
            DateTime? startDate, DateTime? endDate, byte? blockOption, int? userEmpNo, int? createdByOtherEmpNo, byte? createdByTypeID,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetVisitorPassLogV2(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, visitCostCenter,
                startDate, endDate, blockOption, userEmpNo, createdByOtherEmpNo, createdByTypeID,
                pageNumber, pageSize, ref error, ref innerError);
        }

        public List<FireTeamMember> GetEmergencyResponseTeam(int actionType, DateTime processDate, int empNo, string costCenter, string imageRootPath, ref string error, ref string innerError)
        {
            return TASDataService.GetEmergencyResponseTeam(actionType, processDate, empNo, costCenter, imageRootPath, ref error, ref innerError);
        }

        public int GetLastSwipeStatus(int empNo, DateTime swipeDate, ref string error, ref string innerError)
        {
            return TASDataService.GetLastSwipeStatus(empNo, swipeDate, ref error, ref innerError);
        }

        public void SaveManualAttendance(int saveTypeID, EmployeeAttendanceEntity attendanceData, ref string error, ref string innerError)
        {
            TASDataService.SaveManualAttendance(saveTypeID, attendanceData, ref error, ref innerError);
        }

        public EmployeeAttendanceEntity GetEmployeeDetailForManualAttendance(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeDetailForManualAttendance(empNo, ref error, ref innerError);
        }

        public List<ContractorAttendance> GetContractorAttendance(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetContractorAttendance(startDate, endDate, contractorNo, contractorName, costCenter, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<ContractorAttendanceExcel> GetContractorAttendanceAll(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName,
            string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetContractorAttendanceAll(startDate, endDate, contractorNo, contractorName, costCenter, ref error, ref innerError);
        }

        public List<ContractorAttendanceExcel> GetContractorAttendanceExcel(DateTime? startDate, DateTime? endDate,
            int contractorNo, string contractorName, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetContractorAttendanceExcel(startDate, endDate, contractorNo, contractorName, costCenter, ref error, ref innerError);
        }

        public List<ContractorAttendance> GetContractorAttendanceReport(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetContractorAttendanceReport(startDate, endDate, contractorNo, contractorName, costCenter, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetEmployeeAttendance(string empName, string costCenter, DateTime? attendanceDate, string imageRootPath, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeAttendance(empName, costCenter, attendanceDate, imageRootPath, ref error, ref innerError);
        }

        public List<CostCenterEntity> GetCostCenterList(byte loadType, ref string error, ref string innerError)
        {
            return TASDataService.GetCostCenterList(loadType, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetSwipeDetails(int empNo, DateTime? attendanceDate, ref string error, ref string innerError)
        {
            return TASDataService.GetSwipeDetails(empNo, attendanceDate, ref error, ref innerError);
        }

        public List<CostCenterEntity> GetManagedCostCenter(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetManagedCostCenter(empNo, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetOvertimeAttendance(DateTime? startDate, DateTime? endDate, string costCenter, int? empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetOvertimeAttendance(startDate, endDate, costCenter, empNo, ref error, ref innerError);
        }

        public List<UDCEntity> GetOvertimeReasons(byte loadType, ref string error, ref string innerError)
        {
            return TASDataService.GetOvertimeReasons(loadType, ref error, ref innerError);
        }

        public DatabaseSaveResult SaveEmployeeOvertime(int autoID, string otReasonCode, string comment, string userID, string otApprovalCode, string mealVoucherApprovalCode,
            int otDuration, ref string error, ref string innerError)
        {
            return TASDataService.SaveEmployeeOvertime(autoID, otReasonCode, comment, userID, otApprovalCode, mealVoucherApprovalCode, otDuration, ref error, ref innerError);
        }

        public List<DutyROTAEntity> GetDutyROTAEntry(int? autoID, int? empNo, DateTime? effectiveDate, DateTime? endingDate,
            string dutyType, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetDutyROTAEntry(autoID, empNo, effectiveDate, endingDate, dutyType, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<DutyROTAEntity> GetDutyROTAType(ref string error, ref string innerError)
        {
            return TASDataService.GetDutyROTAType(ref error, ref innerError);
        }

        public void InsertUpdateDeleteDutyROTA(int saveTypeNum, List<DutyROTAEntity> dataList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteDutyROTA(saveTypeNum, dataList, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetEmployeeShiftPattern(int? autoID, int? empNo, string costCenter, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeShiftPattern(autoID, empNo, costCenter, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<ShiftPatternEntity> GetEmployeeShiftPatternExcel(int? autoID, int? empNo, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeShiftPatternExcel(autoID, empNo, costCenter, ref error, ref innerError);
        }

        public List<ShiftProjectionEntity> GetShiftProjection(DateTime? startDate, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftProjection(startDate, costCenter, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetEmployeeAttendanceHistory(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeAttendanceHistory(startDate, endDate, costCenter, empNo, ref error, ref innerError);
        }

        public List<TASFormEntity> GetFormList(string formCode, ref string error, ref string innerError)
        {
            return TASDataService.GetFormList(formCode, ref error, ref innerError);
        }

        public List<DutyROTAEntity> GetDutyROTAReportData(DateTime? startDate, DateTime? endDate, string costCenterList, int? empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetDutyROTAReportData(startDate, endDate, costCenterList, empNo, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetDailyAttendanceReportData(byte employeeType, DateTime? startDate, DateTime? endDate, string costCenterList, ref string error, ref string innerError)
        {
            return TASDataService.GetDailyAttendanceReportData(employeeType, startDate, endDate, costCenterList,  ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAbsenceReasonReportData(DateTime? startDate, DateTime? endDate, byte employeeType, string costCenter, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetAbsenceReasonReportData(startDate, endDate, employeeType, costCenter, empNo, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetDILDueToLateEntryOfDutyROTA(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetDILDueToLateEntryOfDutyROTA(startDate, endDate, costCenter, empNo, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetDILReportData(int empNo, string costCenter, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetDILReportData(empNo, costCenter, startDate, endDate, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetWeeklyOvertimeReportData(DateTime? startDate, DateTime? endDate, string costCenterList, ref string error, ref string innerError)
        {
            return TASDataService.GetWeeklyOvertimeReportData(startDate, endDate, costCenterList, ref error, ref innerError);
        }

        public List<EmployeeDirectoryEntity> GetEmployeeDirectory(int empNo, string costCenter, string searchString, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeDirectory(empNo, costCenter, searchString, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAspirePayrolReport(DateTime? startDate, DateTime? endDate, int processType, ref string error, ref string innerError)
        {
            return TASDataService.GetAspirePayrolReport(startDate, endDate, processType, ref error, ref innerError);
        }

        public List<AttendanceStatisticsEntity> GetPunctualityReport(DateTime? startDate, DateTime? endDate, string costCenter, bool showDayOff, bool showCount, ref string error, ref string innerError)
        {
            return TASDataService.GetPunctualityReport(startDate, endDate, costCenter, showDayOff, showCount, ref error, ref innerError);
        }

        public List<TASJDEComparisonEntity> GetTASJDEComparisonReport(ref string error, ref string innerError)
        {
            return TASDataService.GetTASJDEComparisonReport(ref error, ref innerError);
        }

        public void GetTASJDETransactionHistory(string PDBA, out List<TASJDEComparisonEntity> tasHistoryList, out List<TASJDEComparisonEntity> jdeHistoryList, ref string error, ref string innerError)
        {
            TASDataService.GetTASJDETransactionHistory(PDBA, out tasHistoryList, out jdeHistoryList, ref error, ref innerError);
        }

        public List<ContractorEntity> GetContractorShiftPattern(int? autoID, int? empNo, string empName, DateTime? dateJoinedStart, DateTime? dateJoinedEnd,
            DateTime? dateResignedStart, DateTime? dateResignedEnd, ref string error, ref string innerError)
        {
            return TASDataService.GetContractorShiftPattern(autoID, empNo, empName, dateJoinedStart, dateJoinedEnd, dateResignedStart, dateResignedEnd, ref error, ref innerError);
        }

        public void InsertUpdateDeleteContractorShiftPattern(int saveTypeNum, List<ContractorEntity> dataList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteContractorShiftPattern(saveTypeNum, dataList, ref error, ref innerError);
        }

        public List<ContractorEntity> GetContractorShiftPatternV2(int? autoID, int? empNo, string empName, DateTime? dateJoinedStart, DateTime? dateJoinedEnd,
            DateTime? dateResignedStart, DateTime? dateResignedEnd, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetContractorShiftPatternV2(autoID, empNo, empName, dateJoinedStart, dateJoinedEnd, dateResignedStart, dateResignedEnd, pageNumber, pageSize, ref error, ref innerError);
        }

        public List<TrainingRecordEntity> GetTrainingRecord(long? trainingRecordID, int? empNo, string costCenter, int? trainingProgramID, int? trainingProviderID,
            string qualificationCode, string typeOfTrainingCode, string statusCode, DateTime? fromDate, DateTime? toDate, byte? createdByTypeID, int? userEmpNo, int? createdByOtherEmpNo,
            DateTime? createdStartDate, DateTime? createdEndDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetTrainingRecord(trainingRecordID, empNo, costCenter, trainingProgramID, trainingProviderID, qualificationCode, typeOfTrainingCode, statusCode,
                fromDate, toDate, createdByTypeID, userEmpNo, createdByOtherEmpNo, createdStartDate, createdEndDate, pageNumber, pageSize, ref error, ref innerError);
        }

        public bool CheckIfTimesheetProcessingInProgress(ref string error, ref string innerError)
        {
            return TASDataService.CheckIfTimesheetProcessingInProgress(ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetDailyAttendanceForSalaryStaff(DateTime? startDate, DateTime? endDate, string costCenterList, ref string error, ref string innerError)
        {
            return TASDataService.GetDailyAttendanceForSalaryStaff(startDate, endDate, costCenterList, ref error, ref innerError);
        }

        public List<CostCenterAccessEntity> GetCostCenterPermission(byte loadType, int? empNo, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetCostCenterPermission(loadType, empNo, costCenter, ref error, ref innerError);
        }

        public DatabaseSaveResult InsertUpdateDeleteCostCenterPermission(byte actionType, int permitID, int permitEmpNo, string permitCostCenter, int userEmpNo, ref string error, ref string innerError)
        {
            return TASDataService.InsertUpdateDeleteCostCenterPermission(actionType, permitID, permitEmpNo, permitCostCenter, userEmpNo, ref error, ref innerError);
        }

        public void InsertAllowedCostCenter(int empNo, List<CostCenterAccessEntity> costCenterList, ref string error, ref string innerError)
        {
            TASDataService.InsertAllowedCostCenter(empNo, costCenterList, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetWorkflowActionMember(int empNo, string distListCode, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetWorkflowActionMember(empNo, distListCode, costCenter, ref error, ref innerError);
        }

        public List<PunctualityEntity> GetPunctualitySummaryReport(byte loadType, DateTime? startDate, DateTime? endDate, string costCenter, int occurenceLimit, int lateAttendanceThreshold, int earlyLeavingThreshold,
            bool hideDayOffHoliday, ref string error, ref string innerError)
        {
            return TASDataService.GetPunctualitySummaryReport(loadType, startDate, endDate, costCenter, occurenceLimit, lateAttendanceThreshold, earlyLeavingThreshold, hideDayOffHoliday, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetOvertimeByPeriod(int currentUserEmpNo, byte dataFilterID, DateTime? startDate, DateTime? endDate, string costCenter, int? empNo,
            long? otRequestNo, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            return TASDataService.GetOvertimeByPeriod(currentUserEmpNo, dataFilterID, startDate, endDate, costCenter, empNo, otRequestNo, pageNumber, pageSize, ref error, ref innerError);
        }

        public DatabaseSaveResult SaveEmployeeOvertimeByClerk(int autoID, string otReasonCode, string comment, int userEmpNo, string userEmpName, string userID, string otApprovalCode,
            string mealVoucherApprovalCode, int otDuration, ref string error, ref string innerError)
        {
            return TASDataService.SaveEmployeeOvertimeByClerk(autoID, otReasonCode, comment, userEmpNo, userEmpName, userID, otApprovalCode, mealVoucherApprovalCode, otDuration, ref error, ref innerError);
        }

        public DatabaseSaveResult ManageOvertimeRequest(byte actionType, long otRequestNo, int userEmpNo, string userEmpName, string userID, ref string error, ref string innerError)
        {
            return TASDataService.ManageOvertimeRequest(actionType, otRequestNo, userEmpNo, userEmpName, userID, ref error, ref innerError);
        }

        public List<WorkflowEmailDeliveryEntity> GetWFEmailDueForDelivery(byte actionType, int? createdByEmpNo, int? assignedToEmpNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            return TASDataService.GetWFEmailDueForDelivery(actionType, createdByEmpNo, assignedToEmpNo, startDate, endDate, ref error, ref innerError);
        }

        public void CloseEmailDelivery(List<WorkflowEmailDeliveryEntity> emailDeliveryList, ref string error, ref string innerError)
        {
            TASDataService.CloseEmailDelivery(emailDeliveryList, ref error, ref innerError);
        }

        public DatabaseSaveResult ProcessOvertimeWorflow(byte actionType, long otRequestNo, int tsAutoID, string currentUserID, int? createdByEmpNo, string createdByEmpName,
            int? assigneeEmpNo, string assigneeEmpName, bool? isApproved, string appRemarks, DateTime? requestSubmissionDate, string otComment, ref string error, ref string innerError)
        {
            return TASDataService.ProcessOvertimeWorflow(actionType, otRequestNo, tsAutoID, currentUserID, createdByEmpNo, createdByEmpName,
                assigneeEmpNo, assigneeEmpName, isApproved, appRemarks, requestSubmissionDate, otComment, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetEmployeeEmailInfo(int empNo, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeEmailInfo(empNo, costCenter, ref error, ref innerError);
        }

        public List<RoutineHistoryEntity> GetRoutineHistory(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            return TASDataService.GetRoutineHistory(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
        }

        public List<ApprovalEntity> GetApprovalHistory(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            return TASDataService.GetApprovalHistory(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
        }

        public List<WFTransActivityEntity> GetWorkflowHistory(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            return TASDataService.GetWorkflowHistory(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetAssignedOvertimeRequest(int currentUserEmpNo, byte assignTypeID, int @assignedToEmpNo,
            DateTime? startDate, DateTime? endDate, string costCenter, int? empNo, bool show12HourShift, ref string error, ref string innerError)
        {
            return TASDataService.GetAssignedOvertimeRequest(currentUserEmpNo, assignTypeID, @assignedToEmpNo, startDate, endDate, costCenter, empNo, show12HourShift, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetRequestApprovers(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            return TASDataService.GetRequestApprovers(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetOvertimeRequisition(int currentUserEmpNo, long otRequestNo, int empNo, string costCenter, int createdByEmpNo, int assignedToEmpNo,
            DateTime? startDate, DateTime? endDate, string statusCode, ref string error, ref string innerError)
        {
            return TASDataService.GetOvertimeRequisition(currentUserEmpNo, otRequestNo, empNo, costCenter, createdByEmpNo, assignedToEmpNo, startDate, endDate, statusCode, ref error, ref innerError);
        }

        public DatabaseSaveResult SubmitOvertimeChanges(long otRequestNo, string otReasonCode, string comment, int userEmpNo, string userEmpName, string userID, string otApprovalCode,
           string mealVoucherApprovalCode, int? otDuration, ref string error, ref string innerError)
        {
            return TASDataService.SubmitOvertimeChanges(otRequestNo, otReasonCode, comment, userEmpNo, userEmpName, userID, otApprovalCode,
                mealVoucherApprovalCode, otDuration, ref error, ref innerError);
        }

        public List<CostCenterEntity> GetCostCenterOTAllowed(int userEmpNo, ref string error, ref string innerError)
        {
            return TASDataService.GetCostCenterOTAllowed(userEmpNo, ref error, ref innerError);
        }

        public void InsertSystemErrorLog(byte actionType, int logID, long requisitionNo, byte errorCode, string errorDesc, int userEmpNo, string userID)
        {
            TASDataService.InsertSystemErrorLog(actionType, logID, requisitionNo, errorCode, errorDesc, userEmpNo, userID);
        }

        public bool? CheckIfHRApprover(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.CheckIfHRApprover(empNo, ref error, ref innerError);
        }

        public void HoldOvertimeRequest(List<EmployeeAttendanceEntity> otRequisitionList, ref string error, ref string innerError)
        {
            TASDataService.HoldOvertimeRequest(otRequisitionList, ref error, ref innerError);
        }

        public bool CheckIfPayrollProcessing(ref string error, ref string innerError)
        {
            return TASDataService.CheckIfPayrollProcessing(ref error, ref innerError);
        }

        public bool CheckIfPayrollProcessingByEmpNo(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.CheckIfPayrollProcessingByEmpNo(empNo, ref error, ref innerError);
        }

        public List<FireTeamMember> GetFireTeamAndFireWatch(byte actionType, DateTime processDate, string shiftCode, int empNo, string costCenter, string imageRootPath, ref string error, ref string innerError)
        {
            return TASDataService.GetFireTeamAndFireWatch(actionType, processDate, shiftCode, empNo, costCenter, imageRootPath, ref error, ref innerError);
        }

        public List<FireTeamMember> GetFireTeamAndFireWatchWithPaging(byte loadType, DateTime processDate, string shiftCodeArray, int empNo, string costCenter, int pageNumber, int pageSize,
            string imageRootPath, ref string error, ref string innerError)
        {
            return TASDataService.GetFireTeamAndFireWatchWithPaging(loadType, processDate, shiftCodeArray, empNo, costCenter, pageNumber, pageSize, imageRootPath, ref error, ref innerError);
        }

        public List<PunctualityEntity> GetUnpunctualEmployeeSummary(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetUnpunctualEmployeeSummary(startDate, endDate, costCenter, empNo, ref error, ref innerError);
        }

        public List<OvertimeBudgetEntity> GetOvertimeBudgetStatistics(byte loadType, int fiscalYear, string costCenter, ref string error, ref string innerError)
        {
            return TASDataService.GetOvertimeBudgetStatistics(loadType, fiscalYear, costCenter, ref error, ref innerError);
        }

        public bool? IsOTBudgetAdmin(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.IsOTBudgetAdmin(empNo, ref error, ref innerError);
        }

        public List<string> GetAllocatedCostCenter(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetAllocatedCostCenter(empNo, ref error, ref innerError);
        }

        public List<EmployeeDetail> GetWorkingCostCenterHistory(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetWorkingCostCenterHistory(empNo, ref error, ref innerError);
        }

        public List<UserFormAccessEntity> GetUserFormAccess(string appCode, int userEmpNo, string formCode, ref string error, ref string innerError)
        {
            return TASDataService.GetUserFormAccess(appCode, userEmpNo, formCode, ref error, ref innerError);
        }

        public List<UserFormAccessEntity> GetCommonAdminComboData(byte loadType, string appCode, string formCode, ref string error, ref string innerError)
        {
            return TASDataService.GetCommonAdminComboData(loadType, appCode, formCode, ref error, ref innerError);
        }

        public void InsertUpdateDeleteUserFormAccess(List<UserFormAccessEntity> dirtyUserAccessList, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteUserFormAccess(dirtyUserAccessList, ref error, ref innerError);
        }

        public List<MasterShiftPatternEntity> GetShiftPatternList(ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPatternList(ref error, ref innerError);
        }

        public List<MasterShiftPatternEntity> GetShiftPatternDetail(byte loadType, string shiftPatCode, byte isDayShift, byte isFlexitime, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftPatternDetail(loadType, shiftPatCode, isDayShift, isFlexitime, ref error, ref innerError);
        }

        public List<MasterShiftPatternEntity> GetShiftCodeList(string shiftCode, ref string error, ref string innerError)
        {
            return TASDataService.GetShiftCodeList(shiftCode, ref error, ref innerError);
        }

        public void SaveMasterShiftPattern(string shiftPatCode, List<MasterShiftPatternEntity> shiftTimingScheduleList, List<MasterShiftPatternEntity> shiftTimingPointerList, ref string error, ref string innerError)
        {
            TASDataService.SaveMasterShiftPattern(shiftPatCode, shiftTimingScheduleList, shiftTimingPointerList, ref error, ref innerError);
        }

        public void InsertUpdateDeleteShiftPattern(int saveTypeID, MasterShiftPatternEntity shiftPatternInfo, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteShiftPattern(saveTypeID, shiftPatternInfo, ref error, ref innerError);
        }

        public List<ServiceLogDetail> GetTimesheetAndSPULogDetail(byte loadType, DateTime? processDate, ref string error, ref string innerError)
        {
            return TASDataService.GetTimesheetAndSPULogDetail(loadType, processDate, ref error, ref innerError);
        }

        public SystemValueEntity GetSystemValues(ref string error, ref string innerError)
        {
            return TASDataService.GetSystemValues(ref error, ref innerError);
        }

        public List<EmployeeAbsentEntity> GetEmployeeAbsences(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeAbsences(startDate, endDate, costCenter, empNo, ref error, ref innerError);
        }

        public List<EmergencyContactEntity> GetEmployeeEmergencyContact(byte loadType, string costCenter, int empNo, string searchString, int userEmpNo, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeEmergencyContact(loadType, costCenter, empNo, searchString, userEmpNo, ref error, ref innerError);
        }

        public bool CheckIfCanAccessEmergencyContact(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.CheckIfCanAccessEmergencyContact(empNo, ref error, ref innerError);
        }

        public bool CheckIfCanAccessDependentInfo(int empNo, ref string error, ref string innerError)
        {
            return TASDataService.CheckIfCanAccessDependentInfo(empNo, ref error, ref innerError);
        }

        public List<EmployeeAttendanceEntity> GetEmployeeAttendanceHistoryCompact(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            return TASDataService.GetEmployeeAttendanceHistoryCompact(startDate, endDate, costCenter, empNo, ref error, ref innerError);
        }

        public List<RelativeType> GetFamilyRelativeTypes(byte degreeLevel, string relativeTypeCode, ref string error, ref string innerError)
        {
            return TASDataService.GetFamilyRelativeTypes(degreeLevel, relativeTypeCode, ref error, ref innerError);
        }

        public void InsertUpdateDeleteDeathReasonOfAbsence(int saveTypeID, DeathReasonOfAbsenceEntity deathEntity, ref string error, ref string innerError)
        {
            TASDataService.InsertUpdateDeleteDeathReasonOfAbsence(saveTypeID, deathEntity, ref error, ref innerError);
        }

        public List<object> GetContractorRegistrationLookup()
        {
            return TASDataService.GetContractorRegistrationLookup();
        }

        public async Task<List<object>> GetContractorRegistrationLookupAsync()
        {
            return await TASDataService.GetContractorRegistrationLookupAsync();
        }
        #endregion
    }
}