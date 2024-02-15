/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_AutoMarkLeaveRemoveDayOff_V2
*	Description: This stored procedure is used to remove day-off and mark as unplanned leave or sick-leave
*
*	Date			Author		Revision No.	Comments:
*	21/08/2020		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_AutoMarkLeaveRemoveDayOff_V2
(	
	@actionType			TINYINT,		--(Note: 1 = Mark leave remove dayoff; 2 = Undo removal of dayoff)	
	@empNo				INT,
	@dayOffArray		VARCHAR(200),
	@leaveType			VARCHAR(10) 
)
AS	

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT,
			@CONST_RDUL				VARCHAR(10),
			@CONST_RDSL				VARCHAR(10),
			@CONST_RDIL				VARCHAR(10),
			@CONST_MARO				VARCHAR(10)

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected			INT			

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1,
			@CONST_RDUL				= 'RDUL',	--Remove Dayoff - Mark Unpaid Leave
			@CONST_RDSL				= 'RDSL',	--Remove Dayoff - Mark Unpaid Sick Leave
			@CONST_RDIL				= 'RDIL',	--Remove Dayoff - Mark Unpaid Injury Leave
			@CONST_MARO				= 'MARO'	--Mark Absent - Remove Dayoff

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0				

	IF @actionType = 1		--Remove dayoff - Mark Leave
	BEGIN

		--Check if record exist
		IF EXISTS
        (
			SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.IsLastRow = 1
				AND a.DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))	
		)
		BEGIN
        
			IF NOT EXISTS
            (
				SELECT 1 FROM tas.DayOffUnpaidLeaveLog a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))	
			)
			BEGIN
            
				--Insert the log record first
				INSERT INTO tas.DayOffUnpaidLeaveLog
				(
					TSAutoID,
					EmpNo,
					DT,
					CostCenter,
					ShiftPatCode,
					ShiftCode,
					LeaveType,
					Processed,
					CorrectionCode,
					CreatedDate,
					CreatedByEmpNo,
					CreatedByUserID
				)
				SELECT	a.AutoID,
						a.EmpNo, 
						a.DT,
						RTRIM(a.BusinessUnit),
						a.ShiftPatCode,
						a.ShiftCode,
						a.LeaveType,
						a.Processed,
						a.CorrectionCode,
						GETDATE(),
						0,
						'System Admin'
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.IsLastRow = 1
					AND a.DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))											
					
				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN
			
					--PRINT '@leaveType: ' + @leaveType

					/*	Commented code as per Helpdesk request
					--Update the attendance record
					UPDATE tas.Tran_Timesheet
					SET tas.Tran_Timesheet.CorrectionCode = CASE WHEN @leaveType = 'SLP' THEN @CONST_RDSL						--Remove Dayoff - Mark Unpaid Sick Leave
																WHEN @leaveType = 'IL' THEN @CONST_RDIL							--Remove Dayoff - Mark Unpaid Injury Leave
																WHEN @leaveType IN ('AL', 'EL', 'DD') THEN @CONST_MARO			--Mark Absent - Remove Dayoff
																ELSE @CONST_RDUL												--Remove Dayoff - Mark Unpaid Leave
															END,
						tas.Tran_Timesheet.LeaveType = CASE WHEN @leaveType = 'SLP' THEN 'SLU'
															WHEN @leaveType = 'IL' THEN 'ILU'
															WHEN @leaveType IN ('AL', 'EL', 'DD') THEN ''
															ELSE 'UL'
														END,
						tas.Tran_Timesheet.RemarkCode = CASE WHEN @leaveType IN ('AL', 'EL', 'DD') THEN 'A' ELSE a.RemarkCode END,
						tas.Tran_Timesheet.Processed = 0,
						tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
						tas.Tran_Timesheet.LastUpdateTime = GETDATE()
					FROM tas.Tran_Timesheet a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.IsLastRow = 1
						AND a.DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))

					--Get the number of affected records in the "Tran_Timesheet" table
					SELECT @rowsAffected = @@rowcount 
					*/

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END
				END 
			END 

			ELSE
			BEGIN

				/*	Commented code as per Helpdesk request
				--Update the attendance record (Rev. #1.2)
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.CorrectionCode = CASE WHEN @leaveType = 'SLP' THEN @CONST_RDSL						--Remove Dayoff - Mark Unpaid Sick Leave
															WHEN @leaveType = 'IL' THEN @CONST_RDIL							--Remove Dayoff - Mark Unpaid Injury Leave
															WHEN @leaveType IN ('AL', 'EL', 'DD') THEN @CONST_MARO			--Mark Absent - Remove Dayoff
															ELSE @CONST_RDUL												--Remove Dayoff - Mark Unpaid Leave
														END,
					tas.Tran_Timesheet.LeaveType = CASE WHEN @leaveType = 'SLP' THEN 'SLU'
														WHEN @leaveType = 'IL' THEN 'ILU'
														WHEN @leaveType IN ('AL', 'EL', 'DD') THEN ''
														ELSE 'UL'
													END,
					tas.Tran_Timesheet.RemarkCode = CASE WHEN @leaveType IN ('AL', 'EL', 'DD') THEN 'A' ELSE a.RemarkCode END,
					tas.Tran_Timesheet.Processed = 0,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.IsLastRow = 1
					AND a.DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))

				--Get the number of affected records in the "Tran_Timesheet" table
				SELECT @rowsAffected = @@rowcount 

				*/

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
			END 
		END 
	END 

	ELSE IF @actionType = 2		--Undo removal of dayoff
	BEGIN

		UPDATE tas.Tran_Timesheet
		SET tas.Tran_Timesheet.CorrectionCode = b.CorrectionCode,
			tas.Tran_Timesheet.LeaveType = b.LeaveType,
			tas.Tran_Timesheet.Processed = b.Processed,
			tas.Tran_Timesheet.RemarkCode = '',
			tas.Tran_Timesheet.NoPayHours = 0,
			tas.Tran_Timesheet.LastUpdateUser = NULL,
			tas.Tran_Timesheet.LastUpdateTime = NULL
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.DayOffUnpaidLeaveLog b WITH (NOLOCK) ON a.AutoID = b.TSAutoID
		WHERE a.EmpNo = @empNo
			AND a.IsLastRow = 1
			AND a.DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))
		
		--Get the number of affected records in the "Tran_Timesheet" table
		SELECT @rowsAffected = @@rowcount 

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		-- Checks if there's no error
		IF @retError = @CONST_RETURN_OK
		BEGIN

			DELETE FROM tas.DayOffUnpaidLeaveLog 
			WHERE EmpNo = @empNo
				AND DT IN (SELECT DateValue FROM tas.fnParseDateArrayToDateTime(@dayOffArray, ','))
		END 
	END 

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected AS RowsAffected

