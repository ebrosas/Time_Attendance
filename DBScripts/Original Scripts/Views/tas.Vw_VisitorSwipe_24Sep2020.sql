USE [tas2]
GO

/****** Object:  View [tas].[Vw_VisitorSwipe]    Script Date: 24/09/2020 12:11:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_VisitorSwipe
*	Description: Get the swipe records of specific Visitor
*
*	Date:			Author:		Rev. #:		Comments:
*	08/08/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_VisitorSwipe]
AS		

	--Get the Main Gate swipe records
	SELECT	0 AS SwipeID,
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
			a.LName AS EmpName,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) AS SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) AS SwipeTime,
			RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
			NULL AS SwipeTypeCode,
			(
				CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType,
			b.LocationCode,
			b.LocationName,
			b.ReaderNo,
			b.ReaderName,			
			'MAINGATE' AS SwipeCode,
			NULL AS LogID
	FROM tas.sy_EvnLog a
		INNER JOIN tas.Master_AccessReaders b ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE a.[Event] = 8	--(Note: 8 means successful swipe)

	UNION

	--Get the workplace swipe records
	SELECT	0 AS SwipeID,
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
			a.LName AS EmpName,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) AS SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) AS SwipeTime,			
			RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
			NULL AS SwipeTypeCode,
			(
				CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType,
			b.LocationCode,
			b.LocationName,
			b.ReaderNo,
			b.ReaderName,
			'WORKPLACE' AS SwipeCode,
			NULL AS LogID
	FROM tas.sy_ExtLog a
		INNER JOIN tas.Master_AccessReaders b ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE a.[Event] = 8	--(Note: 8 means successful swipe)

	UNION 

	--Get Manual Swipe recorded at the main gate entrance 
	SELECT	b.SwipeID,
			a.VisitorCardNo AS EmpNo,
			a.VisitorName AS EmpName,
			b.SwipeDate,
			b.SwipeTime,
			'Manual Swipe' AS SwipeLocation,
			b.SwipeTypeCode,
			CASE WHEN RTRIM(b.SwipeTypeCode) = 'valIN' THEN 'IN'
				WHEN RTRIM(b.SwipeTypeCode) = 'valOUT' THEN 'OUT'
				ELSE ''
			END AS SwipeType,
			NULL AS LocationCode,
			'' AS LocationName,
			NULL AS ReaderNo,
			'' AS ReaderName,
			'MANUAL' AS SwipeCode,
			b.LogID
	FROM tas.VisitorPassLog a
		INNER JOIN tas.VisitorSwipeLog b ON a.LogID = b.LogID

GO


