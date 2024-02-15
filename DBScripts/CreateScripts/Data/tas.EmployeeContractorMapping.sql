DECLARE	@actionType			TINYINT,
		@isCommitTrans		BIT,
		@empNo				INT,
		@contractorNo		INT,
		@costCenter			VARCHAR(12),
		@primaryIDNoType	TINYINT,
		@userEmpNo			INT,
		@userID				VARCHAR(50)

SELECT	@actionType			= 0,	--(Note: 0 = View record; 1 = Insert record; 2 = Update record; 3 = Delete record)
		@isCommitTrans		= 0,
		@empNo				= 10002148,
		@contractorNo		= 56836,
		@costCenter			= '7600',
		@primaryIDNoType	= 1,	--(Note: 0 = Use Employee No; 1 = Use Contractor No.)
		@userEmpNo			= 10003632,
		@userID				= 'ervin'

	IF @actionType = 0			--Check existing records
	BEGIN

		--Check all records
		SELECT * FROM tas.EmployeeContractorMapping a
		ORDER BY a.EmpNo
	END
    
	ELSE IF @actionType = 1		--Insert new record
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.EmployeeContractorMapping
		(
			[EmpNo]
			,[ContractorNo]
			,[CostCenter]
			,[PrimaryIDNoType]
			,[CreatedDate]
			,[CreatedByEmpNo]
			,[CreatedByUser]
		)
		VALUES
		(
			@empNo,
			@contractorNo,
			@costCenter,
			@primaryIDNoType,
			GETDATE(),
			@userEmpNo,
			@userID
		)

		--Check inserted record
		SELECT * FROM tas.EmployeeContractorMapping a
		WHERE a.EmpNo = @empNo
			AND a.ContractorNo = @contractorNo

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1

		--Check all records
		SELECT * FROM tas.EmployeeContractorMapping a
		ORDER BY a.EmpNo
	END
    
	ELSE IF @actionType = 2		--Update existing record
	BEGIN
    
		BEGIN TRAN T1

		UPDATE tas.EmployeeContractorMapping
		SET PrimaryIDNoType = @primaryIDNoType,
			LastUpdateTime = GETDATE(),
			LastUpdateEmpNo = @userEmpNo,
			LastUpdateUser = @userID
		WHERE EmpNo = @empNo
			AND ContractorNo = @contractorNo

		--Check updated record
		SELECT * FROM tas.EmployeeContractorMapping a
		WHERE a.EmpNo = @empNo
			AND a.ContractorNo = @contractorNo

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END

