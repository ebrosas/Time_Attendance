/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetOTReason
*	Description: Get the list of overtime reasons
*
*	Date			Author		Revision No.	Comments:
*	30/10/2016		Ervin		1.0				Created
*	30/11/2016		Ervin		1.1				Added condition that allows saving of overtime records with zero duration
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_InsertUpdateDeleteOvertime
(	
	@autoID						INT, 	
	@otReason					VARCHAR(10),	
	@comment					VARCHAR(1000),
	@userID						VARCHAR(30), 
	@otApproved					VARCHAR(1) = '0', 
	@mealVoucherEligibilityCode	VARCHAR(10) = NULL,
	@otDuration					INT = 0
	
)
AS
	
	--Define constants
	DECLARE @CONST_RETURN_OK		int,
			@CONST_RETURN_ERROR		INT

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Define variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected_TS		INT,
			@rowsAffected_TSE		INT

	--Initialize variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected_TS		= 0,
			@rowsAffected_TSE		= @rowsAffected_TSE

	DECLARE	@otDifference	INT,
			--@mealVoucherEligibilityCode varchar(10),
			@isOTApproved	BIT

	--Start a transaction
	BEGIN TRAN T1

	BEGIN TRY

		--Check for OT approval
		IF @otApproved = 'Y'
			SET @isOTApproved = 1
		ELSE 
			SET @isOTApproved = 0

		--Check for Meal Voucher approval
		--IF @mealVoucherApproved = 'Y'
		--	SET @mealVoucherEligibilityCode = 'YA'
		--ELSE IF @mealVoucherApproved = 'N'
		--	SET @mealVoucherEligibilityCode = 'N'
		--ELSE 
		--	SET @mealVoucherEligibilityCode = 'Y'

		IF @isOTApproved = 1
		BEGIN

			SELECT @otDifference = DateDiff(n, a.OTstartTime, a.OTendTime) 
			FROM tas.Tran_Timesheet_Extra a
			WHERE a.XID_AutoID = @autoID		
			
			--If default then set full OT
			--IF @otDuration = 0 
			--	SET @otDuration = @otDifference

			IF @otDuration > 0 
			BEGIN
            
				IF @otReason IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW')
				BEGIN

					UPDATE tas.Tran_TimeSheet 
					SET OTStartTime =  DATEADD(n, @otDuration * -1, a.OTEndTime),
						OTEndTime = a.OTEndTime,
						OTtype = a.OTtype,
						CorrectionCode = @otReason,	
						Processed = 0,
						LastUpdateUser = @userID, 
						LastUpdateTime = GETDATE(),
						MealVoucherEligibility = @mealVoucherEligibilityCode
					FROM tas.Tran_Timesheet_Extra a
					WHERE  	
						Tran_TimeSheet.AutoID = a.XID_AutoID
						AND Tran_TimeSheet.AutoID = @autoID

					--Get the number of affected rows
					SELECT @rowsAffected_TS = @@rowcount
				END

				ELSE
				BEGIN

					UPDATE tas.Tran_TimeSheet 
					SET OTstartTime = a.OTstartTime, 
						OTendTime = DATEADD(n, @otDuration, a.OTstartTime),
						OTtype = a.OTtype,
						CorrectionCode = @otReason,
						Processed = 0,
						LastUpdateUser = @userID, 
						LastUpdateTime = GETDATE(),
						MealVoucherEligibility = @mealVoucherEligibilityCode
					FROM tas.Tran_TimeSheet_Extra a
					WHERE  	
						Tran_TimeSheet.AutoID = a.XID_AutoID
						AND Tran_TimeSheet.AutoID = @autoID

					--Get the number of affected rows
					SELECT @rowsAffected_TS = @@rowcount
				END
			END

            ELSE
            BEGIN

				UPDATE tas.Tran_TimeSheet 
				SET CorrectionCode = @otReason,
					Processed = 0,
					LastUpdateUser = @userID, 
					LastUpdateTime = GETDATE(),
					MealVoucherEligibility = @mealVoucherEligibilityCode
				FROM tas.Tran_TimeSheet_Extra a
				WHERE  	
					Tran_TimeSheet.AutoID = a.XID_AutoID
					AND Tran_TimeSheet.AutoID = @autoID

				--Get the number of affected rows
				SELECT @rowsAffected_TS = @@rowcount
            END 
		
			UPDATE tas.Tran_Timesheet_Extra
			SET Approved = 1,
				LastUpdateUser = @userID,
				LastUpdateTime = GETDATE(),
				Comment = @comment,
				OTApproved = @otApproved,
				OTReason = @otReason
			WHERE  	
				XID_AutoID = @autoID

			--Get the number of affected rows
			SELECT @rowsAffected_TSE = @@rowcount
		END

		ELSE IF @isOTApproved = 0
		BEGIN

			UPDATE tas.Tran_TimeSheet 
			SET OTstartTime = null, 
				OTendTime = null, 
				OTtype = null, 
				CorrectionCode = @otReason,	
				Processed = 0,
				LastUpdateUser = @userID, 
				LastUpdateTime = GETDATE(),
				MealVoucherEligibility = @mealVoucherEligibilityCode
			WHERE  	
				AutoID = @autoID

			--Get the number of affected rows
			SELECT @rowsAffected_TS = @@rowcount

			UPDATE tas.Tran_Timesheet_Extra
			SET Approved = 0,
				LastUpdateUser = @userID,
				LastUpdateTime = GETDATE(),
				Comment = @comment,
				OTApproved = @otApproved,
				OTReason = @otReason
			WHERE  	
				XID_AutoID = @autoID

			--Get the number of affected rows
			SELECT @rowsAffected_TSE = @@rowcount
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
			@rowsAffected_TS AS TimesheetRowsAffected,
			@rowsAffected_TSE AS TimesheetExtraRowsAffected


/*	Debugging:

PARAMETERS:
	@autoID					INT, 	
	@otReason				VARCHAR(10),	
	@comment				VARCHAR(1000),
	@userID					VARCHAR(30), 
	@otApproved				VARCHAR(1) = '0', 
	@mealVoucherEligibilityCode	VARCHAR(10) = NULL,
	@otDuration				INT = 0

	EXEC tas.Pr_InsertUpdateDeleteOvertime

*/

/*	Checking:

	SELECT	b.*, 
		a.EmpNo,
		a.BusinessUnit,
		a.DT,
		a.dtIN,
		a.dtOUT,
		a.OTStartTime,
		a.OTEndTime,
		a.OTType,
		a.MealVoucherEligibility,
		a.* 
	FROM tas.Tran_Timesheet a 
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE a.EmpNo = 10002160
		AND a.IsLastRow = 1
		AND a.DT IN ('11/01/2016', '10/27/2016')

	BEGIN TRAN T1

	UPDATE tas.Tran_Timesheet_Extra
	SET	Approved = 0,
		LastUpdateUser = NULL,
		LastUpdateTime = NULL,
		OTApproved = 0,
		OTReason = ''
	WHERE XID_AutoID = 4832477

	UPDATE tas.Tran_Timesheet
	SET	OTStartTime =NULL,
		OTEndTime = NULL,
		OTType = NULL,
		LastUpdateUser = NULL,
		LastUpdateTime = NULL
	WHERE AutoID = 4832477

	COMMIT TRAN T1
	ROLLBACK TRAN T1

*/