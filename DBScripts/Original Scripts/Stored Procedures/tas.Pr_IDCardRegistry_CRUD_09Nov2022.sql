USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_IDCardRegistry_CRUD]    Script Date: 09/11/2022 09:40:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_IDCardRegistry_CRUD
*	Description: This stored procedure is used to perform CRUD operations for "IDCardRegistry" table
*
*	Date			Author		Revision No.	Comments:
*	07/10/2021		Ervin		1.0				Created
*	13/12/2021		Ervin		1.1				Implemented automation to populate data in UNIS system
*	17/03/2022		Ervin		1.1				Populated the missing required data in several Unis tables
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_IDCardRegistry_CRUD]
(	
	@actionType			TINYINT,	--(Notes: 0 = Check records, 1 = Insert, 2 = Update, 3 = Delete)	
	@registryID			INT OUTPUT, 
	@empNo				INT = NULL,
	@empName			VARCHAR(100) = NULL,
	@position			VARCHAR(50) = NULL,
	@customCostCenter	VARCHAR(100) = NULL,
	@cprNo				VARCHAR(30) = NULL,
	@bloodGroup			VARCHAR(10) = NULL,
	@isContractor		BIT = NULL,
	@empPhoto			VARBINARY(MAX)= NULL,
	@base64Photo		VARCHAR(MAX)= NULL,
	@imageFileName		VARCHAR(100) = NULL,
	@imageFileExt		VARCHAR(10) = NULL,
	@userActionDate		DATETIME = NULL,
	@userEmpNo			INT = NULL,
	@userID				VARCHAR(50) = NULL,
	@excludePhoto		BIT = NULL	
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
		SELECT	a.RegistryID,
				a.EmpNo,
				CASE WHEN a.IsContractor = 1 
					THEN RTRIM(c.FirstName) + ' ' + RTRIM(c.LastName) 
					ELSE RTRIM(a.EmpName)
				END AS EmpName,
				CASE WHEN a.IsContractor = 1 
					THEN RTRIM(c.JobTitle)
					ELSE RTRIM(a.Position)
				END AS Position,
				CASE WHEN a.IsContractor = 1 
					THEN RTRIM(c.CostCenter)
					ELSE RTRIM(b.CostCenter)
				END AS CostCenter,	
				CASE WHEN a.IsContractor = 1 
					THEN RTRIM(c.CostCenterName)
					ELSE RTRIM(b.CostCenterName)
				END AS CostCenterName,	
				a.CustomCostCenter,			
				a.CPRNo,
				a.BloodGroup,
				RTRIM(d.UDCDesc1) AS 'BloodGroupDesc', 
				CASE WHEN a.IsContractor = 1 
					THEN RTRIM(c.IDNumber)
					ELSE NULL 
				END AS IDNumber,
				CASE WHEN a.IsContractor = 1 
					THEN RTRIM(c.CompanyName)
					ELSE NULL 
				END AS CompanyName,
				CASE WHEN a.IsContractor = 1 THEN RTRIM(c.CostCenter) ELSE RTRIM(b.CostCenter) END AS CostCenter,	
				CASE WHEN a.IsContractor = 1 THEN RTRIM(c.CostCenterName) ELSE RTRIM(b.CostCenterName) END AS CostCenterName,		
				CASE WHEN a.IsContractor = 1 THEN NULL ELSE b.SupervisorNo END AS SupervisorNo,
				CASE WHEN a.IsContractor = 1 THEN NULL ELSE RTRIM(b.SupervisorName) END AS SupervisorName,
				CASE WHEN a.IsContractor = 1 THEN NULL ELSE b.ManagerNo END AS ManagerNo,
				CASE WHEN a.IsContractor = 1 THEN NULL ELSE RTRIM(b.ManagerName) END AS ManagerName,
				a.EmpPhoto,
				a.Base64Photo,
				a.ImageFileName,
				a.ImageFileExt				
		FROM tas.IDCardRegistry a WITH (NOLOCK)
			LEFT JOIN tas.Vw_Employee b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			OUTER APPLY
            (
				SELECT x.FirstName, x.LastName, x.IDNumber, RTRIM(z.UDCDesc1) AS JobTitle, x.CompanyName, 
					x.VisitedCostCenter AS CostCenter, RTRIM(y.BUname) AS CostCenterName 
				FROM tas.ContractorRegistry x WITH (NOLOCK) 
					LEFT JOIN tas.Master_BusinessUnit_JDE_view y WITH (NOLOCK) ON RTRIM(x.VisitedCostCenter) = RTRIM(y.BU)
					LEFT JOIN tas.sy_UserDefinedCode z WITH (NOLOCK) ON RTRIM(x.JobTitle) = RTRIM(z.UDCCode)
				WHERE x.ContractorNo = a.EmpNo
			) c
			LEFT JOIN tas.sy_UserDefinedCode d WITH (NOLOCK) ON RTRIM(a.BloodGroup) = RTRIM(d.UDCCode)
		WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.RegistryID
    END

	ELSE IF @actionType = 1		--Insert record
	BEGIN

		INSERT INTO [tas].[IDCardRegistry]
		(
			EmpNo,
			EmpName,
			Position,
			CustomCostCenter,
			CPRNo,
			BloodGroup,
			IsContractor,
			EmpPhoto,
			Base64Photo,
			ImageFileName,
			ImageFileExt,			
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUser
		)
		VALUES
		(
			@empNo,
			@empName,
			@position,
			@customCostCenter,
			@cprNo,
			@bloodGroup,
			@isContractor,
			@empPhoto,
			@base64Photo,
			@imageFileName,
			@imageFileExt,			
			@userActionDate,
			@userEmpNo,
			@userID
		)
		
		--Get the new ID
		SET @registryID = @@identity				
					
		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@hasError = 1,
					@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE()
		END

		ELSE
        BEGIN

			IF @isContractor = 1		--Rev. #1.1
			BEGIN
            
				--Automate updating the related data in UNIS system
				EXEC	@return_value = tas.Pr_Automate_UNISAccess
						@actionType = 1,
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
		SELECT	@registryID AS NewIdentityID,
				@rowsAffected AS RowsAffected,
				@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription
	END 

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		UPDATE tas.IDCardRegistry
		SET EmpName = @empName,
			Position = @position,
			CustomCostCenter = @customCostCenter,
			CPRNo = @cprNo,
			BloodGroup = @bloodGroup,
			EmpPhoto = CASE WHEN ISNULL(@excludePhoto, 0) = 0 THEN @empPhoto ELSE EmpPhoto END,
			Base64Photo = CASE WHEN ISNULL(@excludePhoto, 0) = 0 THEN @base64Photo ELSE Base64Photo END,
			ImageFileName = CASE WHEN ISNULL(@excludePhoto, 0) = 0 THEN @imageFileName ELSE ImageFileName END,
			ImageFileExt = CASE WHEN ISNULL(@excludePhoto, 0) = 0 THEN @imageFileExt ELSE ImageFileExt END,			
			LastUpdatedDate = @userActionDate,
			LastUpdatedByEmpNo = @userEmpNo,
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
		ELSE
        BEGIN
		
			IF @isContractor = 1		--Rev. #1.1
			BEGIN

				--Automate updating the related data in UNIS system
				EXEC	@return_value = tas.Pr_Automate_UNISAccess
						@actionType = 2,
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
		DELETE FROM tas.IDCardRegistry 
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

			IF @isContractor IS NULL
			BEGIN

				--Check if record exist in the contractor registry table
				IF EXISTS 
				(
					SELECT 1 FROM tas.ContractorRegistry a WITH (NOLOCK)
					WHERE a.ContractorNo = @empNo
				)
				BEGIN 
				
					SET @isContractor = 1
				END
				ELSE BEGIN
					
					--Check if record exists in Unis database
					IF EXISTS
                    (
						SELECT 1 FROM tas.UNIS_tUser a WITH (NOLOCK) 
						WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) ELSE 0 END = @empNo
					)
					SET @isContractor = 1
                END 
            END 

			IF @isContractor = 1		--Rev. #1.1
			BEGIN
			
				--Automate updating the related data in UNIS system
				EXEC	@return_value = tas.Pr_Automate_UNISAccess
						@actionType = 3,
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
	
	SELECT * FROM tas.IDCardRegistry a

	TRUNCATE TABLE tas.IDCardRegistry

PARAMETERS:
	@actionType			TINYINT,	--(Notes: 0 = Check records, 1 = Insert, 2 = Update, 3 = Delete)	
	@registryID			INT OUTPUT, 
	@empNo				INT = NULL,
	@empName			VARCHAR(100) NULL,
	@position			VARCHAR(50) NULL,
	@customCostCenter		VARCHAR(100) NULL,
	@cprNo				VARCHAR(30) = NULL,
	@bloodGroup			VARCHAR(10) = NULL,
	@isContractor		BIT = NULL,
	@empPhoto			VARBINARY(MAX)= NULL,
	@base64Photo		VARCHAR(MAX)= NULL,
	@imageFileName		VARCHAR(100) = NULL,
	@imageFileExt		VARCHAR(10) = NULL,
	@userActionDate		DATETIME = NULL,
	@userEmpNo			INT = NULL,
	@userID				VARCHAR(50) = NULL,
	@excludePhoto		BIT = NULL	

	EXEC tas.Pr_IDCardRegistry_CRUD 0, 0, 70001
	EXEC tas.Pr_IDCardRegistry_CRUD 0, 0, 10003632
	EXEC tas.Pr_IDCardRegistry_CRUD 3, 0, 70001				--Delete contractor

*/