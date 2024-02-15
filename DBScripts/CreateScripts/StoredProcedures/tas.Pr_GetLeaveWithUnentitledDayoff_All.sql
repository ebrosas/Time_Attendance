/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetLeaveWithUnentitledDayoff_All
*	Description: Retrieve the list of employees with unentitled dayoff
*
*	Date:			Author:		Rev. #:		Comments:
*	08/10/2020		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetLeaveWithUnentitledDayoff_All
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SELECT a.* 
	FROM dbo.stg_UnentitledDayoffAprilToJune a WITH (NOLOCK)
		INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DT AND b.IsLastRow = 1
	WHERE a.DT BETWEEN @startDate AND @endDate	--(Note: This condition will filter data wherein the unetitled date is between April to June 2020 only)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		AND NOT EXISTS
        (
			SELECT 1 FROM tas.LeaveUnentitledDayoffLog WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DT = a.DT
		)
	ORDER BY a.LeaveType, a.CostCenter, a.EmpNo, a.DT

	/* Notes: Code below was commented so that data will be fetched from the staging table called "dbo.stg_UnentitledDayoffAprilToJune"
	SELECT	*
	FROM
    (
		--Get all normal leave request
		SELECT DISTINCT  
			a.BusinessUnit AS CostCenter, 
			RTRIM(h.BUname) AS CostCenterName,
			a.EmpNo, 
			b.EmpName,
			a.ShiftPatCode,
			CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
				THEN 
					CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
				ELSE
					CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
			END AS DT,
			d.RequisitionNo AS LeaveNo, 
			d.LeaveStartDate, 
			d.LeaveEndDate, 
			d.LeaveResumeDate, 
			d.LeaveDuration, 
			d.NoOfWeekends,		
			tas.fnGetLeaveBalance(a.EmpNo, 'AL', '12/31/' + CAST(YEAR(GETDATE()) AS VARCHAR)) AS LeaveBalance,
			d.ApprovalFlag,
			CASE WHEN RTRIM(d.ApprovalFlag) = 'A' THEN 'Approved / Paid'
				WHEN RTRIM(d.ApprovalFlag) = 'N' THEN 'Approved / Not Paid'
				WHEN RTRIM(d.ApprovalFlag) = 'W' THEN 'Waiting for Approval'
				WHEN RTRIM(d.ApprovalFlag) = 'C' THEN 'Cancelled'
			END AS LeaveStatus,
			CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
				THEN CASE WHEN ISNULL(g.HoursWorkedHoliday, 0) = 0 THEN 'DayOffOrHoliday' ELSE 'WorkedOnHoliday' END 
				ELSE 'LeaveOrAbsent'
			END AS DayBeforeLeaveStartDateDesc,
			f.PrevStartDateShiftCode,
			g.HolidayDate,
			g.HolidayCode,
			CONVERT
			(
				VARCHAR, 
				CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
					THEN 
						CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
					ELSE
						CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
				END, 
			12) AS DayOffArray,
			d.LeaveType		
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1 AND b.DateResigned IS NULL
			INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DateX
			INNER JOIN tas.sy_LeaveRequisition2 d ON a.EmpNo = d.EmpNo AND a.DT BETWEEN d.LeaveStartDate AND d.LeaveEndDate /*AND RTRIM(d.LeaveType) = 'AL'*/ AND d.ApprovalFlag NOT IN ('C', 'R') AND d.NoOfWeekends > 0
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
			AND
			(
				CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
					THEN CASE WHEN ISNULL(g.HoursWorkedHoliday, 0) = 0 THEN 'DayOffOrHoliday' ELSE 'WorkedOnHoliday' END 
					ELSE 'LeaveOrAbsent'
				END <> 'WorkedOnHoliday'
			)
			AND a.DT BETWEEN @startDate AND @endDate
			--AND 
			--(
			--	CASE WHEN f.PrevStartDateShiftCode = 'O' OR g.HolidayDate IS NOT NULL
			--		THEN 
			--			CASE WHEN d.NoOfWeekends >= 2 THEN e.DT ELSE e2.DT END 
			--		ELSE
			--			CASE WHEN d.NoOfWeekends >= 2 THEN e3.DT ELSE e4.DT END 
			--	END
			--) BETWEEN @startDate AND @endDate
			--AND (a.EmpNo = @empNo OR @empNo IS NULL)
			--AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION

			--Get all Special Leaves
			SELECT	* FROM tas.Vw_SpecialLeave a WITH (NOLOCK)
			WHERE (a.LeaveStartDate >= @startDate AND a.LeaveEndDate <= @endDate)
				--AND (a.EmpNo = @empNo OR @empNo IS NULL)
				--AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)

		UNION
    
			--Get all Exceptional Leaves
			SELECT	* FROM tas.Vw_ExceptionalLeave a WITH (NOLOCK)
			WHERE (a.LeaveStartDate >= @startDate AND a.LeaveEndDate <= @endDate)
				--AND (a.EmpNo = @empNo OR @empNo IS NULL)
				--AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		
		UNION
    
			--Get other ROA
			SELECT	* FROM tas.Vw_OtherReasonOfAbsence a WITH (NOLOCK)
			WHERE (a.LeaveStartDate >= @startDate AND a.LeaveEndDate <= @endDate)
				--AND (a.EmpNo = @empNo OR @empNo IS NULL)
				--AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a
	--WHERE a.DT BETWEEN @startDate AND @endDate	--(Note: This condition will filter data wherein the unetitled date is between April to June 2020 only)
	--	AND (a.EmpNo = @empNo OR @empNo IS NULL)
	--	AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	ORDER BY a.LeaveType, a.CostCenter, a.EmpNo, a.DT
	*/

GO 

/*	Debug:

	EXEC tas.Pr_GetLeaveWithUnentitledDayoff_All '04/01/2020', '06/30/2020'
	EXEC tas.Pr_GetLeaveWithUnentitledDayoff_All '09/16/2020', '10/15/2020'
	EXEC tas.Pr_GetLeaveWithUnentitledDayoff_All '04/01/2020', '06/30/2020', 0, '5200'
	EXEC tas.Pr_GetLeaveWithUnentitledDayoff_All '07/30/2020', '09/15/2020', 10003729
	EXEC tas.Pr_GetLeaveWithUnentitledDayoff_All '07/30/2020', '09/15/2020', 10003505, ''

PARAMETERS:	
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)

*/