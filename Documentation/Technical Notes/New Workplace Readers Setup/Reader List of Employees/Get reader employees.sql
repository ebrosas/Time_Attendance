DECLARE	@costCenter		VARCHAR(12) = '5300'

	SELECT RTRIM(a.BusinessUnit) AS CostCenter, RTRIM(b.BUname) AS CostCenterName, a.EmpNo, a.EmpName, a.Position, a.GradeCode AS PayGrade,
		c.ShiftPatCode, d.IsDayShift 
	FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
		INNER JOIN tas.Master_BusinessUnit_JDE_view b WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(b.BU)
		INNER JOIN tas.Master_EmployeeAdditional c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND ISNULL(c.ShiftPatCode, '') <> ''
		INNER JOIN tas.Master_ShiftPatternTitles d WITH (NOLOCK) ON RTRIM(c.ShiftPatCode) = RTRIM(d.ShiftPatCode)
	WHERE RTRIM(a.BusinessUnit) = @costCenter
		AND ISNUMERIC(a.PayStatus) = 1
		AND d.IsDayShift = 1
		AND a.EmpNo NOT IN
		(
			SELECT EmpNo 
			FROM tas.WorkplaceSwipeExclusion a WITH (NOLOCK)
				CROSS APPLY
				(
					SELECT GenericNo AS ExcludedReaderNo 
					FROM tas.fnParseStringArrayToInt(RTRIM(a.ReaderNoList), ',') x
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.GenericNo = y.ReaderNo AND y.LocationCode = 8 AND y.ReaderNo BETWEEN 41 AND 70
				) b
			WHERE RTRIM(CostCenter) = @costCenter
				AND IsActive = 1
		)
	ORDER BY a.EmpNo

/*
	SELECT a.* 
	FROM tas.WorkplaceSwipeExclusion a WITH (NOLOCK)
		CROSS APPLY
		(
			SELECT GenericNo AS ExcludedReaderNo 
			FROM tas.fnParseStringArrayToInt(RTRIM(a.ReaderNoList), ',') x
				INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.GenericNo = y.ReaderNo AND y.LocationCode = 8 AND y.ReaderNo BETWEEN 41 AND 70
		) b
	WHERE RTRIM(CostCenter) = '5300'
		AND IsActive = 1

	SELECT a.* 
	FROM tas.WorkplaceSwipeExclusion a WITH (NOLOCK)
	WHERE RTRIM(CostCenter) = '5300'
		AND IsActive = 1

*/