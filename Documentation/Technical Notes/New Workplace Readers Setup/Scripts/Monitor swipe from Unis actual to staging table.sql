DECLARE	@empNo	INT = 10003589,
		@startDate	DATETIME = '12/23/2021',
		@endDate	DATETIME = '12/23/2021'

	--New staging table
	--SELECT * FROM [unis].[dbo].[tEnter_stg] a WITH (NOLOCK)
	--WHERE CAST(a.C_Unique AS INT) + 10000000 = @empNo 
	--	AND CAST(a.C_date AS DATETIME) BETWEEN @startDate AND @endDate
	--ORDER BY a.C_Time DESC

	SELECT * FROM
    (
		SELECT 
			CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,	--Rev. #1.2
			RTRIM(a.C_Name) AS EmpName,
			CAST(a.C_date AS DATETIME) AS SwipeDate,
			CONVERT
			(
				TIME, 
				CONVERT(VARCHAR(2), a.C_Time / 10000) + ':' +
				CONVERT(VARCHAR(2), (a.C_Time % 10000) / 100) + ':' +
				CONVERT(VARCHAR(2), a.C_Time % 100)          
			)  AS SwipeTime,
			CAST(a.C_date AS DATETIME) +
				CONVERT
				(
					TIME, 
					CONVERT(VARCHAR(2), a.C_Time / 10000) + ':' +
					CONVERT(VARCHAR(2), (a.C_Time % 10000) / 100) + ':' +
					CONVERT(VARCHAR(2), a.C_Time % 100)          
				)  AS SwipeDateTime,
			b.LocationCode,
			a.L_TID AS ReaderNo,
			CASE WHEN a.L_Result = 0 THEN 8 ELSE a.L_Result END AS EventCode,
			'A' AS Source,
			CASE WHEN RTRIM(b.Direction) = 'I' THEN 'IN'
				WHEN RTRIM(b.Direction) = 'O' THEN 'OUT'
				WHEN RTRIM(b.Direction) = 'IO' THEN 'IN/OUT'
				ELSE ''
			END AS Direction,
			RTRIM(a.C_Card) AS CardNo,
			a.L_UID AS UserID,
			b.SourceID		
		FROM [unis].[dbo].[tEnter_stg] a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo --AND b.SourceID = 1		
		WHERE b.LocationCode IN (1, 2, 8)	
	) x
	WHERE x.EmpNo = @empNo
		AND x.SwipeDate BETWEEN @startDate AND @endDate
	ORDER BY x.SwipeDate DESC, x.EmpNo, x.SwipeTime DESC

	--Actual UNIS table
	--SELECT * FROM tas.unis_tenter a WITH (NOLOCK)
	--WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END = @empNo 
	--	AND CAST(a.C_date AS DATETIME) BETWEEN @startDate AND @endDate
	--ORDER BY a.C_Time DESC

	SELECT * FROM
    (
		SELECT 
			CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,	--Rev. #1.2
			RTRIM(a.C_Name) AS EmpName,
			CAST(a.C_date AS DATETIME) AS SwipeDate,
			CONVERT
			(
				TIME, 
				CONVERT(VARCHAR(2), a.C_Time / 10000) + ':' +
				CONVERT(VARCHAR(2), (a.C_Time % 10000) / 100) + ':' +
				CONVERT(VARCHAR(2), a.C_Time % 100)          
			)  AS SwipeTime,
			CAST(a.C_date AS DATETIME) +
				CONVERT
				(
					TIME, 
					CONVERT(VARCHAR(2), a.C_Time / 10000) + ':' +
					CONVERT(VARCHAR(2), (a.C_Time % 10000) / 100) + ':' +
					CONVERT(VARCHAR(2), a.C_Time % 100)          
				)  AS SwipeDateTime,
			b.LocationCode,
			a.L_TID AS ReaderNo,
			CASE WHEN a.L_Result = 0 THEN 8 ELSE a.L_Result END AS EventCode,
			'A' AS Source,
			CASE WHEN RTRIM(b.Direction) = 'I' THEN 'IN'
				WHEN RTRIM(b.Direction) = 'O' THEN 'OUT'
				WHEN RTRIM(b.Direction) = 'IO' THEN 'IN/OUT'
				ELSE ''
			END AS Direction,
			RTRIM(a.C_Card) AS CardNo,
			a.L_UID AS UserID		
		FROM [UNIS].[dbo].[tEnter] a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
		WHERE b.SourceID = 1				
			AND b.LocationCode IN (1, 2, 8)	
	) x
	WHERE x.EmpNo = @empNo
		AND x.SwipeDate BETWEEN @startDate AND @endDate
	ORDER BY x.SwipeDate DESC, x.EmpNo, x.SwipeTime DESC

/*	Debug:

	SELECT b.LocationCode, b.SourceID, a.* 
	FROM tas.unis_tenter a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END = 10001859
		AND CAST(a.C_date AS DATETIME) = '12/07/2021'

	SELECT b.LocationCode, b.SourceID, a.*
	FROM [UNIS].[dbo].[tEnter] a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END = 10001859
		AND CAST(a.C_date AS DATETIME) = '12/07/2021'

	SELECT b.LocationCode, b.SourceID, a.*
	FROM [UNIS].[dbo].[tEnter_stg] a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END = 10001859
		AND CAST(a.C_date AS DATETIME) = '12/07/2021'

*/