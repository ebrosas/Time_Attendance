/**************************************************************************************************************************
	Notes:	This data script is used to activate or deactivate flexi time for all day shift workers
**************************************************************************************************************************/

DECLARE	@activationType		TINYINT,
		@isCommitTrans		BIT,
		@arrivalFrom		DATETIME,
		@arrivalTo			DATETIME,
		@departFrom			DATETIME,
		@departTo			DATETIME,
		@rArrivalFrom		DATETIME,
		@rArrivalTo			DATETIME,
		@rDepartFrom		DATETIME,
		@rDepartTo			DATETIME

SELECT	@activationType		= 0,	--(Note: 0 = View Current Setup; 1 = Disable Flexitime; 2 = Enable Flexitime)
		@isCommitTrans		= 0


	IF @activationType = 1			--Disable Flexitime
	BEGIN
    
		SELECT	@arrivalFrom	= '2000-01-01 06:00:00.000',
				@arrivalTo		= '2000-01-01 07:30:00.000',
				@departFrom		= '2000-01-01 16:00:00.000',
				@departTo		= '2000-01-01 16:30:00.000',
				@rArrivalFrom	= '2000-01-01 06:00:00.000',
				@rArrivalTo		= '2000-01-01 07:30:00.000',
				@rDepartFrom	= '2000-01-01 13:30:00.000',
				@rDepartTo		= '2000-01-01 14:00:00.000'

		--Start a transaction
		BEGIN TRAN T1

		--Disable flexi-time flag
		UPDATE tas.FlexiTimeSetting
		SET IsActive = 0

		UPDATE tas.Master_ShiftTimes
		SET ArrivalFrom = @arrivalFrom,
			ArrivalTo = @arrivalTo,
			DepartFrom = @departFrom,
			DepartTo = @departTo,
			RArrivalFrom = @rArrivalFrom,
			RArrivalTo = @rArrivalTo,
			RDepartFrom = @rDepartFrom,
			RDepartTo = @rDepartTo,
			LastUpdateUser = 'System Admin',
			LastUpdateTime = GETDATE()
		WHERE RTRIM(ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			AND RTRIM(ShiftCode) = 'D'

		--Get the updated record
		SELECT * FROM tas.FlexiTimeSetting

		SELECT * FROM tas.Master_ShiftTimes
		WHERE RTRIM(ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			AND RTRIM(ShiftCode) = 'D'

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 

	ELSE IF @activationType = 2		--Enable Flexitime
	BEGIN
    
		SELECT	@arrivalFrom	= '2000-01-01 05:00:00.000',
				@arrivalTo		= '2000-01-01 07:00:00.000',
				@departFrom		= '2000-01-01 15:30:00.000',
				@departTo		= '2000-01-01 15:31:00.000',
				@rArrivalFrom	= '2000-01-01 05:00:00.000',
				@rArrivalTo		= '2000-01-01 07:00:00.000',
				@rDepartFrom	= '2000-01-01 13:00:00.000',
				@rDepartTo		= '2000-01-01 13:01:00.000'

		--Start a transaction
		BEGIN TRAN T1

		--Activate flexi-time for all Day Shift Workers
		UPDATE tas.FlexiTimeSetting
		SET IsActive = 1

		UPDATE tas.Master_ShiftTimes
		SET ArrivalFrom = @arrivalFrom,
			ArrivalTo = @arrivalTo,
			DepartFrom = @departFrom,
			DepartTo = @departTo,
			RArrivalFrom = @rArrivalFrom,
			RArrivalTo = @rArrivalTo,
			RDepartFrom = @rDepartFrom,
			RDepartTo = @rDepartTo,
			LastUpdateUser = 'System Admin',
			LastUpdateTime = GETDATE()
		WHERE RTRIM(ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			AND RTRIM(ShiftCode) = 'D'

		--Get the updated record
		SELECT * FROM tas.FlexiTimeSetting

		SELECT * FROM tas.Master_ShiftTimes
		WHERE RTRIM(ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			AND RTRIM(ShiftCode) = 'D'

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 

	ELSE
    BEGIN

		--Show current setup
		SELECT * FROM tas.FlexiTimeSetting

		SELECT * FROM tas.Master_ShiftTimes
		WHERE RTRIM(ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			AND RTRIM(ShiftCode) = 'D'
    END 
	
	




