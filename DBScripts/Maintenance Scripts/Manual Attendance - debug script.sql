	SELECT * FROM tas.Tran_ManualAttendance a
	WHERE a.EmpNo < 10000000
	ORDER BY a.dtIN DESC, a.dtOUT DESC, a.EmpNo
		
	SELECT * FROM tas.Master_ContractEmployee a
	WHERE a.EmpNo = 1008080

	SELECT * FROM tas.sy_NAMES a
	WHERE CONVERT(INT, a.FName) = 3632

	SELECT * FROM tas.sy_NAMES a
	WHERE RTRIM(a.FName) LIKE '53599' + '%'

	SELECT * FROM tas.sy_COMPANY a
	WHERE a.Company = 50

	SELECT * FROM tas.sy_COMPANY a
	ORDER BY a.Company

	SELECT * FROM [tas].[Master_BusinessUnit_JDE_view] a
	ORDER BY a.BUname

	SELECT JobTitle, * FROM tas.Master_JobTitles_JDE 

	SELECT TOP 100 * FROM tas.Vw_MainGateSwipeRawData a
	WHERE a.EmpNo < 10000000
	ORDER BY a.SwipeDate DESC	

	SELECT * FROM tas.Tran_ManualAttendance a
	WHERE (a.dtIN = '08/31/2016' OR a.dtOUT = '08/31/2016')
	ORDER BY a.EmpNo

	EXEC tas.prGetEmployeeDetailsForManualAttendance 10002149
	EXEC tas.prGetEmployeeDetailsForManualAttendance 53599

	SELECT * FROM tas.Tran_ManualAttendance a
	WHERE a.EmpNo = 10003619
	ORDER BY a.AutoID DESC	

	SELECT * FROM tas.Tran_ManualAttendance a
	WHERE a.EmpNo < 10000000
	ORDER BY a.AutoID DESC	

	--Employee info
	SELECT * FROM tas.Tran_ManualAttendance a
	WHERE a.EmpNo = 10003632

	SELECT * FROM tas.Vw_MainGateSwipeRawData a
	WHERE a.EmpNo = 10003632
		AND a.SwipeDate = '08/30/2016'

	EXEC tas.prGetEmployeeDetailsForManualAttendance 10002149

	--Manual attendance history
	EXEC tas.prGetEmployeeManualAttendanceHistory 10003619
	EXEC tas.Pr_GetManualTimesheetEntry 0, 10003620