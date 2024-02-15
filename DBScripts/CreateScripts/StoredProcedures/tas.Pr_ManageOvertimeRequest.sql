/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_ManageOvertimeRequest
*	Description: This stored procedure is used to manage a submitted overtime request
*
*	Date			Author		Rev. #			Comments:
*	26/07/2017		Ervin		1.0				Created
*	06/09/2017		Ervin		1.1				Set the OT start and end times based from the values of "OTStartTime_Orig" and "OTEndTime_Orig" fields 	
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_ManageOvertimeRequest
(	
	@actionType			TINYINT,		--(Note: 1 = Cancel overtime request)
	@otRequestNo		BIGINT,
	@userEmpNo			INT,	
	@userEmpName		VARCHAR(100), 
	@userID				VARCHAR(50)
)
AS	
	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT

	--Define workflow status variables
	DECLARE	@statusID					int,
			@statusCode					varchar(10),
			@statusDesc					varchar(200),
			@statusHandlingCode			varchar(50)
				
	--Define other variables
	DECLARE @hasError						BIT,
			@retError						INT,
			@retErrorDesc					VARCHAR(200),
			@rowsAffected_OTDetail			INT,
			@rowsAffected_OTReq				INT,
			@tsAutoID						INT,
			@currentRequestSubmissionDate	DATETIME 

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize workflow status variables
	SELECT	@statusID						= NULL,
			@statusCode						= NULL,
			@statusDesc						= NULL,
			@statusHandlingCode				= NULL

	--Initialize other variables
	SELECT	@hasError						= 0,
			@retError						= @CONST_RETURN_OK,
			@retErrorDesc					= '',
			@rowsAffected_OTDetail			= 0,
			@rowsAffected_OTReq				= 0,
			@tsAutoID						= 0,
			@currentRequestSubmissionDate	= NULL

	--Start a transaction
	BEGIN TRAN T1

	BEGIN TRY

		--Get the user employee name and email address
		/*
		IF @userEmpNo > 0
		BEGIN

			SELECT	@userEmpName = RTRIM(a.EmpName),
					@userEmail = LTRIM(RTRIM(ISNULL(b.EAEMAL, '')))
			FROM tas.Master_Employee_JDE_View_V2 a
				LEFT JOIN tas.syJDE_F01151 b ON a.EmpNo = CAST(b.EAAN8 AS INT) AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
			WHERE a.EmpNo = @userEmpNo
        END 
		*/

		--Get Overtime Request details
		SELECT	@tsAutoID = a.TS_AutoID,
				@currentRequestSubmissionDate = b.RequestSubmissionDate
		FROM tas.OvertimeRequest a
			INNER JOIN tas.OvertimeWFTransactionActivity b ON a.OTRequestNo = b.OTRequestNo AND a.TS_AutoID = b.TS_AutoID
		WHERE a.OTRequestNo = @otRequestNo

		IF @actionType = 1		--Cancel submitted overtime request
		BEGIN

			--Update "Tran_Timesheet_Extra" table to set overtime to unprocessed
			UPDATE tas.Tran_Timesheet_Extra
			SET Approved = 0,
				OTApproved = '0',
				OTReason = NULL,
				Comment = NULL,
				OTstartTime = a.OTStartTime_Orig,	--Rev. #1.1
				OTendTime = a.OTEndTime_Orig,
				LastUpdateUser = @userID, 
				LastUpdateTime = GETDATE()
			FROM tas.OvertimeRequest a
			WHERE 
				Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
				AND a.OTRequestNo = @otRequestNo 	

			--Get the number of overtime details record affected
			SELECT @rowsAffected_OTDetail = @@rowcount

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Get "Cancelled By User" status
				SELECT	@statusID	= UDCID,
						@statusCode = RTRIM(UDCCode), 
						@statusDesc = RTRIM(UDCDesc1),
						@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
				FROM tas.syJDE_UserDefinedCode a	
				WHERE RTRIM(a.UDCSpecialHandlingCode) = 'Cancelled'
					AND a.UDCUDCGID = 9
					AND RTRIM(a.UDCField) = '01'									

				--Update "OvertimeRequest" table set workflow status to cancelled
				UPDATE tas.OvertimeRequest
				SET StatusID = @statusID,
					StatusCode = @statusCode,
					StatusDesc = @statusDesc,
					StatusHandlingCode = @statusHandlingCode,
					LastUpdateEmpNo = @userEmpNo,
					LastUpdateEmpName = @userEmpName,
					LastUpdateUserID = @userID,
					LastUpdateTime = GETDATE()
				WHERE OTRequestNo = @otRequestNo

				--Get the number of overtime records affected
				SELECT @rowsAffected_OTReq = @@ROWCOUNT

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Insert history record
					INSERT INTO tas.OvertimeWFRoutineHistory
					(						
						OTRequestNo,
						TS_AutoID,
						RequestSubmissionDate,
						HistDesc,
						HistCreatedBy,
						HistCreatedName,
						HistCreatedDate
					)
					SELECT	@otRequestNo,
							@tsAutoID,
							@currentRequestSubmissionDate,
							ISNULL(@statusDesc, '') AS HistDesc, 
							ISNULL(@userEmpNo, 0) AS HistCreatedBy,
							@userEmpName AS HistCreatedName,
							GETDATE() AS HistCreatedDate

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END

					-- Checks if there's no error
					IF @retError = @CONST_RETURN_OK
					BEGIN

						--Update the workflow status
						--Set the current activity into completed state
						--UPDATE tas.OvertimeWFTransactionActivity
						--SET IsCompleted = 1
						--WHERE OTRequestNo = @otRequestNo
						--	AND TS_AutoID = @tsAutoID
						--	AND IsCurrent = 1

						--Set all activities into bypassed state
						UPDATE tas.OvertimeWFTransactionActivity
						SET ActStatusID = 108
						WHERE OTRequestNo = @otRequestNo
							AND TS_AutoID = @tsAutoID
							--AND ISNULL(IsCurrent, 0) = 0
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

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected_OTDetail AS OvertimeRowsAffected,
			@rowsAffected_OTReq AS OvertimeRequestRowsAffected


/*	Debugging:

PARAMETERS:
	@actionType			TINYINT,		--(Note: 1 = Cancel overtime request)
	@otRequestNo		BIGINT,
	@userEmpNo			INT,	
	@userID				VARCHAR(50)

	EXEC tas.Pr_ManageOvertimeRequest 1, 1, 10003632, 'ervin'

*/

/*	Checking:

	SELECT * FROM tas.OvertimeRequest a
	WHERE a.OTRequestNo = 1

	--Show submitted overtime
	SELECT c.OTRequestNo, a.EmpNo, a.BusinessUnit, a.DT, a.MealVoucherEligibility, a.AutoID, b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
		INNER JOIN tas.OvertimeRequest c ON a.AutoID = c.TS_AutoID AND a.EmpNo = c.EmpNo AND a.DT = c.DT
	WHERE 
		a.DT BETWEEN '16/03/2016' AND '15/04/2016' 
		AND a.EmpNo > 10000000
		AND RTRIM(a.BusinessUnit) = '2110'
	ORDER BY a.BusinessUnit, a.DT, a.EmpNo

*/