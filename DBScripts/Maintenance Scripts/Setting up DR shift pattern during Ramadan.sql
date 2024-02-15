	SELECT * FROM tas.Master_ShiftPatternTitles WHERE RTRIM(ShiftPatCode) = 'DR'
	SELECT * FROM tas.Master_ShiftPattern WHERE RTRIM(ShiftPatCode) = 'DR'
	SELECT * FROM tas.Master_ShiftTimes WHERE RTRIM(ShiftPatCode) = 'DR'

	SELECT * FROM tas.FlexiTimeSetting a

/*
	
	BEGIN TRAN T1

	UPDATE tas.FlexiTimeSetting
	SET RamadanArrivalFrom = '08:00:00.0000000'
	WHERE RTRIM(ShiftPatCode) = 'DR'

	UPDATE tas.Master_ShiftTimes
	SET RArrivalFrom = '2000-01-01 07:00:00.000',
		RArrivalTo = '2000-01-01 08:00:00.000',
		RDepartFrom = '2000-01-01 14:00:00.000',
		RDepartTo = '2000-01-01 14:01:00.000'
	WHERE RTRIM(ShiftPatCode) = 'DR'
		AND RTRIM(ShiftCode) = 'D'

	COMMIT TRAN T1

*/