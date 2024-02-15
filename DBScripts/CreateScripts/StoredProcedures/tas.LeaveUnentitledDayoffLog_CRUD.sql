/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.LeaveUnentitledDayoffLog_CRUD
*	Description: This stored procedure is used to perform CRUD operations in "LeaveUnentitledDayoffLog" table
*
*	Date			Author		Revision No.	Comments:
*	14/09/2020		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

CREATE PROCEDURE tas.LeaveUnentitledDayoffLog_CRUD
(	
	@actionType		TINYINT,
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''
)
AS	

	--Define constants
	DECLARE @CONST_RETURN_OK		INT = 0,
			@CONST_RETURN_ERROR		INT = -1

	--Define other variables
	DECLARE @hasError				BIT = 0,
			@retError				INT = -1,
			@retErrorDesc			VARCHAR(200) = '',
			@rowsAffected			INT = 0			

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @actionType = 0		--Check existing records
	BEGIN

		--Check existing records
		SELECT * FROM tas.LeaveUnentitledDayoffLog a
		WHERE (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.CostCenter, a.EmpNo
    END

	ELSE IF @actionType = 1		--Insert record
	BEGIN

		INSERT INTO tas.LeaveUnentitledDayoffLog
		(
			CostCenter,
			EmpNo,
			DT,
			LeaveNo,
			LeaveStartDate,
			LeaveEndDate,
			LeaveResumeDate,
			LeaveDuration,
			NoOfWeekends,
			LeaveBalance,
			ApprovalFlag,
			LeaveStatus,
			DayBeforeLeaveStartDateDesc,
			PrevStartDateShiftCode,
			HolidayDate,
			HolidayCode,
			IsProcessed,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID
		)
		SELECT DISTINCT  
			a.BusinessUnit AS CostCenter, 
			a.EmpNo, 
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
			tas.fnGetLeaveBalance(a.EmpNo, 'AL', '12/31/2020') AS LeaveBalance,
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
			0 AS IsProcessed,
			GETDATE() AS CreatedDate,
			0 AS CreatedByEmpNo,
			'System Admin' AS CreatedByUserID
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
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
			AND d.LeaveDuration >= 1
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND a.DT BETWEEN @startDate AND @endDate
		ORDER BY a.BusinessUnit, a.EmpNo		
		
		--Get the number of affected records 
		SELECT @rowsAffected = @@rowcount 						
					
		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription,
				@rowsAffected AS RowsAffected
	END 

	ELSE IF @actionType = 2		--Delete record
	BEGIN

		--Check existing records
		DELETE FROM tas.LeaveUnentitledDayoffLog 
		WHERE (RTRIM(CostCenter) = @costCenter OR @costCenter IS NULL)
			AND (EmpNo = @empNo OR @empNo IS NULL)

		--Get the number of affected records 
		SELECT @rowsAffected = @@rowcount 

		--Check affected records
		SELECT * FROM tas.LeaveUnentitledDayoffLog a
		WHERE (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.CostCenter, a.EmpNo

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription,
				@rowsAffected AS RowsAffected
    END

GO 


/*	Debug:

PARAMETERS:
	@actionType		TINYINT,
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''

	
	SELECT * FROM tas.LeaveUnentitledDayoffLog a

	EXEC tas.LeaveUnentitledDayoffLog_CRUD 0, '07/30/2020', '09/15/2020'
	EXEC tas.LeaveUnentitledDayoffLog_CRUD 0, '07/30/2020', '09/15/2020', 10003435				--Check record
	EXEC tas.LeaveUnentitledDayoffLog_CRUD 1, '07/30/2020', '09/15/2020', 10003830				--Insert record (by emp. no.)
	EXEC tas.LeaveUnentitledDayoffLog_CRUD 1, '07/30/2020', '09/15/2020', 0, '2110'				--Insert record (by cost center)
	EXEC tas.LeaveUnentitledDayoffLog_CRUD 2, '07/30/2020', '09/15/2020', 10003505				--Delete record

*/