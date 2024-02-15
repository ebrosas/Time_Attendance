	SELECT * FROM 
	(
		SELECT *, 933 AS TotalRecords, ROW_NUMBER() OVER (ORDER BY SwipeDate DESC, EmpNo) as RowNumber 
		FROM 
		(
			SELECT	a.EmpNo,
					a.EmpName,
					a.CostCenter,
					a.CostCenterName,
					a.CPRNo,
					a.JobTitle,
					a.EmployerName,
					a.StatusID,
					a.StatusDesc,
					a.ContractorTypeID,
					a.ContractorTypeDesc,
					a.IDStartDate,
					a.IDEndDate,
					a.ContractStartDate,
					a.ContractEndDate,
					a.RequiredWorkDuration,
					a.CreatedDate,
					a.CreatedByNo,
					a.CreatedByName,						
					b.SwipeDate,
					--b.SwipeTime,
					--b.SwipeType,
					b.LocationName,
					b.ReaderName,
					c.SwipeIn,
					c.SwipeOut,
					c.NetMinutes,
					c.Overtime
			FROM tas.Vw_ContractorAttendance_V2 a WITH (NOLOCK)
				CROSS APPLY	
				(
					SELECT DISTINCT SwipeDate, LocationName, ReaderName
					FROM tas.ContractorSwipeLog WITH (NOLOCK)
					WHERE EmpNo = a.EmpNo
				) b
				CROSS APPLY tas.fnGetContractorWorkDuration(a.EmpNo, b.SwipeDate, a.RequiredWorkDuration) c 
			WHERE a.EmpNo < 10000000 AND a.EmpNo > 50000 
				AND a.StatusID = 1 
				AND  CONVERT(VARCHAR, b.SwipeDate, 12) BETWEEN '210601' AND '210630' 
				AND (LOWER(LTRIM(RTRIM((a.EmpName)))) LIKE '%flora%' OR LOWER(LTRIM(RTRIM((a.EmployerName)))) LIKE '%flora%')
		) as innerTable 
	) as outerTable 
	--WHERE RowNumber BETWEEN 1 AND 100 
	ORDER BY SwipeDate DESC, EmpNo
