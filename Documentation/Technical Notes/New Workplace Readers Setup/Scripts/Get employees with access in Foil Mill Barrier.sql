DECLARE	@startDate	DATETIME = 	'07/01/2021',
		@endDate	DATETIME = '08/12/2021'

	SELECT	DISTINCT 
			a.EmpNo AS 'Emp. No.', 
			b.EmpName AS 'Emp. Name',
			RTRIM(b.BusinessUnit) AS 'Cost Center',
			RTRIM(c.BUname) AS 'Cost Center Name',
			b.GradeCode AS 'Pay Grade'
	FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1 AND b.DateResigned IS NULL
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(c.BU)
	WHERE a.ReaderNo = 1	--Foil Mill Barrier (Out)
		AND a.SwipeDate BETWEEN @startDate AND @endDate 
	ORDER BY [Cost Center], [Emp. No.]