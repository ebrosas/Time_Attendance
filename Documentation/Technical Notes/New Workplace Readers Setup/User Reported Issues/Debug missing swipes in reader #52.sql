	SELECT	a.EmpNo,
			a.EmpName,
			a.SwipeDate,
			a.SwipeDateTime AS SwipeTime,
			RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
			(
				CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType,
			b.LocationCode,
			b.ReaderNo,
			'WORKPLACE' AS SwipeCode,
			a.EventCode
	FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
		LEFT JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
	WHERE /*a.EventCode = 8	--(Note: 8 means successful swipe)
		AND*/ a.ReaderNo = 52