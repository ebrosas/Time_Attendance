/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Automate_UNISAccess
*	Description: This stored procedure is used to populate the data to Unis table for registering new ID card
*
*	Date			Author		Revision No.	Comments:
*	12/12/2021		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_Automate_UNISAccess
(	
	@actionType			TINYINT,	--(Notes: 0 = Check records, 1 = Insert, 2 = Update, 3 = Delete, 4 = Synchronize card history)	
	@empNo				INT,
	@rowsAffected		INT OUTPUT,
	@hasError			BIT OUTPUT,
	@retError			INT OUTPUT,
	@retErrorDesc		VARCHAR(200) OUTPUT
)
AS	
BEGIN

	--Define constants
	DECLARE @CONST_RETURN_OK	INT = 0,
			@CONST_RETURN_ERROR	INT = -1

	--Define variables
	SELECT  @rowsAffected		= 0,
			@hasError			= 0,
			@retError			= @CONST_RETURN_OK,
			@retErrorDesc		= ''

	DECLARE @maxLID				INT = 0,
			@LID				INT = 0,
			@isContractor		BIT = 0,
			@empName			VARCHAR(64) = '',
			@contractStartDate	DATETIME = NULL,	
			@contractEndDate	DATETIME = NULL,
			@purposeOfVisit		VARCHAR(255) = NULL,
			@idNumber			VARCHAR(255) = NULL,
			@mobileNo			VARCHAR(255) = NULL,
			@cOffice			VARCHAR(30) = NULL,
			@cPost				VARCHAR(30) = NULL

	IF @actionType IN (1, 2)		
	BEGIN

		--Determine if contractor
		SELECT @isContractor = a.IsContractor
		FROM tas.IDCardRegistry a WITH (NOLOCK) 
		WHERE a.EmpNo = @empNo
    END 

	IF @actionType = 0		--Check existing records
	BEGIN

		SELECT	a.L_ID,
				CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,
				RTRIM(a.C_Name) AS EmpName,
				a.C_AccessGroup,
				a.C_RegDate,
				a.C_DateLimit,
				a.L_Type,
				a.L_OptDateLimit,
				a.L_Identify,
				a.L_VerifyLevel,
				a.L_Blacklist,
				a.L_AuthValue  
		FROM tas.UNIS_tUser a WITH (NOLOCK) 
		WHERE  CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) ELSE 0 END = @empNo
    END

	ELSE IF @actionType = 1		--Insert record
	BEGIN

		BEGIN TRY
        
			--Get the maximum LID incremented by 1
			SELECT @maxLID = MAX(a.L_ID) + 1 FROM tas.UNIS_tUser a WITH (NOLOCK)

			IF @isContractor = 1
			BEGIN
        
				--Get contractor information
				SELECT	@empName = RTRIM(b.FirstName) + ' ' + RTRIM(b.LastName),
						@contractStartDate = b.ContractStartDate,
						@contractEndDate = b.ContractEndDate,
						@purposeOfVisit = RTRIM(e.UDCDesc1), --RTRIM(b.PurposeOfVisit),
						@idNumber = RTRIM(b.IDNumber),
						@mobileNo = RTRIM(b.MobileNo),
						@cOffice = RTRIM(c.OfficeCode),
						@cPost = RTRIM(d.PostCode)
				FROM tas.IDCardRegistry a WITH (NOLOCK) 
					INNER JOIN tas.ContractorRegistry b WITH (NOLOCK) ON a.EmpNo = b.ContractorNo
					LEFT JOIN tas.UnisOfficeCostCenterMapping c WITH (NOLOCK) ON RTRIM(b.VisitedCostCenter) = RTRIM(c.CostCenter)
					LEFT JOIN tas.UnisPostCostCenterMapping d WITH (NOLOCK) ON RTRIM(b.VisitedCostCenter) = RTRIM(d.CostCenter)
					LEFT JOIN tas.sy_UserDefinedCode e WITH (NOLOCK) ON RTRIM(b.JobTitle) = RTRIM(e.UDCCode)
				WHERE a.EmpNo = @empNo
			END 

			ELSE
			BEGIN

				--Get employee information
				SELECT	@empName = LTRIM(RTRIM(b.YAALPH)),
						@contractStartDate = CASE WHEN ISNULL(c.T3EFT, 0) = 0 
												THEN tas.ConvertFromJulian(ISNULL(b.YADST, 0)) 
												ELSE tas.ConvertFromJulian(c.T3EFT) 
											END,
						@contractEndDate = CASE WHEN ISNULL(c.T3EFT, 0) = 0 
												THEN tas.ConvertFromJulian(ISNULL(b.YADST, 0)) 
												ELSE tas.ConvertFromJulian(c.T3EFT) 
											END,
						@purposeOfVisit = NULL,
						@idNumber = a.CPRNo,
						@mobileNo = CASE WHEN ISNULL(d.WPPH1, '') <> '' THEN '+973' + LTRIM(RTRIM(d.WPPH1)) ELSE '' END,
						@cOffice = NULL,
						@cPost = NULL
				FROM tas.IDCardRegistry a WITH (NOLOCK) 
					INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT)
					LEFT JOIN tas.syJDE_F00092 c WITH (NOLOCK) ON b.YAAN8 = c.T3SBN1 AND LTRIM(RTRIM(c.T3TYDT)) = 'WH' AND LTRIM(RTRIM(c.T3SDB)) = 'E'
					LEFT JOIN tas.syJDE_F0115 d WITH (NOLOCK) ON b.YAAN8 = d.WPAN8 AND LTRIM(RTRIM(d.WPPHTP)) = 'DL' 		
			END 

			--Insert employee details in "tUser" table
			INSERT INTO tas.UNIS_tUser
			(
				L_ID,
				C_Unique,
				C_Name,
				C_AccessGroup,
				C_RegDate,
				C_DateLimit,
				L_Type,
				L_OptDateLimit,
				L_Identify,
				L_VerifyLevel,
				L_Blacklist,
				L_AuthValue 
			)
			SELECT	@maxLID AS L_ID,
					@empNo AS C_Unique,
					@empName AS C_Name,
					'0001' AS C_AccessGroup,
					REPLACE(REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR, GETDATE(), 120), '-', ''), ':', ''), '.', ''), ' ', '') AS C_RegDate,
					REPLACE(CONVERT(VARCHAR, @contractStartDate, 111), '/', '') + REPLACE(CONVERT(VARCHAR, @contractEndDate, 111), '/', '') AS C_DateLimit,
					1 AS L_Type,
					1 AS L_OptDateLimit,
					1 AS L_Identify,
					0 AS L_VerifyLevel,
					0 AS L_Blacklist,
					72 AS L_AuthValue 
		
			--Get no. of affected rows
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

				IF @isContractor = 1
				BEGIN
            
					--Insert visit information to "tVisitor" table
					INSERT INTO tas.UNIS_tVisitor
					(
						L_UID,
						C_Office,
						C_Post,
						C_Goal,
						C_Info,
						C_Phone
					)
					SELECT	@maxLID AS L_UID,
							@cOffice AS C_Office,
							@cPost AS C_Post,
							@purposeOfVisit AS C_Goal,
							@idNumber AS C_Info,
							@mobileNo AS C_Phone
				END 

				--Insert card number history to "iUserCard" table
				/*
				INSERT INTO tas.UNIS_iUserCard
				(
					C_CardNum,
					L_UID
				)
				SELECT	RTRIM(a.CardRefNo),
						@maxLID
				FROM tas.IDCardHistory a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
				*/

				--Insert employee photo in "iUserPicture" table
				INSERT INTO tas.UNIS_iUserPicture
				(
					L_UID,
					B_Picture
				)
				SELECT	@maxLID,
						CAST(a.EmpPhoto AS IMAGE)
				FROM tas.IDCardRegistry a WITH (NOLOCK) 
				WHERE a.EmpNo = @empNo
			END 

		END TRY
		BEGIN CATCH

			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1
		END CATCH
	END 

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		BEGIN TRY
        
			IF @isContractor = 1
			BEGIN
        
				--Get contractor information
				SELECT	@empName = RTRIM(b.FirstName) + ' ' + RTRIM(b.LastName),
						@contractStartDate = b.ContractStartDate,
						@contractEndDate = b.ContractEndDate,
						@purposeOfVisit = RTRIM(e.UDCDesc1), --RTRIM(b.PurposeOfVisit),
						@idNumber = RTRIM(b.IDNumber),
						@mobileNo = RTRIM(b.MobileNo),
						@cOffice = RTRIM(c.OfficeCode),
						@cPost = RTRIM(d.PostCode)
				FROM tas.IDCardRegistry a WITH (NOLOCK) 
					INNER JOIN tas.ContractorRegistry b WITH (NOLOCK) ON a.EmpNo = b.ContractorNo
					LEFT JOIN tas.UnisOfficeCostCenterMapping c WITH (NOLOCK) ON RTRIM(b.VisitedCostCenter) = RTRIM(c.CostCenter)
					LEFT JOIN tas.UnisPostCostCenterMapping d WITH (NOLOCK) ON RTRIM(b.VisitedCostCenter) = RTRIM(d.CostCenter)
					LEFT JOIN tas.sy_UserDefinedCode e WITH (NOLOCK) ON RTRIM(b.JobTitle) = RTRIM(e.UDCCode)
				WHERE a.EmpNo = @empNo
			END 

			ELSE
			BEGIN

				--Get employee information
				SELECT	@empName = LTRIM(RTRIM(b.YAALPH)),
						@contractStartDate = CASE WHEN ISNULL(c.T3EFT, 0) = 0 
												THEN tas.ConvertFromJulian(ISNULL(b.YADST, 0)) 
												ELSE tas.ConvertFromJulian(c.T3EFT) 
											END,
						@contractEndDate = CASE WHEN ISNULL(c.T3EFT, 0) = 0 
												THEN tas.ConvertFromJulian(ISNULL(b.YADST, 0)) 
												ELSE tas.ConvertFromJulian(c.T3EFT) 
											END,
						@purposeOfVisit = NULL,
						@idNumber = a.CPRNo,
						@mobileNo = CASE WHEN ISNULL(d.WPPH1, '') <> '' THEN '+973' + LTRIM(RTRIM(d.WPPH1)) ELSE '' END,
						@cOffice = NULL,
						@cPost = NULL
				FROM tas.IDCardRegistry a WITH (NOLOCK) 
					INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT)
					LEFT JOIN tas.syJDE_F00092 c WITH (NOLOCK) ON b.YAAN8 = c.T3SBN1 AND LTRIM(RTRIM(c.T3TYDT)) = 'WH' AND LTRIM(RTRIM(c.T3SDB)) = 'E'
					LEFT JOIN tas.syJDE_F0115 d WITH (NOLOCK) ON b.YAAN8 = d.WPAN8 AND LTRIM(RTRIM(d.WPPHTP)) = 'DL' 		
			END 

			--Get the LID
			SELECT @LID = a.L_ID
			FROM tas.UNIS_tUser a WITH (NOLOCK) 
			WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) ELSE 0 END = @empNo

			IF @LID > 0
			BEGIN
        
				--Update employee info 
				UPDATE tas.UNIS_tUser
				SET	C_Name = @empName,
					C_RegDate = REPLACE(REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR, GETDATE(), 120), '-', ''), ':', ''), '.', ''), ' ', ''),
					C_DateLimit = REPLACE(CONVERT(VARCHAR, @contractStartDate, 111), '/', '') + REPLACE(CONVERT(VARCHAR, @contractEndDate, 111), '/', '')
				WHERE L_ID = @LID

				--Get no. of affected rows
				SELECT @rowsAffected = @@rowcount 

				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@hasError = 1,
							@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
							@retErrorDesc = ERROR_MESSAGE()
				END

				ELSE
				BEGIN
            
					IF @isContractor = 1
					BEGIN
            
						--Update visit information info
						UPDATE tas.UNIS_tVisitor
						SET C_Office = @cOffice,
							C_Post = @cPost,
							C_Goal = @purposeOfVisit,
							C_Info = @idNumber,
							C_Phone = @mobileNo
						WHERE L_UID = @LID
					END 

					--Delete existing card history data
					/*
					IF EXISTS
					(
						SELECT 1 FROM tas.UNIS_iUserCard
						WHERE L_UID = @LID
					)
					BEGIN

						DELETE FROM tas.UNIS_iUserCard
						WHERE L_UID = @LID
					END 

					--Insert card number history data
					INSERT INTO tas.UNIS_iUserCard
					(
						C_CardNum,
						L_UID
					)
					SELECT	RTRIM(a.CardRefNo),
							@LID
					FROM tas.IDCardHistory a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
					*/

					--Update photo
					UPDATE tas.UNIS_iUserPicture
					SET tas.UNIS_iUserPicture.B_Picture = CAST(a.EmpPhoto AS IMAGE)
					FROM tas.IDCardRegistry a WITH (NOLOCK) 
					WHERE a.EmpNo = @empNo

					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@hasError = 1,
								@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
								@retErrorDesc = ERROR_MESSAGE()
					END
				END 
			END 

		END TRY
		BEGIN CATCH

			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1
		END CATCH
	END 

	ELSE IF @actionType = 3		--Delete record
	BEGIN

		BEGIN TRY
        
			--Get the LID
			SELECT @LID = a.L_ID
			FROM tas.UNIS_tUser a WITH (NOLOCK) 
			WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) ELSE 0 END = @empNo

			IF @LID > 0
			BEGIN

				--Delete related visitor info
				DELETE FROM tas.UNIS_tVisitor
				WHERE L_UID = @LID

				--Delete related card history info
				DELETE FROM tas.UNIS_iUserCard
				WHERE L_UID = @LID
				
				--Delete related photo info
				DELETE FROM tas.UNIS_iUserPicture
				WHERE L_UID = @LID

				--Delete employee details in the header table
				DELETE FROM tas.UNIS_tUser 
				WHERE L_ID = @LID

				--Get the number of affected records 
				SELECT @rowsAffected = @@rowcount 

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@hasError = 1,
							@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
							@retErrorDesc = ERROR_MESSAGE()
				END
			END
		
		END TRY
		BEGIN CATCH
		
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1
		END CATCH
    END	
	
	ELSE IF @actionType = 4		--Synchronize card history
	BEGIN

		BEGIN TRY

			--Get the LID
			SELECT @LID = a.L_ID
			FROM tas.UNIS_tUser a WITH (NOLOCK) 
			WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) ELSE 0 END = @empNo

			IF @LID > 0
			BEGIN

				--Delete existing card history data
				IF EXISTS
				(
					SELECT 1 FROM tas.UNIS_iUserCard
					WHERE L_UID = @LID
				)
				BEGIN

					DELETE FROM tas.UNIS_iUserCard
					WHERE L_UID = @LID
				END 

				--Insert card number history data
				INSERT INTO tas.UNIS_iUserCard
				(
					C_CardNum,
					L_UID
				)
				SELECT	RTRIM(a.CardRefNo),
						@LID
				FROM tas.IDCardHistory a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
			END 

		END TRY
		BEGIN CATCH
		
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1
		END CATCH
	END 	
