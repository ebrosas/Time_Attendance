	SELECT a.* 
	FROM tas.sy_PrintedCards a WITH (NOLOCK)
		LEFT JOIN tas.AccessSystemCostCenterMapping b WITH (NOLOCK) ON CONVERT(SMALLINT, a.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON TRIM(b.CostCenter) = TRIM(c.BU)	--Rev #1.4
		LEFT JOIN tas.sy_CardDates d WITH (NOLOCK) ON (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END) = CAST(d.FName AS INT)		--Rev. #1.1
		--INNER JOIN tas.sy_Vw_LIC_PrintedCards e WITH (NOLOCK) ON (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END) = (CASE WHEN ISNUMERIC(e.EmpNo) = 1 THEN e.EmpNo ELSE 0 END) 	
	WHERE (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CAST(a.EmpNo AS INT) ELSE 0 END) = 50080

	SELECT a.* 
	FROM tas.sy_PrintedCards a WITH (NOLOCK)
	WHERE (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CAST(a.EmpNo AS INT) ELSE 0 END) = 50080

	SELECT * FROM tas.sy_CardDates a
	WHERE (CASE WHEN ISNUMERIC(a.FName) = 1 THEN CAST(a.FName AS INT) ELSE 0 END) = 50080

	SELECT * FROM tas.sy_Vw_LIC_PrintedCards a
	WHERE (CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CAST(a.EmpNo AS INT) ELSE 0 END) = 50080

	SELECT * FROM tas.Vw_ContractorSwipe a WITH (NOLOCK)
	WHERE a.EmpNo = 50080
		--AND a.SwipeDate BETWEEN '06/16/2021' AND '07/15/2021'
	ORDER BY A.SwipeDate DESC

	SELECT EmpNo, COUNT(EmpNo) 
	FROM 
	(
		SELECT DISTINCT
			EmpNo, FName, LName, CostCode, DateStart, DateStop, PrintDate
		FROM tas.sy_PrintedCards
	) as a
	GROUP BY EmpNo
	HAVING ( COUNT(EmpNo) > 1 )
	ORDER BY EmpNo

	SELECT * FROM  tas.sy_PrintedCards a WITH (NOLOCK)
	WHERE CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END = 60559          

	SELECT * FROM tas.AccessSystemCostCenterMapping a
	SELECT * FROM tas.Master_BusinessUnit_JDE_view

	SELECT * FROM tas.fnGetContractorWorkDuration(61048, '06/27/2021', 8)

	SELECT COUNT(*) FROM tas.sy_EvnLog a WITH (NOLOCK)

	SELECT CONVERT(DATETIME, CONVERT(VARCHAR(8), GETDATE(), 12))
	SELECT CONVERT(DATETIME, CONVERT(VARCHAR(8), DATEADD(DAY, -1, GETDATE()), 12))

	SELECT * FROM tas.ContractorSwipeLog a WITH (NOLOCK)
	ORDER BY a.SwipeDate DESC, a.EmpNo, a.SwipeTime 