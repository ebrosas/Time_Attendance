USE [tas2]
GO

/****** Object:  View [tas].[Vw_FireTeamAndFireWatchMember]    Script Date: 16/08/2018 15:32:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_FireTeamAndFireWatchMember
*	Description: Get the list of all Fire Team nenbers
*
*	Date:			Author:		Rev. #:		Comments:
*	05/02/2018		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_FireTeamAndFireWatchMember]
AS		

	SELECT	CASE WHEN ISNUMERIC(a.FName) = 1 
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
			RTRIM(d.LocationName)  + ' - ' + RTRIM(d.ReaderName) AS SwipeLocation,
			(
				CASE	WHEN UPPER(RTRIM(d.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(d.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType, 		
			LTRIM(RTRIM(CONVERT(VARCHAR(500), b.Notes))) AS Notes,
			CASE WHEN c.UdfNum = 11 THEN 'Fire Team' ELSE 'Fire Watch' END AS GroupType
	FROM tas.sy_EvnLog a 
		INNER JOIN tas.sy_NAMES b ON LTRIM(RTRIM(a.FName)) = LTRIM(RTRIM(b.FName)) 
		INNER JOIN tas.sy_UDF c ON b.ID = c.NameID
		INNER JOIN tas.Master_AccessReaders d ON a.Loc = d.LocationCode AND a.Dev = d.ReaderNo
	WHERE 
		a.[Event] = 8	--(Note: 8 means successful swipe)
		AND c.UdfNum IN (11, 14)
		AND UPPER(LTRIM(RTRIM(c.UdfText))) = 'Y' 
		AND 
		(
			(a.Loc = 1 AND a.Dev IN (4, 6, 5, 7))
			OR (a.Loc = 2 AND a.Dev IN (0, 1, 2, 3)) 
		) 

GO


