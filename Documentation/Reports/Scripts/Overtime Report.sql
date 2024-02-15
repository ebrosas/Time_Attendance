	SELECT	
			--a.DT,
			a.EmpNo,
			b.EmpName,
			b.Position,
			b.GradeCode,
			a.BusinessUnit AS CostCenter,
			c.BUname AS CostCenterName,
			ROUND(CONVERT(FLOAT, SUM(DATEDIFF(n, a.OTStartTime, a.OTEndTime))) / 60, 2) AS TotalOTHrs
			--ROUND(CONVERT(FLOAT, a.NetMinutes) / 60, 2) AS TotalHoursWorked,
			--ROUND(CONVERT(FLOAT, DATEDIFF(n, a.OTStartTime, a.OTEndTime)) / 60, 2) AS OTHours
			--a.ShiftPatCode,
			--a.ShiftCode,
			--a.Actual_ShiftCode,
			--a.dtIN,
			--a.dtOUT,
			--a.OTStartTime,
			--a.OTEndTime
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
	WHERE 
		a.DT BETWEEN '07/01/2016' AND '07/31/2016'
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNUMERIC(b.PayStatus) = 1
		AND (a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
	GROUP BY 
		--a.DT,
		a.EmpNo,
		b.EmpName,
		b.Position,
		b.GradeCode,
		a.BusinessUnit, 
		c.BUname 
	ORDER BY a.BusinessUnit, a.EmpNo