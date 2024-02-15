/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_PermitCostCenter_CRUD
*	Description: Performs insert, update, and delete operations against "genuser.PermitCostCenter" table
*
*	Date:			Author:		Rev.#:		Comments:
*	18/01/2017		Ervin		1.0			Created
**************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_PermitCostCenter_CRUD
(
	@actionType				TINYINT,	
	@permitID				INT,
	@permitEmpNo			INT,	
	@permitCostCenter		VARCHAR(12),
	@userEmpNo				INT
)
AS	

	--Define constants
	DECLARE @CONST_RETURN_OK		int,
			@CONST_RETURN_ERROR		INT

	--Define variables
	DECLARE @newID				INT,
			@rowsAffected		INT,
			@hasError			BIT,
			@retError			INT,
			@retErrorDesc		VARCHAR(200),
			@permitAppID		INT

	--Initialize constants
	SELECT	@CONST_RETURN_OK	= 0,
			@CONST_RETURN_ERROR	= -1

	--Initialize variables
	SELECT	@newID				= 0,
			@rowsAffected		= 0,
			@hasError			= 0,
			@retError			= @CONST_RETURN_OK,
			@retErrorDesc		= ''

	--Get the Application ID
	SELECT	@permitAppID = UDCID 
	FROM tas.syJDE_UserDefinedCode
	WHERE UDCUDCGID = 17 
		AND RTRIM(UDCCode) = 'TAS3'

	--Start a transaction
	BEGIN TRAN T1

	BEGIN TRY

		IF @actionType = 1		--Insert new record
		BEGIN
		
			INSERT INTO tas.syJDE_PermitCostCenter
			(
				PermitEmpNo,
				PermitCostCenter,
				PermitAppID,
				PermitCreatedBy,
				PermitCreatedDate
			)
			VALUES
			(
				@permitEmpNo,
				@permitCostCenter,
				@permitAppID,
				@userEmpNo,
				GETDATE()
			)
		
			--Get the new ID
			SET @newID = @@identity
		END

		ELSE IF @actionType = 2		--Update existing record
		BEGIN

			UPDATE tas.syJDE_PermitCostCenter
			SET	PermitCostCenter = @permitCostCenter,
				PermitModifiedBy = @userEmpNo,
				PermitModifiedDate = GETDATE()
			WHERE PermitID = @permitID

			SELECT @rowsAffected = @@rowcount 
		END

		ELSE IF @actionType = 3		--Delete existing record 
		BEGIN

			DELETE FROM tas.syJDE_PermitCostCenter
			WHERE PermitID = @permitID

			SELECT @rowsAffected = @@rowcount
		END

		ELSE IF (@actionType = 4)  --Delete record by Emp. No.
		BEGIN

			DELETE FROM tas.syJDE_PermitCostCenter
			WHERE PermitEmpNo = @permitEmpNo

			SELECT @rowsAffected = @@rowcount
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
	SELECT	@newID AS NewIdentityID,
			@rowsAffected AS RowsAffected,
			@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription

GO


/*	Debugging:

	SELECT * FROM tas.syJDE_PermitCostCenter a
	WHERE a.PermitEmpNo = 10003073
	
*/



