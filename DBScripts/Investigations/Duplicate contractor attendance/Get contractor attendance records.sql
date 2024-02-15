DECLARE	@startDate	DATETIME = '06/16/2021',	
		@endDate	DATETIME = '07/15/2021',
		@empNo		INT = 51301

	SELECT * FROM 
	(
		SELECT *, 28 AS TotalRecords, ROW_NUMBER() OVER (ORDER BY SwipeDate DESC, EmpNo) as RowNumber 
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
					b.SwipeTime,
					b.SwipeType,
					b.LocationName,
					b.ReaderName
			FROM tas.Vw_ContractorAttendance_V2 a WITH (NOLOCK)
				INNER JOIN tas.Vw_ContractorSwipe b WITH (NOLOCK) ON a.EmpNo = b.EmpNo 
			WHERE a.EmpNo < 10000000 
				AND a.EmpNo > 50000 
				AND  b.SwipeDate BETWEEN @startDate AND @endDate 
				AND  (a.EmpNo = @empNo)
				AND (a.IDStartDate < @startDate AND a.IDEndDate > @endDate)
		) as innerTable 
	) as outerTable 
	WHERE RowNumber BETWEEN 1 AND 28 
	ORDER BY SwipeDate DESC, EmpNo, SwipeTime