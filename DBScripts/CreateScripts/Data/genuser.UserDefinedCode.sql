DECLARE	@actionType					TINYINT = 0,	--(Note: 0 = Check existing records; 1 = Insert new record)
		@UDCGCode					VARCHAR(10) = 'CONTJOBTLE',
		@UDCUDCGID					INT,
        @UDCCode					VARCHAR(10),
        @UDCDesc1					VARCHAR(50),
        @UDCDesc2					VARCHAR(50),
        @UDCSpecialHandlingCode		VARCHAR(50),
        @UDCDate					DATETIME,
        @UDCAmount					DECIMAL(18,0),
        @UDCField					VARCHAR(10),
		@isCommitTran				BIT = 1

	IF @actionType = 0
	BEGIN
    
		SELECT * FROM genuser.UserDefinedCode a
		WHERE UDCUDCGID = (SELECT UDCGID FROM genuser.UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = @UDCGCode)
		ORDER BY a.UDCID
	END 

	ELSE IF @actionType = 1
	BEGIN 
    
		SELECT	@UDCUDCGID = UDCGID FROM genuser.UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = @UDCGCode

		--Populate License Types
		/*
		SELECT	@UDCCode = 'LICOHC1',
				@UDCDesc1 = 'O.H.C. (Pendant)',
				@UDCDesc2 = 'Overhead Crane - Pendant',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 1,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICOHC2',
				@UDCDesc1 = 'O.H.C. (Cabin)',
				@UDCDesc2 = 'Overhead Crane - Cabin',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 2,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICHL',
				@UDCDesc1 = 'H.L.',
				@UDCDesc2 = 'High Lift',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 3,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICPC',
				@UDCDesc1 = 'P.C.',
				@UDCDesc2 = 'Personal Car',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 4,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICTL',
				@UDCDesc1 = 'T.L.',
				@UDCDesc2 = 'Top Loader',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 5,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICBC',
				@UDCDesc1 = 'B.C.',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 6,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICJCB',
				@UDCDesc1 = 'JCB',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 7,
				@UDCField = NULL

		SELECT	@UDCCode = 'LICCC',
				@UDCDesc1 = 'C.C.',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 8,
				@UDCField = NULL

		SELECT	@UDCCode = 'LIFLH',
				@UDCDesc1 = 'F.L. (Heavy)',
				@UDCDesc2 = 'Fork Lift - Heavy',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 9,
				@UDCField = NULL

		SELECT	@UDCCode = 'LIFLS',
				@UDCDesc1 = 'F.L. (Small)',
				@UDCDesc2 = 'Fork Lift - Small',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 10,
				@UDCField = NULL
		*/

		--Populate Blood Groups
		/*
		SELECT	@UDCCode = 'BGAPLUS',
				@UDCDesc1 = 'A+',
				@UDCDesc2 = 'A RhD positive (A+)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 1,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGAMINUS',
				@UDCDesc1 = 'A-',
				@UDCDesc2 = 'A RhD negative (A-)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 2,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGBPLUS',
				@UDCDesc1 = 'B+',
				@UDCDesc2 = 'B RhD positive (B+)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 3,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGBMINUS',
				@UDCDesc1 = 'B-',
				@UDCDesc2 = 'B RhD negative (B-)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 4,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGOPLUS',
				@UDCDesc1 = 'O+',
				@UDCDesc2 = 'O RhD positive (O+)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 5,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGOMINUS',
				@UDCDesc1 = 'O-',
				@UDCDesc2 = 'O RhD negative (O-)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 6,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGABPLUS',
				@UDCDesc1 = 'AB+',
				@UDCDesc2 = 'AB RhD positive (AB+)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 7,
				@UDCField = NULL

		SELECT	@UDCCode = 'BGABMINUS',
				@UDCDesc1 = 'AB-',
				@UDCDesc2 = 'AB RhD negative (AB-)',
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 8,
				@UDCField = NULL
		*/

		--Populate Contractor Job Titles
		/*
		SELECT	@UDCCode = 'JTCARPENTR',
				@UDCDesc1 = 'Carpenter',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 1,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTELECTRCN',
				@UDCDesc1 = 'Electrician',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 2,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTFABRCTOR',
				@UDCDesc1 = 'Fabricator',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 3,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTFITTER',
				@UDCDesc1 = 'Fitter',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 4,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTFLDRIVER',
				@UDCDesc1 = 'Forklift Driver ',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 5,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTHELPER',
				@UDCDesc1 = 'Helper',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 6,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTLABOR',
				@UDCDesc1 = 'Labor',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 7,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTMASON',
				@UDCDesc1 = 'Mason',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 8,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTPACKER',
				@UDCDesc1 = 'Packer',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 9,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTPAINTER',
				@UDCDesc1 = 'Painter',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 10,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTPLUMBER',
				@UDCDesc1 = 'Plumber',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 11,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTSUPERVSR',
				@UDCDesc1 = 'Supervisor',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 12,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTWELDER',
				@UDCDesc1 = 'Welder',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 13,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTMANAGER',
				@UDCDesc1 = 'Manager',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 14,
				@UDCField = NULL

		SELECT	@UDCCode = 'JTTECHCN',
				@UDCDesc1 = 'Technician',
				@UDCDesc2 = NULL,
				@UDCSpecialHandlingCode = NULL,
				@UDCDate = NULL,
				@UDCAmount = 15,
				@UDCField = NULL
		*/

		IF NOT EXISTS 
		(
			SELECT UDCID FROM genuser.UserDefinedCode 
			WHERE UDCUDCGID = @UDCUDCGID AND UPPER(RTRIM(UDCCode)) = @UDCCode
		)
		BEGIN

			BEGIN TRAN T1

		
			INSERT INTO genuser.UserDefinedCode
			(
				[UDCUDCGID]
				,[UDCCode]
				,[UDCDesc1]
				,[UDCDesc2]
				,[UDCSpecialHandlingCode]
				,[UDCDate]
				,[UDCAmount]
				,[UDCField]
			)
			SELECT	@UDCUDCGID, 
					@UDCCode, 
					@UDCDesc1,
					@UDCDesc2,
					@UDCSpecialHandlingCode, 
					@UDCDate, 
					@UDCAmount, 
					@UDCField

			SELECT * FROM genuser.UserDefinedCode WHERE UDCUDCGID = @UDCUDCGID ORDER BY UDCID

			IF @isCommitTran = 1
				COMMIT TRAN T1
			ELSE
				ROLLBACK TRAN T1
		END
	END 

/*	Debugging:

	SELECT * FROM genuser.UserDefinedCode 
	WHERE UDCUDCGID = (SELECT UDCGID FROM genuser.UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'LICENSETYP')

	BEGIN TRAN T1	

	DELETE FROM genuser.UserDefinedCode where UDCID IN (595)

	DELETE FROM genuser.UserDefinedCode 
	WHERE UDCUDCGID = (SELECT UDCGID FROM genuser.UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'LICENSETYP')

	COMMIT TRAN T1

*/
