/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_TimesheetIntegrity
*	Description: This view is used to fetc data for the "Timesheet Integrity by Correction Code" form
*
*	Date:			Author:		Rev. #:		Comments:
*	05/07/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_TimesheetIntegrity
AS

	SELECT	a.AutoID,
			a.CorrectionCode,	
			LTRIM(RTRIM(d.DRDL01)) AS CorrectionDesc,
			a.DT,
			a.dtIN,
			a.dtOUT,
			a.EmpNo,
			b.EmpName,
			a.BusinessUnit,
			c.BusinessUnitName,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.ShiftAllowance,
			a.Duration_ShiftAllowance_Evening,
			a.Duration_ShiftAllowance_Night,
			a.OTType,
			a.OTStartTime,
			a.OTEndTime,
			a.NoPayHours,
			a.AbsenceReasonCode,
			a.LeaveType,
			a.DIL_Entitlement,
			a.RemarkCode,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.Processed
	FROM tas.Tran_Timesheet a
		LEFT JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
		LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.CorrectionCode) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) + '-' + LTRIM(RTRIM(d.DRRT)) = '55-T0'

GO

/*	Debugging:

	SELECT * FROM tas.Vw_TimesheetIntegrity a
	WHERE a.EmpNo = 10003512
	ORDER BY a.LeaveStartDate DESC

*/