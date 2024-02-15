	--SELECT * FROM tas.System_Values a

	SELECT DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) AS OTDuration, a.CorrectionCode, a.GradeCode, a.ShiftPatCode, a.ShiftCode, a.IsSalStaff,
	* FROM tas.Tran_Timesheet a
	WHERE a.EmpNo = 10003353
		AND a.DT BETWEEN '04/16/2021' AND '05/30/2021'
		AND ISNULL(a.CorrectionCode, '') <> ''
	ORDER BY a.DT DESC