END  


/*	Debug:
	
	SELECT * FROM tas.UNIS_tUser a

	TRUNCATE TABLE tas.UNIS_tUser

PARAMETERS:
	@actionType			TINYINT,	--(Notes: 0 = Check records, 1 = Insert, 2 = Update, 3 = Delete, 4 = Check for duplicate records)	
	@contractorNo		INT = NULL,
	@registrationDate	DATETIME = NULL,	
	@idNumber			VARCHAR(20) = NULL,
	@idType				TINYINT = NULL,
	@firstName			VARCHAR(30) = NULL,
	@lastName			VARCHAR(30) = NULL,
	@companyName		VARCHAR(50) = NULL,
	@companyID			INT NULL = NULL,
	@companyCRNo		VARCHAR(20) = NULL,
	@purchaseOrderNo	FLOAT = NULL,
	@jobTitle			VARCHAR(50) = NULL,
	@mobileNo			VARCHAR(20) = NULL,
	@visitedCostCenter	VARCHAR(12) = NULL,
	@supervisorEmpNo	INT = NULL,
	@supervisorEmpName	VARCHAR(100) = NULL,
	@purposeOfVisit		VARCHAR(300) = NULL,
	@contractStartDate	DATETIME = NULL,
	@contractEndDate	DATETIME = NULL,
	@bloodGroup			VARCHAR(10) = NULL,
	@remarks			VARCHAR(500) = NULL,
	@userActionDate		DATETIME = NULL,
	@userEmpNo			INT = NULL,
	@userID				VARCHAR(50) = NULL,
	@registryID			INT OUTPUT	 

	EXEC tas.Pr_Automate_UNISAccess 0
	EXEC tas.Pr_Automate_UNISAccess 1, 0, 60001, '09/07/2021', '781202647', 0, 'Ervin', 'Brosas', 'ABS-CBN', 1223, '110234', NULL, 'Software Engineer', '32229611', '7600', 10003512, 'Testing only', '09/01/2021', '09/30/2021', 'Sample data', '09/07/2021', 10003632, 'ervin' 

*/