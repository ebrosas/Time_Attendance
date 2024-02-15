USE [tas2]
GO

/****** Object:  View [tas].[Vw_MainGateSwipeRawData]    Script Date: 11/11/2020 15:48:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_MainGateSwipeRawData
*	Description: This view fetches the workplace swipes raw data
*
*	Date:			Author:		Rev. #:		Comments:
*	29/10/2015		Ervin		1.0			Created
*	15/11/2016		Ervin		1.1			Added condition to exclude swipes at reader nos. 8 and 9 which are used as test readers
*	22/03/2020		Ervin		1.2			Refactored the logic in fetching the employee number
*	02/07/2020		Ervin		1.3			Added the following fields: "EventCode", "TimeDate"
*	08/08/2020		Ervin		1.4			Implemented logic for Car Park #5 reader nos. 11 (IN) and 12 (OUT)
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_MainGateSwipeRawData]
AS

	
	/* Old code commented
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
		a.LName,
		a.[Event] EventCode,
		a.TimeDate
	FROM tas.sy_EvnLog a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE a.[Event] = 8	--(Note: 8 means successful swipe)
		AND a.Dev NOT IN (8, 9)	--(Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)      
	*/           

	--Start of Rev. #1.4
	SELECT	a.EmpNo,  
			CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) AS SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 126)) AS SwipeTime,
			RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
			CASE WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'In' 
				WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'Out' 
				ELSE '' 
			END AS SwipeType,
			b.LocationCode,
			b.ReaderNo,
			'MAINGATE' AS SwipeCode,
			b.LocationName,
			b.ReaderName,
			a.EmpName AS LName,
			a.EventCode,
			a.DT AS TimeDate
	FROM
    (
		SELECT	TOP 100000
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN CONVERT(INT, a.FName) <= 9999 
						THEN CONVERT(INT, a.FName) + 10000000
						ELSE CONVERT(INT, a.FName) 
					END
				ELSE 0 
			END AS EmpNo,
			RTRIM(a.LName) AS EmpName,
			a.TimeDate AS DT,
			a.Loc AS LocationCode,
			a.Dev AS ReaderNo,
			a.[Event] AS EventCode,
			'A' 'Source'
		FROM tas.External_DSX_evnlog a WITH (NOLOCK)
		WHERE ISNULL(a.FName, '') <> ''				
		ORDER BY a.TimeDate DESC

		UNION ALL  

		SELECT	TOP 20000
				a.EmpNo, 
				a.EmpName,
				a.SwipeDateTime AS DT,
				a.LocationCode,
				a.ReaderNo,
				CASE WHEN a.EventCode = 0 THEN 8 ELSE a.EventCode END AS EventCode,
				a.Source
		FROM tas.Vw_CarParkSwipeData a WITH (NOLOCK)
		WHERE a.EmpNo > 0
		ORDER BY a.SwipeDateTime DESC 
	) A
	INNER JOIN tas.Master_AccessReaders B WITH (NOLOCK) ON A.LocationCode = B.LocationCode AND A.ReaderNo = B.ReaderNo
	WHERE A.EventCode = 8	--(Note: 8 means successful swipe)
	--End of Rev. #1.4
	
GO


