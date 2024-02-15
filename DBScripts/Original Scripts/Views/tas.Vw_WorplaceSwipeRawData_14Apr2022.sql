USE [tas2]
GO

/****** Object:  View [tas].[Vw_WorplaceSwipeRawData]    Script Date: 14/04/2022 10:31:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_WorplaceSwipeRawData
*	Description: This view fetches the workplace swipes raw data
*
*	Date:			Author:		Rev. #:		Comments:
*	29/10/2015		Ervin		1.0			Created
*	29/12/2015		Ervin		1.1			Added 3 hours in the workplace swipe in/out time due to the delay in the reader device's clock
*	22/03/2020		Ervin		1.2			Refactored the logic in fetching the employee number
*	24/09/2020		Ervin		1.3			Implemented the new workplace reader from "unis_tenter" database
*	29/11/2020		Ervin		1.4			Added "UsedForTS" filter to return only readers that are used in Timesheet Processing
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_WorplaceSwipeRawData]
AS

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
		'WORKPLACE' AS SwipeCode,
		b.LocationName,
		b.ReaderName,
		a.LName
	FROM tas.sy_ExtLog a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE a.[Event] = 8	--(Note: 8 means successful swipe)	

	UNION
    
	--Get Workplace swipes (Note: New reader where data is fetch from "unis_tenter" database)	Rev. #1.3
	SELECT	a.EmpNo,
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
			b.LocationName,
			b.ReaderName,
			RTRIM(a.EmpName) AS LName
	FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	--Rev. #1.4
	WHERE a.EventCode = 8	--(Note: 8 means successful swipe)

GO


