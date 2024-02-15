/**************************************************************************************************************************
	Notes:	This data script is used to update the Shift Patten information to enable flexitime
**************************************************************************************************************************/

DECLARE	@isCommitTrans	BIT,
		@shiftPatCode	VARCHAR(2),
		@shiftCode		VARCHAR(10),
		@arrivalFrom	DATETIME,
		@arrivalTo		DATETIME,
		@departFrom		DATETIME,
		@departTo		DATETIME,
		@rArrivalFrom	DATETIME,
		@rArrivalTo		DATETIME,
		@rDepartFrom	DATETIME,
		@rDepartTo		DATETIME

	/* Enable Flexi-time
	SELECT	@isCommitTrans	= 0,
			@shiftPatCode	= 'G',
			@shiftCode		= 'D',
			@arrivalFrom	= '2000-01-01 05:00:00.000',
			@arrivalTo		= '2000-01-01 07:00:00.000',
			@departFrom		= '2000-01-01 15:30:00.000',
			@departTo		= '2000-01-01 15:31:00.000',
			@rArrivalFrom	= '2000-01-01 05:00:00.000',
			@rArrivalTo		= '2000-01-01 07:00:00.000',
			@rDepartFrom	= '2000-01-01 13:00:00.000',
			@rDepartTo		= '2000-01-01 13:01:00.000'
	*/

	--/* Disable Flexi-time
	SELECT	@isCommitTrans	= 0,
			@shiftPatCode	= 'D',
			@shiftCode		= 'D',
			@arrivalFrom	= '2000-01-01 06:00:00.000',
			@arrivalTo		= '2000-01-01 07:30:00.000',
			@departFrom		= '2000-01-01 16:00:00.000',
			@departTo		= '2000-01-01 16:30:00.000',
			@rArrivalFrom	= '2000-01-01 06:00:00.000',
			@rArrivalTo		= '2000-01-01 07:30:00.000',
			@rDepartFrom	= '2000-01-01 13:30:00.000',
			@rDepartTo		= '2000-01-01 14:00:00.000'

	--*/

	--Start a transaction
	BEGIN TRAN T1

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
	WHERE RTRIM(ShiftPatCode) = @shiftPatCode
		AND RTRIM(ShiftCode) = @shiftCode

	--Get the updated record
	SELECT * FROM tas.Master_ShiftTimes
	WHERE RTRIM(ShiftPatCode) = @shiftPatCode
		AND RTRIM(ShiftCode) = @shiftCode

	IF @isCommitTrans = 1
		COMMIT TRAN T1
	ELSE
		ROLLBACK TRAN T1


/*	Debugging:

	--Get all affected Shift Pattern where timing is from 07:00 AM to 04:00 PM
	SELECT * FROM tas.Master_ShiftTimes a
	WHERE RTRIM(a.ShiftCode) = 'D'
		AND RTRIM(a.ShiftPatCode) IN ('D', 'D6', 'G', 'X')
	ORDER BY a.ShiftPatCode

*/

