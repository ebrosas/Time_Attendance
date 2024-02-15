using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.BL.Helpers;

namespace GARMCO.AMS.TAS.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class TASService : ITASService
    {
        #region Members
        private DataService _dataService;
        #endregion

        #region Constructors
        public TASService()
        {
            string connectionString = GetSQLConnectionString();
            _dataService = new DataService(connectionString);
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
        public static string GetSQLConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString.Trim();
        }
        #endregion

        #region Public Methods
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

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
        #endregion
    }
}
