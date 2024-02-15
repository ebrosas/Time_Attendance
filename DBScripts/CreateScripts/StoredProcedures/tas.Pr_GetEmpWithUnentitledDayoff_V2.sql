/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpWithUnentitledDayoff_V2
*	Description: Retrieve the list of employees with unentitled dayoff
*
*	Date:			Author:		Rev. #:		Comments:
*	28/08/2020		Ervin		1.0			Created
*******************************************************************************************************************************************************/

CREATE PROCEDURE tas.Pr_GetEmpWithUnentitledDayoff_V2
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SELECT DISTINCT  
		b.ActualCostCenter, a.EmpNo, b.EmpName,
		a.ShiftPatCode, c.Effective_ShiftCode AS ShiftCode,
		CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END AS DayOffDate,
		d.RequisitionNo, d.LeaveStartDate, d.LeaveResumeDate, d.LeaveDuration, d.NoOfWeekends,
		f.PrevStartDateShiftCode,
		g.HolidayDate, g.HolidayCode
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1 AND b.DateResigned IS NULL
		INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DateX
		INNER JOIN tas.sy_LeaveRequisition2 d ON a.EmpNo = d.EmpNo AND a.DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate AND RTRIM(d.LeaveType) = 'AL' AND d.ApprovalFlag NOT IN ('C', 'R') AND d.NoOfWeekends > 0
		CROSS APPLY	
		(
			SELECT TOP 2 DT 
			FROM tas.Tran_Timesheet x WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND x.DT = y.DateX
			WHERE x.EmpNo = a.EmpNo
				AND DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate
				AND RTRIM(y.Effective_ShiftCode) = 'O'
			ORDER BY a.DT
		) e
		CROSS APPLY	
		(
			SELECT TOP 1 DT 
			FROM tas.Tran_Timesheet x WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND x.DT = y.DateX
			WHERE x.EmpNo = a.EmpNo
				AND DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate
				AND RTRIM(y.Effective_ShiftCode) = 'O'
			ORDER BY a.DT
		) e2
		CROSS APPLY
        (
			SELECT Effective_ShiftCode AS PrevStartDateShiftCode
			FROM tas.Tran_ShiftPatternUpdates WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DateX = DATEADD(DAY, -1, d.LeaveStartDate)
		) f
		OUTER APPLY
		(
			SELECT tas.ConvertFromJulian(HOHDT) AS HolidayDate, LTRIM(RTRIM(HOHLCD)) AS HolidayCode
			FROM tas.syJDE_F55HOLID WITH (NOLOCK)
			WHERE tas.ConvertFromJulian(HOHDT) = DATEADD(DAY, -1, d.LeaveStartDate)
				AND LTRIM(RTRIM(HOHLCD)) IN ('H', 'D')
		) g
	WHERE 
		a.IsLastRow = 1		
		AND RTRIM(c.Effective_ShiftCode) = 'O'
		AND NOT EXISTS
        (
			SELECT 1 FROM tas.DayOffUnpaidLeaveLog WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DT = a.DT
		)
		AND NOT 
		(
			ISNULL(a.CorrectionCode, '') <> '' AND SUBSTRING(RTRIM(a.CorrectionCode), 0, 2) <> 'RA'
		)
		AND 
		(
			f.PrevStartDateShiftCode = 'O'
			OR g.HolidayDate IS NOT NULL
		)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		AND a.DT BETWEEN @startDate AND @endDate
	ORDER BY a.EmpNo

GO 

/*	Debug:

	EXEC tas.Pr_GetEmpWithUnentitledDayoff_V2 '04/01/2020', '09/15/2020', 0, ''
	EXEC tas.Pr_GetEmpWithUnentitledDayoff_V2 '08/16/2020', '09/15/2020', 0, ''

PARAMETERS:	
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)

*/