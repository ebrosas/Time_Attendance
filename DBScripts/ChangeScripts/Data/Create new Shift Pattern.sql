/**************************************************************************************************************************
	Notes:	This data script is used to create new Shift Pattern Setup
**************************************************************************************************************************/

DECLARE	@rowsAffected			INT,
		@isCommitTrans			BIT,  
		@targetShiftPatCode		CHAR(2),
		@sourceShiftPatCode		CHAR(2),
		@shiftPatDescription	VARCHAR(50),
		@isDayShift				BIT,
		@lastUpdateUser			VARCHAR(50),
		@lastUpdateTime			DATETIME,
		@overrideTiming			BIT, 
		@arrivalFrom			DATETIME,
		@arrivalTo				DATETIME,
		@departFrom				DATETIME,
		@departTo				DATETIME,
		@rArrivalFrom			DATETIME,
		@rArrivalTo				DATETIME,
		@rDepartFrom			DATETIME,
		@rDepartTo				DATETIME,
		@shiftCodeToModify		VARCHAR(10)				

	/*	DP - Plant Day Shift Work Schedule
	SELECT	@rowsAffected			= 0,
			@isCommitTrans			= 1,
			@targetShiftPatCode		= 'DP',
			@sourceShiftPatCode		= 'D',
			@shiftPatDescription	= 'Plant Day Shift Work Schedule',
			@isDayShift				= 1,
			@lastUpdateUser			= 'GARMCO\ervin',
			@lastUpdateTime			= GETDATE(),
			@overrideTiming			= 1, 
			@arrivalFrom			= CONVERT(DATETIME, '2000-01-01 06:00:00.000'),
			@arrivalTo				= CONVERT(DATETIME, '2000-01-01 07:30:00.000'),
			@departFrom				= CONVERT(DATETIME, '2000-01-01 16:00:00.000'),
			@departTo				= CONVERT(DATETIME, '2000-01-01 16:30:00.000'),
			@rArrivalFrom			= CONVERT(DATETIME, '2000-01-01 06:00:00.000'),
			@rArrivalTo				= CONVERT(DATETIME, '2000-01-01 07:30:00.000'),
			@rDepartFrom			= CONVERT(DATETIME, '2000-01-01 13:30:00.000'),
			@rDepartTo				= CONVERT(DATETIME, '2000-01-01 14:00:00.000'),
			@shiftCodeToModify		= 'D'
	*/

	/*	DR - Special shift for Muslims during Ramadan
	SELECT	@rowsAffected			= 0,
			@isCommitTrans			= 1,
			@targetShiftPatCode		= 'DR',
			@sourceShiftPatCode		= 'D',
			@shiftPatDescription	= 'Special shift for Muslims during Ramadan',
			@isDayShift				= 1,
			@lastUpdateUser			= 'GARMCO\ervin',
			@lastUpdateTime			= GETDATE(),
			@overrideTiming			= 1, 
			@arrivalFrom			= CONVERT(DATETIME, '2000-01-01 05:00:00.000'),
			@arrivalTo				= CONVERT(DATETIME, '2000-01-01 07:00:00.000'),
			@departFrom				= CONVERT(DATETIME, '2000-01-01 15:30:00.000'),
			@departTo				= CONVERT(DATETIME, '2000-01-01 15:31:00.000'),
			@rArrivalFrom			= CONVERT(DATETIME, '2000-01-01 08:30:00.000'),
			@rArrivalTo				= CONVERT(DATETIME, '2000-01-01 09:00:00.000'),
			@rDepartFrom			= CONVERT(DATETIME, '2000-01-01 15:00:00.000'),
			@rDepartTo				= CONVERT(DATETIME, '2000-01-01 15:01:00.000'),
			@shiftCodeToModify		= 'D'
	*/

	--Start a transaction
	BEGIN TRAN T1

	IF NOT EXISTS
    (
		SELECT AutoID FROM tas.Master_ShiftPatternTitles
		WHERE RTRIM(ShiftPatCode) = @targetShiftPatCode
	)
	BEGIN
    
		INSERT INTO tas.Master_ShiftPatternTitles
		(
			ShiftPatCode,
			ShiftPatDescription,
			IsDayShift,
			LastUpdateUser,
			LastUpdateTime
		)
		SELECT	@targetShiftPatCode,
				@shiftPatDescription,
				@isDayShift,
				@lastUpdateUser,
				@lastUpdateTime

		--Get the number of log records processed
		SELECT @rowsAffected = @@ROWCOUNT

		IF @rowsAffected > 0
		BEGIN

			INSERT INTO tas.Master_ShiftPattern
			(
				ShiftPatCode,
				ShiftPointer,
				ShiftCode
			)
			SELECT	@targetShiftPatCode AS ShiftPatCode, 
					ShiftPointer, 
					ShiftCode
			FROM tas.Master_ShiftPattern
			WHERE RTRIM(ShiftPatCode) = @sourceShiftPatCode
			ORDER BY ShiftPointer

			--Get the number of log records processed
			SELECT @rowsAffected = @@ROWCOUNT

			IF @rowsAffected > 0
			BEGIN

				INSERT INTO tas.Master_ShiftTimes
				(
					[ShiftPatCode]
					,[ShiftCode]
					,[xxxxxx1]
					,[ArrivalFrom]
					,[ArrivalTo]
					,[DepartFrom]
					,[DepartTo]
					,[TotalHrs]
					,[xxxxxx2]
					,[RArrivalFrom]
					,[RArrivalTo]
					,[RDepartFrom]
					,[RDepartTo]
					,[RTotalHrs]
					,[xxxxxx3]
					,[LastUpdateUser]
					,[LastUpdateTime]
				)
				SELECT	@targetShiftPatCode AS ShiftPatCode, 
						ShiftCode, 
						xxxxxx1, 
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @arrivalFrom
							ELSE ArrivalFrom
						END AS ArrivalFrom, 
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @arrivalTo
							ELSE ArrivalTo
						END AS ArrivalTo,
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @departFrom
							ELSE DepartFrom
						END AS DepartFrom,
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @departTo
							ELSE DepartTo
						END AS DepartTo,
						TotalHrs, 
						xxxxxx2, 
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @rArrivalFrom
							ELSE RArrivalFrom
						END AS RArrivalFrom,
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @rArrivalTo
							ELSE RArrivalTo
						END AS RArrivalTo,
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @rDepartFrom
							ELSE RDepartFrom
						END AS RDepartFrom,
						CASE WHEN @overrideTiming = 1 AND RTRIM(ShiftCode) = @shiftCodeToModify
							THEN @rDepartTo
							ELSE RDepartTo
						END AS RDepartTo,
						RTotalHrs, 
						xxxxxx3,
						@lastUpdateUser,
						@lastUpdateTime
				FROM tas.Master_ShiftTimes
				WHERE RTRIM(ShiftPatCode) = @sourceShiftPatCode
			END 
		END
	END
    
	SELECT * FROM tas.Master_ShiftPatternTitles WHERE RTRIM(ShiftPatCode) = @targetShiftPatCode
	SELECT * FROM tas.Master_ShiftPattern WHERE RTRIM(ShiftPatCode) = @targetShiftPatCode
	SELECT * FROM tas.Master_ShiftTimes WHERE RTRIM(ShiftPatCode) = @targetShiftPatCode

	IF @isCommitTrans = 1
		COMMIT TRAN T1
	ELSE
		ROLLBACK TRAN T1 
	

