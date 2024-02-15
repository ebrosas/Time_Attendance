DECLARE	@UDCGCode varchar(10),
        @UDCGDesc1 varchar(50),
        @UDCGDesc2 varchar(50),
		@isCommitTran bit

	--Add License Types
	/*
	SELECT	@UDCGCode = 'LICENSETYP',
			@UDCGDesc1 = 'License Types',
			@UDCGDesc2 = 'Types of licenses that will be used in printing the ID card',
			@isCommitTran = 1
	*/

	--Add Blood Group
	/*
	SELECT	@UDCGCode = 'BLOODGROUP',
			@UDCGDesc1 = 'Blood Groups',
			@UDCGDesc2 = '',
			@isCommitTran = 1
	*/

	--Add Contractor Job Titles
	/*
	SELECT	@UDCGCode = 'CONTJOBTLE',
			@UDCGDesc1 = 'Contractor Job Titles',
			@UDCGDesc2 = '',
			@isCommitTran = 1
	*/

	IF NOT EXISTS 
	(
		SELECT UDCGID FROM genuser.UserDefinedCodeGroup 
		WHERE RTRIM(UDCGCode) = @UDCGCode
	)
	BEGIN

		BEGIN TRAN T1

		INSERT INTO genuser.UserDefinedCodeGroup
        (
			UDCGCode,
			UDCGDesc1,
			UDCGDesc2
		)
		SELECT	@UDCGCode, 
				@UDCGDesc1,
				@UDCGDesc2

		SELECT * FROM genuser.UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = @UDCGCode

		IF @isCommitTran = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END


/*	Debugging:

	SELECT * FROM genuser.UserDefinedCodeGroup ORDER BY UDCGID
	SELECT * FROM genuser.UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'LEAVETYPE'

	BEGIN TRAN T1	
	DELETE FROM genuser.UserDefinedCodeGroup where UDCID = 93
	COMMIT TRAN T1

*/



