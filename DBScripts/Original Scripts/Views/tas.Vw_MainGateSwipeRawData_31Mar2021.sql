USE [tas2]
GO

/****** Object:  View [tas].[Vw_MainGateSwipeRawData]    Script Date: 31/03/2021 09:41:25 ******/
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
*	11/11/2020		Ervin		1.5			Added union to "Vw_NewReaderSwipeData" view
*	05/01/2021		Ervin		1.6			Returned the top 30000 data from "tas.External_DSX_evnlog" view
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_MainGateSwipeRawData]
AS
		
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
		SELECT	TOP 40000
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

		--Get swipe data from the Car Park #5
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

		UNION ALL
    
		--Get swipe data from the new readers that use "UNIS_TENTER" database (Rev. #1.5)
		SELECT	TOP 20000
				A.EmpNo, 
				A.EmpName,
				A.SwipeDateTime AS DT,
				A.LocationCode,
				A.ReaderNo,
				CASE WHEN A.EventCode = 0 THEN 8 ELSE A.EventCode END AS EventCode,
				A.SOURCE
		FROM tas.Vw_NewReaderSwipeData A WITH (NOLOCK)
		WHERE A.EmpNo > 0
		ORDER BY A.SwipeDateTime DESC
	) A
	INNER JOIN tas.Master_AccessReaders B WITH (NOLOCK) ON A.LocationCode = B.LocationCode AND A.ReaderNo = B.ReaderNo
	WHERE A.EventCode = 8	--(Note: 8 means successful swipe)
		

/*	Debug:

	--Get the count of records
	SELECT * FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)
	WHERE a.SwipeDate BETWEEN '11/16/2020' AND '12/15/2020'

	SELECT * FROM tas.Vw_CarParkSwipeData a WITH (NOLOCK)
	WHERE a.SwipeDate BETWEEN '10/16/2020' AND '11/15/2020'

	SELECT * FROM tas.External_DSX_evnlog a WITH (NOLOCK)
	WHERE a.TimeDate BETWEEN '10/16/2020' AND '11/15/2020'


	SELECT * FROM tas.Vw_MainGateSwipeRawData a
	WHERE a.EmpNo = 10003631
		AND a.SwipeDate BETWEEN '12/16/2020' AND '01/15/2021' 

	SELECT * FROM tas.Vw_MainGateSwipeRawData a
	WHERE a.SwipeDate BETWEEN '11/16/2020' AND '12/15/2020'

*/


GO


