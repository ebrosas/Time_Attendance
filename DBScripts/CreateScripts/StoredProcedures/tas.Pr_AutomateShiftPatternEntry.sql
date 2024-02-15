/**************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_AutomateShiftPatternEntry
*	Description: This stored procedure is used to automate the adding of new shift pattern change record when the ending date is equal to today's date for temporary shift pattern
*
*	Date			Author		Rev.#		Comments:
*	03/06/2018		Ervin		1.0			Created
*	16/06/2018		Ervin		1.1			Refactored the code to get the old ShiftPatCode from "tas.Tran_ShiftPatternUpdates" table
*	23/06/2018		Ervin		1.2			Commented the code that returns an output dataset 
****************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_AutomateShiftPatternEntry
(   
	@empNo	INT = 0 
)
AS

	--Validate parameter
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected			INT

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0

	IF EXISTS
    (
		SELECT a.AutoID
		FROM tas.Tran_ShiftPatternChanges a
			CROSS APPLY		--Rev. #1.1
			(
				SELECT TOP 1 * FROM tas.Tran_ShiftPatternUpdates 
				WHERE EmpNo = a.EmpNo
					AND DateX < a.EffectiveDate
				ORDER BY DateX DESC
			) b
			CROSS APPLY
			(
				SELECT TOP 1 ShiftPatCode
				FROM tas.Tran_Timesheet 
				WHERE EmpNo = a.EmpNo
					AND IsLastRow = 1
					AND DT < a.EffectiveDate
				ORDER BY DT DESC
			) c
		WHERE 
			RTRIM(a.ChangeType) = 'T'
			AND a.EndingDate IS NOT NULL	
			AND a.EndingDate = CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
			--AND a.EndingDate BETWEEN '06/14/2018' AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))	--(Note: Uncomment this line of code to process previous date)
			AND ISNULL(b.Effective_ShiftPatCode, '') <> ''			
			AND NOT EXISTS
			(
				SELECT AutoID FROM tas.Tran_ShiftPatternChanges
				WHERE EmpNo = a.EmpNo 
					AND EffectiveDate = DATEADD(DAY, 1, a.EndingDate)		
					--AND EffectiveDate = CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, 1, GETDATE()), 12))		--(Note: Uncomment this line of code to process previous date)				
					AND RTRIM(ShiftPatCode) = b.Effective_ShiftPatCode
					AND ShiftPointer = tas.fnGetShiftPointerBasedOnDate(a.EmpNo, DATEADD(DAY, 1, a.EndingDate)) 
					AND RTRIM(ChangeType) = 'D'
			)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
	)
	BEGIN 

		--Start a transaction
		BEGIN TRAN T1

		BEGIN TRY

		INSERT INTO tas.Tran_ShiftPatternChanges
		(
			EmpNo,
			EffectiveDate,
			ShiftPatCode,
			ShiftPointer,
			ChangeType,
			LastUpdateUser,
			LastUpdateTime
		)
		SELECT	a.EmpNo,
				DATEADD(DAY, 1, a.EndingDate) AS EffectiveDate,
				--CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, 1, GETDATE()), 12)) AS EffectiveDate,	--(Note: Uncomment this line of code to process previous date)

				RTRIM(b.Effective_ShiftPatCode) AS Old_ShiftPatCode,  

				tas.fnGetShiftPointerBasedOnDate(a.EmpNo, DATEADD(DAY, 1, a.EndingDate)) AS New_ShiftPointer,
				--tas.fnGetShiftPointerBasedOnDate(a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, 1, GETDATE()), 12))) AS New_ShiftPointer,		--(Note: Uncomment this line of code to process previous date)

				'D' AS ChangeType,
				'TAS Admin' AS LastUpdateUser,
				GETDATE() AS LastUpdateTime
		FROM tas.Tran_ShiftPatternChanges a			
			CROSS APPLY
			(
				SELECT TOP 1 * FROM tas.Tran_ShiftPatternUpdates 
				WHERE EmpNo = a.EmpNo
					AND DateX < a.EffectiveDate
				ORDER BY DateX DESC
			) b
			CROSS APPLY
			(
				SELECT TOP 1 ShiftPatCode
				FROM tas.Tran_Timesheet 
				WHERE EmpNo = a.EmpNo
					AND IsLastRow = 1
					AND DT < a.EffectiveDate
				ORDER BY DT DESC
			) c
		WHERE 
			RTRIM(a.ChangeType) = 'T'
			AND a.EndingDate IS NOT NULL	
			
			AND a.EndingDate = CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
			--AND a.EndingDate BETWEEN '06/14/2018' AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))	--(Note: Uncomment this line of code to process previous date)

			AND ISNULL(b.Effective_ShiftPatCode, '') <> ''
			AND NOT EXISTS
			(
				SELECT AutoID FROM tas.Tran_ShiftPatternChanges
				WHERE EmpNo = a.EmpNo 
					AND EffectiveDate = DATEADD(DAY, 1, a.EndingDate)				
					--AND EffectiveDate = CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, 1, GETDATE()), 12))	--(Note: Uncomment this line of code to process previous date)		

					AND RTRIM(ShiftPatCode) = b.Effective_ShiftPatCode

					AND ShiftPointer = tas.fnGetShiftPointerBasedOnDate(a.EmpNo, DATEADD(DAY, 1, a.EndingDate))
					--AND ShiftPointer = tas.fnGetShiftPointerBasedOnDate(a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, 1, GETDATE()), 12)))	--(Note: Uncomment this line of code to process previous date)
					AND RTRIM(ChangeType) = 'D'
			)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.EmpNo

		--Get the number of affected records in the "Tran_Timesheet_Extra" table
		SELECT @rowsAffected = @@rowcount

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		END TRY
		BEGIN CATCH

			--Capture the error
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1

		END CATCH

		IF @retError = @CONST_RETURN_OK
		BEGIN

			IF @@TRANCOUNT > 0
				COMMIT TRANSACTION		
		END

		ELSE
		BEGIN

			IF @@TRANCOUNT > 0
				ROLLBACK TRANSACTION
		END

		--Rev. #1.2
		--Return error information to the caller
		--SELECT	@hasError AS HasError, 
		--		@retError AS ErrorCode, 
		--		@retErrorDesc AS ErrorDescription,
		--		@rowsAffected AS RowsAffected
    END 

	/*	Rev. #1.2
	ELSE
    BEGIN

		SELECT	1 AS HasError, 
				1000 AS ErrorCode, 
				'No record found!' AS ErrorDescription,
				0 AS RowsAffected
    END 
	*/

GO 

/*	Debugging:

	EXEC tas.Pr_AutomateShiftPatternEntry 
	EXEC tas.Pr_AutomateShiftPatternEntry 10003512

	SELECT * FROM tas.Tran_ShiftPatternChanges a
	WHERE RTRIM(LastUpdateUser) = 'TAS Admin'
		AND RTRIM(a.ChangeType) = 'D'


	BEGIN TRAN T1

	DELETE FROM tas.Tran_ShiftPatternChanges
	WHERE RTRIM(LastUpdateUser) = 'TAS Admin'

	DELETE FROM tas.Tran_ShiftPatternChanges
	WHERE AutoID = 19037

	COMMIT TRAN T1
	
*/


