DECLARE	@actionType			TINYINT,
		@isCommitTrans		BIT,
		@companyID			SMALLINT,
		@costCenter			VARCHAR(12),
		@userEmpNo			INT,
		@userID				VARCHAR(50)

SELECT	@actionType			= 1,	--(Note: 0 = View record; 1 = Insert record; 2 = Update record; 3 = Delete record)
		@isCommitTrans		= 0,
		@companyID			= 71,
		@costCenter			= '7910',
		@userEmpNo			= 10003632,
		@userID				= 'ervin'

	IF @actionType = 0			--Check existing records
	BEGIN

		--Check all records
		SELECT * FROM tas.AccessSystemCostCenterMapping a
		ORDER BY a.CostCenter
	END
    
	ELSE IF @actionType = 1		--Insert new record
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.AccessSystemCostCenterMapping
		(
			[CompanyID]
			,[CostCenter]
			,[CreatedDate]
			,[CreatedByEmpNo]
			,[CreatedByUser]
		)
		VALUES
		(
			@companyID,
			@costCenter,
			GETDATE(),
			@userEmpNo,
			@userID
		)

		--Check inserted record
		SELECT * FROM tas.AccessSystemCostCenterMapping a
		WHERE a.CompanyID = @companyID

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1

		--Check all records
		SELECT * FROM tas.AccessSystemCostCenterMapping a
		ORDER BY a.CostCenter
	END
    
	ELSE IF @actionType = 2		--Update existing record
	BEGIN
    
		BEGIN TRAN T1

		UPDATE tas.AccessSystemCostCenterMapping
		SET CostCenter = @costCenter,
			LastUpdateTime = GETDATE(),
			LastUpdateEmpNo = @userEmpNo,
			LastUpdateUser = @userID
		WHERE CompanyID = @companyID

		--Check updated record
		SELECT * FROM tas.AccessSystemCostCenterMapping a
		WHERE a.CompanyID = @companyID

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END

