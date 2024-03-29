USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_AutoMarkLeaveRemoveDayOff]    Script Date: 19/08/2020 12:15:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_AutoMarkLeaveRemoveDayOff
*	Description: This stored procedure is used to remove day-off and mark as unplanned leave or sick-leave
*
*	Date			Author		Revision No.	Comments:
*	28/04/2019		Ervin		1.0				Created
*	15/05/2019		Ervin		1.1				Implemented logic for Sick Leave Paid and Injury Leave 
*	24/06/2019		Ervin		1.2				Added condition to check if record already exists in "DayOffAbsentLog" table. If true, then just update the Tran_Timeheet table.
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_AutoMarkLeaveRemoveDayOff]
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
			@CONST_SR				VARCHAR(10),
			@CONST_AL				VARCHAR(10)

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
			@CONST_SR				= 'SR',		--Leave (Sick,Injury,Light Duty)
			@CONST_AL				= 'AL'

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0				

	IF @actionType = 1		--Remove dayoff - Mark Unpaid Leave
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

					--Update the attendance record
					UPDATE tas.Tran_Timesheet
					SET tas.Tran_Timesheet.CorrectionCode = CASE WHEN @leaveType = 'SLU' THEN @CONST_RDSL
																WHEN @leaveType = 'ILU' THEN @CONST_RDIL
																WHEN @leaveType = 'UL' THEN @CONST_RDUL
																WHEN @leaveType = 'AL' THEN @CONST_AL
																ELSE @CONST_SR
															END,
						tas.Tran_Timesheet.LeaveType = @leaveType,
						tas.Tran_Timesheet.Processed = 0,
						tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
						tas.Tran_Timesheet.LastUpdateTime = GETDATE()
					FROM tas.Tran_Timesheet a WITH (NOLOCK)
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
				END 
			END 

			ELSE
			BEGIN

				--Update the attendance record (Rev. #1.2)
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.CorrectionCode = CASE WHEN @leaveType = 'SLU' THEN @CONST_RDSL
															WHEN @leaveType = 'ILU' THEN @CONST_RDIL
															WHEN @leaveType = 'UL' THEN @CONST_RDUL
															WHEN @leaveType = 'AL' THEN @CONST_AL
															ELSE @CONST_SR
														END,
					tas.Tran_Timesheet.LeaveType = @leaveType,
					tas.Tran_Timesheet.Processed = 0,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
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
			END 
		END 
	END 

	ELSE IF @actionType = 2		--Undo removal of dayoff
	BEGIN

		UPDATE tas.Tran_Timesheet
		SET tas.Tran_Timesheet.CorrectionCode = b.CorrectionCode,
			tas.Tran_Timesheet.LeaveType = b.LeaveType,
			tas.Tran_Timesheet.Processed = b.Processed,
			tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
			tas.Tran_Timesheet.LastUpdateTime = GETDATE()
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

