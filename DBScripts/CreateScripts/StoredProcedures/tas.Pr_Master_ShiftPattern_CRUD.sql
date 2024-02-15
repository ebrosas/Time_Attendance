/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Master_ShiftPattern_CRUD
*	Description: Performs insert, update, and delete operations against "tas.Master_ShiftPattern" table
*
*	Date:			Author:		Rev.#:		Comments:
*	16/06/2018		Ervin		1.0			Created
**************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_Master_ShiftPattern_CRUD
(
	@actionType				TINYINT,	
	@shiftPatCode			VARCHAR(2),
	@shiftPointer			INT,
	@shiftCode				VARCHAR(10)
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
			@retErrorDesc		VARCHAR(200)

	--Initialize constants
	SELECT	@CONST_RETURN_OK	= 0,
			@CONST_RETURN_ERROR	= -1

	--Initialize variables
	SELECT	@newID				= 0,
			@rowsAffected		= 0,
			@hasError			= 0,
			@retError			= @CONST_RETURN_OK,
			@retErrorDesc		= ''

	--Start a transaction
	--BEGIN TRAN T1

	BEGIN TRY

		IF @actionType = 1		--Insert new record
		BEGIN
		
			INSERT INTO tas.Master_ShiftPattern
			(
				ShiftPatCode,
				ShiftPointer,
				ShiftCode
			)
			VALUES
			(
				@shiftPatCode,
				@shiftPointer,
				@shiftCode
			)
		
			--Get the new ID
			SET @newID = @@identity
		END

		ELSE IF @actionType = 2		--Update existing record
		BEGIN

			UPDATE tas.Master_ShiftPattern
			SET	ShiftPointer = @shiftPointer,
				ShiftCode = @shiftCode
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode

			SELECT @rowsAffected = @@rowcount 
		END

		ELSE IF @actionType = 3		--Delete existing record 
		BEGIN

			DELETE FROM tas.Master_ShiftPattern
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode

			SELECT @rowsAffected = @@rowcount
		END

	END TRY

	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

	--IF @retError = @CONST_RETURN_OK
	--	COMMIT TRANSACTION T1		
	--ELSE
	--	ROLLBACK TRANSACTION T1

	--Return error information to the caller
	SELECT	@newID AS NewIdentityID,
			@rowsAffected AS RowsAffected,
			@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription

GO


/*	Debugging:

	SELECT * FROM tas.Master_ShiftPattern a
	WHERE a.PermitEmpNo = 10003073
	
*/



