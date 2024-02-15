/***************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_AutoCorrectDuplicateAttendance
*	Description: This stored procedure is used to correct the attendance records of the employee who has incorrect timing due to duplicate record
*
*	Date:			Author:		Rev.#:		Comments:
*	03/08/2022		Ervin		1.0			Created
*
***************************************************************************************************************************************************************************************************************************/

CREATE PROCEDURE tas.Pr_AutoCorrectDuplicateAttendance
(
	@startDate			DATETIME,
	@endDate			DATETIME,	
	@costCenter			VARCHAR(12) = NULL,
	@empNo				INT = NULL	
)
AS	
BEGIN
	
	DECLARE	@dtTemp			DATETIME = NULL,
			@empNoTemp		INT = NULL,
			@autoIDToDelete	INT = 0,
			@dtOUT			DATETIME = NULL,
			@shavedOUT		DATETIME = NULL

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF EXISTS
    (
		SELECT 1
		FROM tas.vuTran_Timesheet2_CopyBack  a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1
			INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DateX
			CROSS APPLY
			(
				SELECT COUNT(AutoID) AS SwipeCount FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE EmpNo = a.EmpNo 
					AND DT = a.DT 
			) d
			INNER JOIN TAS.Tran_Timesheet e WITH (NOLOCK) ON a.EmpNo = e.EmpNo AND a.DT = e.DT AND e.IsLastRow = 1 AND ISNULL(e.XXXXXXXXXXXX_900, 0) = 0
		WHERE TS_AutoID IS NULL
			AND (a.dtIN IS NULL AND a.dtOUT IS NOT NULL)
			AND ISNULL(a.IsDriver, 0) = 0
			AND d.SwipeCount <= 2
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
	)
	BEGIN
    
		--Create temporary table
		DECLARE TimesheetCursor CURSOR READ_ONLY FOR 
		SELECT a.EmpNo, a.DT
		FROM tas.vuTran_Timesheet2_CopyBack  a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1
			INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DateX
			CROSS APPLY
			(
				SELECT COUNT(AutoID) AS SwipeCount FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE EmpNo = a.EmpNo 
					AND DT = a.DT 
			) d
			INNER JOIN TAS.Tran_Timesheet e WITH (NOLOCK) ON a.EmpNo = e.EmpNo AND a.DT = e.DT AND e.IsLastRow = 1 AND ISNULL(e.XXXXXXXXXXXX_900, 0) = 0
		WHERE TS_AutoID IS NULL
			AND (a.dtIN IS NULL AND a.dtOUT IS NOT NULL)
			AND ISNULL(a.IsDriver, 0) = 0
			AND d.SwipeCount <= 2
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)

		--Open the cursor and fetch the data
		OPEN TimesheetCursor
		FETCH NEXT FROM TimesheetCursor
		INTO @empNoTemp, @dtTemp

		--Loop through each record to remove the NPH
		WHILE @@FETCH_STATUS = 0 
		BEGIN			

			SELECT	TOP 1
					@autoIDToDelete = a.AutoID,
					@dtOUT = a.dtOUT,
					@shavedOUT = a.Shaved_OUT 
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @empNoTemp
				AND a.DT = @dtTemp
			ORDER BY a.AutoID DESC

			IF @dtOUT IS NOT NULL AND @shavedOUT IS NOT NULL 
			BEGIN
        
				--Fix attendance record
				UPDATE tas.Tran_Timesheet
				SET dtOUT = @dtOUT,				
					Shaved_OUT = @shavedOUT,
					Duration_Worked = DATEDIFF(mi, dtIN, @shavedOUT),
					Duration_Worked_Cumulative = DATEDIFF(mi, Shaved_IN, @shavedOUT),
					NetMinutes = DATEDIFF(mi, dtIN, @dtOUT),
					XXXXXXXXXXXX_900 = 1,				--(Notes: Set value to 1 to enable flag that this record was modified by the system)
					ShiftSpan_XID = @autoIDToDelete		--(Note: Store the AutoID to this field for reference purpose)
				WHERE EmpNo = @empNoTemp
					AND DT = @dtTemp
					AND IsLastRow = 1

				--Delete the duplicate attendance record
				IF @autoIDToDelete > 0
				BEGIN 

					DELETE FROM tas.Tran_Timesheet 
					WHERE AutoID = @autoIDToDelete
				END 
			END 

			--SELECT	@empNoTemp AS EmpNo_Process,
			--		@dtTemp AS DT_Process,
			--		@autoIDToDelete AS AutoID_Delete,
			--		@dtOUT AS dtOUT,
			--		@shavedOUT AS Shaved_OUT 

			--Fetch next record
			FETCH NEXT FROM TimesheetCursor
			INTO @empNoTemp, @dtTemp
		END 

		--Close and deallocate
		CLOSE TimesheetCursor
		DEALLOCATE TimesheetCursor
	END
	ELSE
    BEGIN

		SELECT 'No duplicate attendance record found!' AS ErrorMsg
    END 
END 

/*	Debug:

PARAMETERS:
	@startDate			DATETIME,
	@endDate			DATETIME,	
	@costCenter			VARCHAR(12) = NULL,
	@empNo				INT = NULL	

	EXEC tas.Pr_AutoCorrectDuplicateAttendance '07/16/2022', '08/15/2022', '', 10003385

*/

/*	Checking:

	--Get all attendance corrected attendance records
	SELECT a.ShiftSpan_XID AS AutoID_Deleted, XXXXXXXXXXXX_900 AS CorrectionFlag,
		a.BusinessUnit, * 
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE ShiftSpan_XID > 0
		AND XXXXXXXXXXXX_900 = 1
		AND a.DT >= '08/03/2022'		--(Notes: August 8, 2022 is the start date of the service execution)
	ORDER BY a.DT DESC, a.BusinessUnit, a.EmpNo

	--Check the audit logs
	SELECT a.LOGMACHINE, a.LOGTYPE, a.LOGTIME, a.* 
	FROM tas.Tran_Timesheet_JN a WITH (NOLOCK)
		INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.AutoID = b.AutoID
	WHERE b.ShiftSpan_XID > 0
		AND b.XXXXXXXXXXXX_900 = 1
		AND b.DT >= '08/03/2022'		--(Notes: August 8, 2022 is the start date of the service execution)
	ORDER BY a.LOGTIME DESC

*/