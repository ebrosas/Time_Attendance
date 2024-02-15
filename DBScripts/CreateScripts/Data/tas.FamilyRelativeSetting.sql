DECLARE	@actionTypeID			TINYINT = 0,
		@isCommitTrans			BIT = 0,
		@degreeLevel			TINYINT = 4,
		@relativeTypeCode		VARCHAR(15) = 'DEG4UNAUFATMOT',
		@relativeTypeName		VARCHAR(300) = 'Uncle/Aunt of the Father/Mother of the Employee/Spouse',
		@sequenceNo				TINYINT = 5,
		@userEmpNo				INT = 0,
		@userID					VARCHAR(50) = 'System Admin'

	IF @actionTypeID = 0			--Check existing records
	BEGIN

		SELECT * FROM tas.FamilyRelativeSetting a
		--WHERE RTRIM(a.RelativeTypeCode) = @relativeTypeCode
		--	AND a.DegreeLevel = @degreeLevel
		ORDER BY a.DegreeLevel, a.SequenceNo
	END 
	
	ELSE IF @actionTypeID = 1		--Insert new record
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.FamilyRelativeSetting
		(
			DegreeLevel,
			RelativeTypeCode,
			RelativeTypeName,
			SequenceNo,
			CreatedByEmpNo,
			CreatedByUserID
		)
		VALUES
		(
			@degreeLevel,
			@relativeTypeCode,
			@relativeTypeName,
			@sequenceNo,
			@userEmpNo,
			@userID
		)

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1

		SELECT * FROM tas.FamilyRelativeSetting a
		WHERE a.DegreeLevel = @degreeLevel
		ORDER BY a.SequenceNo
	END 

	ELSE IF @actionTypeID = 2		--Update existing record
	BEGIN

		UPDATE tas.FamilyRelativeSetting
		SET RelativeTypeName = @relativeTypeName,
			SequenceNo = @sequenceNo,
			LastUpdateEmpNo = @userEmpNo,
			LastUpdateUserID = @userID
		WHERE RTRIM(RelativeTypeCode) = @relativeTypeCode
			AND DegreeLevel = @degreeLevel
	END 

	ELSE IF @actionTypeID = 3		--Delete existing record
	BEGIN

		DELETE FROM tas.FamilyRelativeSetting
		WHERE RTRIM(RelativeTypeCode) = @relativeTypeCode
			AND DegreeLevel = @degreeLevel
	END 


