/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ExpiredLicenseNotificationLog_CRUD
*	Description: This stored procedure is used to perform CRUD operations against "ExpiredLicenseNotificationLog" table
*
*	Date			Author		Revision No.	Comments:
*	22/01/2023		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.ExpiredLicenseNotificationLog_CRUD
(	
	@actionType			TINYINT,		
	@supervisorNo		INT = 0,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = NULL,
	@licenseTypeCode	VARCHAR(10) = NULL,
	@issuedDate			DATETIME = NULL,
	@expiryDate			DATETIME = NULL
)
AS	

	--Define constants
	DECLARE @CONST_RETURN_OK		INT = 0,
			@CONST_RETURN_ERROR		INT = -1

	--Define other variables
	DECLARE @hasError				BIT = 0,
			@retError				INT = -1,
			@retErrorDesc			VARCHAR(200) = '',
			@rowsAffected			INT = 0			

	IF ISNULL(@supervisorNo, 0) = 0
		SET @supervisorNo = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @actionType = 0		--Check existing records
	BEGIN

		--Check existing records
		SELECT a.* 
		FROM tas.ExpiredLicenseNotificationLog a WITH (NOLOCK)
			INNER JOIN tas.IDCardRegistry b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			INNER JOIN tas.LicenseRegistry c WITH (NOLOCK) ON b.EmpNo = c.EmpNo
		WHERE (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (a.SupervisorNo = @supervisorNo OR @supervisorNo IS NULL)
		ORDER BY a.CostCenter, a.EmpNo
    END

	ELSE IF @actionType = 1		--Insert record
	BEGIN

		INSERT INTO [tas].[ExpiredLicenseNotificationLog]
        (
			[EmpNo]
           ,[SupervisorNo]
           ,[CostCenter]
           ,[LicenseTypeCode]
           ,[IssuedDate]
           ,[ExpiryDate]
           ,[NotificationCounter]
           ,[CreatedDate]
           ,[CreatedByEmpNo]
           ,[CreatedByUserID]
		)
		SELECT	@empNo,
				@supervisorNo,
				@costCenter,
				@licenseTypeCode,
				@issuedDate,
				@expiryDate,
				1 AS NotificationCounter,
				GETDATE(),
				0,
				'System Admin'
		
		--Get the number of affected records 
		SELECT @rowsAffected = @@rowcount 						
					
		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription,
				@rowsAffected AS RowsAffected
	END 

	ELSE IF @actionType = 2		--Update the counter 
	BEGIN

		IF EXISTS
		(
			SELECT 1 FROM [tas].[ExpiredLicenseNotificationLog] a WITH (NOLOCK)
			WHERE (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.SupervisorNo = @supervisorNo OR @supervisorNo IS NULL) 
		)
		BEGIN

			UPDATE tas.ExpiredLicenseNotificationLog
			SET NotificationCounter = ISNULL(NotificationCounter, 0) + 1
			WHERE (RTRIM(CostCenter) = @costCenter OR @costCenter IS NULL)
				AND (EmpNo = @empNo OR @empNo IS NULL)
				AND (SupervisorNo = @supervisorNo OR @supervisorNo IS NULL) 

			--Get the number of affected records 
			SELECT @rowsAffected = @@rowcount 

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			--Return error information to the caller
			SELECT	@hasError AS HasError, 
					@retError AS ErrorCode, 
					@retErrorDesc AS ErrorDescription,
					@rowsAffected AS RowsAffected
        END 
	END 

	ELSE IF @actionType = 3		--Delete record
	BEGIN

		--Check existing records
		DELETE FROM tas.ExpiredLicenseNotificationLog 
		WHERE (RTRIM(CostCenter) = @costCenter OR @costCenter IS NULL)
			AND (EmpNo = @empNo OR @empNo IS NULL)
			AND (SupervisorNo = @supervisorNo OR @supervisorNo IS NULL)

		--Get the number of affected records 
		SELECT @rowsAffected = @@rowcount 

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription,
				@rowsAffected AS RowsAffected
    END

GO 


/*	Debug:

PARAMETERS:
	@actionType		TINYINT,
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''

	
	SELECT * FROM tas.ExpiredLicenseNotificationLog a

	EXEC tas.ExpiredLicenseNotificationLog_CRUD 0, '07/30/2020', '09/15/2020'
	EXEC tas.ExpiredLicenseNotificationLog_CRUD 0, '07/30/2020', '09/15/2020', 10003435				--Check record
	EXEC tas.ExpiredLicenseNotificationLog_CRUD 1, '07/30/2020', '09/15/2020', 10003830				--Insert record (by emp. no.)
	EXEC tas.ExpiredLicenseNotificationLog_CRUD 1, '07/30/2020', '09/15/2020', 0, '2110'				--Insert record (by cost center)
	EXEC tas.ExpiredLicenseNotificationLog_CRUD 2, '07/30/2020', '09/15/2020', 10003505				--Delete record

*/