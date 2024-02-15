DECLARE	@loadType		TINYINT		= 0,				--(Note: 0 = Filter by Reader Number, 1 = Get swipe from all new workplace readers)
		@startDate		DATETIME	= '03/17/2021',		--(Note: 29-Nov-2020 is the date when the new workplace readers were deployed in the plant)
		@endDate		DATETIME	= '03/31/2021',
		@readerNo		INT			= 53,				--(Note: 13/44 = EMD Workshop, 41 = Annealing 123, 42 = Annealing 456, 43 = Roll Grinder 1, 48 = CM1 Floor Intercom, 49 - CM2 Floor Intercom, 50 - Water Treatment)
		@empNo			INT			= 0,
		@costCenter		VARCHAR(12)	= ''

	IF ISNULL(@readerNo, 0) = 0
		SET @readerNo = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL
	
	IF @loadType = 0
	BEGIN
				
		SELECT * FROM
		(
			SELECT 
				CASE WHEN ISNUMERIC(a.FName) = 1 
					THEN 
						CASE WHEN 
							(
								((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000) 
								OR CAST(a.FName AS INT) BETWEEN 10010000 AND 10019999		--Rev. #2.02
							) 
							THEN CONVERT(INT, a.FName)
							ELSE CONVERT(INT, a.FName) + 10000000 
						END
					ELSE 0 
				END AS EmpNo,
				a.LName AS EmpName,
				CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) AS SwipeDate,
				CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) AS SwipeTime,
				RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
				(
					CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
							WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
							ELSE '' END
				) AS SwipeType,
				b.LocationCode,
				b.ReaderNo,
				'WORKPLACE' AS SwipeCode
			FROM tas.sy_ExtLog a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
			WHERE a.[Event] = 8	--(Note: 8 means successful swipe)			
				AND (b.ReaderNo = @readerNo OR @readerNo IS NULL)

			UNION
    
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
					'WORKPLACE' AS SwipeCode
			FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
			WHERE a.EventCode = 8	--(Note: 8 means successful swipe)
				AND (a.ReaderNo = @readerNo OR @readerNo IS NULL)
		) a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
		WHERE a.SwipeDate BETWEEN @startDate AND @endDate
			AND (a.EmpNo = @empNo OR @empNo IS NULL) 
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		ORDER BY a.SwipeDate DESC, a.SwipeTime DESC
	END
	
	ELSE IF @loadType = 1
	BEGIN

		SELECT * FROM
		(
			SELECT 
				CASE WHEN ISNUMERIC(a.FName) = 1 
					THEN 
						CASE WHEN 
							(
								((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000) 
								OR CAST(a.FName AS INT) BETWEEN 10010000 AND 10019999		--Rev. #2.02
							) 
							THEN CONVERT(INT, a.FName)
							ELSE CONVERT(INT, a.FName) + 10000000 
						END
					ELSE 0 
				END AS EmpNo,
				a.LName AS EmpName,
				CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) AS SwipeDate,
				CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) AS SwipeTime,
				RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
				(
					CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
							WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
							ELSE '' END
				) AS SwipeType,
				b.LocationCode,
				b.ReaderNo,
				'WORKPLACE' AS SwipeCode
			FROM tas.sy_ExtLog a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
			WHERE a.[Event] = 8	--(Note: 8 means successful swipe)			
				AND (b.ReaderNo BETWEEN 0 AND 19)

			UNION
    
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
					'WORKPLACE' AS SwipeCode
			FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
			WHERE a.EventCode = 8	--(Note: 8 means successful swipe)
				--AND (a.ReaderNo IN (13, 41, 42, 43, 44))
		) a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
		WHERE a.SwipeDate BETWEEN @startDate AND @endDate
			AND (a.EmpNo = @empNo OR @empNo IS NULL) 
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)			
		ORDER BY a.SwipeDate DESC, a.ReaderNo, a.EmpNo, a.SwipeTime DESC
    END 


/*	Debug:

	SELECT * FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
	WHERE a.SwipeDate = '01/25/2021'
		AND a.ReaderNo IN (48, 49, 50)
	ORDER BY a.ReaderNo, a.EmpNo

*/