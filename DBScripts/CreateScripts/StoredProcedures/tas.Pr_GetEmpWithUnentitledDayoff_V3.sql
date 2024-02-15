/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpWithUnentitledDayoff_V3
*	Description: Retrieve the list of employees with unentitled dayoff
*
*	Date:			Author:		Rev. #:		Comments:
*	10/09/2020		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmpWithUnentitledDayoff_V3
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
		a.BusinessUnit AS CostCenter, 
		RTRIM(h.BUname) AS CostCenterName,
		a.EmpNo, b.EmpName,
		d.RequisitionNo, 
		d.LeaveStartDate, 
		d.LeaveEndDate, 
		d.LeaveResumeDate, 
		d.LeaveDuration, 
		d.NoOfWeekends,
		CASE WHEN RTRIM(d.ApprovalFlag) = 'A' THEN 'Approved / Paid'
			WHEN RTRIM(d.ApprovalFlag) = 'N' THEN 'Approved / Not Paid'
			WHEN RTRIM(d.ApprovalFlag) = 'W' THEN 'Waiting for Approval'
			WHEN RTRIM(d.ApprovalFlag) = 'C' THEN 'Cancelled'
		END AS LeaveStatus,
		CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
			THEN 
				CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
			ELSE
				CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
		END AS UnentitledDayOff,
		CONVERT
		(
			VARCHAR, 
			CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
				THEN 
					CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
				ELSE
					CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
			END, 
		12) AS DayOffArray		
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1 AND b.DateResigned IS NULL
		INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DateX
		LEFT JOIN tas.sy_LeaveRequisition2 d ON a.EmpNo = d.EmpNo AND a.DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate AND RTRIM(d.LeaveType) = 'AL' AND d.ApprovalFlag NOT IN ('C', 'R') AND d.NoOfWeekends > 0
		CROSS APPLY	
		(
			SELECT TOP 2 DT 
			FROM tas.Tran_Timesheet x WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND x.DT = y.DateX
			WHERE x.EmpNo = a.EmpNo
				AND DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate
				AND RTRIM(y.Effective_ShiftCode) = 'O'
			ORDER BY x.DT
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
		OUTER APPLY		--Get the first 2 unentitled day-off days
		(
			SELECT TOP 2 DT 
			FROM tas.Tran_Timesheet x WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND x.DT = y.DateX
				CROSS APPLY tas.fnCheckIfEntitledtoDayoff(x.EmpNo, x.DT) z
			WHERE x.EmpNo = a.EmpNo
				AND DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate
				AND RTRIM(y.Effective_ShiftCode) = 'O'
				AND ISNULL(z.DayOffArray, '') <> ''
			ORDER BY x.DT
		) e3
		OUTER APPLY		--Get the first unentitled day-off
		(
			SELECT TOP 1 DT 
			FROM tas.Tran_Timesheet x WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND x.DT = y.DateX
				CROSS APPLY tas.fnCheckIfEntitledtoDayoff(x.EmpNo, x.DT) z
			WHERE x.EmpNo = a.EmpNo
				AND DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate
				AND RTRIM(y.Effective_ShiftCode) = 'O'
				AND ISNULL(z.DayOffArray, '') <> ''
			ORDER BY x.DT
		) e4
		CROSS APPLY
        (
			SELECT Effective_ShiftCode AS PrevStartDateShiftCode
			FROM tas.Tran_ShiftPatternUpdates WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DateX = DATEADD(DAY, -1, d.LeaveStartDate)
		) f
		OUTER APPLY
		(
			SELECT tas.ConvertFromJulian(HOHDT) AS HolidayDate, LTRIM(RTRIM(HOHLCD)) AS HolidayCode, y.Duration_Worked_Cumulative AS HoursWorkedHoliday
			FROM tas.syJDE_F55HOLID x WITH (NOLOCK)
				LEFT JOIN tas.Tran_Timesheet y WITH (NOLOCK) ON a.EmpNo = y.EmpNo AND y.DT = tas.ConvertFromJulian(HOHDT) AND y.IsLastRow = 1
			WHERE tas.ConvertFromJulian(HOHDT) = DATEADD(DAY, -1, d.LeaveStartDate)
				AND LTRIM(RTRIM(HOHLCD)) IN ('H', 'D')
		) g
		LEFT JOIN tas.Master_BusinessUnit_JDE_view h ON RTRIM(a.BusinessUnit) = RTRIM(h.BU)
		OUTER APPLY
		(
			SELECT LeaveType, RemarkCode, Duration_Worked_Cumulative 
			FROM tas.Tran_Timesheet WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DT = DATEADD(DAY, -1, d.LeaveStartDate)
				AND IsLastRow = 1
		) i
	WHERE 
		a.IsLastRow = 1		
		AND RTRIM(c.Effective_ShiftCode) = 'O'
		AND 
		(
			CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
				THEN 
					CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
				ELSE
					CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
			END
		) IS NOT NULL 
		AND NOT EXISTS
        (
			SELECT 1 FROM tas.DayOffAbsentLog WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DT = CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
							THEN 
								CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
							ELSE
								CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
						END
		)
		AND 
		(
			f.PrevStartDateShiftCode = 'O'
			OR g.HolidayDate IS NOT NULL
			OR (ISNULL(i.LeaveType, '') <> '' OR RTRIM(i.RemarkCode) = 'A' OR i.Duration_Worked_Cumulative = 0)
		)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		AND a.DT BETWEEN @startDate AND @endDate
	ORDER BY a.BusinessUnit, a.EmpNo

GO 

/*	Debug:

	EXEC tas.Pr_GetEmpWithUnentitledDayoff_V3 '04/01/2020', '09/15/2020', 0, ''
	EXEC tas.Pr_GetEmpWithUnentitledDayoff_V3 '07/30/2020', '09/15/2020', 0, ''

PARAMETERS:	
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)

*/