USE [tas2]
GO

/****** Object:  View [tas].[Vw_VisitorSwipe]    Script Date: 07/08/2023 02:14:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*******************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_VisitorSwipe
*	Description: Get the swipe records of specific Visitor
*
*	Date:			Author:		Rev. #:		Comments:
*	08/08/2016		Ervin		1.0			Created
*	24/09/2020		Ervin		1.1			Implemented the new workplace reader from "unis_tenter" database
*	29/11/2020		Ervin		1.2			Added "UsedForTS" filter to return only readers that are used in Timesheet Processing
*	22/12/2022		Ervin		1.3			Commented code that fetch data from the old Access System
**********************************************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_VisitorSwipe]
AS		

	/*	Start of Rev. #1.3
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

	End of Rev. #1.3	*/
    
	--Get Workplace swipes (Note: New reader where data is fetch from "unis_tenter" database)	Rev. #1.1
	SELECT	0 AS SwipeID,
			a.EmpNo,
			a.EmpName,
			a.SwipeDate,
			a.SwipeDateTime AS SwipeTime,
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
	FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	--Rev. #1.2
	WHERE a.EventCode = 8	--(Note: 8 means successful swipe)

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


