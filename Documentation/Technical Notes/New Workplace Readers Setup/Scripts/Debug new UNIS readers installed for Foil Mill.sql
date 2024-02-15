	EXEC tas.Pr_GetEmployeeSwipeInfo '08/15/2021', '08/15/2021', 10003589, '', '', ''

	SELECT * FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)
	WHERE a.EmpNo = 10003589
		AND a.SwipeDate = '08/15/2021'

	SELECT * FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)
	WHERE a.EmpNo = 10003589
		AND a.SwipeDate = '08/15/2021'

	--(Note: Data from the Access System database might not be return due to the TOP 5000 query condition applied to "tas.External_DSX_evnlog" view
	SELECT * FROM tas.Tran_SwipeData_dsx1 a WITH (NOLOCK)
	WHERE a.EmpNo = 10003589
		AND CONVERT(DATETIME, CONVERT(VARCHAR(10), a.DT, 12)) = '08/15/2021'

	EXEC tas.Pr_GetMainGateSwipe 10003589, '08/15/2021'

	