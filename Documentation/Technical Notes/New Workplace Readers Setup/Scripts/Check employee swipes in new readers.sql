DECLARE @readerNo		INT = 15,	--(Main Mill: 13 = Turnstile (In), 14 =	Turnstile (Out), 15 = Barrier (In), 16 = Barrier (Out))
									--(Foil Mill: 31 = Turnstile (In), 32 =	Turnstile (Out), 33 = Barrier (In), 34 = Barrier (Out))
		@swipeDate		DATETIME = '11/09/2021'
	
	SELECT b.BusinessUnit, a.* 
	FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
	WHERE a.ReaderNo = @readerNo
		AND a.SwipeDate >= @swipeDate
		--AND a.EventCode = 8
	ORDER BY b.BusinessUnit, a.EmpNo