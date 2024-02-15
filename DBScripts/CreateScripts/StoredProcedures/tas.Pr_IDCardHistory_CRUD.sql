/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_IDCardHistory_CRUD
*	Description: This stored procedure is used to perform CRUD operations for "IDCardHistory" table
*
*	Date			Author		Revision No.	Comments:
*	11/10/2021		Ervin		1.0				Created
*	13/12/2021		Ervin		1.1				Implemented automation to populate data in UNIS system
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_IDCardHistory_CRUD
(	
	@actionType			TINYINT,	--(Notes: 0 = Check existing record, 1 = Insert, 2 = Update, 3 = Delete)	
	@historyID			INT OUTPUT, 
	@empNo				INT = NULL,
	@isContractor		BIT = NULL,
	@cardRefNo			VARCHAR(20) = NULL,
	@remarks			VARCHAR(300) = NULL,
	@cardGUID			VARCHAR(50) = NULL,
	@userActionDate		DATETIME = NULL,
	@userEmpNo			INT = NULL,
	@userID				VARCHAR(50) = NULL	
)
AS	
BEGIN

	--Define constants
	DECLARE @CONST_RETURN_OK	INT = 0,
			@CONST_RETURN_ERROR	INT = -1

	--Define variables
	DECLARE @rowsAffected		INT = 0,
			@hasError			BIT = 0,
			@retError			INT = @CONST_RETURN_OK,
			@retErrorDesc		VARCHAR(200) = '',
			@return_value		INT = 0,
			@retErrorMessage	VARCHAR(1000) = ''

	IF @actionType = 0		--Check existing records
	BEGIN

		IF ISNULL(@empNo, 0) = 0
			SET @empNo = NULL

		--Check existing records
		SELECT	a.HistoryID,
				a.EmpNo,
				a.IsContractor,
				a.CardRefNo,
				a.Remarks,
				a.CardGUID,
				a.CreatedDate,
				a.CreatedByEmpNo,
				LTRIM(RTRIM(b.YAALPH)) AS 'CreatedByEmpName',
				a.CreatedByUser,
				a.LastUpdatedDate,
				a.LastUpdatedByEmpNo,
				LTRIM(RTRIM(c.YAALPH)) AS LastUpdatedByEmpName,
				a.LastUpdatedByUser
		FROM tas.IDCardHistory a WITH (NOLOCK)
			LEFT JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.CreatedByEmpNo = CAST(b.YAAN8 AS INT)
			LEFT JOIN tas.syJDE_F060116 c WITH (NOLOCK) ON a.LastUpdatedByEmpNo = CAST(c.YAAN8 AS INT)
		WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.HistoryID DESC
    END

	ELSE IF @actionType = 1		--Insert record
	BEGIN

		INSERT INTO tas.IDCardHistory
		(
			EmpNo,
			IsContractor,
			CardRefNo,
			Remarks,
			CardGUID,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUser
		)
		VALUES
		(
			@empNo,
			@isContractor,
			@cardRefNo,
			@remarks,
			@cardGUID,
			@userActionDate,
			@userEmpNo,
			@userID
		)
		
		--Get the new ID
		SET @historyID = @@identity				
					
		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@hasError = 1,
					@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE()
		END

		ELSE			
        BEGIN

			IF @isContractor = 1	--Rev. #1.1
			BEGIN

				--Automate updating the related data in UNIS system
				EXEC	@return_value = tas.Pr_Automate_UNISAccess
						@actionType = 4,
						@empNo = @empNo,
						@rowsAffected = @rowsAffected OUTPUT,
						@hasError = @hasError OUTPUT,
						@retError = @retError OUTPUT,
						@retErrorDesc = @retErrorDesc OUTPUT

				IF @hasError = 1
				BEGIN

					SET @retErrorMessage = 'Message= tas.Pr_Automate_UNISAccess - ' + @retErrorDesc + '|' + 'Number=' + @retError 
					RAISERROR(@retErrorMessage, 10, 1)
				END 
			END 
        END 

		--Return error information to the caller
		SELECT	@historyID AS NewIdentityID,
				@rowsAffected AS RowsAffected,
				@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription
	END 

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		UPDATE tas.IDCardHistory
		SET CardRefNo = @cardRefNo,
			Remarks = @remarks,
			LastUpdatedDate = @userActionDate,
			LastUpdatedByEmpNo = @userEmpNo,
			LastUpdatedByUser = @userID
		WHERE HistoryID = @historyID

		SELECT @rowsAffected = @@rowcount 

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@hasError = 1,
					@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE()
		END

		ELSE
        BEGIN

			IF @isContractor = 1	--Rev. #1.1
			BEGIN

				--Automate updating the related data in UNIS system
				EXEC	@return_value = tas.Pr_Automate_UNISAccess
						@actionType = 4,
						@empNo = @empNo,
						@rowsAffected = @rowsAffected OUTPUT,
						@hasError = @hasError OUTPUT,
						@retError = @retError OUTPUT,
						@retErrorDesc = @retErrorDesc OUTPUT

				IF @hasError = 1
				BEGIN

					SET @retErrorMessage = 'Message= tas.Pr_Automate_UNISAccess - ' + @retErrorDesc + '|' + 'Number=' + @retError 
					RAISERROR(@retErrorMessage, 10, 1)
				END 
			END 
        END 

		--Return error information to the caller
		SELECT	@rowsAffected AS RowsAffected,
				@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription
	END 

	ELSE IF @actionType = 3		--Delete record
	BEGIN

		--Check existing records
		DELETE FROM tas.IDCardHistory 
		WHERE EmpNo = @empNo

		--Get the number of affected records 
		SELECT @rowsAffected = @@rowcount 

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@hasError = 1,
					@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE()
		END

		ELSE
        BEGIN

			IF @isContractor = 1	--Rev. #1.1
			BEGIN

				--Automate updating the related data in UNIS system
				EXEC	@return_value = tas.Pr_Automate_UNISAccess
						@actionType = 4,
						@empNo = @empNo,
						@rowsAffected = @rowsAffected OUTPUT,
						@hasError = @hasError OUTPUT,
						@retError = @retError OUTPUT,
						@retErrorDesc = @retErrorDesc OUTPUT

				IF @hasError = 1
				BEGIN

					SET @retErrorMessage = 'Message= tas.Pr_Automate_UNISAccess - ' + @retErrorDesc + '|' + 'Number=' + @retError 
					RAISERROR(@retErrorMessage, 10, 1)
				END 
			END 
        END 

		--Return error information to the caller
		SELECT	@rowsAffected AS RowsAffected,
				@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription
    END		
END  


/*	Debug:
	
	SELECT * FROM tas.IDCardHistory a

	TRUNCATE TABLE tas.IDCardHistory

PARAMETERS:
	@actionType			TINYINT,	--(Notes: 0 = Check existing record, 1 = Insert, 2 = Update, 3 = Delete)	
	@historyID			INT OUTPUT, 
	@empNo				INT = NULL,
	@isContractor		BIT = NULL,
	@cardRefNo			VARCHAR(20) = NULL,
	@remarks			VARCHAR(300) = NULL,
	@userActionDate		DATETIME = NULL,
	@userEmpNo			INT = NULL,
	@userID				VARCHAR(50) = NULL	

	EXEC tas.Pr_IDCardHistory_CRUD 0, 0, 10003632

*/