DECLARE	@isCommitTrans		BIT,
		@shiftPatCode		VARCHAR(2),
		@shiftCode			VARCHAR(10),
		@arrivalFrom		DATETIME,
		@arrivalTo			DATETIME,
		@departFrom			DATETIME,
		@departTo			DATETIME,
		@rArrivalFrom		DATETIME,
		@rArrivalTo			DATETIME,
		@rDepartFrom		DATETIME,
		@rDepartTo			DATETIME

	SELECT	@isCommitTrans	= 0,
			@shiftPatCode	= 'X',
			@shiftCode		= 'D',
			@arrivalFrom	= '2000-01-01 06:00:00.000',
			@arrivalTo		= '2000-01-01 07:30:00.000',
			@departFrom		= '2000-01-01 16:00:00.000',
			@departTo		= '2000-01-01 16:30:00.000',
			@rArrivalFrom	= '2000-01-01 06:00:00.000',
			@rArrivalTo		= '2000-01-01 07:30:00.000',
			@rDepartFrom	= '2000-01-01 13:30:00.000',
			@rDepartTo		= '2000-01-01 14:00:00.000'

	--Start a transaction
	BEGIN TRAN T1

	UPDATE tas.FlexiTimeSetting
	SET	ArrivalFrom_Old = @arrivalFrom,
		ArrivalTo_Old = @arrivalTo,
		DepartFrom_Old = @departFrom,
		DepartTo_Old = @departTo,
		RArrivalFrom_Old = @rArrivalFrom,
		RArrivalTo_Old = @rArrivalTo,
		RDepartFrom_Old = @rDepartFrom,
		RDepartTo_Old = @rDepartTo
	WHERE RTRIM(ShiftPatCode) = @shiftPatCode
		AND RTRIM(ShiftCode) = @shiftCode

	--Check affected record
	SELECT * FROM tas.FlexiTimeSetting
	WHERE RTRIM(ShiftPatCode) = @shiftPatCode
		AND RTRIM(ShiftCode) = @shiftCode

	IF @isCommitTrans = 1
		COMMIT TRAN T1
	ELSE
		ROLLBACK TRAN T1


