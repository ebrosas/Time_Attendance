DECLARE	@isCommitTran			BIT,
		@actionType				TINYINT,
		@shiftPatCode			VARCHAR(2),
		@shiftCode				VARCHAR(10),
		@normalArrivalFrom		TIME(7),
		@normalArrivalTo		TIME(7),
		@ramadanArrivalFrom		TIME(7),
		@ramadanArrivalTo		TIME(7),
		@isActive				BIT,
		@userEmpNo				INT,
		@userID					VARCHAR(50),
		@userEmpName			VARCHAR(100),
		@arrivalFrom			DATETIME,
		@arrivalTo				DATETIME,
		@departFrom				DATETIME,
		@departTo				DATETIME,
		@rArrivalFrom			DATETIME,
		@rArrivalTo				DATETIME,
		@rDepartFrom			DATETIME,
		@rDepartTo				DATETIME		

	SELECT	@isCommitTran			= 0,
			@actionType				= 0,	--Note: 1 = Insert; 2 = Update; 3 = Delete
			@shiftPatCode			= 'DR',
			@shiftCode				= 'D',
			@normalArrivalFrom		= '07:00:00',
			@normalArrivalTo		= '08:00:00',
			@ramadanArrivalFrom		= '09:00:00',
			@ramadanArrivalTo		= '10:00:00',
			@isActive				= 1,
			@userEmpNo				= 10003632,
			@userID					= 'ervin',
			@userEmpName			= 'ERVIN OLINAS BROSAS',
			@arrivalFrom			= CONVERT(DATETIME, '2000-01-01 06:00:00.000'),
			@arrivalTo				= CONVERT(DATETIME, '2000-01-01 07:30:00.000'),
			@departFrom				= CONVERT(DATETIME, '2000-01-01 16:00:00.000'),
			@departTo				= CONVERT(DATETIME, '2000-01-01 16:30:00.000'),
			@rArrivalFrom			= CONVERT(DATETIME, '2000-01-01 06:00:00.000'),
			@rArrivalTo				= CONVERT(DATETIME, '2000-01-01 07:30:00.000'),
			@rDepartFrom			= CONVERT(DATETIME, '2000-01-01 13:30:00.000'),
			@rDepartTo				= CONVERT(DATETIME, '2000-01-01 14:00:00.000')		

	IF @actionType = 1		--Insert new record
	BEGIN    		

		IF NOT EXISTS
		(
			SELECT SettingID FROM tas.FlexiTimeSetting
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode
		)
		BEGIN

			--Start a transaction
			BEGIN TRAN T1

			INSERT INTO [tas].[FlexiTimeSetting]
			(
				[ShiftPatCode]
				,[ShiftCode]
				,[NormalArrivalFrom]
				,[NormalArrivalTo]
				,[RamadanArrivalFrom]
				,[RamadanArrivalTo]
				,[IsActive]
				,[CreatedDate]
				,[CreatedByEmpNo]
				,[CreatedByUser]
				,[CreatedByEmpName]
				,[ArrivalFrom_Old]
				,[ArrivalTo_Old]
				,[DepartFrom_Old]
				,[DepartTo_Old]
				,[RArrivalFrom_Old]
				,[RArrivalTo_Old]
				,[RDepartFrom_Old]
				,[RDepartTo_Old]
			)
			SELECT	@shiftPatCode, 
					@shiftCode, 
					@normalArrivalFrom,
					@normalArrivalTo,
					@ramadanArrivalFrom,
					@ramadanArrivalTo,
					@isActive,
					GETDATE(), 
					@userEmpNo, 
					@userID, 
					@userEmpName,
					@arrivalFrom,
					@arrivalTo,
					@departFrom,
					@departTo,
					@rArrivalFrom,
					@rArrivalTo,
					@rDepartFrom,
					@rDepartTo

			--View the inserted records
			SELECT * FROM tas.FlexiTimeSetting 
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode

			IF @isCommitTran = 1
				COMMIT TRAN T1
			ELSE
				ROLLBACK TRAN T1
		END
	END 

	ELSE IF @actionType = 2		--Update existing record
	BEGIN
    
		IF EXISTS
		(
			SELECT SettingID FROM tas.FlexiTimeSetting
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode
		)
		BEGIN

			--Start a transaction
			BEGIN TRAN T1

			UPDATE tas.FlexiTimeSetting
			SET NormalArrivalFrom = @normalArrivalFrom, 
				NormalArrivalTo = @normalArrivalTo, 
				RamadanArrivalFrom = @ramadanArrivalFrom, 
				RamadanArrivalTo = @ramadanArrivalTo, 
				IsActive = @isActive, 
				LastUpdateTime = GETDATE(),
				LastUpdateEmpNo = @userEmpNo,
				LastUpdateUser = @userID,
				LastUpdateEmpName = @userEmpName
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode

			--View the inserted records
			SELECT * FROM tas.FlexiTimeSetting 
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode

			IF @isCommitTran = 1
				COMMIT TRAN T1
			ELSE
				ROLLBACK TRAN T1
		END
	END 

	ELSE IF @actionType = 3		--Delete existing record
	BEGIN
    
		IF EXISTS
		(
			SELECT SettingID FROM tas.FlexiTimeSetting
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode
		)
		BEGIN

			--Start a transaction
			BEGIN TRAN T1

			DELETE FROM tas.FlexiTimeSetting
			WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
				AND RTRIM(ShiftCode) = @shiftCode

			IF @isCommitTran = 1
				COMMIT TRAN T1
			ELSE
				ROLLBACK TRAN T1
		END
	END 

	--View all records
	SELECT * FROM tas.FlexiTimeSetting ORDER BY ShiftPatCode, ShiftCode


/*	Debugging:

	SELECT * FROM tas.FlexiTimeSetting
	WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
		AND RTRIM(ShiftCode) = @shiftCode

*/

/*	Data update:

	BEGIN TRAN T1

	--Deactive Flexi-time for all Shift Pattern Codes
	UPDATE tas.FlexiTimeSetting
	SET IsActive = 1


	COMMIT TRAN T1

*/



