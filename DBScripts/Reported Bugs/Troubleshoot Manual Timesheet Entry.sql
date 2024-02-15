	SELECT a.IsLastRow, a.Duration_Worked, a.Duration_Worked_Cumulative, a.NetMinutes,
		a.Shaved_IN, a.Shaved_OUT,
	* FROM tas.Tran_Timesheet a
	WHERE a.EmpNo = 10003673
		AND a.DT = '11/28/2017'

/*
	BEGIN TRAN T1

	UPDATE tas.Tran_Timesheet 
	SET dtOUT = '2017-11-28 12:55:00.000',
		Shaved_OUT = '2017-11-28 12:55:00.000'
	WHERE EmpNo = 10003673
		AND DT = '11/28/2017'
		AND AutoID = 5238060

	UPDATE tas.Tran_Timesheet 
	SET Duration_Worked = DATEDIFF(MINUTE, dtIN, '2017-11-28 12:55:00.000'),
		NetMinutes = 35 + DATEDIFF(MINUTE, dtIN, '2017-11-28 12:55:00.000'),
		Duration_Worked_Cumulative = 35 + DATEDIFF(MINUTE, '2017-11-28 08:02:00.000', '2017-11-28 12:55:00.000')
	WHERE EmpNo = 10003673
		AND DT = '11/28/2017'
		AND AutoID = 5238060



	UPDATE tas.Tran_Timesheet 
	SET dtOUT = '2017-11-28 16:29:04.000',
		Shaved_OUT = '2017-11-28 16:29:04.000'
	WHERE EmpNo = 10003673
		AND DT = '11/28/2017'
		AND AutoID = 5239125

	UPDATE tas.Tran_Timesheet 
	SET Duration_Worked = DATEDIFF(MINUTE, dtIN, '2017-11-28 16:29:04.000'),
		NetMinutes = 328 + DATEDIFF(MINUTE, dtIN, '2017-11-28 16:29:04.000'),
		Duration_Worked_Cumulative = 35 + DATEDIFF(MINUTE, dtIN, '2017-11-28 16:29:04.000')
	WHERE EmpNo = 10003673
		AND DT = '11/28/2017'
		AND AutoID = 5239125

	COMMIT TRAN T1

*/