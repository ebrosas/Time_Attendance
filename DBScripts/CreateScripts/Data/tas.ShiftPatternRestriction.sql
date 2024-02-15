DECLARE	@actionType					TINYINT,	--(Note: 0 => View all records; 1 => Insert new record)
		@isCommitTrans				BIT,
		@ShiftPatCode				VARCHAR(2),
		@RestrictionType			TINYINT,	--(Note: 0 => No restriction; 1 => Access restricted to specific employee number; 2 => Access restricted to specific cost center) 
		@RestrictedEmpNoArray		VARCHAR(200),
		@RestrictedCostCenterArray	VARCHAR(200),
		@ErrorMessage				VARCHAR(200),
		@CreatedDate				DATETIME,		
		@CreatedByEmpNo				INT,
		@CreatedByUserID			VARCHAR(50) 

SELECT	@actionType					= 1,
		@isCommitTrans				= 1,
		@ShiftPatCode				= 'DR',
		@RestrictionType			= 3,
		@RestrictedEmpNoArray		= '',
		@RestrictedCostCenterArray	= '7500',
		@ErrorMessage				= 'Sorry, access to ''DR'' shift pattern code is restricted to HR employees only!',
		@CreatedDate				= GETDATE(),		
		@CreatedByEmpNo				= 10003632,
		@CreatedByUserID			= 'ervin'

	IF @actionType = 0			--Get all records
	BEGIN
		
		SELECT * FROM tas.ShiftPatternRestriction a
		ORDER BY a.ShiftPatCode
    END 

	ELSE IF @actionType = 1		--Insert new record
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.ShiftPatternRestriction
		(
			ShiftPatCode,
			RestrictionType,
			RestrictedEmpNoArray,
			RestrictedCostCenterArray,
			ErrorMessage,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID
		)
		VALUES
		(
			@ShiftPatCode,
			@RestrictionType,
			@RestrictedEmpNoArray,
			@RestrictedCostCenterArray,
			@ErrorMessage,
			@CreatedDate,		
			@CreatedByEmpNo,
			@CreatedByUserID
		)

		SELECT * FROM tas.ShiftPatternRestriction a
		ORDER BY a.ShiftPatCode

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 

/*	Debugging:

	SELECT * FROM tas.ShiftPatternRestriction a

	BEGIN TRAN T1

	UPDATE tas.ShiftPatternRestriction
	SET RestrictionType = 2
	WHERE SettingID = 1

	UPDATE tas.ShiftPatternRestriction
	SET RestrictedEmpNoArray = ''
	WHERE SettingID = 1

	UPDATE tas.ShiftPatternRestriction
	SET RestrictedCostCenterArray = '7500,7550'
	WHERE SettingID = 1

	TRUNCATE TABLE tas.ShiftPatternRestriction

	COMMIT TRAN T1
	ROLLBACK TRAN T1

*/



