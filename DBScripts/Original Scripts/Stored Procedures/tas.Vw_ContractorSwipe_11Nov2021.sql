USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorSwipe]    Script Date: 11/11/2021 14:58:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ContractorSwipe
*	Description: Get the Contractors swipe data starting from the date the new ID badge has been implemented
*
*	Date:			Author:		Rev. #:		Comments:
*	14/10/2016		Ervin		1.0			Created
*	17/10/2016		Ervin		1.1			Added filter condition for the LocationCode and ReaderNo		
*	26/08/2018		Ervin		1.2			Commented the filter condition "a.[Event] = 8"
*	26/09/2019		Ervin		1.3			Added "Event" field
*	16/06/2021		Ervin		1.4			Return the top 10000 records to enhance data retrieval performance
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_ContractorSwipe]
AS

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
		a.LName,
		a.[Event]
	FROM tas.sy_EvnLog a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE 
		--a.[Event] = 8	--(Note: 8 means successful swipe)
		--AND 
		(
			(b.LocationCode = 1 AND b.ReaderNo IN (4, 5, 6, 7))
			OR
            (b.LocationCode = 2 AND b.ReaderNo IN (0, 1, 2, 3))
		)
		AND 
		(
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN 
						CONVERT(INT, a.FName)
					ELSE 
						CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END
		) < 10000000
		AND 
		(
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN 
						CONVERT(INT, a.FName)
					ELSE 
						CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END
		) > 50000
		AND CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) >= '10/10/2016'	--Refers to the date the new Contractor ID badge was implemented
		
GO