GO

/*	Debug:

PARAMETERS:
	@actionType			TINYINT,		--(Note: 1 = Mark leave remove dayoff; 2 = Undo removal of dayoff)	
	@empNo				INT,
	@dayOffArray		VARCHAR(200),
	@leaveType			VARCHAR(10) 

	--Test server
	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 1, 10006032, '160226,160227', 'UL'			--Remove dayoff - Mark unpaid leave
	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 2, 10006032, '160226,160227', 'UL'			--Undo removal of dayoff

	--Production server
	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 1, 10003815, '200704', 'EL'				--Remove dayoff 
	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 1, 10003662, '200530', ''					--Remove dayoff - Unpaid leave
	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 2, 10001415, '200821', ''					--Undo removal of dayoff

	SELECT a.LeaveType, * FROM tas.Tran_Timesheet a
	WHERE a.EmpNo = 10003324 
		AND a.DT = '05/16/2020'

	--Check the data
	SELECT * FROM tas.DayOffUnpaidLeaveLog a
	ORDER BY a.CreatedDate DESC

	SELECT * FROM tas.DayOffUnpaidLeaveLog a
	WHERE a.EmpNo = 10001559
	ORDER BY EmpNo, DT

	SELECT a.ShiftCode,	CorrectionCode, RemarkCode, LeaveType, AbsenceReasonCode, Processed, LastUpdateUser, LastUpdateTime, * 
	FROM tas.Tran_Timesheet a
	WHERE a.AutoID IN
	(
		SELECT TSAutoID FROM tas.DayOffUnpaidLeaveLog 
		WHERE EmpNo = 10003804
	)

	
	BEGIN TRAN T1

	UPDATE tas.Tran_Timesheet 
	SET LeaveType = 'IL'
	WHERE AutoID = 5787792

	DELETE FROM tas.DayOffUnpaidLeaveLog 

	COMMIT TRAN T1

*/