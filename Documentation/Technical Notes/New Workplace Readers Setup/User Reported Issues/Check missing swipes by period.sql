	SELECT a.NoPayHours, a.BusinessUnit, a.* 
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX 
		INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND ISNUMERIC(c.PayStatus) = 1 AND c.DateResigned IS NULL
	WHERE a.IsLastRow = 1
		AND RTRIM(a.BusinessUnit) IN ('3310', '3320', '3322', '5300')
		AND (a.dtIN IS NULL AND a.dtOUT IS NULL)
		AND RTRIM(b.Effective_ShiftCode) <> 'O'
		AND ISNULL(a.LeaveType, '') = ''
		AND ISNULL(a.AbsenceReasonCode, '') = ''
		AND ISNULL(a.AbsenceReasonColumn, '') = ''
		AND ISNULL(a.RemarkCode, '') = ''
		--AND ISNULL(a.CorrectionCode, '') = ''
		AND a.DT BETWEEN '11/28/2020' AND '12/03/2020'