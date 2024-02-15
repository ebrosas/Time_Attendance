DECLARE	@loadType		TINYINT		= 0,				--(Note: 0 = Filter by Reader Number, 1 = Get swipe from all new workplace readers)
		@startDate		DATETIME	= '03/17/2021',		--(Note: 29-Nov-2020 is the date when the new workplace readers were deployed in the plant)
		@endDate		DATETIME	= '03/31/2021',
		@readerNo		INT			= 56,				--(Note: 13/44 = EMD Workshop, 41 = Annealing 123, 42 = Annealing 456, 43 = Roll Grinder 1, 48 = CM1 Floor Intercom, 49 - CM2 Floor Intercom, 50 - Water Treatment)
		@empNo			INT			= 0,
		@costCenter		VARCHAR(12)	= ''
			
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
			a.EventCode
	FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
	WHERE a.ReaderNo IN (54, 55, 56, 57, 58, 59)
		AND a.SwipeDate BETWEEN @startDate AND @endDate
		AND a.EventCode <> 8
	ORDER BY a.ReaderNo