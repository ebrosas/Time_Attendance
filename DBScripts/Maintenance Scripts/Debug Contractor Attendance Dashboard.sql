	SELECT * FROM tas.Master_EmployeeAdditional a
	WHERE a.EmpNo = 55763

	SELECT TOP 10 * FROM tas.Tran_ShiftPatternUpdates a
	WHERE a.EmpNo = 55763
	ORDER BY a.DateX DESC 

	SELECT TOP 10 * FROM tas.Tran_Timesheet a
	WHERE a.EmpNo = 55763
		AND a.IsLastRow = 1
	ORDER BY a.DT DESC

	SELECT empno, DT, LocationCode, ReaderNo, EventCode, Source 
	FROM tas.Tran_SwipeData_dsx1 a
	WHERE a.EmpNo = 55763
		AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) = '04/17/2016'

	SELECT * FROM grmacc.acslog.dbo.evnlog a
	WHERE a.FName LIKE '%3632%'


	SELECT * FROM grmacc.acslog.dbo.evnlog a
	WHERE UPPER(RTRIM(a.LName)) LIKE '%SREEJITH%'

	SELECT * FROM grmacc.acslog.dbo.evnlog a
	WHERE UPPER(RTRIM(a.LName)) LIKE '%SHIVA NAIK%'

	SELECT * FROM tas.Master_ContractEmployee a
	WHERE a.EmpNo = 55763

	SELECT * FROM tas.Vw_MainGateSwipeRawData a
	WHERE a.EmpNo = 55763
		AND a.SwipeDate = '03/02/2016'