/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_CorrectTimesheetAndSwipeLog
*	Description: This stored procedure is used to correct invalid Timesheet and swipe hitstory log record
*
*	Date:			Author:		Rev.#:		Comments:
*	16/06/2016		Ervin		1.0			Created
**************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_CorrectTimesheetAndSwipeLog
(
	@empNo			INT,
	@processDate	DATETIME,	
	@userID			VARCHAR(50)	
)
AS	

	--Define constants
	DECLARE @CONST_RETURN_OK		int,
			@CONST_RETURN_ERROR		int

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Define variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected			INT 

	--Initialize variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0

	--Start a transaction
	BEGIN TRAN T1

	BEGIN TRY

		IF NOT EXISTS
		(
			SELECT LogID 
			FROM tas.SyncWorkplaceSwipeToTimesheetLog a
				INNER JOIN tas.Tran_Timesheet b ON a.AutoID = b.AutoID
			WHERE b.EmpNo = @empNo
				AND b.DT = @processDate
				AND b.IsLastRow = 1
		)
		BEGIN

			--Add new record
			INSERT INTO tas.SyncWorkplaceSwipeToTimesheetLog
			(
				LogDate,
				AutoID,
				DT,
				CostCenter,
				EmpNo,
				dtIN_Old,
				dtIN_New,
				dtOUT_Old,
				dtOUT_New,
				ShavedIn_Old,
				ShavedIn_New,
				ShavedOut_Old,
				ShavedOut_New,
				OTStartTime_Old,
				OTStartTime_New,
				OTEndTime_Old,
				OTEndTime_New,
				NoPayHours_Old,
				NoPayHours_New,
				DurationWorkedCumulative_Old,
				DurationWorkedCumulative_New,
				NetMinutes_Old,
				NetMinutes_New,
				LastUpdateUser,
				LastUpdateTime
			)
			SELECT	GETDATE() AS LogDate,
					b.AutoID,
					b.DT,
					b.BusinessUnit,
					b.EmpNo,
					c.TimeInMG AS dtIN_Old,
					c.TimeInWP AS dtIN_New,
					c.TimeOutMG AS dtOUT_Old,
					c.TimeOutWP AS dtOUT_New,
					e.Shaved_IN AS ShavedIn_Old,
					d.Shaved_IN AS ShavedIn_New,
					e.Shaved_OUT AS ShavedOut_Old,					
					d.Shaved_OUT AS ShavedOut_New,
					e.OTStartTime AS OTStartTime_Old,
					d.OTStartTime AS OTStartTime_New,
					e.OTEndTime AS OTEndTime_Old,
					d.OTEndTime AS OTEndTime_New,
					e.NoPayHours AS NoPayHours_Old,
					d.NoPayHours AS NoPayHours_New,
					e.Duration_Worked_Cumulative AS DurationWorkedCumulative_Old,
					d.Duration_Worked_Cumulative AS DurationWorkedCumulative_New,
					e.NetMinutes AS NetMinutes_Old,
					d.NetMinutes AS NetMinutes_New,
					'System Admin',
					GETDATE()
			FROM tas.Tran_Timesheet b
				INNER JOIN tas.Tran_WorkplaceSwipe c ON b.EmpNo = c.EmpNo AND b.DT = c.SwipeDate
				CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(b.EmpNo, b.DT, c.TimeInWP, c.TimeOutWP, 0) d		--Workplace Swipe
				CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(b.EmpNo, b.DT, c.TimeInMG, c.TimeOutMG, 0) e		--Maingate Swipe
			WHERE b.EmpNo = @empNo
				AND b.DT = @processDate
				AND b.IsLastRow = 1

			--Get the number of records processed
			SELECT @rowsAffected = @@ROWCOUNT	
		END
        
		ELSE			
		BEGIN

			--Update existing record
			UPDATE tas.SyncWorkplaceSwipeToTimesheetLog
			SET tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_Old = c.TimeInMG,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_Old = c.TimeOutMG,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_New = c.TimeInWP,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_New = c.TimeOutWP,
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_Old = e.Shaved_IN,
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_Old = e.Shaved_OUT,
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_New = d.Shaved_IN,
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_New = d.Shaved_OUT,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_Old = e.OTStartTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_Old = e.OTEndTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_New = d.OTStartTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_New = d.OTEndTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_Old = e.NoPayHours,
				tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_New = d.NoPayHours,
				tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_Old = e.Duration_Worked_Cumulative,
				tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_New = d.Duration_Worked_Cumulative,
				tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_Old = e.NetMinutes,
				tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_New = d.NetMinutes,
				tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateUser = 'System Admin',
				tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateTime = GETDATE()
			FROM tas.SyncWorkplaceSwipeToTimesheetLog a 
				INNER JOIN tas.Tran_Timesheet b ON a.AutoID = b.AutoID
				INNER JOIN tas.Tran_WorkplaceSwipe c ON a.EmpNo = c.EmpNo AND a.DT = c.SwipeDate
				CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, c.TimeInWP, c.TimeOutWP, 0) d		--Workplace Swipe
				CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, c.TimeInMG, c.TimeOutMG, 0) e		--Maingate Swipe
			WHERE b.EmpNo = @empNo
				AND b.DT = @processDate
				AND b.IsLastRow = 1

			--Get the number of records processed
			SELECT @rowsAffected = @@ROWCOUNT	
		END 

	END TRY

	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

	IF @retError = @CONST_RETURN_OK
		COMMIT TRANSACTION T1		
	ELSE
		ROLLBACK TRANSACTION T1

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected AS RowsAffected

	
/*	Testing:

PARAMETERS:
	@empNo			INT,
	@processDate	DATETIME,	
	@userID			VARCHAR(50)	

	EXEC Pr_CorrectTimesheetAndSwipeLog  10003541, '06/06/2016', 'System Admin'

	DELETE FROM tas.SyncWorkplaceSwipeToTimesheetLog WHERE LogID = 53626

*/
    