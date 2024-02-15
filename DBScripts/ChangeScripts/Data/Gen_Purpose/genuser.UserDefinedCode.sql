DECLARE	@UDCGCode VARCHAR(10),
		@UDCUDCGID int,
        @UDCCode varchar(10),
        @UDCDesc1 varchar(500),
        @UDCDesc2 varchar(500),
        @UDCSpecialHandlingCode varchar(50),
        @UDCDate datetime,
        @UDCAmount decimal(18,0),
        @UDCField varchar(10),
		@isCommitTran bit


--/*	Populate Search Criteria items used in "Timesheet Integrity by Correction Code" form

	SET @UDCGCode = 'TSINTEGRTY'
	SELECT	@UDCUDCGID = UDCGID FROM genuser.UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = @UDCGCode

	SELECT	@UDCCode = 'TSOPT1',
			@UDCDesc1 = 'Add OT, but there is no OT',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 1,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT2',
			@UDCDesc1 = 'Add NoPayHour, but there is no NoPayHour',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 2,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT3',
			@UDCDesc1 = 'Add Shift Allowance, but there is no allowance',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 3,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT4',
			@UDCDesc1 = 'Mark Absent, but not absent',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 4,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT5',
			@UDCDesc1 = 'Mark DIL, but there is no DIL',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 5,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT6',
			@UDCDesc1 = 'Remove OT, but still there is OT',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 6,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT7',
			@UDCDesc1 = 'Remove NoPayHour, but still there is NoPayHour',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 7,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT8',
			@UDCDesc1 = 'Remove Shift Allowances, but it is not removed',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 8,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT9',
			@UDCDesc1 = 'Remove Absence, but still there is Absence',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 9,
			@UDCField = NULL,
			@isCommitTran = 1

	SELECT	@UDCCode = 'TSOPT10',
			@UDCDesc1 = 'Remove DIL, but still there is DIL',
			@UDCDesc2 = NULL,
			@UDCSpecialHandlingCode = NULL,
			@UDCDate = NULL,
			@UDCAmount = 10,
			@UDCField = NULL,
			@isCommitTran = 1
--*/

	IF NOT EXISTS 
	(
		SELECT UDCID FROM genuser.UserDefinedCode 
		WHERE UDCUDCGID = @UDCUDCGID 
			AND UPPER(RTRIM(UDCCode)) = @UDCCode
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

	ELSE
	BEGIN
		
		SELECT * FROM genuser.UserDefinedCode 
		WHERE UDCUDCGID = @UDCUDCGID 
			AND UPPER(RTRIM(UDCCode)) = @UDCCode
	END


/*	Debugging:

	SELECT * FROM genuser.UserDefinedCode 
	WHERE UDCUDCGID = (SELECT UDCGID FROM genuser.UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'TSINTEGRTY')
	ORDER BY UDCAmount
	

	BEGIN TRAN T1	

	UPDATE genuser.UserDefinedCode 
	SET UDCDesc1 = 'Mark Absent, but not absent'
	WHERE UDCID = 3470

	DELETE FROM genuser.UserDefinedCode 
	WHERE UDCUDCGID = (SELECT UDCGID FROM genuser.UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'TSINTEGRTY')

	DELETE FROM genuser.UserDefinedCode where UDCUDCGID = 54
	DELETE FROM genuser.UserDefinedCode where UDCID = 3426

	ROLLBACK TRAN T1
	COMMIT TRAN T1

*/
