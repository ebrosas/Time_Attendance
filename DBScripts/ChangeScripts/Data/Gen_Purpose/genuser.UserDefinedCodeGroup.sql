DECLARE	@UDCGCode varchar(10),
        @UDCGDesc1 varchar(50),
        @UDCGDesc2 varchar(50),
		@isCommitTran bit

	/************** Add Search Criteria code to be used in "Timesheet Integrity by Correction Code" form ************************/
	SELECT	@UDCGCode = 'TSINTEGRTY',
			@UDCGDesc1 = 'Search Criteria Items',
			@UDCGDesc2 = 'Used in Timesheet Integrity by Correction Code form',
			@isCommitTran = 1
	/********************************************************* END *************************************************************/


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
	SELECT * FROM genuser.UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'TSINTEGRTY'

	BEGIN TRAN T1	
	DELETE FROM genuser.UserDefinedCodeGroup where UDCID = 93
	COMMIT TRAN T1

*/



