/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAttendanceHistory
*	Description: Used to fetch attendance dashboard data based on selected cost center
*
*	Date			Author		Revision No.	Comments:
*	28/04/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAttendanceHistory
(   
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME
)
AS

	SELECT	a.AutoID,
			a.EmpNo,
			--b.EmpName,
			a.BusinessUnit,
			--b.ActualCostCenter,
			a.IsLastRow,
			a.Processed,
			a.CorrectionCode,
			a.DT,
			a.dtIN,
			a.dtOUT,
			a.Shaved_IN,
			a.Shaved_OUT, 
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,			
			c.OTtype,
			c.OTstartTime,
			c.OTendTime,
			a.NoPayHours,
			a.AbsenceReasonCode,
			a.LeaveType,
			a.DIL_Entitlement,
			a.RemarkCode,
			a.LastUpdateUser,
			a.LastUpdateTime
	FROM tas.Tran_Timesheet a
		--INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Audit_Tran_Timesheet_Extra c ON a.AutoID = c.XID_AutoID
	WHERE a.EmpNo = @empNo
		AND a.DT BETWEEN @startDate AND @endDate
	ORDER BY a.DT DESC 

GO 

/*	Debugging:

	EXEC tas.Pr_GetAttendanceHistory 10003632, '16/03/2016', '15/04/2016'

*/


