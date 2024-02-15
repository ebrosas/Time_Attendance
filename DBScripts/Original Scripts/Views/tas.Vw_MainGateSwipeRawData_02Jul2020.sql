USE [tas2]
GO

/****** Object:  View [tas].[Vw_MainGateSwipeRawData]    Script Date: 02/07/2020 10:05:00 ******/
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
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_MainGateSwipeRawData]
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
		'MAINGATE' AS SwipeCode,
		b.LocationName,
		b.ReaderName,
		a.LName
	FROM tas.sy_EvnLog a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE a.[Event] = 8	--(Note: 8 means successful swipe)
		AND a.Dev NOT IN (8, 9)	--(Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)                 

GO


