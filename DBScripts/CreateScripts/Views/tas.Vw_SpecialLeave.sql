/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_SpecialLeave
*	Description: Get the list of all special leaves 
*
*	Date:			Author:		Rev. #:		Comments:
*	08/10/2020		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_SpecialLeave
AS		
	
	SELECT DISTINCT 
		b.BusinessUnit AS CostCenter,
		RTRIM(g.BUname) AS CostCenterName,
		b.EmpNo,
		d.EmpName,
		b.ShiftPatCode,
		b.DT, 
		0 AS LeaveNo,
		a.EffectiveDate AS LeaveStartDate,
		a.EndingDate AS LeaveEndDate,
		a.EndingDate AS LeaveResumeDate,
		DATEDIFF(DAY, a.EffectiveDate, a.EndingDate) AS LeaveDuration,
		0 AS NoOfWeekends,
		tas.fnGetLeaveBalance(a.EmpNo, 'AL', '12/31/' + CAST(YEAR(GETDATE()) AS VARCHAR)) AS LeaveBalance,
		'A' AS ApprovalFlag,
		'Approved / Paid' AS LeaveStatus,
		'Special Leave' AS DayBeforeLeaveStartDateDesc,
		'' AS PrevStartDateShiftCode,
		NULL AS HolidayDate,
		'' AS HolidayCode,
		f.DayOffArray,
		a.AbsenceReasonCode AS LeaveType
	FROM tas.Tran_Absence a WITH (NOLOCK)
		INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate AND b.IsLastRow = 1 
		INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON b.EmpNo = c.EmpNo AND b.DT = c.DateX AND RTRIM(c.Effective_ShiftCode) = 'O' 
		INNER JOIN tas.Master_Employee_JDE_View_V2 d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND ISNUMERIC(d.PayStatus) = 1 AND d.DateResigned IS NULL
		CROSS APPLY tas.fnCheckIfEntitledtoDayoffAll(b.EmpNo, b.DT) f
		INNER JOIN tas.Master_BusinessUnit_JDE_view g WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(g.BU)
	WHERE  
		/* (a.EffectiveDate >= '04/01/2020' AND a.EndingDate <= '06/30/2020') AND  */
		RTRIM(a.AbsenceReasonCode) = 'SL'
		AND ISNULL(f.DayOffArray, '') <> ''
		AND NOT EXISTS
		(
			SELECT 1 FROM tas.sy_LeaveRequisition2 WITH (NOLOCK)  
			WHERE EmpNo = b.EmpNo
				AND b.DT BETWEEN LeaveStartDate AND LeaveEndDate
				AND ApprovalFlag NOT IN ('C', 'R') 
		)

GO 


/*	Debug:

	SELECT * FROM tas.Vw_SpecialLeave a
	WHERE (a.LeaveStartDate >= '04/01/2020' AND a.LeaveEndDate <= '06/30/2020')

*/