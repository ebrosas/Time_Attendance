DECLARE	@starDate	DATETIME,
		@endDate	DATETIME
		
SELECT	@starDate	= '12/15/2015',
		@endDate	= '03/15/2016'
	
	--Employees			
	SELECT	DISTINCT
			A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName, 
			ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) AS TotalHoursWorked 
	FROM
	(					
		SELECT a.BusinessUnit, c.BusinessUnitName, a.EmpNo, b.EmpName, a.GradeCode, a.Duration_Worked_Cumulative, a.Duration_Required
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
		WHERE a.DT BETWEEN @starDate AND @endDate
			AND a.IsLastRow = 1
			AND a.Duration_Worked_Cumulative > 0
			AND ISNULL(a.LeaveType, '') NOT IN ('SLP', 'SLU', 'IL')
	) A
	GROUP BY A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName
	ORDER BY A.BusinessUnit, A.EmpNo

	--Employees with leave exception			
	SELECT	DISTINCT 
			A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName, 
			ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) AS TotalHoursWorked ,
			ROUND(CONVERT(FLOAT, SUM(A.Duration_Required)) / 60, 0) AS TotalHoursRequired
	FROM
	(					
		SELECT a.BusinessUnit, c.BusinessUnitName, a.EmpNo, b.EmpName, a.GradeCode, 
			CASE WHEN (ISNULL(a.Duration_Worked_Cumulative, 0) = 0 AND (ISNULL(a.LeaveType, '') <> '' OR ISNULL(a.AbsenceReasonCode, '') <> '')) OR ISNULL(d.ShiftPatCode, '') = ''
				THEN a.Duration_Required
				ELSE a.Duration_Worked_Cumulative
			END AS Duration_Worked_Cumulative,
			a.Duration_Required, 
			b.PayStatus,
			a.*
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Master_EmployeeAdditional d ON a.EmpNo = d.EmpNo
		WHERE a.DT BETWEEN @starDate AND @endDate
			AND a.IsLastRow = 1
			AND ISNULL(a.LeaveType, '') NOT IN ('SLP', 'SLU', 'IL')
	) A
	GROUP BY A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName
	ORDER BY A.BusinessUnit, A.EmpNo

	