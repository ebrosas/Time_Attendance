/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.LicenseRegistry_CRUD
*	Description: This stored procedure is used to perform CRUD operations in "LicenseRegistry" table
*
*	Date			Author		Revision No.	Comments:
*	21/09/2021		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.LicenseRegistry_CRUD
(	
	@actionType				TINYINT,	--(Notes: 0 = Check records, 1 = Insert, 2 = Update, 3 = Delete, 4 = Gets the total number of existing records)	
	@registryID				INT OUTPUT,	 
	@empNo					INT = NULL,
	@licenseNo				VARCHAR(20) = NULL,
	@licenseTypeCode		VARCHAR(10) = NULL,
	@licenseTypeDesc		VARCHAR(50) = NULL,
	@issuingAuthority		VARCHAR(200) = NULL,
	@issuedDate				DATETIME = NULL,	
	@expiryDate				DATETIME = NULL,
	@remarks				VARCHAR(300) = NULL,
	@licenseGUID			VARCHAR(50) = NULL,
	@userActionDate			DATETIME = NULL,
	@userEmpNo				INT = NULL,
	@userEmpName			VARCHAR(100) = NULL,
	@userID					VARCHAR(50) = NULL	
)
AS	
BEGIN

	--Define constants
	DECLARE @CONST_RETURN_OK	INT,
			@CONST_RETURN_ERROR	INT

	--Define variables
	DECLARE @rowsAffected		INT,
			@hasError			BIT,
			@retError			INT,
			@retErrorDesc		VARCHAR(200)

	--Initialize constants
	SELECT	@CONST_RETURN_OK	= 0,
			@CONST_RETURN_ERROR	= -1

	--Initialize variables
	SELECT	@rowsAffected		= 0,
			@hasError			= 0,
			@retError			= @CONST_RETURN_OK,
			@retErrorDesc		= ''

	IF @actionType = 0		--Check existing records
	BEGIN

		IF ISNULL(@empNo, 0) = 0
			SET @empNo = NULL

		--Check existing records
		SELECT * FROM tas.LicenseRegistry a WITH (NOLOCK)
		WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.RegistryID
    END

	ELSE IF @actionType = 1		--Insert record
	BEGIN

		INSERT INTO [tas].[LicenseRegistry]
		(
			EmpNo,
			LicenseNo,
			LicenseTypeCode,
			LicenseTypeDesc,
			IssuingAuthority,
			IssuedDate,
			ExpiryDate,
			Remarks,
			LicenseGUID,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByEmpName,
			CreatedByUser
		)
		VALUES
		(
			@empNo,
			@licenseNo,
			@licenseTypeCode,
			@licenseTypeDesc,
			@issuingAuthority,
			@issuedDate,	
			@expiryDate,
			@remarks,
			@licenseGUID,
			@userActionDate,
			@userEmpNo,
			@userEmpName,
			@userID
		)
		
		--Get the new ID
		SELECT @registryID = SCOPE_IDENTITY()
					
		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@hasError = 1,
					@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE()
		END

		--Return error information to the caller
		SELECT	@registryID AS NewIdentityID,
				@rowsAffected AS RowsAffected,
				@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription
	END 

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		UPDATE tas.LicenseRegistry
		SET LicenseNo = @licenseNo,
			LicenseTypeCode = @licenseTypeCode,
			LicenseTypeDesc = @licenseTypeDesc,
			IssuingAuthority = @issuingAuthority,
			IssuedDate = @issuedDate,
			ExpiryDate = @expiryDate,
			Remarks = @remarks,
			LicenseGUID = @licenseGUID,
			LastUpdatedDate = @userActionDate,
			LastUpdatedByEmpNo = @userEmpNo,
			LastUpdatedByEmpName = @userEmpName,
			LastUpdatedByUser = @userID
		WHERE RegistryID = @registryID

		SELECT @rowsAffected = @@rowcount 

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@hasError = 1,
					@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE()
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
		DELETE FROM tas.LicenseRegistry 
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

		--Return error information to the caller
		SELECT	@rowsAffected AS RowsAffected,
				@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription
    END

	ELSE IF @actionType = 4		--Get record count
	BEGIN

		IF ISNULL(@empNo, 0) = 0
			SET @empNo = NULL

		--Check existing records
		SELECT COUNT(*) AS RecorCount 
		FROM tas.LicenseRegistry a WITH (NOLOCK)
		WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
    END

END  


/*	Debug:
	
	SELECT * FROM tas.LicenseRegistry a

	TRUNCATE TABLE tas.LicenseRegistry

PARAMETERS:
	@actionType				TINYINT,	--(Notes: 0 = Check records, 1 = Insert, 2 = Update, 3 = Delete)	
	@registryID				INT OUTPUT,	
	@empNo					INT = NULL,
	@licenseNo				VARCHAR(20) = NULL,
	@licenseTypeCode		VARCHAR(10) = NULL,
	@licenseTypeDesc		VARCHAR(50) = NULL,
	@issuingAuthority		VARCHAR(200) = NULL,
	@issuedDate				DATETIME = NULL,	
	@expiryDate				DATETIME = NULL,
	@remarks				VARCHAR(300) = NULL,
	@licenseGUID			VARCHAR(50) = NULL,
	@userActionDate			DATETIME = NULL,
	@userEmpNo				INT = NULL,
	@userEmpName			VARCHAR(100) = NULL,
	@userID					VARCHAR(50) = NULL	 

	EXEC tas.LicenseRegistry_CRUD 0, 0
	EXEC tas.LicenseRegistry_CRUD 3, 0, 60002
	EXEC tas.LicenseRegistry_CRUD 4, 0, 60002

*/