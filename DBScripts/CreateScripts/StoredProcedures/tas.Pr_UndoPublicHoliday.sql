/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_AutoApproveOvertime
*	Description: This stored procedure is use to undo the public holiday processing in the attendance records
*
*	Date:			Author:		Rev. #:		Comments:
*	03/09/2019		Ervin		1.0			Created
*****************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_UndoPublicHoliday
(
	@actionType			INT,	--(Note: 1 => Change holiday type from special holiday (HE) into in-lieu holiday (D))
	@attendanceDate		DATETIME
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 
	
	--Define constants
	DECLARE @CONST_RETURN_OK		int,
			@CONST_RETURN_ERROR		int		

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Define variables
	DECLARE @rowsAffected			int,
			@hasError				bit,
			@retError				int,
			@retErrorDesc			varchar(200)			

	--Initialize variables
	SELECT	@rowsAffected			= 0,
			@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= ''

	BEGIN TRY

		IF @actionType = 1
		BEGIN
    
			--Set value of "AbsenceReasonColumn" to 'CAL DILdw'
			UPDATE tas.Tran_Timesheet 
			SET AbsenceReasonColumn = 'CAL DILdw',
				LastUpdateUser = 'System Admin',
				LastUpdateTime = GETDATE()
			WHERE RTRIM(AbsenceReasonColumn) = 'CAL HOL'
				AND DT = @attendanceDate

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			--Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Remove auto approved OT in "Tran_Timesheet" table
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.OTStartTime = NULL, 
					tas.Tran_Timesheet.OTEndTime = NULL, 
					tas.Tran_Timesheet.OTType = NULL,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE() 
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
				WHERE a.OTStartTime IS NOT NULL
					AND a.OTEndTime IS NOT NULL 
					AND a.DT = @attendanceDate

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				--Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Remove overtime records in "Tran_Timesheet_Extra" table 
					UPDATE tas.Tran_Timesheet_Extra
					SET tas.Tran_Timesheet_Extra.OTStartTime = NULL,
						tas.Tran_Timesheet_Extra.OTEndTime = NULL,
						tas.Tran_Timesheet_Extra.OTType = NULL,
						tas.Tran_Timesheet_Extra.LastUpdateUser = 'System Admin',
						tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()					
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
					WHERE a.DT = @attendanceDate
						AND (b.OTstartTime IS NOT NULL AND b.OTendTime IS NOT NULL)
						AND RTRIM(a.ShiftCode) <> 'O'
						AND a.Duration_Worked_Cumulative <= a.Duration_Required

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END

					--Checks if there's no error
					IF @retError = @CONST_RETURN_OK
					BEGIN

						--Set No-Pay-Hours to Shift Workers who did not complete the required work duration
						UPDATE tas.Tran_Timesheet
						SET tas.Tran_Timesheet.NoPayHours = a.Duration_Required - a.Duration_Worked_Cumulative,
							tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
							tas.Tran_Timesheet.LastUpdateTime = GETDATE()
						FROM tas.Tran_Timesheet a
							INNER JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL 
						WHERE a.DT = @attendanceDate
							AND a.Duration_Required > 0
							AND a.Duration_Worked_Cumulative < a.Duration_Required
							AND (a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL)
							AND ISNULL(a.NoPayHours, 0) = 0
							AND NOT
							(
								a.IsSalStaff = 1
								OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)
								OR (a.IsSalStaff = 0 AND a.IsDayWorker_OR_Shifter = 1)
								OR a.IsDriver = 1 
								OR a.IsLiasonOfficer = 1 
								OR RTRIM(a.ShiftCode) = 'O'		
								OR ISNULL(a.LeaveType, '') <> ''
							)

						--Checks for error
						IF @@ERROR <> @CONST_RETURN_OK
						BEGIN
				
							SELECT	@retError = @CONST_RETURN_ERROR,
									@hasError = 1
						END

						--Checks if there's no error
						IF @retError = @CONST_RETURN_OK
						BEGIN

							--Set value of "IsPublicHoliday" to zero
							UPDATE tas.Tran_Timesheet 
							SET IsPublicHoliday = 0,
								LastUpdateUser = 'System Admin',
								LastUpdateTime = GETDATE()
							WHERE DT = @attendanceDate

							--Get the number of affected records 
							SELECT @rowsAffected = @@rowcount
						END 
					END 
				END 
			END 
		END 

	END TRY

	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected AS RowsAffected

GO 

/*	Debug:

PARAMETERS:
	@actionType			INT,	--(Note: 1 => Change holiday type from special holiday (HE) into in-lieu holiday (D))
	@attendanceDate		DATETIME

	EXEC tas.Pr_UndoPublicHoliday 1, '01/01/2016'

*/