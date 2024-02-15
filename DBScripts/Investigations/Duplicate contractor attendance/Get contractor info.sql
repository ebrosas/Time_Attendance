	SELECT	DISTINCT 
		CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END AS EmpNo,
		RTRIM(e.FName) + ' ' + RTRIM(e.LName) AS EmpName,
		b.CostCenter,
		ISNULL(c.BUname, e.Department) AS CostCenterName,
		RTRIM(e.CPR) AS CPRNo,
		e.JobTitle,
		RTRIM(e.ContractCompany) AS EmployerName,
		
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= e.DateStop AND e.DateStop IS NOT NULL
			THEN 0
			ELSE 1
		END AS StatusID,
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= e.DateStop AND e.DateStop IS NOT NULL
			THEN 'Inactive'
			ELSE 'Active'
		END AS StatusDesc,
		NULL AS ContractorTypeID,
		NULL AS ContractorTypeDesc,
		e.DateStart AS IDStartDate,
		e.DateStop AS IDEndDate,
		e.DateStart AS ContractStartDate,
		e.DateStop AS ContractEndDate,
		

		--ISNULL(e.IssueDate, d.StartDate) AS IDStartDate,
		--ISNULL(e.ExpiryDate, d.StopDate) AS IDEndDate,
		--ISNULL(e.IssueDate, d.StartDate) AS ContractStartDate,
		--ISNULL(e.ExpiryDate, d.StopDate) AS ContractEndDate,		

		--CONVERT(FLOAT, ISNULL(a.Nhrs, 0)) * 60 AS RequiredWorkDuration,
		--ISNULL(e.PrintDate, a.PrintDate) AS CreatedDate,
		e.PrintDate AS CreatedDate,
		--RTRIM(e.IssuedBy) AS CreatedByName,	
		NULL AS 	CreatedByName,	
		NULL AS CreatedByNo	
	FROM  --tas.sy_PrintedCards a WITH (NOLOCK)
		--(
		--	SELECT TOP 1 * FROM tas.sy_PrintedCards WITH (NOLOCK)
		--	WHERE CASE WHEN ISNUMERIC(EmpNo) = 1 THEN CONVERT(INT, EmpNo) ELSE 0 END = 51301
		--	ORDER BY PrintDate DESC
		--) a
		(
			SELECT DISTINCT EmpNo--, FName, LName, DateStart, DateStop, NULL AS CostCode, Department, ContractCompany, CPR, JobTitle, NULL as PrintDate
			FROM tas.sy_PrintedCards WITH (NOLOCK)
		) a
		CROSS APPLY	
		(
			SELECT TOP 1 FName, LName, CostCode, PrintDate, DateStart, DateStop, Department, ContractCompany, CPR, JobTitle 
			FROM tas.sy_PrintedCards WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
			ORDER BY PrintDate DESC
		) e
		LEFT JOIN tas.AccessSystemCostCenterMapping b WITH (NOLOCK) ON CONVERT(SMALLINT, e.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON TRIM(b.CostCenter) = TRIM(c.BU)	--Rev #1.4
		--LEFT JOIN tas.sy_CardDates d WITH (NOLOCK) ON (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END) = CAST(d.FName AS INT)		--Rev. #1.1
		--INNER JOIN tas.sy_Vw_LIC_PrintedCards e WITH (NOLOCK) ON (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END) = (CASE WHEN ISNUMERIC(e.EmpNo) = 1 THEN e.EmpNo ELSE 0 END) 	
		--	AND  ('06/16/2021' > e.ExpiryDate AND '07/15/2021' < e.ExpiryDate)
	WHERE CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END = 61020     