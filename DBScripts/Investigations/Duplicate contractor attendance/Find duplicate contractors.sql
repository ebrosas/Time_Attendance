	SELECT EmpNo, COUNT(EmpNo) 
	FROM 
	(
		SELECT DISTINCT
			EmpNo, FName, LName
		FROM tas.sy_PrintedCards a WITH (NOLOCK)
	) as a
	GROUP BY EmpNo--, FName, LName
	HAVING ( COUNT(EmpNo) > 1 )
	ORDER BY EmpNo