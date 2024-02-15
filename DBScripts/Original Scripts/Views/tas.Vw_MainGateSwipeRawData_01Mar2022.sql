USE [tas2]
GO

/****** Object:  View [tas].[Vw_MainGateSwipeRawData]    Script Date: 01/03/2022 12:21:58 ******/
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
*	31/03/2021		Ervin		1.7			Cleaned up the code
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_MainGateSwipeRawData]
AS
		
	SELECT	x.EmpNo,  
			CONVERT(DATETIME, CONVERT(VARCHAR, x.DT, 12)) AS SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, x.DT, 126)) AS SwipeTime,
			RTRIM(y.LocationName)  + ' - ' + RTRIM(y.ReaderName) AS SwipeLocation,
			CASE WHEN UPPER(RTRIM(y.Direction)) = 'I' THEN 'In' 
				WHEN UPPER(RTRIM(y.Direction)) = 'O' THEN 'Out' 
				ELSE '' 
			END AS SwipeType,
			y.LocationCode,
			y.ReaderNo,
			'MAINGATE' AS SwipeCode,
			y.LocationName,
			y.ReaderName,
			x.EmpName AS LName,
			x.EventCode,
			x.DT AS TimeDate
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
				a.EventCode,
				a.[Source]
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
				A.EventCode,
				A.[SOURCE]
		FROM tas.Vw_NewReaderSwipeData A WITH (NOLOCK)
		WHERE A.EmpNo > 0
		ORDER BY A.SwipeDateTime DESC
	) x
	INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.LocationCode = y.LocationCode AND x.ReaderNo = y.ReaderNo
	WHERE x.EventCode = 8	--(Note: 8 means successful swipe)
		

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
	WHERE a.SwipeDate BETWEEN '03/16/2021' AND '04/15/2021'
		AND a.EmpNo = 10003632
	ORDER BY a.SwipeDate DESC, a.SwipeTime

*/


GO


