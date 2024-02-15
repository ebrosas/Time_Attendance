using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using GARMCO.AMS.TAS.BL.Entities;

namespace GARMCO.AMS.TAS.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ITASService
    {

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
        [OperationContract]
        EmployeeDetail GetEmployeeDetail(int empNo, ref string error, ref string innerError);

        [OperationContract]
        List<AllowedCostCenter> GetPermittedCostCenter(int empNo, ref string error, ref string innerError);

        [OperationContract]
        List<AllowedCostCenter> GetPermittedCostCenterByApplication(string appCode, int empNo, ref string error, ref string innerError);

        [OperationContract]
        List<VisitorPassEntity> GetVisitorPassLog(long? logID, string visitorName, string idNumber, int visitorCardNo, int? visitEmpNo, string visitCostCenter,
            DateTime? startDate, DateTime? endDate, byte? blockOption, int? createdByEmpNo, ref string error, ref string innerError);

        [OperationContract]
        void InsertUpdateDeleteVisitorPassLog(int saveTypeID, VisitorPassEntity visitorPassData, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetSwipeHistory(DateTime? startDate, DateTime? endDate, int? empNo, string costCenter, string locationName,
            string readerName, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeDetail> GetEmployeeInfoFromJDE(int empNo, string costCenter, bool? isActiveOnly, ref string error, ref string innerError);

        [OperationContract]
        List<AccessReaderEntity> GetAccessReaders(int loadType, int locationCode, int readerNo, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetAbsencesHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetAbsencesHistoryv2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetLeaveHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetLeaveHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetAttendanceHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetAttendanceHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<LeaveEntity> GetLeaveDetails(int? empNo, ref string error, ref string innerError);

        [OperationContract]
        List<DILEntity> GetDILEntitlements(byte loadType, int empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError);

        [OperationContract]
        List<ShiftPatternEntity> GetShiftPatternInfo(int? empNo, ref string error, ref string innerError);

        [OperationContract]
        List<LeaveEntity> GetGARMCOCalendar(int? year, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError);

        [OperationContract]
        List<CostCenterEntity> GetCostCenterManagerInfo(string costCenter, string companyCode, ref string error, ref string innerError);

        [OperationContract]
        List<DependentEntity> GetDependentInfo(int empNo, ref string error, ref string innerError);

        [OperationContract]
        List<ShiftPatternEntity> GetShiftPatternChanges(int? autoID, byte? loadType, int? empNo, string changeType,
            DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        void InsertUpdateDeleteShiftPattern(int saveTypeNum, List<ShiftPatternEntity> shiftPatternList, ref string error, ref string innerError);

        [OperationContract]
        List<ShiftPatternEntity> GetShiftPatternCodes(string shiftPatCode, ref string error, ref string innerError);

        [OperationContract]
        List<ShiftPatternEntity> GetShiftPointerCodes(string shiftPatCode, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeDetail> GetFireTeamMember(ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeDetail> GetWorkingCostCenter(int autoID, int empNo, string costCenter, string specialJobCatg, ref string error, ref string innerError);

        [OperationContract]
        List<UDCEntity> GetUDCListItem(string udcKey, ref string error, ref string innerError);

        [OperationContract]
        void InsertUpdateDeleteWorkingCostCenter(int saveTypeID, List<EmployeeDetail> empDetailList, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetEmployeeExceptional(DateTime? startDate, DateTime? endDate, int empNo,
            bool isAbsence, bool isSickLeave, bool isNPH, bool isInjuryLeave, bool isDIL, bool isOvertime, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetOnAnnualLeaveBuSwiped(DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetResignedBuSwiped(DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetResignedBuSwipedV2(DateTime? startDate, DateTime? endDate, int empNo, string costCenter, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetLongAbsences(DateTime? processDate, bool showSLP, bool showUL, bool showAbsent,
            ref DateTime? startDate, ref DateTime? endDate, ref string attendanceHistoryTitle, ref string error, ref string innerError);

        [OperationContract]
        List<UserDefinedCodes> GetUserDefinedCode(string udcCode, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetTimesheetIntegrity(string actionCode, DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetManualTimesheetEntry(int? autoID, int? empNo, string costCenter, DateTime? dateIN, DateTime? dateOUT,
            int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        void InsertUpdateDeleteManualTimesheet(int saveTypeNum, List<EmployeeAttendanceEntity> manualTimesheetList, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeDetail> GetContractors(int empNo, string empName, ref string error, ref string innerError);

        [OperationContract]
        List<ReasonOfAbsenceEntity> GetReasonOfAbsenceEntry(int? autoID, int? empNo, string costCenter, DateTime? effectiveDate, DateTime? endingDate,
            string absenceReasonCode, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<UserDefinedCodes> GetTimesheetUDCCodes(byte actionType, ref string error, ref string innerError);

        [OperationContract]
        void InsertUpdateDeleteReasonOfAbsence(int saveTypeNum, List<ReasonOfAbsenceEntity> dataList, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetTimesheetExceptional(int empNo, DateTime? startDate, DateTime? endDate, bool withExceptionOnly,
            int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetTimesheetHistory(int autoID, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<ShiftPatternEntity> GetShiftPatternChangeHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<ReasonOfAbsenceEntity> GetEmployeeAbsenceHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<LeaveEntity> GetEmployeeLeaveHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        List<EmployeeAttendanceEntity> GetTimesheetCorrection(string costCenter, int empNo, DateTime? startDate, DateTime? endDate, int autoID,
            int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        DatabaseSaveResult InsertUpdateDeleteTimesheet(byte actionType, int autoID, string correctionCode, string otType, DateTime? otStartTime, DateTime? otEndTime,
            int noPayHours, string shiftCode, bool? shiftAllowance, int? durationShiftAllowanceEvening, int? durationShiftAllowanceNight, string dilEntitlement,
            string remarkCode, string userID, ref string error, ref string innerError);

        [OperationContract]
        List<VisitorSwipeEntity> GetVisitorSwipeHistory(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError);

        [OperationContract]
        void InsertUpdateDeleteVisitorSwipeLog(int saveTypeID, List<VisitorSwipeEntity> visitorSwipeList, ref string error, ref string innerError);

        [OperationContract]
        void DeleteVisitorPassMultipleRecord(List<VisitorPassEntity> visitorRecordList, ref string error, ref string innerError);

        [OperationContract]
        List<VisitorPassEntity> GetVisitorPassLogV2(long? logID, string visitorName, string idNumber, int visitorCardNo, int? visitEmpNo, string visitCostCenter,
            DateTime? startDate, DateTime? endDate, byte? blockOption, int? userEmpNo, int? createdByOtherEmpNo, byte? createdByTypeID,
            int pageNumber, int pageSize, ref string error, ref string innerError);
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
