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
					a.AutoID,
					a.DT,
					a.BusinessUnit,
					a.EmpNo,
					b.TimeInMG AS dtIN_Old,
					b.TimeInWP AS dtIN_New,
					b.TimeOutMG AS dtOUT_Old,
					b.TimeOutWP AS dtOUT_New,
					tas.fnGetShavingTime(0, b.TimeInMG, b.ShiftPatCode, b.ShiftCode) AS ShavedIn_Old,
					tas.fnGetShavingTime(0, b.TimeInWP, b.ShiftPatCode, b.ShiftCode) AS ShavedIn_New,
					tas.fnGetShavingTime(1, b.TimeOutMG, b.ShiftPatCode, b.ShiftCode) AS ShavedOut_Old,
					tas.fnGetShavingTime(1, b.TimeOutWP, b.ShiftPatCode, b.ShiftCode) AS ShavedOut_New,

					a.OTStartTime AS OTStartTime_Old,
					c.OTStartTime AS OTStartTime_New,
					a.OTEndTime AS OTEndTime_Old,
					c.OTEndTime AS OTEndTime_New,

					a.NoPayHours AS NoPayHours_Old,
					c.NoPayHours AS NoPayHours_New,
					DATEDIFF(n, tas.fnGetShavingTime(0, b.TimeInMG, b.ShiftPatCode, b.ShiftCode), tas.fnGetShavingTime(1, b.TimeOutMG, b.ShiftPatCode, b.ShiftCode)) AS DurationWorkedCumulative_Old,
					DATEDIFF(n, tas.fnGetShavingTime(0, b.TimeInWP, b.ShiftPatCode, b.ShiftCode), tas.fnGetShavingTime(1, b.TimeOutWP, b.ShiftPatCode, b.ShiftCode)) AS DurationWorkedCumulative_New,
					DATEDIFF(n, b.TimeInMG, b.TimeOutMG) AS NetMinutes_Old,
					DATEDIFF(n, b.TimeInWP, b.TimeOutWP) AS NetMinutes_New,
					'System Admin' AS LastUpdateUser,
					GETDATE() AS LastUpdateTime
			FROM tas.Tran_Timesheet a
				CROSS APPLY tas.fnGetCorrectedMainGateWorkplaceSwipe(a.EmpNo, a.DT) b
				CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, b.TimeInWP, b.TimeOutWP, 0) c
			WHERE a.EmpNo = @empNo
				AND a.DT = @processDate

			--Get the number of records processed
			SELECT @rowsAffected = @@ROWCOUNT	
		END
        
		ELSE			
		BEGIN

			--Update existing record
			UPDATE tas.SyncWorkplaceSwipeToTimesheetLog
			SET 
				--tas.SyncWorkplaceSwipeToTimesheetLog.LogDate = GETDATE(),
				--tas.SyncWorkplaceSwipeToTimesheetLog.AutoID = a.AutoID,
				--tas.SyncWorkplaceSwipeToTimesheetLog.DT = a.DT,
				--tas.SyncWorkplaceSwipeToTimesheetLog.CostCenter = a.BusinessUnit,
				--tas.SyncWorkplaceSwipeToTimesheetLog.EmpNo = a.EmpNo,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_Old = b.TimeInMG,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_New = b.TimeInWP,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_Old = b.TimeOutMG,
				tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_New = b.TimeOutWP,
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_Old = tas.fnGetShavingTime(0, b.TimeInMG, b.ShiftPatCode, b.ShiftCode),
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_New = tas.fnGetShavingTime(0, b.TimeInWP, b.ShiftPatCode, b.ShiftCode),
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_Old = tas.fnGetShavingTime(1, b.TimeOutMG, b.ShiftPatCode, b.ShiftCode),
				tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_New = tas.fnGetShavingTime(1, b.TimeOutWP, b.ShiftPatCode, b.ShiftCode),
				tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_Old = a.OTStartTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_New = c.OTStartTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_Old = a.OTEndTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_New = c.OTEndTime,
				tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_Old = a.NoPayHours,
				tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_New = c.NoPayHours,
				tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_Old = DATEDIFF(n, tas.fnGetShavingTime(0, b.TimeInMG, b.ShiftPatCode, b.ShiftCode), tas.fnGetShavingTime(1, b.TimeOutMG, b.ShiftPatCode, b.ShiftCode)),
				tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_New = DATEDIFF(n, tas.fnGetShavingTime(0, b.TimeInWP, b.ShiftPatCode, b.ShiftCode), tas.fnGetShavingTime(1, b.TimeOutWP, b.ShiftPatCode, b.ShiftCode)),
				tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_Old = DATEDIFF(n, b.TimeInMG, b.TimeOutMG),
				tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_New = DATEDIFF(n, b.TimeInWP, b.TimeOutWP),
				tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateUser = 'System Admin',
				tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateTime = GETDATE()
			FROM tas.SyncWorkplaceSwipeToTimesheetLog d 
				INNER JOIN tas.Tran_Timesheet a ON d.AutoID = a.AutoID
				CROSS APPLY tas.fnGetCorrectedMainGateWorkplaceSwipe(a.EmpNo, a.DT) b
				CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, b.TimeInWP, b.TimeOutWP, 0) c
			WHERE d.EmpNo = @empNo
				AND d.DT = @processDate

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

	EXEC Pr_CorrectTimesheetAndSwipeLog  10001594, '06/18/2016', 'System Admin'

*/
    