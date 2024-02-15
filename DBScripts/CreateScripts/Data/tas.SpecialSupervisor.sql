DECLARE	@actionType			TINYINT,	--(Note: 0 => Check record; 1 => Insert new record; 2 => Update existing record; 3 => Delete record)
		@isCommitTrans		BIT,
		@empNo				INT,
		@isEnabled			BIT,
		@createdByEmpNo		INT,
		@createdByUserID	VARCHAR(50)

SELECT	@actionType			= 0,
		@isCommitTrans		= 0,
		@empNo				= 0,
		@isEnabled			= 1,
		@createdByEmpNo		= 10003632,
		@createdByUserID	= 'ervin'

	IF @actionType = 0		--Fetch existing record
	BEGIN

		IF ISNULL(@empNo, 0) = 0
			SET @empNo = NULL

		SELECT * FROM tas.SpecialSupervisor a
		WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		ORDER BY a.SettingID DESC
    END 

	ELSE IF @actionType = 1		--Insert new record
	BEGIN
    
		--Start transaction
		BEGIN TRAN T1

		INSERT INTO tas.SpecialSupervisor
		(
			EmpNo,
			IsEnabled,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID
		)
		SELECT	@empNo,
				@isEnabled,
				GETDATE(),
				@createdByEmpNo,
				@createdByUserID

		SELECT * FROM tas.SpecialSupervisor a
		ORDER BY a.SettingID DESC

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 

	ELSE IF @actionType = 3		--Delete existing record
	BEGIN

		DELETE FROM tas.SpecialSupervisor 
		WHERE EmpNo = @empNo 
    END 


