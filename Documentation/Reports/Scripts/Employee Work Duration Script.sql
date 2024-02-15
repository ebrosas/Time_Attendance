DECLARE	@starDate	DATETIME,
		@endDate	DATETIME
		
SELECT	@starDate	= '12/15/2015',
		@endDate	= '03/15/2016'
			
	--Employees and Contractors 	
	SELECT	A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName, 
			ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) AS TotalHoursWorked 
	FROM
	(					
		--Employees	
		SELECT a.BusinessUnit, c.BusinessUnitName, a.EmpNo, b.EmpName, a.GradeCode, a.Duration_Worked_Cumulative, a.Duration_Required,
			'Employee' AS EmpType
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
		WHERE a.DT BETWEEN @starDate AND @endDate
			AND a.IsLastRow = 1
			AND a.Duration_Worked_Cumulative > 0
			AND ISNULL(a.LeaveType, '') NOT IN ('SLP', 'SLU', 'IL')		
			
		UNION
		
		--Contractors
		SELECT ISNULL(a.BusinessUnit, '') AS BusinessUnit, ISNULL(c.BusinessUnitName, '') AS BusinessUnitName, 
			a.EmpNo, ISNULL(b.ContractorEmpName, d.EmpName) AS EmpName, a.GradeCode, a.Duration_Worked_Cumulative, a.Duration_Required, 
			'Contractor' AS EmpType
		FROM tas.Tran_Timesheet a
			LEFT JOIN tas.Master_ContractEmployee b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Master_Employee_JDE_View d ON a.EmpNo = d.EmpNo
		WHERE a.DT BETWEEN @starDate AND @endDate
			AND a.IsLastRow = 1
			AND a.Duration_Worked_Cumulative > 0	
	) A
	GROUP BY A.EmpType, A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName
	HAVING ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) > 0
	ORDER BY A.BusinessUnit, A.EmpNo

	--Employees and Contractors with Leave Exception 	
	SELECT	A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName, 
			ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) AS TotalHoursWorked 
	FROM
	(			
		--Employees 
		SELECT a.BusinessUnit, c.BusinessUnitName, a.EmpNo, b.EmpName, a.GradeCode, 
			CASE WHEN (ISNULL(a.Duration_Worked_Cumulative, 0) = 0 AND (ISNULL(a.LeaveType, '') <> '' OR ISNULL(a.AbsenceReasonCode, '') <> '')) OR ISNULL(d.ShiftPatCode, '') = ''
				THEN a.Duration_Required
				ELSE a.Duration_Worked_Cumulative
			END AS Duration_Worked_Cumulative,
			a.Duration_Required, 
			b.PayStatus
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Master_EmployeeAdditional d ON a.EmpNo = d.EmpNo
		WHERE a.DT BETWEEN @starDate AND @endDate
			AND a.IsLastRow = 1

		UNION
		
		--Contractors
		SELECT ISNULL(a.BusinessUnit, '') AS BusinessUnit, ISNULL(c.BusinessUnitName, '') AS BusinessUnitName, 
			a.EmpNo, ISNULL(b.ContractorEmpName, d.EmpName) AS EmpName, a.GradeCode, 
			ISNULL(a.Duration_Worked_Cumulative, a.Duration_Required) AS Duration_Worked_Cumulative, 
			a.Duration_Required, 
			'Contractor' AS EmpType
		FROM tas.Tran_Timesheet a
			LEFT JOIN tas.Master_ContractEmployee b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Master_Employee_JDE_View d ON a.EmpNo = d.EmpNo
		WHERE a.DT BETWEEN @starDate AND @endDate
			AND a.IsLastRow = 1
	) A
	GROUP BY A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName
	ORDER BY A.BusinessUnit, A.EmpNo

	