/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_AllEmployeeSwipe
*	Description: This view fetches the swipe records of all people in GARMCO per given day
*
*	Date:			Author:		Rev.#:		Comments:
*	17/04/2019		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_AllEmployeeSwipe
AS
	
	SELECT 
		a.SwipeDate, 
		a.EmpNo, 
		a.SwipeTime, 
		a.SwipeLocation, 
		a.SwipeType, 
		d.Effective_ShiftPatCode AS ShiftPatCode,
		d.Effective_ShiftPointer AS ShiftPointer,
		d.Effective_ShiftCode AS ShiftCode
	FROM
	(
		--Get main gate swipe records
		SELECT 
			CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
			END AS EmpNo,
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
			'MAINGATE' AS SwipeCode,
			b.LocationName,
			b.ReaderName,
			a.LName
		FROM tas.sy_EvnLog a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
		WHERE 
			a.[Event] = 8				--(Note: 8 means successful swipe)
			AND a.Dev NOT IN (8, 9)		--Rev. #1.5 (Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)                 

		/*
		UNION

		--Get workplace swipe records
		
		SELECT 
			CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
			END AS EmpNo,
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
			'WORKPLACE' AS SwipeCode,
			b.LocationName,
			b.ReaderName,
			a.LName
		FROM tas.sy_ExtLog a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
		WHERE a.[Event] = 8	--(Note: 8 means successful swipe)
		*/

		/*************************************************************************************************************
			Rev. #1.3 - Get manual swipe attendance records at the workplace which is already processed in Timesheet
		*************************************************************************************************************/
		UNION

		SELECT	a.EmpNo,
				a.SwipeDate,
				CASE WHEN CorrectionType = 1 THEN TimeInWP
					WHEN CorrectionType = 2 THEN TimeOutWP
					ELSE NULL
					END AS SwipeTime,
				'Workplace Manual Swipe' AS SwipeLocation,
				CASE WHEN CorrectionType = 1 THEN 'IN'
					WHEN CorrectionType = 2 THEN 'OUT'
					ELSE ''
					END AS SwipeType,
				0 AS LocationCode,
				0 AS ReaderNo,
				'WORKPLACE' AS SwipeCode,
				'' AS LocationName,
				'' AS ReaderName,
				EmpName AS LName
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType IN (1, 2)

		UNION

		SELECT	a.EmpNo,
				a.SwipeDate,
				a.TimeInWP AS SwipeTime,
				'Workplace Manual Swipe' AS SwipeLocation,
				'IN' AS SwipeType,
				0 AS LocationCode,
				0 AS ReaderNo,
				'WORKPLACE' AS SwipeCode,
				'' AS LocationName,
				'' AS ReaderName,
				EmpName AS LName
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType = 3

		UNION 
	
		SELECT	a.EmpNo,
				a.SwipeDate,
				a.TimeOutWP AS SwipeTime,
				'Workplace Manual Swipe' AS SwipeLocation,
				'OUT' AS SwipeType,
				0 AS LocationCode,
				0 AS ReaderNo,
				'WORKPLACE' AS SwipeCode,
				'' AS LocationName,
				'' AS ReaderName,
				EmpName AS LName
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType = 3
	) AS a
	LEFT JOIN tas.Tran_ShiftPatternUpdates d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DateX
	LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(d.Effective_ShiftPatCode) = RTRIM(f.ShiftPatCode) 	

GO



/*	Debug:

	SELECT * FROM tas.Vw_AllEmployeeSwipe a
	WHERE a.EmpNo = 10003631
		AND a.SwipeDate = '04/17/2019'

*/