/*	Debugging:

	SELECT * FROM tas.Master_ShiftPatternTitles WHERE RTRIM(ShiftPatCode) = 'DR'
	SELECT * FROM tas.Master_ShiftPattern WHERE RTRIM(ShiftPatCode) = 'DR'
	SELECT * FROM tas.Master_ShiftTimes WHERE RTRIM(ShiftPatCode) = 'DR'

	SELECT * FROM tas.Master_ShiftPatternTitles WHERE RTRIM(ShiftPatCode) = 'DP'
	SELECT * FROM tas.Master_ShiftPattern WHERE RTRIM(ShiftPatCode) = 'DP'
	SELECT * FROM tas.Master_ShiftTimes WHERE RTRIM(ShiftPatCode) = 'DP'

*/

/*	Data updates:

	BEGIN TRAN T1

	DELETE FROM tas.Master_ShiftTimes WHERE RTRIM(ShiftPatCode) = 'DR'
	DELETE FROM tas.Master_ShiftPattern WHERE RTRIM(ShiftPatCode) = 'DR'
	DELETE FROM tas.Master_ShiftPatternTitles WHERE RTRIM(ShiftPatCode) = 'DR'

	DELETE FROM tas.Master_ShiftTimes WHERE RTRIM(ShiftPatCode) = 'DP'
	DELETE FROM tas.Master_ShiftPattern WHERE RTRIM(ShiftPatCode) = 'DP'
	DELETE FROM tas.Master_ShiftPatternTitles WHERE RTRIM(ShiftPatCode) = 'DP'

	COMMIT TRAN T1

*/
